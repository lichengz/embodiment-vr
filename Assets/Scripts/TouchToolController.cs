using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR = UnityEngine.VR;

public class TouchToolController : MonoBehaviour
{
    public float force;
    public GameObject grabber;
    public GameObject rightHand;

    // Use this for initialization
    void Awake()
    {
        if (!GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingController) gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = rightHand.transform.position;
        transform.rotation = rightHand.transform.rotation;
        force = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        if (grabber != null)
        {
            grabber.GetComponent<GrabberDetect>().force = (1 - force) / 10;
            if (force > 0.8f)
            {
                grabber.GetComponent<GrabberDetect>().isGrabbing = true;
            }
            if (force < 0.3f)
            {
                grabber.GetComponent<GrabberDetect>().isGrabbing = false;
            }
        }
    }
}