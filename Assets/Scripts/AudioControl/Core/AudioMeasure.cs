using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.Core
{
    public class AudioMeasure : MonoBehaviour
    {
        public float RmsValue { get; private set; }
        public float DbValue { get; private set; }
        public float PitchValue { get; private set; }
        public float MelValue { get; private set; }
        private float OldPitchValue { get; set; }
        private float OldDbValue { get; set; }

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
        private float _fSample;

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

        void Start()
        {
            AudioSettings.Reset(new AudioConfiguration
            {
                sampleRate = samplerate
            });

            _samples = new float[QSamples];
            _spectrum = new float[QSamples];
            _fSample = AudioSettings.outputSampleRate;

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
            OldPitchValue = PitchValue;
            OldDbValue = DbValue;

            // Wait until the recording has started. Should clear delay
            while (!(Microphone.GetPosition(audioInputDevice) > 0)) { }

            GetComponent<AudioSource>().GetOutputData(_samples, 0); // fill array with samples
            int i;
            float sum = 0;
            for (i = 0; i < QSamples; i++)
            {
                sum += _samples[i] * _samples[i]; // sum squared samples
            }

            // TODO Perhaps Should calculate Db & Rms by the following method:
            // After calculating the GCD or the real tone, check the top important samples and only if they're close enough to being a multiple
            // of that value, add their amplitutes. Ignore if they are anomalies
            RmsValue = Mathf.Sqrt(sum / QSamples); // rms = square root of average
            DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
            if (DbValue < minDbValue) DbValue = minDbValue; // clamp it to -160dB min
                                                            // get sound spectrum
            GetComponent<AudioSource>().GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
            ValueAndIndex[] samplesAndIndices = new ValueAndIndex[QSamples];

            for (i = 0; i < QSamples; i++)
            {
                samplesAndIndices[i] = new ValueAndIndex(_spectrum[i], i);
            }

            // Get a sub array of the most important samples
            Array.Sort(samplesAndIndices);

            //for (i = 0; i < 20; i++)
            //{
            //    UnityEngine.Debug.Log(Time.frameCount + " no." + i + ", " + samplesAndIndices[i]);
            //}

            // BAD LOGIC
            // Find max - TODO This is bad, we need GCD instead
            //float maxV = 0;
            //var maxN = 0;
            //for (i = 0; i < QSamples; i++)
            //{
            //    if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
            //        continue;

            //    maxV = _spectrum[i];
            //    maxN = i; // maxN is the index of max
            //}
            //float freqN = maxN; // pass the index to a float variable
            //if (maxN > 0 && maxN < QSamples - 1)
            //{ // interpolate index using neighbours
            //    var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            //    var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            //    freqN += 0.5f * (dR * dR - dL * dL);
            //}

            //PitchValue = freqN * (AudioSettings.outputSampleRate / 2) / QSamples; // convert index to frequency

            // GCD APPROXIMATION
            // Select only the most important samples. Re-sort them by index
            Array.Resize(ref samplesAndIndices, NUM_IMPORTANT_SAMPLES);
            Array.Sort(samplesAndIndices, new IndexComparer());
            PitchValue = FindApproximateGCD(FindLocalMaximums(samplesAndIndices)) * (AudioSettings.outputSampleRate / 2) / QSamples; // convert index to frequency

            //// we try to correct the PitchValue if the jump was too large
            //var variance = PitchValue / OldPitchValue;

            //if (/*DbValue != minDbValue && OldDbValue != minDbValue && */variance > 1.5)
            //{
            //    for (i = 2; i < 10; i++)
            //    {
            //        var correctedVariance = PitchValue / (i * OldPitchValue);

            //        if (correctedVariance < 1 + acceptedChangeInPitch && correctedVariance > 1 - acceptedChangeInPitch)
            //        {
            //            PitchValue /= i;
            //            break;
            //        }
            //    }
            //}
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

        // TODO FIND A SOLUTION!
        // https://stackoverflow.com/questions/445113/approximate-greatest-common-divisor
        private int FindApproximateGCD(int[] sampleIndicies)
        {
            // Just debug print for now, and return the smallest number
            Array.Sort(sampleIndicies);

            for (int i = 0; i < sampleIndicies.Length; i++)
            {
                UnityEngine.Debug.Log(Time.frameCount + " no." + i + ", " + sampleIndicies[i]);
            }

            return sampleIndicies[0];
        }
    }
}
