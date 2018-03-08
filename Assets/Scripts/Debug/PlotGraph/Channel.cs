using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Debug.PlotGraph
{
    public class Channel
    {
        public float[] _data = new float[Graph.MAX_HISTORY];
        public Color _color = Color.white;
        public bool isActive = false;

        public Channel(Color _C)
        {
            _color = _C;
        }

        public void Feed(float val)
        {
            for (int i = Graph.MAX_HISTORY - 1; i >= 1; i--)
                _data[i] = _data[i - 1];

            _data[0] = val;
        }
    }

    public class Graph
    {
        public static float YMin = -1, YMax = +1;

        public const int MAX_HISTORY = 1024;
        public const int MAX_CHANNELS = 3;

        public static Channel[] channel = new Channel[MAX_CHANNELS];

        static Graph()
        {
            Graph.channel[0] = new Channel(Color.red);
            Graph.channel[1] = new Channel(Color.green);
            Graph.channel[2] = new Channel(Color.blue);
        }

    }
}
