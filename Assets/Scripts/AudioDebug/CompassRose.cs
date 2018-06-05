using Assets.Scripts.AudioControl;
using Assets.Scripts.AudioControl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AudioDebug
{
    public class CompassRose : MonoBehaviour
    {
        public Image MarkHit;
        public AudioMeasure MicIn;

        private OffsetsProfile PitchOffset, DbOffset;
        private PitchControl PitchControl;
        private DecibelControl DecibelControl;

        private float GridWidth, GridHeight, MarkWidth, MarkHeight;

        private void Awake()
        {
            //PitchOffset = new OffsetsProfile(107, 175, 75, 0.02f);
            //DbOffset = new OffsetsProfile(-8, 5, -20, 0.1f);
        }

        private void Start()
        {
            var profiles = Helpers.LoadPlayerProfile(PlayerPrefs.GetString(Helpers.playerPrefsKey));

            PitchOffset = profiles[Helpers.pitchFileName];
            DbOffset = profiles[Helpers.volFileName];
            PitchControl = new PitchControl(PitchOffset);
            DecibelControl = new DecibelControl(DbOffset);

            GridWidth = GetComponent<Image>().rectTransform.rect.width / 2;
            GridHeight = GetComponent<Image>().rectTransform.rect.height / 2;
            MarkWidth = MarkHit.rectTransform.rect.width / 2;
            MarkHeight = MarkHit.rectTransform.rect.height / 2;

        }

        private void FixedUpdate()
        {
            // Validate pitch by Db
            float xPos = DecibelControl.IsInputValid(MicIn.DbValue) ? PitchControl.SoundForce(MicIn.PitchValue) * (GridWidth - MarkWidth) : PitchControl.NoData;
            float yPos = DecibelControl.SoundForce(MicIn.DbValue) * (GridHeight - MarkHeight);

            // Move hit relative to grid size
            // Interpolate between new and current position to avoid jumping of mark
            MarkHit.rectTransform.localPosition = Vector3.Lerp(MarkHit.rectTransform.localPosition,
                                                    new Vector3(xPos, yPos, MarkHit.rectTransform.localPosition.z), Time.deltaTime);
        }
    }
}
