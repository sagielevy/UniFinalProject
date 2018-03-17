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
        private float stageStartingTime;
        private int numOfPrevSamples = 0;
        private bool continueProcess;
        private bool inputStageInvalid;
        private IEnumerator calibrateProcess;

        // TODO verify these distances!
        private const float minVolumeDist = 6;
        private const float minPitchDist = 20;
        private const float sampleTimeOffset = 3.0f;
        private const float finishStageTime = 3.0f;
        //private const float pauseStageTime = 2.0f;
        private const float defaultBaselineThreshold = 0.01f;

        public CalibrationStage GetCurrentStage()
        {
            return currentStage;
        }

        public bool IsInputStageInvalid()
        {
            // Reset flag
            var result = inputStageInvalid;
            inputStageInvalid = false;

            return result;
        }

        public void StartCalibrating()
        {
            calibrateProcess = Calibrate();
            stageStartingTime = Time.fixedTime;
            continueProcess = true;
            inputStageInvalid = false;
        }

        public void ContinueCalibrating()
        {
            continueProcess = true;
            stageStartingTime = Time.fixedTime;
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
            if (continueProcess)
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
                        VolumeStage(ref silenceValue);
                        InitPauseIfFinished(CalibrationStage.VolumeBaseLine);
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStage(ref volumeBaseLineValue);
                        InitPauseIfFinished(CalibrationStage.VolumeMin);
                        break;

                    case CalibrationStage.VolumeMin:
                        VolumeStage(ref volumeMinValue);
                        InitPauseIfFinished(CalibrationStage.VolumeMax, volumeBaseLineValue - volumeMinValue, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStage(ref volumeMaxValue);
                        InitPauseIfFinished(CalibrationStage.PitchBaseLine, volumeMaxValue - volumeBaseLineValue, minVolumeDist);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStage(ref pitchBaseLineValue);
                        InitPauseIfFinished(CalibrationStage.PitchLow);
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStage(ref pitchLowValue);
                        InitPauseIfFinished(CalibrationStage.PitchHigh, pitchBaseLineValue - pitchLowValue, minPitchDist);
                        break;

                    case CalibrationStage.PitchHigh:
                        PitchStage(ref pitchHighValue);
                        InitPauseIfFinished(CalibrationStage.Finished, pitchHighValue - pitchBaseLineValue, minPitchDist);
                        break;

                    case CalibrationStage.Pause:
                        //// Finised silence
                        //if (Time.fixedTime - stageStartingTime > pauseStageTime)
                        //{
                        //    currentStage = nextStage;
                        //    stageStartingTime = Time.fixedTime;
                        //}

                        break;

                    case CalibrationStage.Finished:
                        continueProcess = false;
                        VolumeProfile = new OffsetsProfile(volumeBaseLineValue, volumeMaxValue, volumeMinValue, defaultBaselineThreshold);
                        PitchProfile = new OffsetsProfile(pitchBaseLineValue, pitchHighValue, pitchLowValue, defaultBaselineThreshold);
                        break;
                }

                yield return null;
            }
        }

        private void VolumeStage(ref float value)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAverageVolume(value);
            }
        }

        private void PitchStage(ref float value)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAveragePitch(value);
            }
        }

        private float RollingAverageVolume(float prevValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            return prevValue + (MicIn.DbValue / numOfPrevSamples);
        }

        private float RollingAveragePitch(float prevValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            return prevValue + (MicIn.PitchValue / numOfPrevSamples);
        }

        private void InitPauseIfFinished(CalibrationStage next)
        {
            // Will always validate as true
            InitPauseIfFinished(next, 0, 0);
        }

        private void InitPauseIfFinished(CalibrationStage next, float valueBaselineDist, float minDistance)
        {
            if (Time.fixedTime - stageStartingTime > finishStageTime)
            {
                numOfPrevSamples = 0;
                continueProcess = false;

                // If no need to validate or validate required and distance is valid move to next stage
                if (valueBaselineDist >= minDistance)
                {
                    currentStage = next;
                } else
                {
                    // Input was bad this stage!
                    inputStageInvalid = true;
                }

                // Stop self from continuing if there's another step in the future
                if (next != CalibrationStage.Finished)
                {
                    StopCoroutine(calibrateProcess);
                }
            }
        }
    }
}
