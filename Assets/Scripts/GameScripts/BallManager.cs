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

namespace Assets.Scripts.GameScripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallManager : MonoBehaviour
    {
        public int currLevelIndex = 0;
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        public float levelFloorY = -10; // The "ground". The level should reset if the ball has reached this "plane"
        public float drag = 0;
        public float angularDrag = 0.05f;
        public Vector3 initialPosition;
        private Rigidbody body;
        private Fading fading;
        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;
        private IEnumerator levelLoader;
        private bool startLoadingNewLevel = false;

        private void Start()
        {
            // Change rigidbody values as set
            body = GetComponent<Rigidbody>();
            body.maxAngularVelocity = maxVelocity;
            body.drag = drag;
            body.angularDrag = angularDrag;

            // Load player data
            var profiles = Helpers.LoadPlayerProfile(PlayerPrefs.GetString(Helpers.playerPrefsKey));

            PitchOffset = profiles[Helpers.pitchFileName];
            DbOffset = profiles[Helpers.volFileName];
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
            levelLoader = LoadNewLevel();
            fading = GameObject.Find("Fading").GetComponent<Fading>();

            // Fade in if necessary
            fading.BeginFade(Fading.FadeIn);

            // TODO Set max velocity and forceScale according to currnet level and difficulty
        }

        public void FixedUpdate()
        {
            // Move ball!
            float xAcc = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) : PitchControl.NoData;
            float zAcc = DecibelControl.SoundForce(MicIn.DbValue);

            body.AddForce(new Vector3(xAcc * forceScale, 0, zAcc * forceScale));

            // Check if to reset level
            if (transform.position.y < levelFloorY)
            {
                LevelRestart();
            }

            // Start level loader coroutine
            if (startLoadingNewLevel)
            {
                StartCoroutine(levelLoader);
            }
        }

        private void LevelRestart()
        {
            // Reset ball position and velocity
            body.position = initialPosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        // Fade out then load
        IEnumerator LoadNewLevel()
        {
            fading.BeginFade(Fading.FadeOut);

            while (!fading.IsComplete())
            {
                yield return null;
            }

            var nextLevel = currLevelIndex + 1 + Constants.FirstLevelSceneBuildIndex;

            // Load next level. Make sure it exists?
            if (SceneManager.sceneCountInBuildSettings > nextLevel)
            {
                SceneManager.LoadScene(nextLevel);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // If reached the finish line
            startLoadingNewLevel = startLoadingNewLevel || (collision.transform.CompareTag("Finish"));
        }
    }
}