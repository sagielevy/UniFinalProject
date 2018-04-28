using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameScripts.UI
{
    [RequireComponent(typeof(MeshRenderer))]
    public class BallShadow : MonoBehaviour
    {
        public Collider ball;
        public Vector3 relativePos = new Vector3(0, -0.5f, 0);
        public float startHideThreshold = 0.1f;

        private MeshRenderer meshRenderer;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void LateUpdate()
        {
            // Force sprite to always stay on the bottom of the ball, but no rotation will ever apply
            transform.position = ball.transform.position + relativePos;

            // Hide if not on floor?
            RaycastHit raycastHit;
            var temp = meshRenderer.material.color;

            if (Physics.Raycast(ball.transform.position, Vector3.down, out raycastHit))
            {
                var relativeVal = Mathf.Lerp(1, 0, raycastHit.distance - ball.bounds.extents.y);
                meshRenderer.material.color = new Color(temp.r, temp.g, temp.b, relativeVal);
                //meshRenderer.material.mainTextureScale = new Vector2(relativeVal, relativeVal);
            } else
            {
                meshRenderer.material.color = new Color(temp.r, temp.g, temp.b, 0);
            }
        }
    }
}
