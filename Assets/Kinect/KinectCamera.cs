using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectCamera : MonoBehaviour {
	GameObject marker;

	// Use this for initialization
	void Start () {
		marker = GameObject.Find("Infrared Marker");
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = marker.transform.position - new Vector3(0, 1, 0);
	}
}
