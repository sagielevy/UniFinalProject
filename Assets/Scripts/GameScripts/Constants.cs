using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameScripts
{
    public static class Constants
    {
        // This is assuming all other scenes are the first indices in the build order
        public const int FirstLevelSceneBuildIndex = 2;

        // TODO Verify these thresholds!! Should they be dynamic??
        public const float DefaultBaselineThreshold = 0.01f;
        public const float DefaultPitchBaselineThreshold = 0.03f;
        public const float DefaultVolumeBaselineThreshold = 0.10f;
    }
}
