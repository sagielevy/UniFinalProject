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
        public OffsetsProfile volumeProfile;
        public OffsetsProfile pitchProfile;
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
        private const float defaultBaselineThreshold = 0.01f;

        private void Start()
        {
            currentStage = CalibrationStage.Silence;
            stageStartingTime = Time.time;
        }

        private void FixedUpdate()
        {
            StartCoroutine(Calibrate());
        }

        private IEnumerator Calibrate()
        {
            while(true)
            {
                switch (currentStage)
                {
                    case CalibrationStage.Silence:
                        VolumeStage(ref silenceValue);
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStage(ref volumeBaseLineValue);
                        break;

                    case CalibrationStage.VolumeMin:
                        VolumeStage(ref volumeMinValue);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStage(ref volumeMaxValue);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStage(ref pitchBaseLineValue);
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStage(ref pitchLowValue);
                        break;

                    case CalibrationStage.PitchHigh:
                        PitchStage(ref pitchHighValue);
                        break;

                    case CalibrationStage.Pause:

                        // Finised silence
                        if (Time.time - stageStartingTime > pauseStageTime)
                        {
                            currentStage = nextStage;
                            stageStartingTime = Time.time;
                        }

                        break;

                    case CalibrationStage.Finished:
                        volumeProfile = new OffsetsProfile(volumeBaseLineValue, volumeMaxValue, volumeMinValue, defaultBaselineThreshold);
                        pitchProfile = new OffsetsProfile(pitchBaseLineValue, pitchHighValue, pitchLowValue, defaultBaselineThreshold);
                        break;

                }

                yield return null;
            }
        }

        private void VolumeStage(ref float value)
        {
            // Start sampling
            if (Time.time - stageStartingTime > sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAvrageVolume(value);
            }

            InitPauseIfFinished();
        }

        private void PitchStage(ref float value)
        {
            // Start sampling
            if (Time.time - stageStartingTime > sampleTimeOffset)
            {
                numOfPrevSamples++;
                value = RollingAvragePitch(value);
            }

            InitPauseIfFinished();
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

        private void InitPauseIfFinished()
        {
            if (Time.time - stageStartingTime > finishStageTime)
            {
                currentStage = CalibrationStage.Pause;
                nextStage = GetNextStage();
                numOfPrevSamples = 0;
                stageStartingTime = Time.time;
            }
        }

        private CalibrationStage GetNextStage()
        {
            switch (currentStage)
            {
                case CalibrationStage.Silence:
                    return CalibrationStage.VolumeBaseLine;
                case CalibrationStage.VolumeBaseLine:
                    return CalibrationStage.VolumeMin;
                case CalibrationStage.VolumeMin:
                    return CalibrationStage.VolumeMax;
                case CalibrationStage.VolumeMax:
                    return CalibrationStage.PitchBaseLine;
                case CalibrationStage.PitchBaseLine:
                    return CalibrationStage.PitchLow;
                case CalibrationStage.PitchLow:
                    return CalibrationStage.PitchHigh;
                case CalibrationStage.PitchHigh:
                    return CalibrationStage.Finished;
                default:
                    return CalibrationStage.Finished;
            }
        }
    }
}
