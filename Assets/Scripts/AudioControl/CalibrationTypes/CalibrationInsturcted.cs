using Assets.Scripts.GameScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.CalibrationTypes
{
    public class CalibrationInsturcted : Calibration
    {
        protected override IEnumerator Calibrate()
        {
            float averageDist = 0;

            while (true)
            {
                switch (currentStage)
                {
                    case CalibrationStage.Silence:
                        VolumeStageTimeBased(ref silence);
                        InitPauseIfFinishedTimeBased(CalibrationStage.VolumeBaseLine);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStageTimeBased(ref volumeBaseLineValue);
                        averageDist = volumeBaseLineValue - silence;
                        InitPauseIfFinishedTimeBased(CalibrationStage.VolumeMin, averageDist, minVolumeDist + minSilenceDist);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeMin:
                        // Verify single sample distance
                        VolumeStageSampleBased(ref volumeMinValue, volumeBaseLineValue - MicIn.DbValue, minVolumeDist);
                        averageDist = volumeBaseLineValue - volumeMinValue;
                        InitPauseIfFinishedSampleBased(CalibrationStage.VolumeMax, averageDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        // Verify single sample distance
                        VolumeStageSampleBased(ref volumeMaxValue, MicIn.DbValue - volumeBaseLineValue, minVolumeDist);
                        averageDist = volumeMaxValue - volumeBaseLineValue;
                        InitPauseIfFinishedSampleBased(CalibrationStage.PitchBaseLine, averageDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStageTimeBased(ref pitchBaseLineValue);
                        InitPauseIfFinishedTimeBased(CalibrationStage.PitchLow);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.PitchLow:
                        // Verify single sample distance
                        PitchStageSampleBased(ref pitchLowValue, pitchBaseLineValue - MicIn.PitchValue, minPitchDist);
                        averageDist = pitchBaseLineValue - pitchLowValue;
                        InitPauseIfFinishedSampleBased(CalibrationStage.PitchHigh, averageDist, minPitchDist);
                        //CalcCurrentDistancePercent(actualDist, minPitchDist);
                        break;

                    case CalibrationStage.PitchHigh:
                        // Verify single sample distance
                        PitchStageSampleBased(ref pitchHighValue, MicIn.PitchValue - pitchBaseLineValue, minPitchDist);
                        averageDist = pitchHighValue - pitchBaseLineValue;
                        InitPauseIfFinishedSampleBased(CalibrationStage.Finished, averageDist, minPitchDist);
                        //CalcCurrentDistancePercent(actualDist, minPitchDist);
                        break;

                    //case CalibrationStage.Pause:
                        //// Finised silence
                        //if (Time.fixedTime - stageStartingTime > pauseStageTime)
                        //{
                        //    currentStage = nextStage;
                        //    stageStartingTime = Time.fixedTime;
                        //}
                        //break;

                    case CalibrationStage.Finished:
                        continueProcess = false;
                        VolumeProfile = new OffsetsProfile(volumeBaseLineValue, volumeMaxValue, volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                        PitchProfile = new OffsetsProfile(pitchBaseLineValue, pitchHighValue, pitchLowValue, Constants.DefaultPitchBaselineThreshold);
                        break;
                }

                yield return null;
            }
        }

        protected override CalibrationStage GetInitialStage()
        {
            return CalibrationStage.Silence;
        }
    }
}
