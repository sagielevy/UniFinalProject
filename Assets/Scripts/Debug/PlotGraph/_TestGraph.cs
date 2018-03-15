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
        private OffsetsProfile PitchOffset, DbOffset;

        private void Awake()
        {
            // DEBUG ONLY! REMOVE AFTERWARDS
            PitchOffset = new OffsetsProfile(130, 175, 75, 0.01f);
            DbOffset = new OffsetsProfile(-8, 5, -20, 0.01f);
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
        }

        
        void Start()
        {
            Graph.YMin = -1;
            Graph.YMax = +1;

            Graph.channel[0].isActive = true;
            Graph.channel[1].isActive = true;
        }

        void FixedUpdate()
        {
            var melValRange = DecibelControl.isInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) : PitchControl.NoData;
            var dbValRange = DecibelControl.SoundForce(MicIn.DbValue);
            Graph.channel[0].Feed(Graph.YMax - melValRange);
            Graph.channel[1].Feed(Graph.YMax - dbValRange);
        }
    }
}
