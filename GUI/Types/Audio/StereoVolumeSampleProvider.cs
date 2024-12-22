using NAudio.Wave;

namespace GUI.Types.Audio;
internal class StereoVolumeSampleProvider : ISampleProvider
{
    private ISampleProvider _sampleProvider;
    public WaveFormat WaveFormat => _sampleProvider.WaveFormat;

    private float _lastLeftVolume = 1f;
    private float _lastRightVolume = 1f;
    public float LeftVolume = 1f;
    public float RightVolume = 1f;

    public void SetFromPan(float pan, float volume, bool immediately = false)
    {
        if (pan > 0)
        {
            RightVolume = volume;
            LeftVolume = (1f - pan) * volume;
        }
        else
        {
            LeftVolume = volume;
            RightVolume = (1f + pan) * volume;
        }

        if (immediately)
        {
            _lastLeftVolume = LeftVolume;
            _lastRightVolume = RightVolume;
        }
    }

    public StereoVolumeSampleProvider(ISampleProvider sampleProvider)
    {
        _sampleProvider = sampleProvider;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var readCount = _sampleProvider.Read(buffer, offset, count);
        for (var i = 0; i < readCount; i += 2)
        {
            float sampleT = (float)i / count;
            buffer[i + offset] *= float.Lerp(_lastLeftVolume, LeftVolume, sampleT);
            buffer[i + offset + 1] *= float.Lerp(_lastRightVolume, RightVolume, sampleT);
        }
        _lastLeftVolume = LeftVolume;
        _lastRightVolume = RightVolume;
        return readCount;
    }
}
