using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    public class DecibelControl : ISoundControl
    {
        OffsetsProfile decibelOffsets;

        public DecibelControl(OffsetsProfile offsets)
        {
            decibelOffsets = offsets;
        }

        public bool IsInputValid(float soundInput)
        {
            // Below range, ignore
            return soundInput > decibelOffsets.Min;
        }

        public float SoundForce(float soundInput)
        {
            // Logarithmic distance
            // Make sure that if is too silent (under a threshold), consider as no input of ANY data

            // Below range, return 0
            if (!IsInputValid(soundInput))
            {
                return 0;
            }

            // Above range, this is an active input, set max
            if (soundInput > decibelOffsets.Max)
            {
                return 1;
            }

            // Linear distance
            // Get actual margin value according to thershold, and distance between min and max
            float actualThreshold = decibelOffsets.BaselineThreshold * (decibelOffsets.Max - decibelOffsets.Min);

            // If threshold was not passed, return 0
            if (Mathf.Abs(soundInput - decibelOffsets.Baseline) < actualThreshold)
            {
                return 0;
            }

            // Return change in range of [-1,1]
            if (soundInput - decibelOffsets.Baseline < 0)
            {
                // Lower than baseline, value is between [-1,0]
                return (soundInput - decibelOffsets.Baseline) /
                       Mathf.Abs(decibelOffsets.Baseline - decibelOffsets.Min);
            }
            else
            {
                // Higher than baseline, value is between [0,1]
                return (soundInput - decibelOffsets.Baseline) /
                       Mathf.Abs(decibelOffsets.Max - decibelOffsets.Baseline);
            }
        }
    }
}
