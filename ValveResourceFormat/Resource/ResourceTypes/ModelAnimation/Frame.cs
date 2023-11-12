using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace ValveResourceFormat.ResourceTypes.ModelAnimation
{
    public class Frame
    {
        public int FrameIndex { get; set; } = 1;
        public FrameBone[] Bones { get; }
        private Dictionary<string, float> morphs = new Dictionary<string, float>();

        public Frame(Skeleton skeleton)
        {
            Bones = new FrameBone[skeleton.Bones.Length];
            morphs = new Dictionary<string, float>();
            Clear(skeleton);
        }

        public void SetAttribute(int bone, AnimationChannelAttribute attribute, Vector3 data)
        {
            switch (attribute)
            {
                case AnimationChannelAttribute.Position:
                    Bones[bone].Position = data;
                    break;

#if DEBUG
                default:
                    Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with Vector3 data");
                    break;
#endif
            }
        }

        public void SetAttribute(int bone, AnimationChannelAttribute attribute, Quaternion data)
        {
            switch (attribute)
            {
                case AnimationChannelAttribute.Angle:
                    Bones[bone].Angle = data;
                    break;

#if DEBUG
                default:
                    Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with Quaternion data");
                    break;
#endif
            }
        }

        public void SetAttribute(int bone, AnimationChannelAttribute attribute, float data)
        {
            switch (attribute)
            {
                case AnimationChannelAttribute.Scale:
                    Bones[bone].Scale = data;
                    break;

#if DEBUG
                default:
                    Console.WriteLine($"Unknown frame attribute '{attribute}' encountered with float data");
                    break;
#endif
            }
        }

        public void SetMorph(string morphName, float value)
        {
            morphs[morphName] = value;
        }

        public float GetMorph(string morphName)
        {
            if (morphs.ContainsKey(morphName))
            {
                return morphs[morphName];
            }
            else
            {
                return 0f;
            }
        }

        public string[] GetMorphNamesSet()
        {
            return morphs.Keys.ToArray();
        }

        /// <summary>
        /// Resets frame bones to their bind pose.
        /// Should be used on animation change.
        /// </summary>
        /// <param name="skeleton">The same skeleton that was passed to the constructor.</param>
        public void Clear(Skeleton skeleton)
        {
            FrameIndex = -1;

            for (var i = 0; i < Bones.Length; i++)
            {
                Bones[i].Position = skeleton.Bones[i].Position;
                Bones[i].Angle = skeleton.Bones[i].Angle;
                Bones[i].Scale = 1;
            }
        }
    }
}
