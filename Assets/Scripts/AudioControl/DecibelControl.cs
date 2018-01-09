using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AudioControl
{
    public class DecibelControl : ISoundControl
    {
        Offsets decibelOffsets;

        public DecibelControl(Offsets offsets)
        {
            decibelOffsets = offsets;
        }

        public float SoundForce(float soundInput)
        {
            // Logarithmic distance
            throw new NotImplementedException();
        }
    }
}
