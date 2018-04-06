using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.CalibrationTypes;
using Assets.Scripts.AudioControl.Core;
using ProgressBar;
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
    public class CalibrateUI : MonoBehaviour
    {
        private const string Welcome = "Before we begin we need to calibrate our systems to meet your vocal capabilities.\nPlease keep a constant distance from your microphone and lock its gain";
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
        private const string PressStartStep = "Press to start this step";
        private const string PressDisabled = "Step processing...";
        private const string Finish = "Calibrating complete!";
        private const string ErrorBadInput = "Bad input while calibrating... Please try again.\nPay close attention to the instructions and try avoiding noisy areas";

        public Text instructions;
        //public Text ErrorMsg;
        public Button nextStageBtn;
        public ProgressBarBehaviour progressBar;
        public Image panel;
        public CalibrationInsturcted calibrator;
        public BallManagerTutorial ballManager;
        public AudioSource audioOutput;
        public float fadeSpeed = 0.05f;
        private Text buttonText;

        private Color orgColor;
        private bool hasBegun;
        private bool hasChangedStageOffsets;
        private Dictionary<CalibrationStage, StepData> stages;
        private IEnumerator handleCalibration;
        private float startTime;

        private void Awake()
        {
            stages = new Dictionary<CalibrationStage, StepData>();
            stages[CalibrationStage.Silence] = new StepData(Silence, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), true, true, null);
            stages[CalibrationStage.PitchLow] = new StepData(PitchLow, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), false, false,
                () => { ballManager.DbOffset = new OffsetsProfile(float.NaN, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(calibrator.pitchBaseLineValue, float.PositiveInfinity, calibrator.pitchLowValue, Constants.DefaultPitchBaselineThreshold); });
            stages[CalibrationStage.PitchHigh] = new StepData(PitchHigh, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), false, false,
                () => { ballManager.DbOffset = new OffsetsProfile(float.NaN, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(calibrator.pitchBaseLineValue, calibrator.pitchHighValue, float.NegativeInfinity, Constants.DefaultPitchBaselineThreshold); }); 
            stages[CalibrationStage.Finished] = new StepData(Finish, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), false, true, null);
            stages[CalibrationStage.PitchBaseLine] = new StepData(PitchBaseline, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), true, true, null);
            stages[CalibrationStage.VolumeBaseLine] = new StepData(VolumeBaseline, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), true, true, null);
            stages[CalibrationStage.VolumeMax] = new StepData(VolumeMax, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), false, false,
                () => { ballManager.DbOffset = new OffsetsProfile(calibrator.volumeBaseLineValue, calibrator.volumeMaxValue, float.NegativeInfinity, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(); });
            stages[CalibrationStage.VolumeMin] = new StepData(VolumeMin, null, Resources.Load<AudioClip>("test/Mono"), Resources.Load<AudioClip>("test/Mono"), false, false,
                () => { ballManager.DbOffset = new OffsetsProfile(calibrator.volumeBaseLineValue, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(); });
        }

        private void Start()
        {
            instructions.text = Welcome;
            buttonText = nextStageBtn.GetComponentInChildren<Text>();
            buttonText.text = PressToBegin;
            hasBegun = false;

            handleCalibration = HandleCalibration();

            // Init onclick event
            nextStageBtn.onClick.AddListener(() => { BeginProcess(); });
        }

        public void BeginProcess()
        {
            // Change to first step
            buttonText.text = PressStartStep;
            instructions.text = stages[calibrator.GetCurrentStage()].text;
            audioOutput.clip = stages[calibrator.GetCurrentStage()].instruction;
            audioOutput.Play();
            nextStageBtn.onClick.RemoveAllListeners();
            nextStageBtn.onClick.AddListener(() => { ContinueProcess(); });
            nextStageBtn.interactable = false;
            StartCoroutine(WaitForSound(audioOutput));
        }

        private IEnumerator WaitForSound(AudioSource sound)
        {
            yield return new WaitUntil(() => !sound.isPlaying);

            // Finally enable button
            nextStageBtn.interactable = true;
        }

        public void ContinueProcess()
        {
            hasBegun = true;
            calibrator.ContinueCalibrating();
            nextStageBtn.interactable = false;
            buttonText.text = PressDisabled;
        }

        public void RestartCalibrationUI()
        {
            calibrator.RestartCalibrationProcess();
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
            while (!calibrator.IsCalibrationComplete())
            {
                var newStage = stages[calibrator.GetCurrentStage()];
                bool inputError = false;

                if (newStage.isTimeBasedSample)
                {
                    // Update bar according to time since start
                    progressBar.SetFillerSizeAsPercentage(calibrator.GetCurrentTimePercent);
                }
                else
                {
                    // Update bar according to number of good samples taken / max samples
                    progressBar.SetFillerSizeAsPercentage(calibrator.GetCurrentSamplePercent);
                }

                // Change offsets to allow certain ball movement
                if (calibrator.CompletedRequiredSamples && !hasChangedStageOffsets)
                {
                    hasChangedStageOffsets = true;

                    // Create offsets if closure exists
                    if (newStage.createOffsets != null)
                    {
                        newStage.createOffsets();
                    }
                }

                // Stage change or error recieved!
                if (newStage.text != instructions.text || (inputError = calibrator.IsInputStageInvalid()))
                {
                    // Clear error message if exists
                    //ErrorMsg.text = "";

                    // TODO add reset buttons and show them when necessary. Also manage their flow & shit
                    startTime = Time.fixedTime;
                    orgColor = panel.color;

                    // Init progress bar
                    progressBar.SetFillerSizeAsPercentage(0);

                    // Restart offsets to nullify them
                    ballManager.DbOffset = new OffsetsProfile();
                    ballManager.PitchOffset = new OffsetsProfile();
                    hasChangedStageOffsets = false;

                    // There was a bad input previously
                    if (inputError)
                    {
                        // Play error audio
                        audioOutput.clip = newStage.onErrorInstruction;
                        audioOutput.Play();

                        // Change text according to step
                        instructions.text = newStage.onErrorText;
                        //ErrorMsg.text = ErrorBadInput;
                    }
                    else
                    {
                        // Play correct audio
                        audioOutput.clip = newStage.instruction;
                        audioOutput.Play();

                        // Change text according to step
                        instructions.text = newStage.text;
                    }

                    // Enable button for the player to continue when instruction is complete
                    StartCoroutine(WaitForSound(audioOutput));

                    // Change button text
                    buttonText.text = PressStartStep;
                }

                // Change fader correctly
                if (newStage.isBlackScreen)
                {
                    panel.color = Color.Lerp(orgColor, new Color(panel.color.r, panel.color.g, panel.color.b, 1),
                        (Time.fixedTime - startTime) * fadeSpeed);
                }
                else
                {
                    panel.color = Color.Lerp(orgColor, new Color(panel.color.r, panel.color.g, panel.color.b, 0),
                        (Time.fixedTime - startTime) * fadeSpeed);
                }

                yield return null;
            }

            // Save calibrated data to files for current player
            var dataToSave = new Dictionary<string, OffsetsProfile>();
            dataToSave[Helpers.volFileName] = calibrator.VolumeProfile;
            dataToSave[Helpers.pitchFileName] = calibrator.PitchProfile;

            Helpers.SavePlayerProfile(DataBetweenScenes.PlayerNameInput, dataToSave);

            // Set as curr player and add to list
            PlayerPrefs.SetString(Helpers.playerPrefsKey, DataBetweenScenes.PlayerNameInput);
            PlayerPrefs.Save();

            // Load the first level
            SceneManager.LoadScene(Constants.FirstLevelSceneBuildIndex);
        }
    }

    class StepData
    {
        public readonly string text;
        public readonly string onErrorText;
        public readonly AudioClip instruction;
        public readonly AudioClip onErrorInstruction;
        public readonly bool isTimeBasedSample;
        public readonly bool isBlackScreen;
        public readonly Action createOffsets;

        public StepData(string text, string onErrorText, AudioClip instruction, AudioClip onErrorInstruction,
            bool isTimeBasedSample, bool isBlackScreen, Action createOffsets)
        {
            this.text = text;
            this.onErrorText = onErrorText;
            this.instruction = instruction;
            this.onErrorInstruction = onErrorInstruction;
            this.isTimeBasedSample = isTimeBasedSample;
            this.isBlackScreen = isBlackScreen;
            this.createOffsets = createOffsets;
        }
    }
}