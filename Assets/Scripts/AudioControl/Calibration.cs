using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl
{
    public enum CalibrationStage
    {
        Silence,
        VolumeBaseLine,
        VolumeMin,
        VolumeMax,
        PitchBaseLine,
        PitchLow,
        PitchHigh,
        Pause,
        Finished
    }

    public class Calibration : MonoBehaviour
    {
        public OffsetsProfile VolumeProfile { get; private set; }
        public OffsetsProfile PitchProfile { get; private set; }
        public AudioMeasure MicIn;

        private float silenceValue = 0;
        private float volumeBaseLineValue = 0;
        private float volumeMinValue = 0;
        private float volumeMaxValue = 0;
        private float pitchBaseLineValue = 0;
        private float pitchLowValue = 0;
        private float pitchHighValue = 0;

        private CalibrationStage currentStage;
        private CalibrationStage nextStage;
        private float stageStartingTime;
        private int numOfPrevSamples = 0;
        private bool calibrate;
        private IEnumerator calibrateProcess;

        private const float sampleTimeOffset = 1.0f;
        private const float finishStageTime = 3.0f;
        private const float pauseStageTime = 2.0f;
        private const float defaultBaselineThreshold = 0.01f;

        public CalibrationStage GetCurrentStage()
        {
            return currentStage;
        }

        public void StartCalibrating()
        {
            calibrateProcess = Calibrate();
            stageStartingTime = Time.fixedTime;
            calibrate = true;
        }

        public bool IsCalibrationComplete()
        {
            return VolumeProfile != null && PitchProfile != null;
        }

        private void Start()
        {
            currentStage = CalibrationStage.Silence;
        }

        private void FixedUpdate()
        {
            if (calibrate)
            {
                StartCoroutine(calibrateProcess);
            }
        }

        private IEnumerator Calibrate()
        {
            while(true)
            {
                switch (currentStage)
                {
                    case CalibrationStage.Silence:
                        VolumeStage(ref silenceValue, CalibrationStage.VolumeBaseLine);
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStage(ref volumeBaseLineValue, CalibrationStage.VolumeMin);
                        break;

                    case CalibrationStage.VolumeMin:
                        VolumeStage(ref volumeMinValue, CalibrationStage.VolumeMax);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStage(ref volumeMaxValue, CalibrationStage.PitchBaseLine);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStage(ref pitchBaseLineValue, CalibrationStage.PitchLow);
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStage(ref pitchLowValue, CalibrationStage.PitchHigh);
                        break;

                    case CalibrationStage.PitchHigh:
                        PitchStage(ref pitchHighValue, CalibrationStage.Finished);
                        break;

                    case CalibrationStage.Pause:

                        // Finised silence
                        if (Time.fixedTime - stageStartingTime > pauseStageTime)
                        {
                            currentStage = nextStage;
                            stageStartingTime = Time.fixedTime;
                        }

                        break;

                    case CalibrationStage.Finished:
                        calibrate = false;
                        VolumeProfile = new OffsetsProfile(volumeBaseLineValue, volumeMaxValue, volumeMinValue, defaultBaselineThreshold);
                        PitchProfile = new OffsetsProfile(pitchBaseLineValue, pitchHighValue, pitchLowValue, defaultBaselineThreshold);
                        break;
                }

                yield return null;
            }
        }

        private void VolumeStage(ref float value, CalibrationStage next)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime > sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAvrageVolume(value);
            }

            InitPauseIfFinished(next);
        }

        private void PitchStage(ref float value, CalibrationStage next)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime > sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAvragePitch(value);
            }

            InitPauseIfFinished(next);
        }

        private float RollingAvrageVolume(float prevValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            return prevValue + (MicIn.DbValue / numOfPrevSamples);
        }

        private float RollingAvragePitch(float prevValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            return prevValue + (MicIn.MelValue / numOfPrevSamples);
        }

        private void InitPauseIfFinished(CalibrationStage next)
        {
            if (Time.fixedTime - stageStartingTime > finishStageTime)
            {
                currentStage = CalibrationStage.Pause;
                nextStage = next;
                numOfPrevSamples = 0;
                stageStartingTime = Time.fixedTime;
            }
        }
    }
}
