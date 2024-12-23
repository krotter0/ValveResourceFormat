using System.Buffers;
using NAudio.Wave;

namespace GUI.Types.Audio;
internal class SoundEventMixer : ISampleProvider
{
    public WaveFormat WaveFormat { get; init; }

    public SoundEventMixer(WaveFormat waveFormat)
    {
        WaveFormat = waveFormat;
    }

    private LinkedList<ISoundEvent> SoundEvents { get; init; } = new();

    public void AddSound(ISoundEvent sound)
    {
        lock (SoundEvents)
        {
            SoundEvents.AddLast(sound);
        }
    }

    public void Stop()
    {
        foreach (var item in SoundEvents)
        {
            item.Stop();
        }
    }

    public void StopImmediately()
    {
        foreach (var item in SoundEvents)
        {
            item.StopImmediately();
        }
    }

    public bool HasSounds()
    {
        return SoundEvents.Count > 0;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        return Read(buffer, offset, count, true);
    }

    public int Read(float[] buffer, int offset, int count, bool clearBuffer)
    {
        var removedSounds = new List<ISoundEvent>(SoundEvents.Count);

        if (clearBuffer)
        {
            for (var i = 0; i < count; i++)
            {
                //TODO: is this needed?
                buffer[offset + i] = 0;
            }
        }

        var readBuffer = ArrayPool<float>.Shared.Rent(buffer.Length);
        try
        {
            lock (SoundEvents)
            {
                foreach (var sound in SoundEvents)
                {
                    var readCount = sound.Read(readBuffer, offset, count);
                    for (var i = 0; i < readCount; i++)
                    {
                        buffer[offset + i] += readBuffer[offset + i];
                    }

                    if (sound.IsOver)
                    {
                        removedSounds.Add(sound);
                    }
                }
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(readBuffer);
        }

        //TODO: this could be more optimal
        lock (SoundEvents)
        {
            foreach (var removedSound in removedSounds)
            {
                SoundEvents.Remove(removedSound);
            }
        }

        return count;
    }
}
