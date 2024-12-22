using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GUI.Types.Audio;
internal class SoundHandle : IDisposable, IWaveProvider
{
    public int SoundHash { get; init; }
    public byte[] SoundData { get; init; }

    public WaveFormat WaveFormat => soundCache.WaveFormat;
    private int position;
    public bool Looping;

    private SoundCache soundCache;

    internal SoundHandle(int soundHash, byte[] soundData, SoundCache source, bool loop)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        SoundHash = soundHash;
        SoundData = soundData;
        soundCache = source;
        Looping = loop;
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(soundCache == null, this);

        soundCache.ReleaseSound(this);
        soundCache = null;
    }

    //TODO: Loop start and end points
    public int Read(byte[] buffer, int offset, int count)
    {
        var maxRead = Math.Min(SoundData.Length - position, count);

        for (var i = 0; i < maxRead; i++)
        {
            buffer[i + offset] = SoundData[i + position];
        }
        position += maxRead;

        if (Looping && maxRead != count)
        {
            var remainingRead = count - maxRead;
            for (var i = 0; i < remainingRead; i++)
            {
                buffer[i + maxRead + offset] = SoundData[i];
            }
            position = remainingRead;

            return count;
        }
        else
        {
            return maxRead;
        }
    }
}
