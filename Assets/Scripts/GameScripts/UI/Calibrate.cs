using Assets.Scripts.AudioControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.GameScripts.UI
{
    [RequireComponent(typeof(Calibration))]
    public class Calibrate : MonoBehaviour
    {
        private const string Welcome = "Before we begin we need to calibrate our systems to meet your vocal capabilities";
        private const string Pause = "Great! Processing your voice. Please wait a moment";
        private const string Silence = "Please make no sound and make sure there's as little noise as possible around you.";
        private const string VolumeBaseline = "Make an \"aaaaaah...\" sound with your normal speaking volume";
        private const string VolumeMin = "Keep making an \"aaaaaah...\" sound, but now lower your volume to a whisper";
        private const string VolumeMax = "Still make that \"aaaaaah...\" sound, but now raise your volume to your maximum comfortable level.\n" +
                                         "We don't want to leave you with a soar throat";
        private const string PitchBaseline = "Make an \"aaaaaah...\" sound with your normal speaking tone";
        private const string PitchLow = "Keep making an \"aaaaaah...\" sound, but now lower your tone as much as you can.\n" +
                                        "Imagine you're Leonard Cohen singing Hallelujah";
        private const string PitchHigh = "Keep making an \"aaaaaah...\" sound,\nbut now gradually increase your tone till you reach a tone that you can't raise elegantly.";
        private const string SecondsLeftMsg = "Seconds left: {0}";
        private const string PressToBegin = "Press to begin";
        private const string PressNextStep = "Press to continue to the next step";
        private const string Finish = "Calibrating complete!";
        private const int InitDelay = 3;

        public Text instructions;
        public Text seconds;
        public Button button;
        private Text buttonText;
        private Calibration calibrator;

        private int secondsLeft;
        private bool hasBegun;
        private Dictionary<CalibrationStage, string> stageText;
        private IEnumerator handleCalibration;

        private void Awake()
        {
            stageText = new Dictionary<CalibrationStage, string>();
            stageText[CalibrationStage.Silence] = Silence;
            stageText[CalibrationStage.Pause] = Pause;
            stageText[CalibrationStage.PitchLow] = PitchLow;
            stageText[CalibrationStage.PitchHigh] = PitchHigh;
            stageText[CalibrationStage.Finished] = Finish;
            stageText[CalibrationStage.PitchBaseLine] = PitchBaseline;
            stageText[CalibrationStage.VolumeBaseLine] = VolumeBaseline;
            stageText[CalibrationStage.VolumeMax] = VolumeMax;
            stageText[CalibrationStage.VolumeMin] = VolumeMin;
        }

        private void Start()
        {
            instructions.text = Welcome;
            seconds.text = "";
            buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = PressToBegin;
            hasBegun = false;
            calibrator = GetComponent<Calibration>();
            handleCalibration = HandleCalibration();
        }

        public void BeginProcess()
        {
            hasBegun = true;
            buttonText.text = PressNextStep;
            button.enabled = false;
        }

        private void FixedUpdate()
        {
            if (hasBegun)
            {
                StartCoroutine(handleCalibration);
            }
        }

        IEnumerator HandleCalibration()
        {
            secondsLeft = InitDelay;
            seconds.text = string.Format(SecondsLeftMsg, secondsLeft.ToString());

            // Wait and inform player
            while (secondsLeft > 0)
            {
                yield return new WaitForSeconds(1);
                secondsLeft--;
                seconds.text = string.Format(SecondsLeftMsg, secondsLeft.ToString());
            }

            // Start calibration process
            calibrator.StartCalibrating();

            yield return null;

            while (!calibrator.IsCalibrationComplete())
            {
                var newStage = stageText[calibrator.GetCurrentStage()];

                // Change text according to step
                if (newStage != instructions.text)
                {
                    instructions.text = newStage;

                    // Count down time somehow?
                }

                yield return null;
            }

            // Save calibrated data to files for current player
            var dataToSave = new Dictionary<string, OffsetsProfile>();
            dataToSave[Helpers.volFileName] = calibrator.VolumeProfile;
            dataToSave[Helpers.pitchFileName] = calibrator.PitchProfile;

            Helpers.SaveFile(PlayerPrefs.GetString(Helpers.playerPrefsKey), dataToSave);

            // Load the first level
            SceneManager.LoadScene("Level 1");
        }
    }
}
