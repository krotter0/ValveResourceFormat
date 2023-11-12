namespace ValveResourceFormat.ResourceTypes.ModelAnimation
{
    public abstract class AnimationSegmentDecoder
    {
        public int[] RemapTable { get; }
        public string[] RemapNameTable { get; }
        public AnimationChannelAttribute ChannelAttribute { get; }

        protected AnimationSegmentDecoder(int[] remapTable, string[] remapNameTable, AnimationChannelAttribute channelAttribute)
        {
            RemapTable = remapTable;
            ChannelAttribute = channelAttribute;
            RemapNameTable = remapNameTable;
        }

        public abstract void Read(int frameIndex, Frame outFrame);
    }
}
