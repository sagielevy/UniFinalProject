using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AudioControl
{
    public interface ISoundControl
    {
        /// <summary>
        /// Calculates value in range of [-1,1] where -1 represents input matching minimum sound value
        /// and 1 represents input matching maximum sound value.
        /// 0 represents no change which can occur due to one of two reasons:
        /// 1. Current input does not pass baseline threshold (is not beyond baseline -+ baseline threshold %).
        /// 2. Current input is higher than maximum value or lower than minimum value.
        /// </summary>
        /// <param name="soundInput">The sound input</param>
        /// <returns>Range value</returns>
        float SoundForce(float hertzSoundInput);
    }
}
