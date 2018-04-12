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
        private const string Welcome = "Start calibrating by pressing the green button.";
        private const string Silence_Start = "First we need to record the level of noise in your area.\n" +
                                             "Please make sure it's as quite as possible around you and when you're ready to record, press the record button.";
        private const string Silence_Fail = "It seems that it is too loud around you.\n" +
                                            "Please make sure it's as quite as possible around you and when you're ready to record, press the record button.";
        private const string VolumeBaseline_Start = "Now we need to know how your unique voice works.\n" +
                                                    "The first aspect of your voice is your volume.\n" +
                                                    "When you're ready to begin, press the record button and\n" +
                                                    "make a sound with your normal speaking volume for a few seconds, like this: 'aaaaaaah'...";
        private const string VolumeBaseline_Fail = "It seems that your volume was to soft, your microphone was too far away, its sensitivity was too low\n" +
                                                   "or your area is too noisy. Try again or re-record area silence.\n" +
                                                   "Press the record button and make a sound with your normal speaking volume for a few seconds, like this: 'aaaaaaah'...";
        private const string VolumeMin_Start = "Great! Now for some fun.. Make a soft sound to move the ball backwards.\n" +
                                               "Try being as quite as you can, like so: 'aaaaaaah'...";
        private const string VolumeMin_Fail = "It seems you're having difficulties with this step. Try speaking farther from your microphone or\n" +
                                              "restart this step altogether.";
        private const string VolumeMin_Command = "Now move backwards!";
        private const string VolumeMax_Start = "Perfect. Now let's try moving in the other direction. Make a loud sound to move the ball forwards but don't go overboard and hurt your throat.\n" +
                                               "Say something like this: 'aaaaaaah'...";
        private const string VolumeMax_Fail = "It seems you're having difficulties with this step. Try speaking closer to your microphone, increase sensitivity or\n" +
                                              "restart this step and reset your normal volume.";
        private const string VolumeMax_Command = "Now move fowards!";
        private const string PitchBaseline_Start = "The other aspect of your voice is your tone. You can make sounds in a low tone like so: 'aaaaaaah', or a high tone: 'aaaaaaah'.\n" +
                                                   "But first we need your normal speaking tone. Try something like this: 'aaaaaaah'...";
        private const string PitchBaseline_Fail = "It seems you were too quite. Try speaking closer to your microphone.\n" +
                                                  "Make a sound with your normal speaking tone, like this: 'aaaaaaah'...";
        private const string PitchLow_Start = "Awesome. Make a low tone 'aaaaaaah' sound to move the ball left. Try it.";
        private const string PitchLow_Fail = "It seems you're having difficulties with this step. Try speaking closer to your microphone, lower your tone even more,\n" +
                                             "or restart this step and reset your normal tone. Make a low tone 'aaaaaaah' sound to move the ball left.";
        private const string PitchLow_Command = "Now move left!";
        private const string PitchHigh_Start = "OK now make a high tone 'aaaaaaah' to move the ball right. Go ahead.";
        private const string PitchHigh_Fail = "It seems you're having difficulties with this step. Try speaking closer to your microphone, increase your tone even more,\n" +
                                              "or restart this step altogether. Make a high tone 'aaaaaaah' sound to move the ball right.";
        private const string PitchHigh_Command = "Now move right!";
        private const string PressToBegin = "Press to begin";
        private const string PressStartStep = "Press to start this step";
        private const string PressDisabled = "Step processing...";
        private const string Finish = "Calibrating complete!";

        public Text instructions;
        public Text skipInstructions;
        public Button nextStageBtn;
        public ProgressBarBehaviour progressBar;
        public Image panel;
        public CalibrationInsturcted calibrator;
        public BallManagerTutorial ballManager;
        public AudioSource audioOutput;
        public float fadeSpeed = 0.05f;
        public GameObject boardTop, boardBottom, boardLeft, boardRight;
        private Text buttonText;

        private Color orgColor;
        private bool hasBegun;
        private bool hasChangedStageOffsets;
        private Dictionary<CalibrationStage, StepData> stages;
        private IEnumerator handleCalibration;
        private float startTime;
        private CalibrationStage currentStage = CalibrationStage.Silence;

        private void Awake()
        {
            stages = new Dictionary<CalibrationStage, StepData>();
            stages[CalibrationStage.Silence] = new StepData(Silence_Start, Silence_Fail, Resources.Load<AudioClip>("Instructions/silence_start"), Resources.Load<AudioClip>("Instructions/silence_fail"), CalibrationStage.Silence, null, true, true, null);
            stages[CalibrationStage.PitchLow] = new StepData(PitchLow_Start, PitchLow_Fail, Resources.Load<AudioClip>("Instructions/pitchlow_start"), Resources.Load<AudioClip>("Instructions/pitchlow_fail"), CalibrationStage.PitchLow, Resources.Load<AudioClip>("Instructions/pitchlow_cmd"), false, false,
                (step) =>
                {
                    // Play command instruction and set correct board
                    audioOutput.clip = step.onSamplingComplete;
                    audioOutput.Play();
                    instructions.text = PitchLow_Command;
                    ballManager.DbOffset = new OffsetsProfile(float.NaN, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(calibrator.pitchBaseLineValue, float.PositiveInfinity, calibrator.pitchLowValue, Constants.DefaultPitchBaselineThreshold);
                });
            stages[CalibrationStage.PitchHigh] = new StepData(PitchHigh_Start, PitchHigh_Fail, Resources.Load<AudioClip>("Instructions/pitchhigh_start"), Resources.Load<AudioClip>("Instructions/pitchhigh_fail"), CalibrationStage.PitchHigh, Resources.Load<AudioClip>("Instructions/pitchhigh_cmd"), false, false,
                (step) =>
                {
                    audioOutput.clip = step.onSamplingComplete;
                    audioOutput.Play();
                    instructions.text = PitchHigh_Command;
                    ballManager.DbOffset = new OffsetsProfile(float.NaN, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile(calibrator.pitchBaseLineValue, calibrator.pitchHighValue, float.NegativeInfinity, Constants.DefaultPitchBaselineThreshold);
                });
            stages[CalibrationStage.Finished] = new StepData(Finish, null, Resources.Load<AudioClip>("Instructions/Mono"), null, CalibrationStage.Finished, null, false, true, null);
            stages[CalibrationStage.PitchBaseLine] = new StepData(PitchBaseline_Start, PitchBaseline_Fail, Resources.Load<AudioClip>("Instructions/pitchbase_start"), Resources.Load<AudioClip>("Instructions/pitchbase_fail"), CalibrationStage.PitchBaseLine, null, true, true, null);
            stages[CalibrationStage.VolumeBaseLine] = new StepData(VolumeBaseline_Start, VolumeBaseline_Fail, Resources.Load<AudioClip>("Instructions/volbase_start"), Resources.Load<AudioClip>("Instructions/volbase_fail"), CalibrationStage.VolumeBaseLine, null, true, true, null);
            stages[CalibrationStage.VolumeMax] = new StepData(VolumeMax_Start, VolumeMax_Fail, Resources.Load<AudioClip>("Instructions/volmax_start"), Resources.Load<AudioClip>("Instructions/volmax_fail"), CalibrationStage.VolumeMax, Resources.Load<AudioClip>("Instructions/volmax_cmd"), false, false,
                (step) =>
                {
                    audioOutput.clip = step.onSamplingComplete;
                    audioOutput.Play();
                    instructions.text = VolumeMax_Command;
                    ballManager.DbOffset = new OffsetsProfile(calibrator.volumeBaseLineValue, calibrator.volumeMaxValue, float.NegativeInfinity, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile();
                });
            stages[CalibrationStage.VolumeMin] = new StepData(VolumeMin_Start, VolumeMin_Fail, Resources.Load<AudioClip>("Instructions/volmin_start"), Resources.Load<AudioClip>("Instructions/volmin_fail"), CalibrationStage.VolumeMin, Resources.Load<AudioClip>("Instructions/volmin_cmd"), false, false,
                (step) =>
                {
                    audioOutput.clip = step.onSamplingComplete;
                    audioOutput.Play();
                    instructions.text = VolumeMin_Command;
                    ballManager.DbOffset = new OffsetsProfile(calibrator.volumeBaseLineValue, float.PositiveInfinity, calibrator.volumeMinValue, Constants.DefaultVolumeBaselineThreshold);
                    ballManager.PitchOffset = new OffsetsProfile();
                });
        }

        private void ChangeBoard(GameObject toActivate)
        {
            boardTop.SetActive(false);
            boardLeft.SetActive(false);
            boardRight.SetActive(false);
            boardBottom.SetActive(false);
            toActivate.SetActive(true);
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
            // Show skip instruction
            skipInstructions.gameObject.SetActive(true);

            yield return new WaitUntil(() => !sound.isPlaying);

            // Hide skip instruction
            skipInstructions.gameObject.SetActive(false);

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
                    // For first time use, activate bar
                    progressBar.gameObject.SetActive(true);

                    // Update bar according to time since start
                    progressBar.SetFillerSizeAsPercentage(calibrator.GetCurrentTimePercent);
                }
                else
                {
                    // For first time use, activate bar
                    progressBar.gameObject.SetActive(true);

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
                        newStage.createOffsets(newStage);
                    }
                }

                // Stage change or error recieved!
                if (newStage.stage != currentStage || (inputError = calibrator.IsInputStageInvalid()))
                {
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
                    }
                    else
                    {
                        // Play correct audio
                        audioOutput.clip = newStage.instruction;
                        audioOutput.Play();

                        // Change text according to step
                        instructions.text = newStage.text;

                        // Change stage
                        currentStage = newStage.stage;

                        // Change board according to direction
                        switch (newStage.stage)
                        {
                            case CalibrationStage.VolumeMin:
                                ChangeBoard(boardBottom);
                                break;
                            case CalibrationStage.VolumeMax:
                                ChangeBoard(boardTop);
                                break;
                            case CalibrationStage.PitchHigh:
                                ChangeBoard(boardRight);
                                break;
                            case CalibrationStage.PitchLow:
                                ChangeBoard(boardLeft);
                                break;
                        }
                        
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

        private void OnGUI()
        {
            // Pressed enter and name input is in focus
            if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) &&
                nextStageBtn.interactable)
            {
                nextStageBtn.onClick.Invoke();
            }

            // Skip instruction if one is being given
            if (Input.GetKeyDown(KeyCode.Space) && audioOutput.isPlaying)
            {
                audioOutput.Stop();
            }
        }
    }

    class StepData
    {
        public readonly string text;
        public readonly string onErrorText;
        public readonly AudioClip instruction;
        public readonly AudioClip onErrorInstruction;
        public readonly AudioClip onSamplingComplete;
        public readonly bool isTimeBasedSample;
        public readonly bool isBlackScreen;
        public readonly CalibrationStage stage;
        public readonly Action<StepData> createOffsets;

        public StepData(string text, string onErrorText, AudioClip instruction, AudioClip onErrorInstruction, CalibrationStage stage,
            AudioClip onSamplingComplete, bool isTimeBasedSample, bool isBlackScreen, Action<StepData> createOffsets)
        {
            this.stage = stage;
            this.text = text;
            this.onErrorText = onErrorText;
            this.instruction = instruction;
            this.onErrorInstruction = onErrorInstruction;
            this.onSamplingComplete = onSamplingComplete;
            this.isTimeBasedSample = isTimeBasedSample;
            this.isBlackScreen = isBlackScreen;
            this.createOffsets = createOffsets;
        }
    }
}