using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    public class PitchControl : ISoundControl
    {
        OffsetsProfile PitchOffsets;

        public PitchControl(OffsetsProfile offsets)
        {
            PitchOffsets = offsets;
        }

        public const float NoData = 0;
        // TODO find out why getting NaN sometimes!!
        public float SoundForce(float hertzSoundInput)
        {
            float melInput = Helpers.HertzToMel(hertzSoundInput);

            // Below range
            if (melInput < PitchOffsets.MelMin)
            {
                return -1;
            }

            // Above range
            if (melInput > PitchOffsets.MelMax)
            {
                return 1;
            }

            // Linear distance
            // Get actual margin value according to thershold, and distance between min and max
            float actualThreshold = PitchOffsets.BaselineThreshold * (PitchOffsets.MelMax - PitchOffsets.MelMin);

            // If threshold was not passed, return 0
            if (Mathf.Abs(melInput - PitchOffsets.MelBaseline) < actualThreshold)
            {
                return 0;
            }

            // Return change in range of [-1,1]
            if (melInput - PitchOffsets.MelBaseline < 0)
            {
                // Lower than baseline, value is between [-1,0]
                var result = (melInput - PitchOffsets.MelBaseline) /
                       Mathf.Abs(PitchOffsets.MelBaseline - PitchOffsets.MelMin);

                return (result == float.NaN) ? 0 : result;
            }
            else
            {
                // Higher than baseline, value is between [0,1]
                var result = (melInput - PitchOffsets.MelBaseline) /
                       Mathf.Abs(PitchOffsets.MelMax - PitchOffsets.MelBaseline);

                return (result == float.NaN) ? 0 : result;
            }
        }
    }
}
