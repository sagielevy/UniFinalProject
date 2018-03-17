using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallManager : MonoBehaviour
    {
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        public float levelFloorY = -10; // The "ground". The level should reset if the ball has reached this "plane"
        public Vector3 initialPosition;
        private Rigidbody body;
        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

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
            // TODO fade out/in too

            // Reset ball position and velocity
            body.position = initialPosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
