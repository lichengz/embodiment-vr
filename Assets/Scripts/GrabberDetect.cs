using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberDetect : MonoBehaviour
{
    //when both isTouching and isGrabbing are ture, the cube starts to move
    public bool isTouching;
    public bool isGrabbing;
    public GameObject grabbedCube;
    public GameObject leftTooth;
    public GameObject RightTooth;
    public Transform leftToothStart;
    public Transform rightToothStart;
    public Transform leftToothEnd;
    public Transform rightToothEnd;
    bool isToothMoved = true;

    //the magnitude of force cooresponds to how much the teeth will move
    public float force;
    Vector3 oldLeftToothPos, oldRightToothPos;
    GameObject tool;
    public GameObject rightController;
    bool isBroken;
    // Use this for initialization
    void Start()
    {
        leftTooth.transform.position = leftToothStart.position;
        RightTooth.transform.position = rightToothStart.position;

    }

    // Update is called once per frame
    void Update()
    {
        isBroken = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isToolBroken;
        if (GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingLeapMotion)
        {
            if (!GameObject.Find("ToolWrapper").GetComponent<Tool>().start) force = 0.1f;
        }
        //catch the cube
        if (isTouching && isGrabbing && grabbedCube != null)
        {
            //grabbedCube.GetComponent<Collider>().isTrigger = true;
            //grabbedCube.transform.position = transform.position;
            if (isBroken)
            {
                float chance = Random.Range(0f, 1f);
                if (chance < 0.01f)
                {
                    grabbedCube.GetComponent<Cube>().isGrabbed = true;
                }
            }
            else
            {
                grabbedCube.GetComponent<Cube>().isGrabbed = true;
            }
        }
        //release the cube
        if (!isGrabbing && grabbedCube != null)
        {
            grabbedCube.GetComponent<Cube>().isGrabbed = false;
        }
        //Update the grabber tooth position
        //leftTooth.transform.position = Vector3.Lerp(leftTooth.transform.position, leftToothEnd.transform.position, Time.deltaTime * 0.5f);
        //RightTooth.transform.position = Vector3.Lerp(RightTooth.transform.position, rightToothEnd.position, Time.deltaTime * 10);
        // oldLeftToothPos = leftTooth.transform.position;
        // oldRightToothPos = RightTooth.transform.position;
        Vector3 dir1 = leftToothEnd.position - leftToothStart.position;
        Vector3 dir2 = rightToothEnd.position - rightToothStart.position;
        float displacement = 0.1f - force;
        if (displacement < 0) displacement = 0;
        if (displacement > 0.04f) displacement = 0.04f;
        if (isBroken)
        {
            float chance = Random.Range(0f, 1f);
            if (chance < 0.01f)
            {
                leftTooth.transform.position = leftToothStart.position + dir1.normalized * displacement;
                RightTooth.transform.position = rightToothStart.position + dir2.normalized * displacement;
            }
        }
        if (isGrabbing)
        {
            // leftTooth.transform.position = leftToothEnd.position;
            // RightTooth.transform.position = rightToothEnd.position;
        }
        else
        {
            //leftTooth.transform.position = leftToothStart.position;
            //RightTooth.transform.position = rightToothStart.position;
        }

        //====================
        // if (grabbedCube == null)
        // {
        //     isTouching = false;
        //     GetComponent<Renderer>().material.color = Color.white;
        //     //grabbedCube.GetComponent<Collider>().isTrigger = false;
        // }

    }
    //Check if any cube is grabbed, if none, then start grabbing
    bool CheckGrabbedStatus()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject go in cubes)
        {
            if (go.GetComponent<Cube>().isGrabbed == true) return true;
        }
        return false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cube" && !CheckGrabbedStatus())
        {
            isTouching = true;
            grabbedCube = other.gameObject;
            GetComponent<Renderer>().material.color = Color.red;

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cube")
        {
            isTouching = false;
            GetComponent<Renderer>().material.color = Color.white;
            //grabbedCube.GetComponent<Collider>().isTrigger = false;
        }
    }
}
