using Assets.Scripts.AudioControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
        private const int InitDelay = 3;

        public Text instructions;
        public Text seconds;
        public Button button;
        private Text buttonText;
        private Calibration calibrator;

        private int secondsLeft;
        private bool hasBegun;
        private Dictionary<CalibrationStage, string> stageText;

        private void Awake()
        {
            stageText = new Dictionary<CalibrationStage, string>();

        }

        private void Start()
        {
            instructions.text = Welcome;
            seconds.text = "";
            buttonText = button.GetComponentInChildren<Text>();
            buttonText.text = PressToBegin;
            hasBegun = false;
            calibrator = GetComponent<Calibration>();
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
                StartCoroutine(WaitAndStart());
            }
        }

        IEnumerator WaitAndStart()
        {
            secondsLeft = InitDelay;
            seconds.text = string.Format(SecondsLeftMsg, secondsLeft.ToString());

            // Wait and inform player
            do
            {
                secondsLeft--;
                yield return new WaitForSeconds(1);
            } while (secondsLeft > 0);

            // Start calibration process
            calibrator.StartCalibrating();

            yield return null;

            while (!calibrator.IsCalibrationComplete())
            {
                // Change text according to step
                instructions.text = stageText[calibrator.GetCurrentStage()];

                yield return null;
            }

            // Save calibrated data to files. Set current user to player prefs
        }
    }
}
