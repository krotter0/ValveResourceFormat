using OpenTK.Graphics.OpenGL;

namespace GUI.Types.Renderer
{
    internal class InfiniteGrid
    {
        private readonly int vao;
        private readonly Shader shader;

        public InfiniteGrid(Scene scene)
        {
            var vertices = new[]
            {
                -1f, 1f,
                -1f, -1f,
                1f, 1f,
                1f, -1f,
                1f, 1f,
                -1f, -1f,
            };

            shader = scene.GuiContext.ShaderLoader.LoadShader("vrf.grid");

            // Create VAO
            GL.CreateVertexArrays(1, out vao);
            GL.CreateBuffers(1, out int buffer);
            GL.NamedBufferData(buffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexArrayVertexBuffer(vao, 0, buffer, 0, sizeof(float) * 2);

            var attributeLocation = GL.GetAttribLocation(shader.Program, "aVertexPosition");
            GL.EnableVertexArrayAttrib(vao, attributeLocation);
            GL.VertexArrayAttribFormat(vao, attributeLocation, 2, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, attributeLocation, 0);

#if DEBUG
            var vaoLabel = nameof(InfiniteGrid);
            GL.ObjectLabel(ObjectLabelIdentifier.VertexArray, vao, vaoLabel.Length, vaoLabel);
#endif
        }

        public void Render()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(shader.Program);
            GL.BindVertexArray(vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.UseProgram(0);
            GL.BindVertexArray(0);

            GL.Disable(EnableCap.Blend);
        }
    }
}
