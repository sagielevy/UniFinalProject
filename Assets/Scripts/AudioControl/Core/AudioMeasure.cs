using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.Core
{
    /// <summary>
    /// Singleton class responsible for receiving and analyzing sound input from the player
    /// </summary>
    public class AudioMeasure : MonoBehaviour
    {
        public float RmsValue { get; private set; }
        public float DbValue { get; private set; }
        public float PitchValue { get; private set; }
        //public float MelValue { get; private set; }

        //private static AudioMeasure instance;
        private AudioSource Audio;
        private string audioInputDevice;
        private const int samplerate = 44100;
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;
        private const float Threshold = 0.002f;
        private const float minDbValue = -160;
        private const float acceptedChangeInPitch = 0.4f;
        private const int NUM_IMPORTANT_SAMPLES = 20;

        float[] _samples;
        private float[] _spectrum;

        private struct ValueAndIndex : IComparable<ValueAndIndex>
        {
            public readonly float value;
            public readonly int index;

            public ValueAndIndex(float value, int index)
            {
                this.value = value;
                this.index = index;
            }

            // Ascending order (first is smallest)
            public int CompareTo(ValueAndIndex other)
            {
                if (value < other.value)
                {
                    return 1;
                }
                else if (value > other.value)
                {
                    return -1;
                }

                return 0;
            }

            public override string ToString()
            {
                return "Value: " + value + ", index: " + index;
            }
        }

        private class IndexComparer : IComparer<ValueAndIndex>
        {
            public int Compare(ValueAndIndex x, ValueAndIndex y)
            {
                return x.index - y.index;
            }
        }

        //private void Awake()
        //{
        //    if (instance == null)
        //    {
        //        instance = this;
        //        DontDestroyOnLoad(gameObject);
        //    }
        //    else
        //    {
        //        // Only instance lives
        //        Destroy(this.gameObject);
        //    }
        //}

        void Start()
        {
            AudioSettings.Reset(new AudioConfiguration
            {
                sampleRate = samplerate
            });

            _samples = new float[QSamples];
            _spectrum = new float[QSamples];

            Audio = GetComponent<AudioSource>();

            audioInputDevice = Microphone.devices[0];

            Audio.clip = Microphone.Start(audioInputDevice, true, 10, AudioSettings.outputSampleRate);
            Audio.loop = true; // Set the AudioClip to loop

            while (!(Microphone.GetPosition(audioInputDevice) > 0)) { } // Wait until the recording has started
            Audio.Play(); // Play the audio source!
        }

        void FixedUpdate()
        {
            AnalyzeSound();
        }

        void AnalyzeSound()
        {
            // Wait until the recording has started. Should clear delay
            while (!(Microphone.GetPosition(audioInputDevice) > 0)) { }

            GetComponent<AudioSource>().GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
            ValueAndIndex[] samplesAndIndices = new ValueAndIndex[QSamples];

            for (int i = 0; i < QSamples; i++)
            {
                samplesAndIndices[i] = new ValueAndIndex(_spectrum[i], i);
            }

            // Get a sub array of the most important samples
            Array.Sort(samplesAndIndices);

            // GCD APPROXIMATION
            // Select only the most important samples. Re-sort them by index
            Array.Resize(ref samplesAndIndices, NUM_IMPORTANT_SAMPLES);
            Array.Sort(samplesAndIndices, new IndexComparer());

            var harmonicIndices = FindLocalMaximums(samplesAndIndices);
            var maxN = FindApproximateGCD(harmonicIndices);
            float freqN = maxN; // pass the index to a float variable

            // Interpolate index using neighbours
            if (maxN > 0 && maxN < QSamples - 1)
            {
                var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }

            // Convert index to frequency
            PitchValue = freqN * (AudioSettings.outputSampleRate / 2) / QSamples;

            if (PitchValue == float.NaN)
            {
                Debug.LogError("GCD is Nan!!!");

                Debug.LogError("Freq: " + freqN);

                foreach (var item in harmonicIndices)
                {
                    Debug.LogError("harmonic index: " + item);
                }
            }

            // Set Mel as well
            //MelValue = Helpers.HertzToMel(PitchValue);

            CalcRmsAndDb(harmonicIndices);
        }

        // Return hz index of any sample i that has a sample i-1 with lower value and sample i+1 with lower value
        private int[] FindLocalMaximums(ValueAndIndex[] samplesAndIndices)
        {
            List<int> locals = new List<int>();

            for (int i = 0; i < samplesAndIndices.Length; i++)
            {
                // Check edges, consider them as having the missing sample as 'lower'
                if (i == 0)
                {
                    if (samplesAndIndices.Length > 1 && samplesAndIndices[i].value > samplesAndIndices[i + 1].value)
                    {
                        locals.Add(samplesAndIndices[i].index);
                    }
                }
                else if (i == samplesAndIndices.Length - 1)
                {
                    if (samplesAndIndices.Length > 1 && samplesAndIndices[i].value > samplesAndIndices[i - 1].value)
                    {
                        locals.Add(samplesAndIndices[i].index);
                    }
                }
                else
                {
                    // Normal case
                    if (samplesAndIndices[i].value > samplesAndIndices[i - 1].value && samplesAndIndices[i].value > samplesAndIndices[i + 1].value)
                    {
                        locals.Add(samplesAndIndices[i].index);
                    }
                }
            }

            return locals.ToArray();
        }

        // Find the smallest difference 's' between neighbours. Make a function where x = i/'s', f(x) = i for each i in samplesIndx
        // Then make a linear regression of said function and its derivative is the GCD. Can produce serious errors
        private int FindApproximateGCD(int[] sampleIndicies)
        {
            int minDiff = int.MaxValue;
            float approx;

            // Just debug print for now, and return the smallest number
            Array.Sort(sampleIndicies);

            // Minimum difference between any two subsequent samples
            for (int i = 1; i < sampleIndicies.Length; i++)
            {
                minDiff = Math.Min(minDiff, sampleIndicies[i] - sampleIndicies[i - 1]);
            }

            // Init approx (with 0 if diff is larger than first freq or first freq = 0, else normal average of freq / (freq / diff))
            approx = (sampleIndicies[0] == 0 || sampleIndicies[0] / minDiff == 0) ? 0 : 
                sampleIndicies[0] / (sampleIndicies[0] / minDiff);

            // Rolling average
            for (int i = 1; i < sampleIndicies.Length; i++)
            {
                // Return 0 if sampleIndicies[i] / minDiff == 0 to avoid division by 0
                approx = ((approx * i) + 
                    ((sampleIndicies[i] / minDiff) == 0 ? 0 : (sampleIndicies[i] / (sampleIndicies[i] / minDiff)))) / (i + 1);
            }

            return Mathf.RoundToInt(approx);
        }

        private void CalcRmsAndDb(int[] harmonicIndices)
        {
            // fill array with samples
            GetComponent<AudioSource>().GetOutputData(_samples, 0);
            float sum = 0;

            for (int i = 0; i < QSamples; i++)
            {
                sum += _samples[i] * _samples[i];
            }

            // rms = square root of average
            RmsValue = Mathf.Sqrt(sum / QSamples);

            // calculate dB
            DbValue = 20 * Mathf.Log10(RmsValue / RefValue);

            // clamp it to -160dB min
            // get sound spectrum
            if (DbValue < minDbValue) DbValue = minDbValue;
        }
    }
}
