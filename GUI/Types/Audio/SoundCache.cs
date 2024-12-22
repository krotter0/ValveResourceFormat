using System.IO;
using NAudio.Wave;
using NLayer.NAudioSupport;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Utils;
using System.Buffers;

namespace GUI.Types.Audio;
internal class SoundCache
{
    private class LoadedSoundData
    {
        public int Uses { get; set; }
        public byte[] SoundData { get; set; }
        public bool Loop { get; set; }
        public LoadedSoundData(byte[] soundData, bool loop)
        {
            SoundData = soundData;
            Loop = loop;
        }
    }

    public IFileLoader FileLoader { get; init; }
    private Dictionary<int, LoadedSoundData> loadedSounds = new();
    public WaveFormat WaveFormat { get; init; }

    public SoundCache(IFileLoader fileLoader, WaveFormat waveFormat)
    {
        FileLoader = fileLoader;
        WaveFormat = waveFormat;
    }

    private static int GetSoundHash(string fileName)
    {
        return fileName.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
    }

    public void ReleaseSound(SoundHandle soundHandle)
    {
        if (loadedSounds.TryGetValue(soundHandle.SoundHash, out var loadedSound))
        {
            loadedSound.Uses--;
            if (loadedSound.Uses <= 0)
            {
                loadedSounds.Remove(soundHandle.SoundHash);
            }
        }
    }

    public SoundHandle GetSound(string fileName)
    {
        var hash = GetSoundHash(fileName);
        if (!loadedSounds.TryGetValue(hash, out var loadedSound))
        {
            var resource = FileLoader.LoadFileCompiled(fileName);
            if (resource.DataBlock is not Sound sound)
            {
                throw new FormatException("File is not a sound file");
            }

            var resampledSound = Resample(sound);

            loadedSound = new LoadedSoundData(resampledSound, sound.LoopStart >= 0);
            loadedSounds[hash] = loadedSound;
        }

        return new SoundHandle(hash, loadedSound.SoundData, this, loadedSound.Loop);
    }

    private byte[] Resample(Sound sound)
    {
        var stream = sound.GetSoundStream();

        using WaveStream waveStream = sound.SoundType switch
        {
            Sound.AudioFileType.WAV => new WaveFileReader(stream),
            Sound.AudioFileType.MP3 => new Mp3FileReaderBase(stream, wf => new Mp3FrameDecompressor(wf)),
            Sound.AudioFileType.AAC => new StreamMediaFoundationReader(stream),
            _ => throw new UnexpectedMagicException("Dont know how to play", (int)sound.SoundType, nameof(sound.SoundType)),
        };

        using var resampler = new MediaFoundationResampler(waveStream, WaveFormat);

        var expectedCapacity = (int)Math.Ceiling(sound.Duration * WaveFormat.SampleRate);
        using var memoryStream = new MemoryStream();

        var buffer = ArrayPool<byte>.Shared.Rent(512);
        try
        {
            var totalReads = 0;
            int readCount;
            do
            {
                readCount = resampler.Read(buffer, 0, buffer.Length);
                memoryStream.Write(buffer, 0, readCount);
                totalReads += readCount;
            } while (readCount == buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return memoryStream.ToArray();
    }
}
