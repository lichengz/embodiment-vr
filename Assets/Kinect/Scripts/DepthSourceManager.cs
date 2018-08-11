using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System;

public class DepthSourceManager : MonoBehaviour
{
    bool isUsingKinect;
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;
    public int position, positionLeft, positionRight;
    public int depth, depthLeft, depthRight, depthMid;
    public int count = -3;
    public int countPitch = -10;
    public int LRShift = 3; //10
    public float rollAlpha = 5f;
    public float beta = 0.5f;
    public float pitchGama = 1f;
    public float roll, pitch;
    bool onceflag = true;
    public ushort[] GetData()
    {
        return _Data;
    }
    void Awake()
    {
        isUsingKinect = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingKinect;
        //isUsingKinect = true;
        if (!isUsingKinect) gameObject.SetActive(false);
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                frame.Dispose();
                frame = null;
            }
        }
        //match the infrared point to depth map
        Vector2 tridot1 = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>().TriDot1;
        Vector2 tridot2 = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>().TriDot2;
        Vector2 tridot3 = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>().TriDot3;
        Vector2 midOf2and3 = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>().midOfTridot2and3;
        Vector2 left = LRShift * (tridot2 - midOf2and3).normalized;
        Vector2 right = LRShift * (tridot3 - midOf2and3).normalized;
        position = GameObject.Find("InfraredManager").GetComponent<InfraredSourceManager>().position + count * 512;
        // positionLeft = (int)(left.y) * 512 + (int)left.x + count * 512;
        // positionRight = (int)(right.y) * 512 + (int)right.x + count * 512;
        positionLeft = position + ((int)(left.y) * 512 + (int)left.x) - 512 * 2;
        positionRight = position + ((int)(right.y) * 512 + (int)right.x) - 512 * 2;
        // positionLeft = (int)tridot2.y + (int)tridot2.x - 5;
        // positionRight = (int)tridot3.y + (int)tridot3.x + 5;
        // Debug.Log(left + " " + right);
        if (position > 0 && positionLeft > 0 && positionRight > 0)
        {
            //----------------------------ROLL----------------------------
            depth = _Data[position];
            depthLeft = _Data[positionLeft];
            depthRight = _Data[positionRight];
            float tmp = (float)(depthLeft - depthRight) / 2;
            roll = Mathf.Atan(tmp / rollAlpha / LRShift) / Mathf.PI * 180;
            //----------------------------Pitch-----------------------------
            depthMid = _Data[(int)(tridot1.y) * 512 + (int)tridot1.x + countPitch * 512];
            pitch = (depthMid - depth) * pitchGama;
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
