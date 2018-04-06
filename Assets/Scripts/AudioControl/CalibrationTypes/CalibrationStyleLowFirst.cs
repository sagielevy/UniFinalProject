using Assets.Scripts.GameScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AudioControl.CalibrationTypes
{
    class CalibrationStyleLowFirst : Calibration
    {
        protected override IEnumerator Calibrate()
        {
            float actualDist = 0;

            while (true)
            {
                switch (currentStage)
                {
                    case CalibrationStage.VolumeMin:
                        VolumeStageTimeBased(ref volumeMinValue);
                        //InitPauseIfFinishedTimeBased(CalibrationStage.VolumeBaseLine);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStageTimeBased(ref volumeBaseLineValue);

                        actualDist = volumeBaseLineValue - volumeMinValue;

                        //InitPauseIfFinishedTimeBased(CalibrationStage.VolumeMax, actualDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStageTimeBased(ref volumeMaxValue);

                        actualDist = volumeMaxValue - volumeBaseLineValue;

                        //InitPauseIfFinishedTimeBased(CalibrationStage.PitchLow, actualDist, minVolumeDist);
                        //CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStageTimeBased(ref pitchLowValue);
                        //InitPauseIfFinishedTimeBased(CalibrationStage.PitchBaseLine);
                        //CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStageTimeBased(ref pitchBaseLineValue);

                        actualDist = pitchBaseLineValue - pitchLowValue;

                        //InitPauseIfFinishedTimeBased(CalibrationStage.PitchHigh, actualDist, minPitchDist);
                        //CalcCurrentDistancePercent(actualDist, minPitchDist);
                        break;
                    
                    case CalibrationStage.PitchHigh:
                        PitchStageTimeBased(ref pitchHighValue);

                        actualDist = pitchHighValue - pitchBaseLineValue;

                        //InitPauseIfFinishedTimeBased(CalibrationStage.Finished, actualDist, minPitchDist);
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
            return CalibrationStage.VolumeMin;
        }
    }
}
