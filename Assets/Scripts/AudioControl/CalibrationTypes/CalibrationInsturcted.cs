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
            float actualDist = 0;

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
                        actualDist = volumeBaseLineValue - silence;
                        InitPauseIfFinishedTimeBased(CalibrationStage.VolumeMin, actualDist, minVolumeDist + minSilenceDist);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeMin:
                        actualDist = volumeBaseLineValue - volumeMinValue;
                        VolumeStageSampleBased(ref volumeMinValue, actualDist, minVolumeDist);
                        InitPauseIfFinishedSampleBased(CalibrationStage.VolumeMax, actualDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        actualDist = volumeMaxValue - volumeBaseLineValue;
                        VolumeStageSampleBased(ref volumeMaxValue, actualDist, minVolumeDist);
                        InitPauseIfFinishedSampleBased(CalibrationStage.PitchBaseLine, actualDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStageTimeBased(ref pitchBaseLineValue);
                        InitPauseIfFinishedTimeBased(CalibrationStage.PitchLow);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.PitchLow:
                        actualDist = pitchBaseLineValue - pitchLowValue;
                        PitchStageSampleBased(ref pitchLowValue, actualDist, minPitchDist);
                        InitPauseIfFinishedTimeBased(CalibrationStage.PitchHigh, actualDist, minPitchDist);
                        //CalcCurrentDistancePercent(actualDist, minPitchDist);
                        break;

                    case CalibrationStage.PitchHigh:
                        actualDist = pitchHighValue - pitchBaseLineValue;
                        PitchStageSampleBased(ref pitchHighValue, actualDist, minPitchDist);
                        InitPauseIfFinishedTimeBased(CalibrationStage.Finished, actualDist, minPitchDist);
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
