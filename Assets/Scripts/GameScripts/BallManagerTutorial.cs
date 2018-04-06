using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallManagerTutorial : MonoBehaviour
    {
        public int currLevelIndex = 0;
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        public float levelFloorY = -5; // The "ground". The level should reset if the ball has reached this "plane"
        public float drag = 0;
        public float angularDrag = 0.05f;
        public Vector3 initialPosition;
        public OffsetsProfile PitchOffset { get { return pitchOffset; }
            set { pitchOffset = value; PitchControl = new PitchControl(pitchOffset); LevelRestart(); } }
        public OffsetsProfile DbOffset { get { return dbOffset; }
            set { dbOffset = value; DecibelControl = new DecibelControl(dbOffset); LevelRestart(); } }
        private OffsetsProfile pitchOffset, dbOffset;
        private Rigidbody body;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private void Start()
        {
            // Change rigidbody values as set
            body = GetComponent<Rigidbody>();
            body.maxAngularVelocity = maxVelocity;
            body.drag = drag;
            body.angularDrag = angularDrag;

            // Default values - non movable ball
            PitchControl = new PitchControl(new OffsetsProfile());
            DecibelControl = new DecibelControl(new OffsetsProfile());
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
    }
}