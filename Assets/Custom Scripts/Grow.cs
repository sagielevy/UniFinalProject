using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Grow : MonoBehaviour
{
    Mesh mesh;
    float acc = 1.01f;

    // Use this for initialization
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        mesh.MarkDynamic();
    }


    public float targetScale = 0.1f;
    public float shrinkSpeed = 0.1f;
    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * shrinkSpeed);
        //var org = transform.position;

        //var newMesh = mesh.vertices;

        //for (int i = 0; i < mesh.vertices.Length; i++)
        //{

        //    newMesh[i] = (newMesh[i] + org) * 1.01f - org;
        //}

        //mesh.vertices = newMesh;
    }
}
