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
    public class MainMenu : MonoBehaviour
    {
        public InputField playerNameInput;
        public Button StartNewGame;

        private void Start()
        {
            StartNewGame.enabled = false;
        }

        public void EnableStartNewGameOnPlayerNameInput()
        {
            StartNewGame.enabled = playerNameInput.text.Length > 0;
        }

        public void OnStartNewGame()
        {
            PlayerPrefs.SetString(Helpers.playerPrefsKey, playerNameInput.text);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Calibrate");
        }
    }
}
