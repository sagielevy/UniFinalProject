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
        public int level;
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        public float levelFloorY = -10; // The "ground". The level should reset if the ball has reached this "plane"
        public Vector3 initialPosition;
        private Rigidbody body;
        private Fading fading;
        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;
        private IEnumerator levelLoader;

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            body.maxAngularVelocity = maxVelocity;

            // Load player data
            var profiles = Helpers.LoadPlayerProfile(PlayerPrefs.GetString(Helpers.playerPrefsKey));

            //PitchOffset = new OffsetsProfile(107, 175, 75, 0.01f);
            //DbOffset = new OffsetsProfile(-8, 5, -20, 0.01f);
            PitchOffset = profiles[Helpers.pitchFileName];
            DbOffset = profiles[Helpers.volFileName];
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
            fading = GameObject.Find("Fading").GetComponent<Fading>();
            levelLoader = LoadNewLevel();

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
            var nextLevel = SceneManager.GetSceneByName("Level " + level + 1);
            SceneManager.MoveGameObjectToScene(fading.gameObject, nextLevel);

            while (!fading.IsComplete())
            {
                yield return null;
            }

            SceneManager.LoadScene(nextLevel.name);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // If reached the finish line
            if (collision.transform.CompareTag("Finish"))
            {
                // Start level loader coroutine
                StartCoroutine(levelLoader);
            }
        }
    }
}
