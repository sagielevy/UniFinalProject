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
    public class BallAccelerator : MonoBehaviour
    {
        public AudioMeasure MicIn;
        public float forceScale = 1.0f;
        public float maxVelocity = 6.0f;
        private Rigidbody rigidbody;
        private Offsets PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = maxVelocity;

            // DEBUG ONLY! REMOVE AFTERWARDS - GET DATA FROM CALLIBRATOR
            PitchOffset = new Offsets(107, 175, 75, 0.01f);
            DbOffset = new Offsets(-8, 5, -20, 0.01f);
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
        }

        public void FixedUpdate()
        {
            float xAcc = DecibelControl.isInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) : PitchControl.NoData;
            float zAcc = DecibelControl.SoundForce(MicIn.DbValue);

            rigidbody.AddForce(new Vector3(xAcc * forceScale, 0, zAcc * forceScale));
        }
    }
}
