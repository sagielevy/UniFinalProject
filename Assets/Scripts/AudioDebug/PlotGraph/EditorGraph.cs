#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Assets.Scripts.AudioDebug.PlotGraph
{
    public class EditorGraph : EditorWindow
    {

        [MenuItem("Window/Graph")]
        static void ShowGraph()
        {
            EditorWindow.GetWindow<EditorGraph>();
        }

        public Material lineMaterial;

        void OnEnable()
        {
            EditorApplication.update += MyDelegate;
        }

        void OnDisable()
        {
            EditorApplication.update -= MyDelegate;
        }

        void MyDelegate()
        {
            Repaint();
        }

        void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (Graph.channel[0] == null)
                return;

            //DrawSquare( );

            int W = (int)this.position.width;
            int H = (int)this.position.height;

            CreateLineMaterial();
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            GL.Begin(GL.LINES);

            for (int chan = 0; chan < Graph.MAX_CHANNELS; chan++)
            {
                Channel C = Graph.channel[chan];

                if (C == null)
                    UnityEngine.Debug.Log("FOO:" + chan);

                if (!C.isActive)
                    continue;

                GL.Color(C._color);

                for (int h = 0; h < Graph.MAX_HISTORY; h++)
                {
                    int xPix = (W - 1) - h;

                    if (xPix >= 0)
                    {
                        float y = C._data[h];

                        float y_01 = Mathf.InverseLerp(Graph.YMin, Graph.YMax, y);

                        int yPix = (int)(y_01 * H);

                        Plot(xPix, yPix);
                    }
                }
            }

            GL.End();

            GL.PopMatrix();
        }

        // plot an X
        void Plot(float x, float y)
        {
            // first line of X
            GL.Vertex3(x - 1, y - 1, 0);
            GL.Vertex3(x + 1, y + 1, 0);

            // second
            GL.Vertex3(x - 1, y + 1, 0);
            GL.Vertex3(x + 1, y - 1, 0);
        }
    }
}
#endif