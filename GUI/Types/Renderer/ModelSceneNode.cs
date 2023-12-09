using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTK.Graphics.OpenGL;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.ResourceTypes.ModelAnimation;
using ValveResourceFormat.Serialization;

namespace GUI.Types.Renderer
{
    class ModelSceneNode : SceneNode, IRenderableMeshCollection
    {
        public Vector4 Tint
        {
            get
            {
                if (meshRenderers.Count > 0)
                {
                    return meshRenderers[0].Tint;
                }

                return Vector4.One;
            }
            set
            {
                foreach (var renderer in meshRenderers)
                {
                    renderer.Tint = value;
                }
            }
        }

        public readonly AnimationController AnimationController;
        public List<RenderableMesh> RenderableMeshes => activeMeshRenderers;
        public string ActiveMaterialGroup => activeMaterialGroup.Name;

        private readonly List<RenderableMesh> meshRenderers = [];
        private readonly List<Animation> animations = [];


        private int animationTexture = -1;
        private readonly int bonesCount;

        private HashSet<string> activeMeshGroups = [];
        private List<RenderableMesh> activeMeshRenderers = [];
        private (string Name, string[] Materials) activeMaterialGroup;
        private Dictionary<string, string> materialTable;

        private readonly (string Name, string[] Materials)[] materialGroups;
        private readonly string[] meshGroups;
        private readonly ulong[] meshGroupMasks;
        private readonly List<(int MeshIndex, string MeshName, long LoDMask)> meshNamesForLod1;

        public ModelSceneNode(Scene scene, Model model, string skin = null, bool optimizeForMapLoad = false)
            : base(scene)
        {
            materialGroups = model.GetMaterialGroups().ToArray();
            meshGroups = model.GetMeshGroups().ToArray();

            if (meshGroups.Length > 1)
            {
                meshGroupMasks = model.Data.GetUnsignedIntegerArray("m_refMeshGroupMasks");
            }

            meshNamesForLod1 = model.GetReferenceMeshNamesAndLoD().Where(m => (m.LoDMask & 1) != 0).ToList();

            if (optimizeForMapLoad)
            {
                model.SetSkeletonFilteredForLod0();
            }

            AnimationController = new(model.Skeleton);
            bonesCount = model.Skeleton.Bones.Length;

            if (skin != null)
            {
                SetMaterialGroup(skin);
            }

            LoadMeshes(model);
            UpdateBoundingBox();
            LoadAnimations(model);
        }

        public override void Update(Scene.UpdateContext context)
        {
            if (!AnimationController.Update(context.Timestep))
            {
                return;
            }

            UpdateBoundingBox(); // Reset back to the mesh bbox

            var newBoundingBox = LocalBoundingBox;

            // Update animation matrices

            var matrices = ArrayPool<Matrix4x4>.Shared.Rent(bonesCount);

            try
            {
                AnimationController.GetAnimationMatrices(matrices);

                // Update animation texture
                GL.BindTexture(TextureTarget.Texture2D, animationTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 4, bonesCount, 0, PixelFormat.Rgba, PixelType.Float, matrices);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                var first = true;
                foreach (var matrix in matrices)
                {
                    var bbox = LocalBoundingBox.Transform(matrix);
                    newBoundingBox = first ? bbox : newBoundingBox.Union(bbox);
                    first = false;
                }

                LocalBoundingBox = newBoundingBox;
            }
            finally
            {
                ArrayPool<Matrix4x4>.Shared.Return(matrices);
            }

            if (AnimationController.ActiveAnimation != null)
            {
                Transform = Matrix4x4.Identity * AnimationController.ActiveAnimation.GetCurrentMovementOffset(AnimationController.Time);
            }
        }

        public override void Render(Scene.RenderContext context)
        {
            // This node does not render itself; it uses the batching system via IRenderableMeshCollection
        }

        public override IEnumerable<string> GetSupportedRenderModes()
            => meshRenderers.SelectMany(renderer => renderer.GetSupportedRenderModes()).Distinct();

