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
        VolumeBaseLine,
        VolumeMin,
        VolumeMax,
        PitchBaseLine,
        PitchLow,
        PitchHigh,
        Pause,
        Finished
    }

    public abstract class Calibration : MonoBehaviour
    {
        #region public propeties
        public AudioMeasure MicIn;
        public OffsetsProfile VolumeProfile { get; protected set; }
        public OffsetsProfile PitchProfile { get; protected set; }
        public float GetCurrentDistancePercent { get; private set; }
        #endregion

        #region protected members
        protected float volumeBaseLineValue = 0;
        protected float volumeMinValue = 0;
        protected float volumeMaxValue = 0;
        protected float pitchBaseLineValue = 0;
        protected float pitchLowValue = 0;
        protected float pitchHighValue = 0;

        protected CalibrationStage currentStage;
        protected bool continueProcess;

        // TODO verify these distances!
        protected const float minVolumeDist = 4.2f;
        protected const float minPitchDist = 20;
        //protected const float pauseStageTime = 2.0f;

        // TODO Verify these thresholds!! Should they be dynamic??
        protected const float defaultPitchBaselineThreshold = 0.03f;
        protected const float defaultVolumeBaselineThreshold = 0.10f;
        #endregion

        #region private members
        private float stageStartingTime;
        private int numOfPrevSamples = 0;
        private bool inputStageInvalid;
        private IEnumerator calibrateProcess;

        // TODO verify these distances!
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

        protected void VolumeStage(ref float value)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAverageVolume(value);
            }
        }

        protected void PitchStage(ref float value)
        {
            // Start sampling
            if (Time.fixedTime - stageStartingTime < sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAveragePitch(value);
            }
        }

        protected void InitPauseIfFinished(CalibrationStage next)
        {
            // Will always validate as true
            InitPauseIfFinished(next, 0, 0);
        }

        protected void InitPauseIfFinished(CalibrationStage next, float valueBaselineDist, float minDistance)
        {
            if (Time.fixedTime - stageStartingTime > finishStageTime)
            {
                numOfPrevSamples = 0;
                continueProcess = false;

                // If no need to validate or validate required and distance is valid move to next stage
                if (valueBaselineDist >= minDistance)
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
            }
        }

        protected void CalcCurrentDistancePercent()
        {
            CalcCurrentDistancePercent(0, 0);
        }

        /// <summary>
        /// Returns the current distance from the required threshold 
        /// </summary>
        /// <returns></returns>
        protected void CalcCurrentDistancePercent(float valueBaselineDist, float minDistance)
        {
            // Ignore no distance - just put 100%
            if (minDistance <= 0)
            {
                GetCurrentDistancePercent = 100;
            }
            else
            {
                GetCurrentDistancePercent = (valueBaselineDist / minDistance) * 100;
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
        #endregion
    }
}
