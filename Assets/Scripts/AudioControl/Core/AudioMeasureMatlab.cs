using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.Core
{
    public class AudioMeasureMatlab : MonoBehaviour
    {
        public float DbValue { get; private set; }
        public float PitchValue { get; private set; }
        public float MelValue { get; private set; }

        private void Start()
        {
        }

        private void Measure(float[,] samples, int sampleRate)
        {

        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // SHRP - a pitch determination algorithm based on Subharmonic-to-Harmonic Ratio(SHR)
        // [f0_time, f0_value, SHR, f0_candidates]=shrp(Y, Fs[, F0MinMax, frame_length, TimeStep, SHR_Threshold, Ceiling, med_smooth, CHECK_VOICING])
        //
        //   Input parameters(There are 9):
        //
        //       Y:              Input data 
        //       Fs:             Sampling frequency(e.g., 16000 Hz)
        //       F0MinMax:       2-d array specifies the F0 range. [minf0 maxf0], default: [50 550]
        //                       Quick solutions:
        //                       For male speech: [50 250]
        //                       For female speech: [120 400]
        //       frame_length:   length of each frame in millisecond (default: 40 ms)
        //       TimeStep:       Interval for updating short-term analysis in millisecond(default: 10 ms)
        //       SHR_Threshold:  Subharmonic-to-harmonic ratio threshold in the range of[0, 1] (default: 0.4). 
        //                       If the estimated SHR is greater than the threshold, the subharmonic is regarded as F0 candidate,
        //                       Otherwise, the harmonic is favored.
        //       Ceiling:        Upper bound of the frequencies that are used for estimating pitch. (default: 1250 Hz)
        //       med_smooth:     the order of the median smoothing(default: 0 - no smoothing);                       
        //       CHECK_VOICING:  check voicing.Current voicing determination algorithm is kind of crude.
        //                       0: no voicing checking (default)
        //                       1: voicing checking
        //   Output parameters:
        //       
        //       f0_time:        an array stores the times for the F0 points
        //       f0_value:       an array stores F0 values
        //       SHR:            an array stores subharmonic-to-harmonic ratio for each frame
        //		f0_candidates:  a matrix stores the f0 candidates for each frames, currently two f0 values generated for each frame.
        //						Each row (a frame) contains two values in increasing order, i.e., [low_f0 higher_f0].
        //						For SHR = 0, the first f0 is 0. The purpose of this is that when you want to test different SHR
        //						thresholds, you don't need to re-run the whole algorithm. You can choose to select the lower or higher
        //						value based on the shr value of this frame.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private ShrpOut Shrp()
        {
            ShrpOut output = new ShrpOut();

            return output;
        }

        private struct ShrpOut
        {
            float[,] f0_time;
            float[,] f0_value;
            //float[,] SHR;
            // Matrix?? f0_candidates;
        }

        // y = medsmooth(x, L) Median smoothing filter.
        // the order of the filter has very different effects on the output. even order tends to have
        // interpolation effect, which is similar to smoothing. I guess I should use even order more often for pitch contours.
        // for image processing, I guess using odd order is better cause it can preserve the sharpness
        //private ? MedSmooth(Vector4 x,)
        //{

        //}

        //public enum AudioType
        //{
        //    HZ,
        //    ST
        //}

        ////////////////////////////////////////////////////////////////////////////////////////////
        //// [y, num_corrected]=destepfilter(x, f0_range, hz_or_st)
        //// de-step filter: see Paul Bagshaw's Ph.D. thesis
        ////   Input parameters :
        ////
        ////       x:              Input f0 
        ////       f0_range:       a vector[low_f0 high_f0]
        ////       hz_or_st:  		'hz': the unit of f0 is Hz; 'st': the unit of f0 is semitone
        ////
        ////   Output parameters:
        ////       
        ////       y:        		filtered f0
        ////       num_corrected:  number of f0 points that have been corrected
        ////
        ////////////////////////////////////////////////////////////////////////////////////////////
        //private DEStepFiltered Filter(Vector4 x, Vector2 f0Range, AudioType audioType)
        //{
        //    DEStepFiltered result = new DEStepFiltered();

        //    return result;
        //}

        //public struct DEStepFiltered
        //{
        //    float FilteredF0;
        //    int NumCorrected;
        //}
    }
}
