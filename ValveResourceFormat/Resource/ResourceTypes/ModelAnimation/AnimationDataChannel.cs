using System;
using System.Collections.Generic;
using System.Linq;
using ValveResourceFormat.Serialization;

namespace ValveResourceFormat.ResourceTypes.ModelAnimation
{
    public class AnimationDataChannel
    {
        public bool IsMorph { get; }
        public int[] RemapTable { get; } // Bone/Morph ID => Element Index
        public string[] RemapNameTable { get; } // Bone/Morph ID => Name
        public string ChannelAttribute { get; }

        public AnimationDataChannel(Skeleton skeleton, IKeyValueCollection dataChannel, int channelElements)
        {
            var channelClass = dataChannel.GetProperty<string>("m_szChannelClass");

            var elementNameArray = dataChannel.GetArray<string>("m_szElementNameArray");
            var elementIndexArray = dataChannel.GetIntegerArray("m_nElementIndexArray");

            RemapNameTable = new string[elementNameArray.Length];
            ChannelAttribute = dataChannel.GetProperty<string>("m_szVariableName");

            if (channelClass == "MorphChannel")
            {
                IsMorph = true;
                RemapTable = new int[elementIndexArray.Length];

                for (var i = 0; i < elementIndexArray.Length; i++)
                {
                    var elementName = elementNameArray[i];
                    RemapTable[i] = (int)elementIndexArray[i];
                    RemapNameTable[i] = elementName;
                }
            }
            else
            {
                IsMorph = false;
                RemapTable = Enumerable.Range(0, skeleton.Bones.Length).Select(_ => -1).ToArray();

                for (var i = 0; i < elementIndexArray.Length; i++)
                {
                    var elementName = elementNameArray[i];
                    var boneID = Array.FindIndex(skeleton.Bones, bone => bone.Name == elementName);
                    if (boneID != -1)
                    {
                        RemapTable[boneID] = (int)elementIndexArray[i];
                    }

                    RemapNameTable[i] = elementName;
                }
            }
        }
    }
}
