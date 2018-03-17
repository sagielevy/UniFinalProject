﻿using Assets.Scripts.AudioControl;
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
        public int MaxNumOfMarks = 1000;
        public int TailLength = 100;

        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private SpriteRenderer[] Hits;
        private int HitIndex = 0;
        private float GridWidth, GridHeight;

        private void Awake()
        {
            // DEBUG ONLY! REMOVE AFTERWARDS
            PitchOffset = new OffsetsProfile(107, 175, 75, 0.01f);
            DbOffset = new OffsetsProfile(-8, 5, -20, 0.01f);
        }

        private void Start()
        {
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);
            Hits = new SpriteRenderer[MaxNumOfMarks];

            GridWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            GridHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;

            // Create a single hit and make it a child
            Hits[HitIndex] = Instantiate(MarkHit);
            Hits[HitIndex].transform.parent = transform;
            Hits[HitIndex].gameObject.SetActive(true);
            //HitIndex++;
        }

        private void FixedUpdate()
        {
            // Validate pitch by Db
            float xPos = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) * (GridWidth / 2) : PitchControl.NoData;
            float yPos = DecibelControl.SoundForce(MicIn.DbValue) * (GridHeight / 2);

            // Init new hit
            if (Hits[HitIndex] == null)
            {
                Hits[HitIndex] = Instantiate(MarkHit);
                Hits[HitIndex].transform.parent = transform;
                Hits[HitIndex].gameObject.SetActive(true);
            }

            // Move hit relative to grid size
            Hits[HitIndex].transform.position = new Vector3(xPos,
                                                     yPos, 
                                                     Hits[HitIndex].transform.position.z);

            // Cyclic growth
            //HitIndex = ++HitIndex % MaxNumOfMarks;

            // Update colors for all hits
            //for (int i = 0; i < MaxNumOfMarks; i++)
            //{
            //    if (Hits[i] != null)
            //    {
            //        ChangeHitColor(i);
            //    }
            //}
        }

        //private void ChangeHitColor(int currIndex)
        //{
        //    int distFromHeadClamped = Mathf.Clamp((currIndex <= HitIndex) ? HitIndex - currIndex : MaxNumOfMarks - currIndex + HitIndex, 0, TailLength);
        //    Hits[currIndex].color = Color.Lerp(Color.black, Color.white, distFromHeadClamped / TailLength);
        //}
    }
}