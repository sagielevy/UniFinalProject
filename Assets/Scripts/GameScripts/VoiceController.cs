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
        public int currLevelIndex = 0;
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        public float levelFloorY = -10; // The "ground". The level should reset if the ball has reached this "plane"
        public float drag = 0;
        public float angularDrag = 0.05f;
        public Vector3 initialPosition;
        private Rigidbody body;
        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;
        private IEnumerator levelLoader;
        private bool startLoadingNewLevel = false;

        private CarController m_Car; // the car controller we want to use


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }

        private void Start()
        {
            // Change rigidbody values as set
            //body = GetComponent<Rigidbody>();
            //body.maxAngularVelocity = maxVelocity;
            //body.drag = drag;
            //body.angularDrag = angularDrag;

            // Load player data
            var profiles = Helpers.LoadPlayerProfile(PlayerPrefs.GetString(Helpers.playerPrefsKey));

            PitchOffset = profiles[Helpers.pitchFileName];
            DbOffset = profiles[Helpers.volFileName];
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
        }

        public void FixedUpdate()
        {
            // Move object!
            float h = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) : PitchControl.NoData;
            float v = DecibelControl.SoundForce(MicIn.DbValue);

            //body.AddForce(new Vector3(xAcc * forceScale, 0, zAcc * forceScale));

            m_Car.Move(h, v, v, 0f);

            // Check if to reset level
            //if (transform.position.y < levelFloorY)
            //{
            //    LevelRestart();
            //}

            // Start level loader coroutine
            //if (startLoadingNewLevel)
            //{
            //    StartCoroutine(levelLoader);
            //}
        }

        private void LevelRestart()
        {
            // Reset ball position and velocity
            body.position = initialPosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}