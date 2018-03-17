using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameScripts
{
    public class LevelManager : MonoBehaviour
    {
        public BallAccelerator ball;

        private void FixedUpdate()
        {
            // Restart level if ball falls

        }

        private void LevelRestart()
        {
            // TODO fade out/in too
        }
    }
}
