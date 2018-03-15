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
        Pause
    }

    public class Calibration : MonoBehaviour
    {
        public OffsetsProfile profile;
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

        private const float sampleTimeOffset = 1.0f;
        private const float finishStageTime = 3.0f;
        private const float pauseStageTime = 2.0f;

        private void Start()
        {
            currentStage = CalibrationStage.Silence;
            stageStartingTime = Time.time;
        }

        private void FixedUpdate()
        {
            StartCoroutine(Calibrate());
        }


        private float rollingAvrageVolume(float prevValue)
        {
            prevValue = (prevValue * (numOfPrevSamples - 1)) / numOfPrevSamples;
            // weight of current sample is 1/numOfPrefSamples
            return prevValue + (MicIn.DbValue / numOfPrevSamples);
        }
        IEnumerator Calibrate()
        {
            switch (currentStage)
            {
                case CalibrationStage.Silence:
                    
                    // Start sampling
                    if(Time.time - stageStartingTime > sampleTimeOffset)
                    {
                        numOfPrevSamples++;
                        silenceValue = rollingAvrageVolume(silenceValue);
                    }

                    // Finised silence
                    if (Time.time - stageStartingTime > finishStageTime)
                    {
                        currentStage = CalibrationStage.Pause;
                        nextStage = CalibrationStage.VolumeBaseLine;
                        numOfPrevSamples = 0;
                        stageStartingTime = Time.time;
                    }

                    yield return null;
                    break;

                case CalibrationStage.VolumeBaseLine:

                    // Start sampling
                    if (Time.time - stageStartingTime > sampleTimeOffset)
                    {
                        numOfPrevSamples++;
                        volumeBaseLineValue = rollingAvrageVolume(volumeBaseLineValue);
                    }

                    // Finised silence
                    if (Time.time - stageStartingTime > finishStageTime)
                    {
                        currentStage = CalibrationStage.Pause;
                        nextStage = CalibrationStage.VolumeBaseLine;
                        numOfPrevSamples = 0;
                        stageStartingTime = Time.time;
                    }

                    yield return null;
                    break;

                case CalibrationStage.Pause:

                    // Finised silence
                    if (Time.time - stageStartingTime > pauseStageTime)
                    {
                        currentStage = nextStage;
                        stageStartingTime = Time.time;
                    }

                    yield return null;
                    break;

            }
        }
    }
}
