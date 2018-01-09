using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    static class Helpers
    {
        /// <summary>
        /// Convert via the following popular formula:
        /// m = 2595 * Log10(1 + f/700)
        /// </summary>
        /// <param name="hertz"></param>
        /// <returns></returns>
        public static float HertzToMel(float hertz)
        {
            return 2595 * Mathf.Log10(1 + hertz / 700);
        }
    }
}
