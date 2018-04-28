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

        private AudioSource Audio;
        private string audioInputDevice;
        private const int micSampleRate = 44100;
        private const int QSamples = 1024;
        private const float RefValue = 0.1f;
        private const float Threshold = 0.002f;
        private const float minDbValue = -160;
        private const float acceptedChangeInPitch = 0.4f;
        private const int NUM_IMPORTANT_SAMPLES = 20;

        float[] _samples;
        private float[] _spectrum;

        #region Elements
        //public Text TextStatus;
        //public Dropdown DropDownMicrophone;
        //public Button ButtonRecord;

        //private Text ButtonText;
        //private AudioSource audioSource;
        //private List<string> ListDevice;
        //private int selectedDevice = 0;

        [Tooltip("Seconds of input that will be buffered. Too short = lost data")]
        public int micRecordLength = 1;
        [Tooltip("Seconds until mic input is played through audio source. Too short = artifacts")]
        public float latencyBuffer = 0.001f;
        [Tooltip("Max latency allowed. If this value is too small the mic will constantly restart!")]
        public float maxLatencySeconds = 0.03f;
        #endregion

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
            var orgConf = AudioSettings.GetConfiguration();

            AudioSettings.Reset(new AudioConfiguration
            {
                // Modify sample rate. Keep the rest the same
                sampleRate = micSampleRate,
                dspBufferSize = orgConf.dspBufferSize,
                numRealVoices = orgConf.numRealVoices,
                numVirtualVoices = orgConf.numVirtualVoices,
                speakerMode = orgConf.speakerMode
            });

            _samples = new float[QSamples];
            _spectrum = new float[QSamples];

            Audio = GetComponent<AudioSource>();

            audioInputDevice = Microphone.devices[0];
            IsRecording = true;
        }

        void FixedUpdate()
        {
            HandleLatency();
            AnalyzeSound();
        }

        void AnalyzeSound()
        {

            Audio.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
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
            // Fill array with samples
            Audio.GetOutputData(_samples, 0);
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

        #region Mic Options
        /// <summary>Loads recording device names into DropDown</summary>
        /// <returns>status string</returns>
        //private string LoadMicrophoneDevices()
        //{
        //    if (Microphone.devices.Length <= 0)
        //        return ("Microphone not connected!");

        //    DropDownMicrophone.ClearOptions();
        //    ListDevice = new List<string>();

        //    foreach (string device in Microphone.devices)
        //        ListDevice.Add(device);

        //    DropDownMicrophone.AddOptions(ListDevice);
        //    return "Recording devices loaded";
        //}

        //private void Start()
        //{
        //    SetupGlobalAudio();
        //    SetupAudioSource();
        //    //setup listeners in code to make less confusing later on
        //    ButtonRecord.onClick.AddListener(Button_Click);
        //    DropDownMicrophone.onValueChanged.AddListener(DropDown_Changed);

        //    ButtonText = ButtonRecord.GetComponentInChildren<Text>();
        //    TextStatus.text = LoadMicrophoneDevices();
        //}
        #endregion

        #region Helpers
        /// <summary>Set = Start / stop mic recording. Get = Microphone.IsRecording</summary>
        private bool IsRecording
        {
            get { return Microphone.IsRecording(audioInputDevice); }
            set
            {
                if (value) //true
                {
                    if (!Microphone.IsRecording(audioInputDevice))
                    {
                        //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource
                        Audio.clip = Microphone.Start(audioInputDevice, true, micRecordLength, micSampleRate);// maxFreq);
                        if (!(RecordHeadPosition > 0)) { }          //wait for mic ready
                        Audio.loop = true;                    //continual output
                        Audio.mute = false;                   //Hack for bug
                        Audio.PlayDelayed(latencyBuffer);     //must be delayed or bad results
                    }
                }
                else //false
                {
                    if (Microphone.IsRecording(audioInputDevice))
                    {
                        Microphone.End(audioInputDevice);//ListDevice[selectedDevice]);
                        Audio.clip = null;
                        Audio.loop = false;
                    }
                }
            }
        }

        private int RecordHeadPosition
        {
            get { return Microphone.GetPosition(audioInputDevice); }//ListDevice[selectedDevice]); }
        }

        private void Record()
        {
            if (!IsRecording)
            {
                IsRecording = true;
                //ButtonText.text = "Stop";
                //TextStatus.text = "Recording";
            }
            else
            {
                //TextStatus.text = "Device is already recording...";
            }
        }
        #endregion

        #region Everything below is for calculating and auto setting latency
        static int Read_Position = 0;
        static int Max_Latency = 0;
        static int old_Write_Position = 0;
        static bool Reload = false;

        private void HandleLatency()
        {
            if (IsRecording)
            {
                float latencySeconds = (float)Max_Latency / AudioSettings.outputSampleRate;
                //TextStatus.text = Max_Latency.ToString() + " Samples / " + AudioSettings.outputSampleRate.ToString() + " samplerate = " + latencySeconds.ToString() + " seconds Latency";

                //auto select best latency value
                if (Max_Latency > latencyBuffer * micSampleRate || latencySeconds > maxLatencySeconds)
                {
                    Debug.Log("Latency got out of hand!\n" + Max_Latency.ToString() + " Samples / " + AudioSettings.outputSampleRate.ToString() + " samplerate = " + latencySeconds.ToString() + " seconds Latency\nRestarting record...");
                    latencyBuffer = Mathf.Max(latencyBuffer, ((float)Max_Latency / micSampleRate) + 0.01f);

                    // Restart recording
                    IsRecording = false;
                    IsRecording = true;
                    Reload = true;
                }
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (Reload)
            {
                Max_Latency = 0;
                Read_Position = 0;
                old_Write_Position = 0;
                Reload = false;
            }

            Read_Position += data.Length;
            int Write_Position = Microphone.GetPosition(audioInputDevice);

            if (Write_Position < old_Write_Position)    //Check if write buffer looped
            {
                Read_Position = data.Length;
            }

            //latency in samples
            int Latency = Read_Position - Write_Position;

            if (Latency > Max_Latency)
                Max_Latency = Latency;

            old_Write_Position = Write_Position;
        }
        #endregion
    }
}