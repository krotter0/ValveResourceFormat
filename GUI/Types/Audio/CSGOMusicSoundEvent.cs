using System.Linq;
using NAudio.Wave;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Serialization.KeyValues;

namespace GUI.Types.Audio;

internal class CSGOMusicSoundEvent : ISoundEvent
{
    public CSGOMusicSoundEvent(SoundContext soundData, WaveFormat waveFormat)
    {
        WaveFormat = waveFormat;
    }
    public SoundContext SoundContext { get; set; }

    public event ISoundEvent.SoundEventOverDelegate SoundEventOver;

    public bool IsOver { get; private set; }

    public WaveFormat WaveFormat { get; init; }

    private SoundHandle SoundHandle { get; set; }
    private ISampleProvider SampleProvider { get; set; }
    public KVObject SoundEventData { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {
        var readCount = SampleProvider?.Read(buffer, offset, count) ?? 0;

        if (readCount == 0)
        {
            if (!IsOver)
            {
                SoundEventOver?.Invoke();
                IsOver = true;
            }
            SoundHandle?.Dispose();
            SoundHandle = null;
        }

        return readCount;
    }

    public void Stop()
    {
        SoundHandle.Looping = false;
    }

    public void StopImmediately()
    {
        SampleProvider = null;
        SoundHandle?.Dispose();
        SoundHandle = null;
    }

    public static CSGOMusicSoundEvent Create(SoundContext soundContext, KVObject soundEventData, SoundCache soundCache, SoundEventCache soundEventCache)
    {
        var soundEvent = new CSGOMusicSoundEvent(soundContext, soundCache.WaveFormat);

        var soundFilesEntry = soundEventData.Properties.GetValueOrDefault("vsnd_files");
        if (soundFilesEntry != null)
        {
            if (soundFilesEntry.Type == KVType.STRING)
            {
                var soundName = soundEventData.GetStringProperty("vsnd_files");
                soundEvent.SoundHandle = soundCache.GetSound(soundName);
            }
            else if (soundFilesEntry.Type == KVType.ARRAY)
            {
                var soundNames = soundEventData.GetArray<string>("vsnd_files");
                var soundNameIndex = Random.Shared.Next(0, soundNames.Length);
                var soundName = soundNames[soundNameIndex];
                soundEvent.SoundHandle = soundCache.GetSound(soundName);
            }
        }

        soundEvent.SampleProvider = soundEvent.SoundHandle.ToSampleProvider();
        soundEvent.SoundEventData = soundEventData;

        soundEvent.SoundContext = soundContext;

        return soundEvent;
    }
}
