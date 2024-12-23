using System.Linq;
using NAudio.Wave;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Serialization.KeyValues;

namespace GUI.Types.Audio;

internal class CSGOMegaSoundEvent : ISoundEvent
{
    public CSGOMegaSoundEvent(SoundContext soundData, WaveFormat waveFormat)
    {
        WaveFormat = waveFormat;
    }
    public SoundContext SoundContext { get; set; }

    private bool isMainSoundOver;

    public event ISoundEvent.SoundEventOverDelegate SoundEventOver;

    public bool IsOver => isMainSoundOver && !(ChildSoundEventMixer?.HasSounds() ?? false);

    public WaveFormat WaveFormat { get; init; }

    private SoundEventMixer ChildSoundEventMixer { get; set; }
    private SoundHandle SoundHandle { get; set; }
    private StereoVolumeSampleProvider PanningSampleProvider { get; set; }
    public KVObject SoundEventData { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {

        var readCount = 0;
        if (PanningSampleProvider != null)
        {
            UpdateFromSoundContext();
            readCount = PanningSampleProvider.Read(buffer, offset, count);
        }

        if (ChildSoundEventMixer != null)
        {
            readCount = Math.Max(ChildSoundEventMixer.Read(buffer, offset, count, false), readCount);
        }

        if (readCount == 0)
        {
            if (!isMainSoundOver)
            {
                SoundEventOver?.Invoke();
                isMainSoundOver = true;
            }
            SoundHandle?.Dispose();
            SoundHandle = null;
        }

        return readCount;
    }

    public void Stop()
    {
        ChildSoundEventMixer.Stop();
        SoundHandle.Looping = false;
    }

    public void StopImmediately()
    {
        ChildSoundEventMixer?.StopImmediately();
        ChildSoundEventMixer = null;

        PanningSampleProvider = null;

        SoundHandle?.Dispose();
        SoundHandle = null;
    }

    private void UpdateFromSoundContext()
    {
        var volume = 1f;
        var distanceVolumeCurve = SoundEventData.GetSubCollection("distance_volume_mapping_curve");
        if (distanceVolumeCurve != null)
        {
            var first = (double)((KVObject)distanceVolumeCurve.First().Value).First().Value;
            var last = (double)((KVObject)distanceVolumeCurve.Last().Value).First().Value;

            var distanceFromCamera = (SoundContext.GlobalData.CameraPosition - SoundContext.SoundData.Position).Length();
            volume = (float)Math.Clamp(1 - (distanceFromCamera - first) / (last - first), 0, 1);
        }
        PanningSampleProvider.SetFromPan(SoundEventCommon.GetPan(SoundContext), volume * SoundContext.SoundData.Volume);
    }

    public static CSGOMegaSoundEvent Create(SoundContext soundContext, KVObject soundEventData, SoundCache soundCache, SoundEventCache soundEventCache)
    {
        var soundEvent = new CSGOMegaSoundEvent(soundContext, soundCache.WaveFormat);

        var soundFilesEntry = soundEventData.Properties.GetValueOrDefault("vsnd_files_track_01");
        if (soundFilesEntry != null)
        {
            if (soundFilesEntry.Type == KVType.STRING)
            {
                var soundName = soundEventData.GetStringProperty("vsnd_files_track_01");
                soundEvent.SoundHandle = soundCache.GetSound(soundName);
            }
            else if (soundFilesEntry.Type == KVType.ARRAY)
            {
                var soundNames = soundEventData.GetArray<string>("vsnd_files_track_01");
                var soundNameIndex = Random.Shared.Next(0, soundNames.Length);
                var soundName = soundNames[soundNameIndex];
                soundEvent.SoundHandle = soundCache.GetSound(soundName);
            }
        }

        soundEvent.PanningSampleProvider = new StereoVolumeSampleProvider(soundEvent.SoundHandle.ToSampleProvider());
        soundEvent.SoundEventData = soundEventData;

        soundEvent.SoundContext = soundContext;

        return soundEvent;
    }
}
