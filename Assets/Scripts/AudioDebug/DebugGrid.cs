using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioDebug
{
    public class DebugGrid : MonoBehaviour
    {
        public SpriteRenderer MarkHit;
        public AudioMeasure MicIn;

        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private float GridWidth, GridHeight;

        private void Awake()
        {
            //PitchOffset = new OffsetsProfile(107, 175, 75, 0.02f);
            //DbOffset = new OffsetsProfile(-8, 5, -20, 0.1f);
        }

        private void Start()
        {
            //PitchControl = new PitchControl(PitchOffset);
            //DecibelControl = new DecibelControl(DbOffset);

            GridWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            GridHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        }

        private void FixedUpdate()
        {
            // Validate pitch by Db
            //float xPos = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) * (GridWidth / 2) : PitchControl.NoData;
            //float yPos = DecibelControl.SoundForce(MicIn.DbValue) * (GridHeight / 2);

            // Move hit relative to grid size
            MarkHit.transform.position = new Vector3(MicIn.PitchValue * (GridWidth / 2), //xPos,
                                                     MicIn.DbValue * (GridHeight / 2), //yPos,
                                                     MarkHit.transform.position.z);
        }
    }
}
