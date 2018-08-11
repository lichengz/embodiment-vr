using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public float distance;
    public LayerMask markerLayer;
    private Vector3 currentPos;
    //check if the cube has moved to the required position
    public bool done = false;
    public GameObject hand;
    bool isHandReached = false;
    public GameObject grabber;
    public bool isGrabbed;
    public GameObject center;

    Material originalMat;
    GameObject theBase;
    void Start()
    {
        currentPos = transform.position;
        //GetComponent<BoxCollider>().center = Vector3.zero - new Vector3(0, 0, 3);
        originalMat = GetComponent<Renderer>().material;
        GetComponent<Rigidbody>().maxDepenetrationVelocity = 1;
        GetComponent<Rigidbody>().maxAngularVelocity = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //currentPos = transform.position;
        //this.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        //Debug.Log(gameObject.name[gameObject.name.Length-1]);
        if (isGrabbed && !done)
        {
            grabber = GameObject.Find("Grabber");
            center = GameObject.Find("grabcenter");
            transform.position = grabber.transform.position;
            transform.rotation = grabber.transform.rotation;
        }
        //cube done, base also done
        if (done)
        {
            theBase.GetComponent<BaseScript>().done = true;
            isGrabbed = false;
        }
    }
    void FixedUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, markerLayer))
        {
            //Debug.Log(hit.transform.name);
            if (gameObject.name[gameObject.name.Length - 1] == hit.transform.name[hit.transform.name.Length - 1])
            {
                if (!hit.collider.gameObject.GetComponent<BaseScript>().done)
                {
                    done = true;
                    theBase = hit.collider.gameObject;
                    FreezeCube();
                    UpdateBaseColorToGreen();
                }
            }
            else
            {
                UpdateBaseColorToOriginal();
            }
        }
    }

    //once a cube is in the required position, freeze its position
    void FreezeCube()
    {
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        transform.position = theBase.transform.position + new Vector3(0, 0.1f, 0);
        transform.rotation = theBase.transform.rotation;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }

    void MoveColliderTowardHand()
    {
        if (isHandReached)
        {
            Vector3 dir = Vector3.Normalize(hand.transform.position - transform.position);

        }
    }

    void UpdateBaseColorToGreen()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }

    void UpdateBaseColorToOriginal()
    {
        GetComponent<Renderer>().material = originalMat;
    }

}
