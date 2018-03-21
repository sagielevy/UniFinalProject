using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AudioControl.CalibrationTypes
{
    class CalibrationStyleMiddleFirst : Calibration
    {
        protected override IEnumerator Calibrate()
        {
            float actualDist = 0;

            while (true)
            {
                switch (currentStage)
                {
                    case CalibrationStage.Silence:
                        VolumeStage(ref silenceValue);
                        InitPauseIfFinished(CalibrationStage.VolumeBaseLine);
                        CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeBaseLine:
                        VolumeStage(ref volumeBaseLineValue);
                        InitPauseIfFinished(CalibrationStage.VolumeMin);
                        CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.VolumeMin:
                        VolumeStage(ref volumeMinValue);

                        actualDist = volumeBaseLineValue - volumeMinValue;

                        InitPauseIfFinished(CalibrationStage.VolumeMax, actualDist, minVolumeDist);
                        CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.VolumeMax:
                        VolumeStage(ref volumeMaxValue);

                        actualDist = volumeMaxValue - volumeBaseLineValue;

                        InitPauseIfFinished(CalibrationStage.PitchBaseLine, actualDist, minVolumeDist);
                        CalcCurrentDistancePercent(actualDist, minVolumeDist);
                        break;

                    case CalibrationStage.PitchBaseLine:
                        PitchStage(ref pitchBaseLineValue);
                        InitPauseIfFinished(CalibrationStage.PitchLow);
                        CalcCurrentDistancePercent();
                        break;

                    case CalibrationStage.PitchLow:
                        PitchStage(ref pitchLowValue);

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
    }
}
