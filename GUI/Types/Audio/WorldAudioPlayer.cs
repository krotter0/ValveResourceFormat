using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization.KeyValues;

namespace GUI.Types.Audio;
internal class WorldAudioPlayer : IDisposable
{
    private SoundCache soundCache;
    private SoundEventCache soundEventCache;
    private SoundEventMixer[] mixers;
    private IWaveProvider mixingWaveProvider;

    private WaveOutEvent waveOut;

    public WaveFormat WaveFormat { get; init; }

    public SoundGlobalData SoundGlobalData { get; init; } = new();

    public WorldAudioPlayer(WaveFormat waveFormat, IFileLoader fileLoader)
    {
        WaveFormat = waveFormat;
        soundCache = new SoundCache(fileLoader, waveFormat);
        soundEventCache = new SoundEventCache(fileLoader);
    }

    public WorldAudioPlayer(IFileLoader fileLoader) : this(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2), fileLoader) { }

    public void Init()
    {
        var worldAudioMixer = new SoundEventMixer(WaveFormat);
        mixers = [worldAudioMixer];

        var mixingSampleProvider = new MixingSampleProvider(mixers);
        mixingWaveProvider = mixingSampleProvider.ToWaveProvider();

        waveOut = new WaveOutEvent();
        waveOut.Init(mixingWaveProvider);
    }

    public void Play()
    {
        waveOut.Play();
    }

    public ISoundEvent PlaySound(int mixerIndex, SoundData soundData, string soundEventName)
    {
        var soundContext = new SoundContext(soundData, SoundGlobalData);

        var soundEvent = soundEventCache.CreateSoundEvent(soundContext, soundCache, soundEventName);

        var mixer = mixers[mixerIndex];
        mixer.AddSound(soundEvent);

        return soundEvent;
    }

    public void LoadManifest(string soundEventManifest)
    {
        soundEventCache.LoadSoundEventsFromManifest(soundEventManifest);
    }

    public void LoadSoundEventsFile(KVObject soundEvents)
    {
        soundEventCache.LoadSoundEvents(soundEvents);
    }

    public void Dispose()
    {
        waveOut.Dispose();
    }
}
