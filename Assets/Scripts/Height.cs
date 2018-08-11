using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Height : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        // if (GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingController)
        // {
        transform.position -= new Vector3(0, 0.3f, 0);
        // }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            transform.localPosition += new Vector3(0, 0.05f, 0);
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            transform.localPosition -= new Vector3(0, 0.05f, 0);
        }
    }
}
