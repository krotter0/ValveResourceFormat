using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace GUI.Types.Audio;
internal class SoundscapeController : ISampleProvider
{
    private ISoundEvent currentSoundscape;
    private ISoundEvent oldSoundscape;

    private float currentFade = 0;

    public WaveFormat WaveFormat => throw new NotImplementedException();

    public int Read(float[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
