﻿using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.CalibrationTypes;
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
    public class MainMenu : MonoBehaviour
    {
        public InputField playerNameInput;
        public Button StartNewGame;
        public RectTransform ScrollViewContent;

        // For debug, use different calibration.
        Dictionary<int, Type> CalibrationTypes;

        private void Start()
        {
            StartNewGame.interactable = false;
            LoadPlayerNames();

            // Set calibration types
            CalibrationTypes = new Dictionary<int, Type>();
            CalibrationTypes[0] = typeof(CalibrationStyleMiddleFirst);
            CalibrationTypes[1] = typeof(CalibrationStyleLowFirst);
            DataBetweenScenes.calibration = CalibrationTypes[0]; // Default val
        }

        public void LoadPlayerNames()
        {
            var players = Helpers.LoadPlayers();

            for (int i = 0; i < players.Length; i++)
            {
                var newBtn = Instantiate(StartNewGame);
                newBtn.transform.SetParent(ScrollViewContent, false);
                newBtn.interactable = true;
                newBtn.GetComponentInChildren<Text>().text = players[i];
                newBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, (int)Math.Pow(-1, i) * 
                    ((i + 1) / 2) * newBtn.GetComponent<RectTransform>().rect.height, 0);

                var playerName = players[i];
                newBtn.onClick.AddListener(() => { SelectPlayer(playerName); });
            }
        }

        private void SelectPlayer(string playerName)
        {
            // Set current player 
            PlayerPrefs.SetString(Helpers.playerPrefsKey, playerName);
            PlayerPrefs.Save();

            SceneManager.LoadScene(Constants.FirstLevelSceneBuildIndex);
        }

        public void EnableStartNewGameOnPlayerNameInput()
        {
            StartNewGame.interactable = playerNameInput.text.Length > 0;
        }

        public void OnStartNewGame()
        {
            DataBetweenScenes.PlayerNameInput = playerNameInput.text;
            SceneManager.LoadScene("Calibrate");
        }

        public void OnCalibrationChaned(int value)
        {
            DataBetweenScenes.calibration = CalibrationTypes[value];
        }
    }
}
