using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using Assets.Scripts.GameScripts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Vehicles.Car;

namespace Assets.Scripts.GameScripts
{
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(Rigidbody))]
    public class VoiceController : MonoBehaviour
    {
        public AudioMeasure MicIn;
        public MeshCollider LevelComplete;
        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;
        private IEnumerator loadMain;
        private bool hasFinished = false;
        private Fading fading;

        private CarController m_Car; // the car controller we want to use


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
            fading = GameObject.Find("Fading").GetComponent<Fading>();
        }

        private void Start()
        {
            // Load player data
            var profiles = Helpers.LoadPlayerProfile(PlayerPrefs.GetString(Helpers.playerPrefsKey));

            PitchOffset = profiles[Helpers.pitchFileName];
            DbOffset = profiles[Helpers.volFileName];
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);

            loadMain = LoadNewLevel();
        }

        public void FixedUpdate()
        {
            // Move object!
            float h = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) : PitchControl.NoData;
            float v = DecibelControl.SoundForce(MicIn.DbValue);

            m_Car.Move(h, v, v, 0f);

            // Start level loader coroutine
            if (hasFinished)
            {
                StartCoroutine(loadMain);
            }
        }

        // Fade out then load
        IEnumerator LoadNewLevel()
        {
            fading.BeginFade(Fading.FadeOut);

            while (!fading.IsComplete())
            {
                yield return null;
            }

            SceneManager.LoadScene(Constants.MainMenu);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // If reached the finish line
            hasFinished = hasFinished || (collision.transform.CompareTag("End"));
        }
    }
}