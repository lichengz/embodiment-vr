using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
    bool isUsingLeapMotion;
    public GameObject hand;
    public GameObject target;
    float checkRate = 0.2f;
    float nextCheck = 0f;
    float checkPosRate = 0.03f;

    float nextCheckPos = 0f;
    //detect is target moving
    Vector3 oldTargetPos;
    Vector3 newTargetPos;
    public bool isTargetMoving;
    public bool start;
    //detect is hand moving
    Vector3 oldHandPos;
    Vector3 newHandPos;
    public bool isHandMoving;
    public GameObject tool;
    //Prevent the tool from going below the table
    public Transform table;
    public bool isAboveTable = true;
    float oldOboveTablePosY;
    //at the exact moment of grabbing, tool shouldn't rotate
    public bool isLock = false;

    public Vector3 track;
    //smooth the target position by taking the average the last 5 frames
    Vector3 smoothedTargetPos;
    Vector3 tmpTargetPos;
    int count = 0; // count the number of frames

    //Pitch Yaw Roll, smooth the data by taking average
    public float pitch, yaw, roll;
    public GameObject scoreBoardText;
    int pitchCount, yawCount;
    float tmpPitch, tmpYaw;
    public float smoothedPitch, smoothedYaw;
    //update pitch yaw roll moving bool status, by detecting a threshold
    public bool isPitchMoving, isYawMoving;
    public float oldPitch, oldYaw;
    public float newPitch, newYaw;
    //if the grabber is rotating too fast, stop it
    public bool isTooFast;
    public float oldSmoothedPitch, oldSmoothedYaw;
    bool firstSpeedCheck = true;
    //ROLL
    public float myRoll = 180;

    //-----------------------
    float myOldYaw = 0, myNewYaw = 0;
    bool firstTime = true;
    //-----------------------
    // Use this for initialization
    void Start()
    {
        isUsingLeapMotion = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingLeapMotion;
        newTargetPos = target.transform.position;
        //newHandPos = hand.transform.position;
        oldSmoothedYaw = 180;
        myRoll = 180;
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            InitialFilterOfPitchYawRoll();
            UpdatePitchYawMovingStatus();
            PitchYawRoll();
            //the following is my own method, without using pitch, yaw or roll.
            detectTable();
            //==============================Switch to Leap Motion==============================
            if (isUsingLeapMotion) updateToolPosition();
        }
        //test myRoll
        if (Input.GetKeyDown(KeyCode.Z))
        {
            myRoll -= 10;
        }
    }
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // if (start)
        // {
        //     updateToolPosition();
        // }
    }
    void InitialFilterOfPitchYawRoll()
    {
        if (pitch > 0 && pitch < 90) pitch = 90;
        if (pitch < 0 && pitch > -90) pitch = -90;
        if (yaw > 0 && yaw < 150) yaw = 150;
        if (yaw < 0 && yaw > -130) yaw = -130;
    }
    void CheckIsTooFast()
    {
        float tmpSmoothedYaw = (smoothedYaw < 0 ? smoothedYaw + 360 : smoothedYaw);
        if (Mathf.Abs(tmpSmoothedYaw - oldSmoothedYaw) > 50)
        {
            isTooFast = true;
        }
        else
        {
            isTooFast = false;
            oldSmoothedYaw = tmpSmoothedYaw;
        }
    }
    void UpdatePitchYawMovingStatus()
    {
        if (pitch < 0) pitch += 360;
        if (yaw < 0) yaw += 360;
        if (Time.time > nextCheck)
        {
            oldPitch = newPitch;
            oldYaw = newYaw;
            nextCheck = Time.time + checkRate;
        }
        if (Mathf.Abs(pitch - oldPitch) > 5)
        {
            newPitch = pitch;
            isPitchMoving = true;
        }
        else
        {
            isPitchMoving = false;
        }
        //Yaw was 2
        if (Mathf.Abs(yaw - oldYaw) > 10)
        {
            newYaw = yaw;
            isYawMoving = true;
        }
        else
        {
            isYawMoving = false;
        }
    }

    float LimitYawWhenPitchIsHigh(float inputPitch, float inputYaw)
    {
        float outputYaw = inputYaw;
        if (inputPitch < 165)
        {
            if (Mathf.Abs(inputYaw) < 170)
            {
                if (inputYaw > 0) outputYaw = 170;
                if (inputYaw < 0) outputYaw = -170;
            }
        }
        return outputYaw;
    }
    void UpdateMyRoll()
    {
        if (Mathf.Abs(smoothedPitch) > 160 && Mathf.Abs(smoothedYaw) > 150)
        {
            myRoll = 180 + roll;
        }
        else
        {
            myRoll = 180;
        }
    }
    void PitchYawRoll()
    {
        int filterDelay = 5;
        //Pitch filter
        if (pitchCount < filterDelay)
        {
            if (isPitchMoving)
            {
                pitchCount++;
                if (pitch < 0) pitch += 360;
                tmpPitch += pitch;
            }
        }
        else
        {
            pitchCount = 0;
            smoothedPitch = tmpPitch / filterDelay;
            if (smoothedPitch > 180) smoothedPitch -= 360;
            tmpPitch = 0f;
            //additional filter, to minimize the up-and-down rotation
            if (smoothedPitch > 0) smoothedPitch += 0.5f * (180 - smoothedPitch);
            if (smoothedPitch < 0) smoothedPitch -= 0.1f * (180 + smoothedPitch);
        }
        //Yaw filter
        if (yawCount < filterDelay)
        {
            if (isYawMoving)
            {
                yawCount++;
                if (yaw < 0) yaw += 360;
                tmpYaw += yaw;
            }
        }
        else
        {
            yawCount = 0;
            smoothedYaw = tmpYaw / filterDelay;
            if (smoothedYaw > 180) smoothedYaw -= 360;
            tmpYaw = 0f;
        }
        //smoothedYaw = LimitYawWhenPitchIsHigh(smoothedPitch, smoothedYaw);
        CheckIsTooFast();
        UpdateMyRoll();
        transform.position = hand.transform.position; //+ new Vector3(0f, 0, 0.08f);
        //transform.eulerAngles = new Vector3(-smoothedPitch, -smoothedYaw, 180);
        Vector3 anglesVector = new Vector3(-smoothedPitch, -smoothedYaw, myRoll);
        //if (!isLock && !isTooFast) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(anglesVector), Time.deltaTime * 10);
    }

    void updateToolPosition()
    {
        this.transform.position = hand.transform.position;
        //apply a buffer to the target position
        //bool needToMove = false;
        if (Time.time > nextCheckPos)
        {
            nextCheckPos = Time.time + checkPosRate;
            //target
            oldTargetPos = newTargetPos;
            //0.005
            if ((target.transform.position - oldTargetPos).magnitude > 0.01f)
            {
                newTargetPos = target.transform.position;
                isTargetMoving = true;
            }
            else
            {
                isTargetMoving = false;
            }

            //hand
            //0.005
            oldHandPos = newHandPos;
            if ((hand.transform.position - oldHandPos).magnitude > 0.02f)
            {
                newHandPos = hand.transform.position;
                isHandMoving = true;
            }
            else
            {
                isHandMoving = false;
            }
        }

        //smooth the target position by taking the average the last 5 frames

        if (count < 5)
        {
            if (!isLock)
            {
                count++;
                tmpTargetPos += target.transform.position;
            }
        }
        else
        {
            count = 0;
            smoothedTargetPos = tmpTargetPos / 5;
            tmpTargetPos = Vector3.zero;
        }

        if (target != null && isTargetMoving && !isLock /*&& !tool.GetComponent<GrabberDetect>().isGrabbing*/)
        {
            var lookPos = smoothedTargetPos - transform.position;

            //var lookPos = track - transform.position;
            lookPos.y *= 0.5f;
            var rotation = Quaternion.LookRotation(lookPos);
            // if ((Mathf.Abs(rotation.eulerAngles.y - myOldYaw) > 3 && Mathf.Abs((rotation.eulerAngles.y + 180) % 360 - (myOldYaw + 180) % 360) < 20) || firstTime)
            // {
            //     myNewYaw = rotation.eulerAngles.y;
            //     //Debug.Log(rotation.eulerAngles.y + " " + myOldYaw);
            //     firstTime = false;
            // }
            // myOldYaw = myNewYaw;
            // scoreBoardText.GetComponent<TextMesh>().text = myNewYaw.ToString();
            UpdateMyRoll();
            //if is almost below the table, change the y axis of targetPos to be Zero
            if (!isAboveTable)
            {
                rotation = Quaternion.Euler(new Vector3(0, rotation.eulerAngles.y, myRoll + 180));
            }
            else
            {
                rotation = Quaternion.Euler(new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y/*myNewYaw*/, myRoll + 180));
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10);
            //transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, target.transform.eulerAngles.z + 90);
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, hand.transform.localEulerAngles.z + 90);
            //Debug.Log(transform.eulerAngles.z + " " + target.transform.eulerAngles.z);
        }
        //transform.rotation = Quaternion.Euler(  0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    //Lock the tool once it goes below the table
    void detectTable()
    {
        if (tool.transform.position.y < table.position.y + 0.08f)
        {
            isAboveTable = false;
            //transform.position = new Vector3(transform.position.x, oldOboveTablePosY, transform.position.z);
            //Debug.Log("!!!!!!!!!!!!!!");
        }
        else
        {
            isAboveTable = true;
            oldOboveTablePosY = transform.position.y;
        }
    }
}
