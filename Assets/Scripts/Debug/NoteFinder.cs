using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;
    using Assets.Scripts.AudioControl;
    using Assets.Scripts.AudioControl.Core;

    public class NoteFinder : MonoBehaviour
    {
        public GameObject audioInputObject;
        public float threshold = 1.0f;
        AudioMeasure micIn;

        public Text debug;

        // Use this for initialization
        void Start()
        {
            if (audioInputObject == null)
                audioInputObject = GameObject.Find("MicMonitor");

            micIn = audioInputObject.GetComponent<AudioMeasure>();

            debug.text = "";
        }

        void FixedUpdate()
        {
            debug.text = "Pitch: " + micIn.PitchValue + "\nMel: " + Helpers.HertzToMel(micIn.PitchValue) + "\nRms: " + micIn.RmsValue + "\ndB: " + micIn.DbValue;
        }
    }
}
