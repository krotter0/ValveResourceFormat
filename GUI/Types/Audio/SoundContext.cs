namespace GUI.Types.Audio;
internal class SoundContext
{
    public SoundData SoundData { get; set; }
    public SoundGlobalData GlobalData { get; init; }

    public SoundContext(SoundData soundData, SoundGlobalData globalData)
    {
        GlobalData = globalData;
        SoundData = soundData;
    }
}
