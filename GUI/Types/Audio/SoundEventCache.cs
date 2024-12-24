using System.Linq;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Serialization.KeyValues;
using ValveResourceFormat.Utils;

namespace GUI.Types.Audio;
class SoundEventCache
{
    private static Dictionary<string, Func<SoundContext, KVObject, SoundCache, SoundEventCache, ISoundEvent>> soundEventTypeConstructors = new()
    {
        { "csgo_mega", CSGOMegaSoundEvent.Create },
        { "csgo_music", CSGOMusicSoundEvent.Create },
    };

    private IFileLoader fileLoader;
    private Dictionary<string, KVObject> soundEvents = new Dictionary<string, KVObject>();

    public SoundEventCache(IFileLoader fileLoader)
    {
        this.fileLoader = fileLoader;
    }

    public KVObject GetSoundEvent(string soundEventName)
    {
        return soundEvents.GetValueOrDefault(soundEventName);
    }

    public void LoadSoundEventsFromManifest(string fileName)
    {
        var file = fileLoader.LoadFileCompiled(fileName);
        if (file.DataBlock is not ResourceManifest manifest)
        {
            throw new FormatException("File is not a manifest file");
        }

        var resources = manifest.Resources.SelectMany(x => x);
        foreach (var resource in resources)
        {
            LoadSoundEvents(resource);
        }
    }

    public void LoadSoundEvents(string fileName)
    {
        var file = fileLoader.LoadFileCompiled(fileName);
        if (file.DataBlock is not BinaryKV3 kv3Block)
        {
            throw new FormatException("File is not a kv3 file");
        }

        LoadSoundEvents(kv3Block.Data);
    }

    public void LoadSoundEvents(KVObject soundEventsFile)
    {
        foreach (var item in soundEventsFile)
        {
            soundEvents[item.Key] = (KVObject)item.Value;
        }

        foreach (var item in soundEvents)
        {
            InheritSoundEventBase(item.Value);
        }
    }

    private void InheritSoundEventBase(KVObject soundEvent)
    {
        var baseSoundEventName = soundEvent.GetStringProperty("base");
        if (baseSoundEventName == null || !soundEvents.TryGetValue(baseSoundEventName, out var baseSoundEvent))
        {
            return;
        }

        soundEvent.Properties.Remove("base");
        InheritSoundEventBase(baseSoundEvent);

        foreach (var baseProperty in baseSoundEvent.Properties)
        {
            if (!soundEvent.Properties.ContainsKey(baseProperty.Key))
            {
                soundEvent.AddProperty(baseProperty.Key, baseProperty.Value);
            }
        }
    }

    public ISoundEvent CreateSoundEvent(SoundContext soundContext, SoundCache soundCache, string soundEventName)
    {
        var soundEventData = soundEvents[soundEventName];
        var soundType = soundEventData.GetStringProperty("type");

        if (!soundEventTypeConstructors.TryGetValue(soundType, out var soundEventConstructor))
        {
            throw new UnexpectedMagicException("Unexpected soundevent type", soundType, nameof(soundType));
        }

        return soundEventConstructor(soundContext, soundEventData, soundCache, this);
    }

    public static bool IsSoundEventTypeValid(string soundEventType)
    {
        return soundEventTypeConstructors.ContainsKey(soundEventType);
    }

    public static bool IsSoundEventTypeValid(KVObject soundEventData)
    {
        var type = soundEventData.GetStringProperty("type");
        return soundEventTypeConstructors.ContainsKey(type);
    }
}