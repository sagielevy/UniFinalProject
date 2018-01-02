using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Manipulation : MonoBehaviour
{
    Mesh mesh;

    // Use this for initialization
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.MarkDynamic();
    }

    // Update is called once per frame
    void Update()
    {
        var newArr = mesh.vertices;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            if (i % 2 == 0)
            {
                newArr[i].x *= 1.01f;
            }

            if (i % 4 == 0)
            {
                newArr[i].y *= 1.01f;
            }

            if (i % 8 == 0)
            {
                newArr[i].z *= 1.01f;
            }

            //newArr[i].x += (newArr[i].x % 1f) / 100;
        }

        mesh.vertices = newArr;
    }
}
