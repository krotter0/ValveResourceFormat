using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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

    private bool isMainSoundOver = false;
    public bool IsOver => isMainSoundOver && !(ChildSoundEventMixer?.HasSounds() ?? false);

    public WaveFormat WaveFormat { get; init; }

    private SoundEventMixer ChildSoundEventMixer { get; set; }
    private SoundHandle SoundHandle { get; set; }
    private StereoVolumeSampleProvider PanningSampleProvider { get; set; }

    public int Read(float[] buffer, int offset, int count)
    {
        UpdateFromSoundContext();

        var readCount = PanningSampleProvider.Read(buffer, offset, count);
        if (ChildSoundEventMixer != null)
        {
            readCount = Math.Max(ChildSoundEventMixer.Read(buffer, offset, count, false), readCount);
        }

        if (readCount == 0)
        {
            isMainSoundOver = true;
        }

        return readCount;
    }

    public void Stop()
    {
        ChildSoundEventMixer.Stop();
        SoundHandle.Looping = false;
    }

    private void UpdateFromSoundContext()
    {
        PanningSampleProvider.SetFromPan(SoundEventCommon.GetPan(SoundContext), 1f);
    }

    public static CSGOMegaSoundEvent Create(SoundContext soundContext, KVObject soundEventData, SoundCache soundCache, SoundEventCache soundEventCache)
    {
        var soundEvent = new CSGOMegaSoundEvent(soundContext, soundCache.WaveFormat);

        if (soundEventData.GetStringProperty("type") == "csgo_music")
        {
            //TODO: Remove music from mega soundevent
            var soundName = soundEventData.GetStringProperty("vsnd_files");
            soundEvent.SoundHandle = soundCache.GetSound(soundName);
        }
        else
        {
            var soundName = soundEventData.GetSubCollection("vsnd_files_track_01").First().Value.ToString();
            soundEvent.SoundHandle = soundCache.GetSound(soundName);
        }

        soundEvent.PanningSampleProvider = new StereoVolumeSampleProvider(soundEvent.SoundHandle.ToSampleProvider().ToStereo());

        soundEvent.SoundContext = soundContext;

        return soundEvent;
    }
}
