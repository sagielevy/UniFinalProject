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
                        VolumeStage(ref volumeMinValue);
                        InitPauseIfFinished(CalibrationStage.VolumeBaseLine);
                        CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStage(ref volumeBaseLineValue);

                        actualDist = volumeBaseLineValue - volumeMinValue;

                        InitPauseIfFinished(CalibrationStage.VolumeMax, actualDist, minVolumeDist);
                        CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStage(ref volumeMaxValue);

                        actualDist = volumeMaxValue - volumeBaseLineValue;

                        InitPauseIfFinished(CalibrationStage.PitchLow, actualDist, minVolumeDist);
                        CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStage(ref pitchLowValue);
                        InitPauseIfFinished(CalibrationStage.PitchBaseLine);
                        CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStage(ref pitchBaseLineValue);

                        actualDist = pitchBaseLineValue - pitchLowValue;

                        InitPauseIfFinished(CalibrationStage.PitchHigh, actualDist, minPitchDist);
                        CalcCurrentDistancePercent(actualDist, minPitchDist);
                        break;
                    
                    case CalibrationStage.PitchHigh:
                        PitchStage(ref pitchHighValue);

                        actualDist = pitchHighValue - pitchBaseLineValue;

                        InitPauseIfFinished(CalibrationStage.Finished, actualDist, minPitchDist);
                        CalcCurrentDistancePercent(actualDist, minPitchDist);
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
                        VolumeProfile = new OffsetsProfile(volumeBaseLineValue, volumeMaxValue, volumeMinValue, defaultVolumeBaselineThreshold);
                        PitchProfile = new OffsetsProfile(pitchBaseLineValue, pitchHighValue, pitchLowValue, defaultPitchBaselineThreshold);
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
