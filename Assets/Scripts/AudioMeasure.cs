using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class AudioMeasure : MonoBehaviour
    {
        public float RmsValue { get; private set; }
        public float DbValue { get; private set; }
        public float PitchValue { get; private set; }
        public float MelValue { get; private set; }
        public float OldDbValue { get; private set; }

        private AudioSource Audio;
        private int samplerate = 44100;
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;
        private const float Threshold = 0.002f;
        private const float minDbValue = -160;
        

        float[] _samples;
        private float[] _spectrum;
        private float _fSample;

        void Start()
        {
            AudioSettings.outputSampleRate = 2000; // ???
            _samples = new float[QSamples];
            _spectrum = new float[QSamples];
            _fSample = AudioSettings.outputSampleRate;

            Audio = GetComponent<AudioSource>();
            Audio.clip = Microphone.Start(null, true, 10, samplerate);
            Audio.loop = true; // Set the AudioClip to loop
            string audioInputDevice = Microphone.devices[0];

            while (!(Microphone.GetPosition(audioInputDevice) > 0)) { } // Wait until the recording has started
            Audio.Play(); // Play the audio source!
        }

        void FixedUpdate()
        {
            AnalyzeSound();
        }

        void AnalyzeSound()
        {
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
                // find max 
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

            var oldPitchValue = PitchValue;
            
            PitchValue = freqN * (AudioSettings.outputSampleRate / 2) / QSamples; // convert index to frequency

            var variance = PitchValue > oldPitchValue 
                ? PitchValue / oldPitchValue 
                : oldPitchValue / PitchValue;
            if (DbValue != minDbValue && variance > 1.5)
            {
                for(i = 2; i < 10; i++)
                {
                    if(Mathf.Abs(pi))
                }
            }
        }
    }
}
