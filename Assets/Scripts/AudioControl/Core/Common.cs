using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AudioControl.Core
{
    public class Common : MonoBehaviour
    {
        private void Awake()
        {
            // Make sure to allow program to work in BG
            // or alternatively stop recording when alt tabbing
            Application.runInBackground = true;
        }
    }
}
