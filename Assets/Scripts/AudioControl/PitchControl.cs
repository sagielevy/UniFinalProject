using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    public class PitchControl : ISoundControl
    {
        private const float DefaultPitchRange = 100f;
        public const float NoData = 0;

        OffsetsProfile PitchOffsets;

        public PitchControl(OffsetsProfile offsets)
        {
            PitchOffsets = offsets;
        }

        public float SoundForce(float hertzSoundInput)
        {
            // Ignore input
            if (float.IsNaN(PitchOffsets.Baseline))
            {
                return 0;
            }

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
            float actualThreshold = PitchOffsets.BaselineThreshold * CalcRange();

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

                return float.IsNaN(result) ? 0 : result;
            }
            else
            {
                // Higher than baseline, value is between [0,1]
                var result = (melInput - PitchOffsets.MelBaseline) /
                       Mathf.Abs(PitchOffsets.MelMax - PitchOffsets.MelBaseline);

                return float.IsNaN(result) ? 0 : result;
            }
        }

        private float CalcRange()
        {
            var range = PitchOffsets.MelMax - PitchOffsets.MelMin;
            return float.IsInfinity(range) ? DefaultPitchRange : range;
        }
    }
}
