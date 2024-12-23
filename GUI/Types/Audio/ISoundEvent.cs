using NAudio.Wave;
using ValveResourceFormat.Serialization.KeyValues;

namespace GUI.Types.Audio;
internal interface ISoundEvent : ISampleProvider
{
    public delegate void SoundEventOverDelegate();
    public event SoundEventOverDelegate SoundEventOver;

    public KVObject SoundEventData { get; set; }
    public SoundContext SoundContext { get; set; }
    public bool IsOver { get; }
    /// <summary>
    /// Called when stop is triggered for a soundevent. This should start handling e.g. fading out the sound and setting IsOver to true when finished.
    /// </summary>
    public void Stop();
    public void StopImmediately();
}
