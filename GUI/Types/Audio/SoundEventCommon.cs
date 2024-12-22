using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Types.Audio;
internal static class SoundEventCommon
{
    public static float GetPan(SoundContext soundContext)
    {
        var cameraToSound = Vector3.Normalize(soundContext.SoundData.Position - soundContext.GlobalData.CameraPosition);
        var panDot = Vector3.Dot(cameraToSound, soundContext.GlobalData.CameraRight);
        return panDot;
    }
}
