using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    bool isUsingKinect;
    //length
    float x_left = -2.1f; //-2.4f
    float x_right = -0.5f; //-0.2
    //width
    float z_top = 0.5f;//1
    float z_bot = -1.1f; //-1.4
    //height
    float y_top = 0.8f;//1f
    float y_bot = 0.3f;
    float kinect_width = 512;
    float kinect_height = 424;
    float kinect_to_table = 1550;
    float x, y, z;
    public float pos_x, pos_y, pos_z;
    float old_yaw = 0f, old_roll = 0f, old_pitch = 0f;
    float new_yaw, new_roll, new_pitch;
    public bool emergency;

    InfraredSourceManager infraredSourceManager;
    DepthSourceManager depthSourceManager;
    void Awake()
    {
        if (GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingController)
        {
            gameObject.SetActive(false);
        }
        if (GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingLeapMotion)
        {
            transform.GetChild(5).gameObject.SetActive(false);
            transform.GetChild(6).gameObject.SetActive(false);
            this.enabled = false;
        }
    }
    // Use this for initialization
    void Start()
    {
        infraredSourceManager = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>();
        depthSourceManager = GameObject.Find("DepthManager").GetComponent<DepthSourceManager>();
        isUsingKinect = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingKinect;
        //isUsingKinect = true;
        if (GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingController)
        {
            gameObject.SetActive(false);
        }
        if (!isUsingKinect) this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //position
        x = infraredSourceManager.x3;
        y = infraredSourceManager.y3;
        z = depthSourceManager.depth;
        if (z == 0) z = kinect_to_table;
        if (z > kinect_to_table) z = kinect_to_table;
        pos_x = x_left + x / kinect_width * (x_right - x_left);
        pos_z = z_bot + y / kinect_height * (z_top - z_bot);
        pos_y = y_bot + (kinect_to_table - z) / kinect_to_table * (y_top - y_bot) * 3f;
        transform.position = Vector3.Slerp(transform.position, new Vector3(pos_x, pos_y, pos_z), Time.deltaTime * 20);
        //yaw
        float yaw = infraredSourceManager.yaw;
        //roll
        float roll = depthSourceManager.roll;
        if (Mathf.Abs(roll - old_roll) < 100f && Mathf.Abs(roll - old_roll) > 5f && !emergency) //40
        {
            new_roll = roll;
        }
        old_roll = new_roll;
        //pitch
        float pitch = depthSourceManager.pitch;
        if (Mathf.Abs(pitch - old_pitch) < 100f && Mathf.Abs(pitch - old_pitch) > 5f && !emergency) //20
        {
            new_pitch = pitch;
        }
        old_pitch = new_pitch;
        //Slerp
        var rotation = Quaternion.Euler(new_pitch, yaw, new_roll);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 20);
    }
}
