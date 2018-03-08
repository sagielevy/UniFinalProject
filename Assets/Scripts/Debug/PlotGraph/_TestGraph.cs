using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Debug.PlotGraph
{
    public class _TestGraph : MonoBehaviour
    {
        public AudioMeasure MicIn;

        private PitchControl PitchControl;
        private DecibelControl DecibelControl;
        private Offsets PitchOffset, DbOffset;

        private void Awake()
        {
            // DEBUG ONLY! REMOVE AFTERWARDS
            PitchOffset = new Offsets(200, 400, 50, 0.01f);
        }

        void Start()
        {
            Graph.YMin = -5;
            Graph.YMax = +2000;

            Graph.channel[0].isActive = true;
            //Graph.channel[1].isActive = true;
        }


        //void Update()
        //{
        //    Graph.channel[0].Feed(Mathf.Sin(Time.time));
        //}

        void FixedUpdate()
        {
            Graph.channel[0].Feed(MicIn.PitchValue); //Mathf.Sin(Time.time));
        }

    }
}