        public override void SetRenderMode(string renderMode)
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.SetRenderMode(renderMode);
            }
        }

        public void SetMaterialGroup(string name)
        {
            if (materialGroups.Length == 0)
            {
                return;
            }

            if (materialTable is null)
            {
                var @default = materialGroups[0];
                activeMaterialGroup = @default;
                materialTable = new(materialGroups[0].Materials.Length);

                if (name == @default.Name)
                {
                    return;
                }
            }

            foreach (var materialGroup in materialGroups)
            {
                if (name == materialGroup.Name)
                {
                    materialTable.Clear();

                    foreach (var (Active, Replacement) in activeMaterialGroup.Materials.Zip(materialGroup.Materials))
                    {
                        materialTable[Active] = Replacement;
                    }

                    activeMaterialGroup = materialGroup;

                    foreach (var mesh in meshRenderers)
                    {
                        mesh.ReplaceMaterials(materialTable);
                    }
                }
            }
        }

        private void LoadAnimations(Model model)
        {
            animations.AddRange(model.GetAllAnimations(Scene.GuiContext.FileLoader));

            if (animations.Count != 0)
            {
                SetupAnimationTextures();
            }
        }

        private void LoadMeshes(Model model)
        {
            // Get embedded meshes
            foreach (var embeddedMesh in model.GetEmbeddedMeshesAndLoD().Where(m => (m.LoDMask & 1) != 0))
            {
                meshRenderers.Add(new RenderableMesh(embeddedMesh.Mesh, embeddedMesh.MeshIndex, Scene, model, materialTable));
            }

            // Load referred meshes from file (only load meshes with LoD 1)
            foreach (var refMesh in GetLod1RefMeshes())
            {
                var newResource = Scene.GuiContext.LoadFileCompiled(refMesh.MeshName);
                if (newResource == null)
                {
                    continue;
                }

                meshRenderers.Add(new RenderableMesh((Mesh)newResource.DataBlock, refMesh.MeshIndex, Scene, model, materialTable));
            }

            // Set active meshes to default
            SetActiveMeshGroups(model.GetDefaultMeshGroups());
        }

        private void SetupAnimationTextures()
        {
            if (animationTexture == -1)
            {
                // Create animation texture
                animationTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, animationTexture);
                // Set clamping to edges
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                // Set nearest-neighbor sampling since we don't want to interpolate matrix rows
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                //Unbind texture again
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public IEnumerable<string> GetSupportedAnimationNames()
            => animations.Select(a => a.Name);

        public void SetAnimation(string animationName)
        {
            var activeAnimation = animations.FirstOrDefault(a => a.Name == animationName);
            SetAnimation(activeAnimation);
        }

        public bool SetAnimationForWorldPreview(string animationName)
        {
            Animation activeAnimation = null;

            if (animationName != null)
            {
                activeAnimation = animations.FirstOrDefault(a => a.Name == animationName || (a.Name[0] == '@' && a.Name[1..] == animationName));
            }

            // TODO: CS2 falls back to the first animation, but other games seemingly do not.
            //activeAnimation ??= animations.FirstOrDefault(); // Fallback to the first animation

            if (activeAnimation != null)
            {
                SetAnimation(activeAnimation);
                return true;
            }

            return false;
        }

        public void SetAnimation(Animation activeAnimation)
        {
            AnimationController.SetAnimation(activeAnimation);
            UpdateBoundingBox();

            if (activeAnimation != default)
            {
                foreach (var renderer in meshRenderers)
                {
                    renderer.SetAnimationTexture(animationTexture, bonesCount);
                }
            }
            else
            {
                foreach (var renderer in meshRenderers)
                {
                    renderer.SetAnimationTexture(null, 0);
                }
            }
        }

        public IEnumerable<(int MeshIndex, string MeshName, long LoDMask)> GetLod1RefMeshes()
            => meshNamesForLod1;

        public IEnumerable<string> GetMeshGroups()
            => meshGroups;

        public ICollection<string> GetActiveMeshGroups()
            => activeMeshGroups;

        private IEnumerable<bool> GetActiveMeshMaskForGroup(string groupName)
        {
            var groupIndex = meshGroups.ToList().IndexOf(groupName);
            if (groupIndex >= 0)
            {
                return meshGroupMasks.Select(mask => (mask & 1UL << groupIndex) != 0);
            }
            else
            {
                return meshGroupMasks.Select(_ => false);
            }
        }

        public void SetActiveMeshGroups(IEnumerable<string> setMeshGroups)
        {
            activeMeshGroups = new HashSet<string>(meshGroups.Intersect(setMeshGroups));

            if (meshGroups.Length > 1)
            {
                activeMeshRenderers.Clear();
                foreach (var group in activeMeshGroups)
                {
                    var meshMask = GetActiveMeshMaskForGroup(group).ToArray();

                    foreach (var meshRenderer in meshRenderers)
                    {
                        if (meshMask[meshRenderer.MeshIndex])
                        {
                            activeMeshRenderers.Add(meshRenderer);
                        }
                    }
                }
            }
            else
            {
                activeMeshRenderers = new List<RenderableMesh>(meshRenderers);
            }
        }

        private void UpdateBoundingBox()
        {
            var first = true;
            foreach (var mesh in meshRenderers)
            {
                LocalBoundingBox = first ? mesh.BoundingBox : LocalBoundingBox.Union(mesh.BoundingBox);
                first = false;
            }
        }
    }
}
