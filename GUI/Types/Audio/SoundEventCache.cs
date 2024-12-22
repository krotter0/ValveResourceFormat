using System.Linq;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Serialization.KeyValues;
using ValveResourceFormat.Utils;

namespace GUI.Types.Audio;
class SoundEventCache
{
    private IFileLoader fileLoader;
    private Dictionary<string, KVObject> soundEvents = new Dictionary<string, KVObject>();

    public SoundEventCache(IFileLoader fileLoader)
    {
        this.fileLoader = fileLoader;
    }

    public KVObject GetSoundEvent(string soundEventName)
    {
        return soundEvents[soundEventName];
    }

    public void AddSoundEventsFromManifest(string fileName)
    {
        var file = fileLoader.LoadFileCompiled(fileName);
        if (file.DataBlock is not ResourceManifest manifest)
        {
            throw new FormatException("File is not a manifest file");
        }

        var resources = manifest.Resources.SelectMany(x => x);
        foreach (var resource in resources)
        {
            AddSoundEvents(resource);
        }
    }

    public void AddSoundEvents(string fileName)
    {
        var file = fileLoader.LoadFileCompiled(fileName);
        if (file.DataBlock is not BinaryKV3 kv3Block)
        {
            throw new FormatException("File is not a kv3 file");
        }

        AddSoundEvents(kv3Block.Data);
    }

    public void AddSoundEvents(KVObject soundEventsFile)
    {
        foreach (var item in soundEventsFile)
        {
            soundEvents[item.Key] = (KVObject)item.Value;
        }
    }

    public ISoundEvent CreateSoundEvent(SoundContext soundContext, SoundCache soundCache, string soundEventName)
    {
        var soundEventData = soundEvents[soundEventName];
        var soundType = soundEventData.GetStringProperty("type");
        return soundType switch
        {
            "csgo_mega" => CSGOMegaSoundEvent.Create(soundContext, soundEventData, soundCache, this),
            "csgo_music" => CSGOMegaSoundEvent.Create(soundContext, soundEventData, soundCache, this),
            _ => throw new UnexpectedMagicException("Unexpected soundevent type", soundType, nameof(soundType))
        };
    }
}
