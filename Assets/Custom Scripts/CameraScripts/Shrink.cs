using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrink : MonoBehaviour {
    Vector3 org;
    float orgFov;
    public float speed = 1.1f;

	// Use this for initialization
	void Start () {
        org = Camera.main.transform.position;
        orgFov = Camera.main.fieldOfView;
    }
	
	// Update is called once per frame
	void Update () {
        //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,
        //                                             Mathf.Lerp(org.y, 0.5f, 0.05f * Time.deltaTime + Time.time),
        //                                             Mathf.Lerp(org.z, 4f, 0.05f * Time.deltaTime + Time.time));
        Camera.main.fieldOfView = Mathf.Lerp(orgFov, 110, 0.05f * Time.deltaTime + Time.time);
    }
}
