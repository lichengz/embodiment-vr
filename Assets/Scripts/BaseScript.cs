using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScript : MonoBehaviour {

	// Use this for initialization
	public bool done = false;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
    {
        //RaycastHit hit;

        //if (Physics.Raycast(transform.position, Vector3.up, out hit, 100f, markerLayer) /*&& hit.transform.gameObject.name == "InteractionCubeA"*/)
            //print(hit.transform.name);
    }
}
