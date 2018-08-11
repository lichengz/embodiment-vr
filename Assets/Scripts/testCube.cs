using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCube : MonoBehaviour {
	GameObject go;
	// Use this for initialization
	void Start () {
		go = GameObject.Find("Interaction Hand (Right)");
	}
	
	// Update is called once per frame
	void Update () {
		//transform.position = go.GetComponent<InteractionHand>().testTransform;
	}
}
