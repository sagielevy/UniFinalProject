using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    public class DecibelControl : ISoundControl
    {
        private const float DefaultdBRange = 10f;
        OffsetsProfile DecibelOffsets;

        public DecibelControl(OffsetsProfile offsets)
        {
            DecibelOffsets = offsets;
        }

        public bool IsInputValid(float soundInput)
        {
            // Below range, ignore
            return soundInput > DecibelOffsets.Min;
        }

        public float SoundForce(float soundInput)
        {
            // Logarithmic distance
            // Make sure that if is too silent (under a threshold), consider as no input of ANY data

            // Below range or baseline is NaN, return 0
            if (float.IsNaN(DecibelOffsets.Baseline) || !IsInputValid(soundInput))
            {
                return 0;
            }

            // Above range, this is an active input, set max
            if (soundInput > DecibelOffsets.Max)
            {
                return 1;
            }

            // Linear distance
            // Get actual margin value according to thershold, and distance between min and max
            float actualThreshold = DecibelOffsets.BaselineThreshold * CalcRange();

            // If threshold was not passed, return 0
            if (Mathf.Abs(soundInput - DecibelOffsets.Baseline) < actualThreshold)
            {
                return 0;
            }

            // Return change in range of [-1,1]
            if (soundInput - DecibelOffsets.Baseline < 0)
            {
                // Lower than baseline, value is between [-1,0]
                var result = (soundInput - DecibelOffsets.Baseline) /
                       Mathf.Abs(DecibelOffsets.Baseline - DecibelOffsets.Min);

                return float.IsNaN(result) ? 0 : result;
            }
            else
            {
                // Higher than baseline, value is between [0,1]
                var result = (soundInput - DecibelOffsets.Baseline) /
                       Mathf.Abs(DecibelOffsets.Max - DecibelOffsets.Baseline);

                return float.IsNaN(result) ? 0 : result;
            }
        }

        private float CalcRange()
        {
            var range = DecibelOffsets.MelMax - DecibelOffsets.MelMin;
            return float.IsInfinity(range) ? DefaultdBRange : range;
        }
    }
}
