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
        //MicrophoneInput micIn;
        AudioMeasure micIn;

        public Text debug;

        // Use this for initialization
        void Start()
        {
            if (audioInputObject == null)
                audioInputObject = GameObject.Find("MicMonitor");
            //micIn = (MicrophoneInput)audioInputObject.GetComponent("MicrophoneInput");
            micIn = audioInputObject.GetComponent<AudioMeasure>();

            debug.text = "";
        }

        // Update is called once per frame
        void Update()
        {
            //int rms = (int)micIn.RmsValue;
            //int db = (int)micIn.DbValue;
            //int pitch = (int)micIn.PitchValue;
            //int f = (int)micIn.GetFundamentalFrequency(); // Get the frequency from our MicrophoneInput script
            //int v = (int)micIn.GetAveragedVolume();

            //if (f >= 261 && f <= 262) // Compare the frequency to known value, take possible rounding error in to account
            //{
            //    debug.text = "Middle-C played!";
            //}
            //else
            //{
            //    debug.text = "Play another note...";
            //}

            debug.text = "Pitch: " + micIn.PitchValue + "\nMel: " + Helpers.HertzToMel(micIn.PitchValue) + "\nRms: " + micIn.RmsValue + "\ndB: " + micIn.DbValue;
        }
    }
}
