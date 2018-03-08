using System;
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
        private int samplerate = 44100;
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;
        private const float Threshold = 0.002f;
        private const float minDbValue = -160;
        private const float acceptedChangeInPitch = 0.4f;

        float[] _samples;
        private float[] _spectrum;
        private float _fSample;

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

            string audioInputDevice = Microphone.devices[0];

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

            GetComponent<AudioSource>().GetOutputData(_samples, 0); // fill array with samples
            int i;
            float sum = 0;
            for (i = 0; i < QSamples; i++)
            {
                sum += _samples[i] * _samples[i]; // sum squared samples
            }
            RmsValue = Mathf.Sqrt(sum / QSamples); // rms = square root of average
            DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
            if (DbValue < minDbValue) DbValue = minDbValue; // clamp it to -160dB min
                                                            // get sound spectrum
            GetComponent<AudioSource>().GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
            float maxV = 0;
            var maxN = 0;
            for (i = 0; i < QSamples; i++)
            {
                // Find max - TODO This is bad, we need GCD instead
                if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                    continue;

                maxV = _spectrum[i];
                maxN = i; // maxN is the index of max
            }
            float freqN = maxN; // pass the index to a float variable
            if (maxN > 0 && maxN < QSamples - 1)
            { // interpolate index using neighbours
                var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }

            PitchValue = freqN * (AudioSettings.outputSampleRate / 2) / QSamples; // convert index to frequency

            // we try to correct the PitchValue if the jump was too large
            var variance = PitchValue / OldPitchValue;

            if (/*DbValue != minDbValue && OldDbValue != minDbValue && */variance > 1.5)
            {
                for (i = 2; i < 10; i++)
                {
                    var correctedVariance = PitchValue / (i * OldPitchValue);

                    if (correctedVariance < 1 + acceptedChangeInPitch && correctedVariance > 1 - acceptedChangeInPitch)
                    {
                        PitchValue /= i;
                        break;
                    }
                }
            }
        }
    }
}
