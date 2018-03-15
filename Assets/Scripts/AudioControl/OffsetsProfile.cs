using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    [Serializable]
    public class OffsetsProfile
    {
        public float Baseline { get; private set; }
        public float Max { get; private set; }
        public float Min { get; private set; }
        public float MelBaseline { get; private set; }
        public float MelMax { get; private set; }
        public float MelMin { get; private set; }

        // Between 0-1. Works as percentage. If input is beyond that in percentage, makes change.
        public float BaselineThreshold { get; private set; }
        //public float MaxMargin { get; }

        // Assumes inputs are valid. That is, 
        // max > baseline > min and 0 < baselineThreshold < 1
        public OffsetsProfile(float baseline, float max, float min, float baselineThreshold)
        {
            Baseline = baseline;
            Max = max;
            Min = min;
            BaselineThreshold = Mathf.Clamp01(baselineThreshold);
            MelBaseline = Helpers.HertzToMel(Baseline);
            MelMax = Helpers.HertzToMel(Max);
            MelMin = Helpers.HertzToMel(Min);
        }
    }
}
