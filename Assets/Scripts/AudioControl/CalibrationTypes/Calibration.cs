using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.CalibrationTypes
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
        //Pause,
        Finished
    }

    public abstract class Calibration : MonoBehaviour
    {
        #region public propeties
        public AudioMeasure MicIn;
        public OffsetsProfile VolumeProfile { get; protected set; }
        public OffsetsProfile PitchProfile { get; protected set; }
        //public float GetCurrentDistancePercent { get; private set; }
        public float GetCurrentTimePercent { get { return 100 * (Time.fixedTime - stageStartingTime) / sampleTimeOffset; } }
        public float GetCurrentSamplePercent { get { return 100 * ((float)numOfPrevSamples / requiredNumOfValidSamplesSampleBased); } }
        public bool CompletedRequiredSamples { get { return numOfPrevSamples >= requiredNumOfValidSamplesSampleBased; } }
        public float volumeBaseLineValue = 0;
        public float pitchBaseLineValue = 0;
        public float volumeMinValue = 0;
        public float volumeMaxValue = 0;
        public float pitchLowValue = 0;
        public float pitchHighValue = 0;
        #endregion

        #region protected members
        protected float silence = 0;

        protected CalibrationStage currentStage;
        protected bool continueProcess;

        // TODO verify these distances!
        protected const float minVolumeDist = 4.2f;
        protected const float minSilenceDist = 12;
        protected const float minPitchDist = 20;
        //protected const float pauseStageTime = 2.0f;

        #endregion

        #region private members
        private float stageStartingTime;
        private int numOfPrevSamples = 0;
        private bool inputStageInvalid;
        private IEnumerator calibrateProcess;

        // TODO verify these distances!
        private const int requiredNumOfValidSamplesTimeBased = 3000;
        private const int requiredNumOfValidSamplesSampleBased = 5000;
        private const int maxNumOfValidSamples = 80000;
        private const float stageTimeoutSeconds = 60;
        private const float sampleTimeOffset = 3.0f;
        private const float finishStageTime = 3.0f;
        #endregion

        #region public methods
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

        public void ContinueCalibrating()
        {
            continueProcess = true;
            stageStartingTime = Time.fixedTime;
        }

        public bool IsCalibrationComplete()
        {
            return VolumeProfile != null && PitchProfile != null;
        }

        // TODO restart a single part and return to where stopped.
        public void RestartCalibrationProcess()
        {
            if (calibrateProcess != null)
            {
                StopCoroutine(calibrateProcess);
            }

            calibrateProcess = Calibrate();
            currentStage = GetInitialStage();
            inputStageInvalid = false;
            continueProcess = false;
            numOfPrevSamples = 0;
        }
        #endregion

        #region protected methods
        protected abstract IEnumerator Calibrate();

        protected abstract CalibrationStage GetInitialStage();

        protected void VolumeStageTimeBased(ref float value)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAverage(value, MicIn.DbValue);
            }
        }

        protected void PitchStageTimeBased(ref float value)
        {
            // Take sample if volume is above min dB
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset &&
                MicIn.DbValue >= volumeMinValue)
            {
                numOfPrevSamples++;
                value = RollingAverage(value, MicIn.PitchValue);
            }
        }

        // Adds valid samples only. REQUIRES VALID SILENCE DATA!
        protected void VolumeStageSampleBased(ref float value, float valueBaselineDist, float minDistance)
        {
            // First of all verify that sound is ABOVE silence
            // Add sample if timeout was not reached and sample is valid
            if (Time.fixedTime - stageStartingTime < stageTimeoutSeconds &&
                valueBaselineDist > minDistance &&
                MicIn.DbValue > (silence + minSilenceDist))
            {
                // Add up to max samples
                numOfPrevSamples++;

                // Not completed all required samples, include in calculation
                if (numOfPrevSamples < requiredNumOfValidSamplesSampleBased)
                {
                    value = RollingAverage(value, MicIn.DbValue);
                }
            }
        }

        // Adds valid samples only. REQUIRES VALID MIN VOLUME DATA!
        protected void PitchStageSampleBased(ref float value, float valueBaselineDist, float minDistance)
        {
            // Take sample if volume is above min dB
            // Add sample if timeout was not reached and sample is valid
            if (Time.fixedTime - stageStartingTime < stageTimeoutSeconds &&
                valueBaselineDist > minDistance &&
                MicIn.DbValue >= volumeMinValue)
            {
                // Add up to max samples
                numOfPrevSamples++;

                // Not completed all required samples, include in calculation
                if (numOfPrevSamples < requiredNumOfValidSamplesSampleBased)
                {
                    value = RollingAverage(value, MicIn.PitchValue);
                }
            }
        }

        protected void InitPauseIfFinishedTimeBased(CalibrationStage next)
        {
            // Will always validate as true
            InitPauseIfFinishedTimeBased(next, 0, 0);
        }

        protected void InitPauseIfFinishedTimeBased(CalibrationStage next, float valueBaselineDist, float minDistance)
        {
            if (Time.fixedTime - stageStartingTime > finishStageTime)
            {
                // If no need to validate or validate required and distance is valid,
                // and number of total samples surpasses required samples for time based - move to next stage
                if (valueBaselineDist >= minDistance && 
                    numOfPrevSamples >= requiredNumOfValidSamplesTimeBased)
                {
                    currentStage = next;
                }
                else
                {
                    // Input was bad this stage!
                    inputStageInvalid = true;
                    Debug.Log("Min distance: " + minDistance);
                    Debug.Log("Difference distance: " + valueBaselineDist);
                    Debug.Log("Samples accumulated: " + numOfPrevSamples);
                }

                // Stop self from continuing if there's another step in the future
                if (next != CalibrationStage.Finished)
                {
                    StopCoroutine(calibrateProcess);
                }

                // Reset temp variables
                numOfPrevSamples = 0;
                continueProcess = false;
            }
        }

        protected void InitPauseIfFinishedSampleBased(CalibrationStage next, float valueBaselineDist, float minDistance)
        {
            // Finish if passed timeout or max samples has been reached
            if (Time.fixedTime - stageStartingTime > stageTimeoutSeconds ||
                numOfPrevSamples >= maxNumOfValidSamples)
            {
                // If no need to validate or validate required and distance is valid move to next stage
                // Number of samples passed required thershold
                if (valueBaselineDist >= minDistance &&
                    numOfPrevSamples >= requiredNumOfValidSamplesSampleBased)
                {
                    currentStage = next;
                }
                else
                {
                    // Input was bad this stage!
                    inputStageInvalid = true;
                    Debug.Log("Min distance: " + minDistance);
                    Debug.Log("Difference distance: " + valueBaselineDist);
                }

                // Stop self from continuing if there's another step in the future
                if (next != CalibrationStage.Finished)
                {
                    StopCoroutine(calibrateProcess);
                }

                // Reset temp variables
                numOfPrevSamples = 0;
                continueProcess = false;
            }
        }
        #endregion

        #region private methods
        private void Start()
        {
            RestartCalibrationProcess();
        }

        private void FixedUpdate()
        {
            if (continueProcess)
            {
                StartCoroutine(calibrateProcess);
            }
        }

        private float RollingAverage(float prevValue, float micValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            return prevValue + (micValue / numOfPrevSamples);
        }
        #endregion
    }
}
