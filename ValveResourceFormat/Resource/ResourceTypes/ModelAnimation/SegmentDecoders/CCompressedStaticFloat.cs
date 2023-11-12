using System;
using System.Linq;

namespace ValveResourceFormat.ResourceTypes.ModelAnimation.SegmentDecoders
{
    public class CCompressedStaticFloat : AnimationSegmentDecoder
    {
        private readonly float[] Data;

        public CCompressedStaticFloat(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, string[] remapNameTable,
            AnimationChannelAttribute channelAttribute) : base(remapTable, remapNameTable, channelAttribute)
        {
            Data = wantedElements.Select(i =>
            {
                return BitConverter.ToSingle(data.Slice(i * 4));
            }).ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            if (ChannelAttribute == AnimationChannelAttribute.Data)
            {
                for (var i = 0; i < RemapNameTable.Length; i++)
                {
                    outFrame.SetMorph(RemapNameTable[i], Data[i]);
                }
            }
            else
            {
                for (var i = 0; i < RemapTable.Length; i++)
                {
                    outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[i]);
                }
            }
        }
    }
}
