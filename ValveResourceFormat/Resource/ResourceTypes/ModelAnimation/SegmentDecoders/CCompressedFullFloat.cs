using System;
using System.Linq;

namespace ValveResourceFormat.ResourceTypes.ModelAnimation.SegmentDecoders
{
    public class CCompressedFullFloat : AnimationSegmentDecoder
    {
        private readonly float[] Data;

        public CCompressedFullFloat(ArraySegment<byte> data, int[] wantedElements, int[] remapTable, string[] remapNameTable,
            int elementCount, AnimationChannelAttribute channelAttribute) : base(remapTable, remapNameTable, channelAttribute)
        {
            const int elementSize = 4;
            var stride = elementCount * elementSize;
            Data = Enumerable.Range(0, data.Count / stride)
                .SelectMany(i => wantedElements.Select(j =>
                {
                    return BitConverter.ToSingle(data.Slice(i * stride + j * elementSize));
                }).ToArray())
                .ToArray();
        }

        public override void Read(int frameIndex, Frame outFrame)
        {
            var offset = frameIndex * RemapTable.Length;

            if (ChannelAttribute == AnimationChannelAttribute.Data)
            {
                for (var i = 0; i < RemapNameTable.Length; i++)
                {
                    outFrame.SetMorph(RemapNameTable[i], Data[offset + i]);
                }
            }
            else
            {
                for (var i = 0; i < RemapTable.Length; i++)
                {
                    outFrame.SetAttribute(RemapTable[i], ChannelAttribute, Data[offset + i]);
                }
            }
        }
    }
}
