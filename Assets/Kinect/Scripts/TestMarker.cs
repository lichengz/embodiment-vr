using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMarker : MonoBehaviour
{
    InfraredSourceManager script;

    // Use this for initialization
    void Start()
    {
        script = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameObject.name)
        {
            case "Marker (1)":
                transform.position = new Vector3(-256 + script.toothPos1.x, -212 + script.toothPos1.y, 0);
                break;
            case "Marker (2)":
                transform.position = new Vector3(-256 + script.toothPos2.x, -212 + script.toothPos2.y, 0);
                break;
            case "Marker (3)":
                transform.position = new Vector3(-256 + script.TriDot1.x, -212 + script.TriDot1.y, 0);
                break;
            case "Marker (4)":
                transform.position = new Vector3(-256 + script.TriDot2.x, -212 + script.TriDot2.y, 0);
                break;
            case "Marker (5)":
                transform.position = new Vector3(-256 + script.TriDot3.x, -212 + script.TriDot3.y, 0);
                break;
            default:
                transform.position = new Vector3(-256 + script.x3, -212 + script.y3, 0);
				break;
        }
    }
}
