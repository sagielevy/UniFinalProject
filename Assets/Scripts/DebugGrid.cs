using Assets.Scripts.AudioControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class DebugGrid : MonoBehaviour
    {
        public SpriteRenderer MarkHit;
        public AudioMeasure MicIn;
        public int MaxNumOfMarks = 1000;

        private Offsets PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private SpriteRenderer[] Hits;
        private int HitIndex = 0;
        private float GridWidth, GridHeight;

        private void Awake()
        {
            // DEBUG ONLY! REMOVE AFTERWARDS
            PitchOffset = new Offsets(200, 400, 50, 0.01f);
        }

        private void Start()
        {
            PitchControl = new PitchControl(PitchOffset);
            //DecibelControl = new DecibelControl(DbOffset);
            Hits = new SpriteRenderer[MaxNumOfMarks];

            GridWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            GridHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;

            // TEMP FOR DEBUG
            // Create a single hit and make it a child
            Hits[0] = Instantiate(MarkHit);
            Hits[0].transform.parent = transform;
            Hits[0].gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            float xPos = PitchControl.SoundForce(MicIn.PitchValue) * (GridWidth / 2);
            //float yPos = DecibelControl.SoundForce(MicIn.DbValue) * (GridHeight / 2);

            // Move hit relative to grid size
            Hits[0].transform.position = new Vector3(xPos,
                                                     Hits[0].transform.position.y, 
                                                     Hits[0].transform.position.z);
        }

        private void ChangeHitColor(int index)
        {
            //hits[index].color = Color.Lerp(Color.black, Color.white, hitIndex + count cyclic.. / MaxNumOfMarks);

        }
    }
}
