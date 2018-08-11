using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System;
using System.Collections.Generic;

public class InfraredSourceManager : MonoBehaviour
{
    bool isUsingKinect;
    private KinectSensor _Sensor;
    private InfraredFrameReader _Reader;
    private ushort[] _Data;
    private byte[] _RawData;
    bool onceflag = true;
    bool flag = true;
    int tooth1, tooth2;
    //x1, y1 is tooth1
    //x2, y2 is tooth2
    public float x1, x2, y1, y2;
    public double distanceBetween2Markers;
    public int grabCount;
    bool countFlag = true;
    public GameObject grabber;

    //center position to track orientation
    public int position, positionLeft, positionRight;
    //x3, y3 is the 3-dot board
    public float x3, y3;
    bool posFlag = true;
    [SerializeField]
    int TotalNumOfDots;
    public GameObject dot, dot1, dot2, dot3, dot4, dot5;
    [SerializeField]
    int totalNumberOfPixels;
    [SerializeField]
    List<Vector2> whiteDots;
    [SerializeField]
    public float yaw, roll, pitch;

    public List<Vector2> dotGroup1, dotGroup2, dotGroup3, dotGroup4, dotGroup5;
    //List<Vector2>[] dotGroups = new List<Vector2>[5];
    List<List<Vector2>> dotGroups = new List<List<Vector2>>();
    int dotGroupIndex = 0;
    int boarder = 20;

    //new 5 dots
    public Vector2 toothPos1, toothPos2, TriDot1, TriDot2, TriDot3, midOfTridot2and3;
    Vector2 oldtoothPos1, oldtoothPos2, oldTriDot1, oldTriDot2, oldTriDot3;
    public ComputeShader shader;

    bool firstDetect = true;
    Marker markerScript;

    public float[] GGG = new float[4];

    // I'm not sure this makes sense for the Kinect APIs
    // Instead, this logic should be in the VIEW
    private Texture2D _Texture;

    public Texture2D GetInfraredTexture()
    {
        return _Texture;
    }
    void Awake()
    {
        isUsingKinect = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingKinect;
        // //isUsingKinect = true;
        if (!isUsingKinect) gameObject.SetActive(false);
    }
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Reader = _Sensor.InfraredFrameSource.OpenReader();
            var frameDesc = _Sensor.InfraredFrameSource.FrameDescription;
            _Data = new ushort[frameDesc.LengthInPixels];
            _RawData = new byte[frameDesc.LengthInPixels * 4];
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.BGRA32, false);

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
        totalNumberOfPixels = _Data.Length;
        whiteDots = new List<Vector2>();
        // dotGroups[0] = new List<Vector2>();
        // dotGroups[1] = new List<Vector2>();
        // dotGroups[2] = new List<Vector2>();
        // dotGroups[3] = new List<Vector2>();
        // dotGroups[4] = new List<Vector2>();
        //RunShader();
        markerScript = GameObject.Find("ToolWrapper").GetComponent<Marker>();
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                int index = 0;
                foreach (var ir in _Data)
                {
                    byte intensity = (byte)(ir >> 8);
                    _RawData[index++] = intensity;
                    _RawData[index++] = intensity;
                    _RawData[index++] = intensity;
                    _RawData[index++] = 255; // Alpha
                }

                _Texture.LoadRawTextureData(_RawData);
                _Texture.Apply();

                frame.Dispose();
                frame = null;
                //test
                CheckPinchMarkers();
                CheckPositionMarker();
            }
        }
        DetectGrabbing();
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
    void CheckPinchMarkers()
    {
        // for (int count = 1; count < _Data.Length - 1538; count++)
        // {
        //     if ((byte)(_Data[count] >> 8) == 255 && (byte)(_Data[count + 1] >> 8) == 255
        //     && (byte)(_Data[count + 512] >> 8) == 255 && (byte)(_Data[count + 513] >> 8) == 255)
        //     {
        //         int boarder = 20;
        //         if (flag && count % 512 > boarder && count % 512 < 512 - boarder && count / 512 > boarder && count / 512 < 424 - boarder)
        //         {
        //             tooth1 = count;
        //             flag = false;
        //             //remove the 3rd line of the marker
        //             if ((byte)(_Data[count + 1024] >> 8) == 255)
        //             {
        //                 _Data[count + 1022] = 0;
        //                 _Data[count + 1023] = 0;
        //                 _Data[count + 1024] = 0;
        //                 _Data[count + 1025] = 0;
        //                 _Data[count + 1026] = 0;
        //             }
        //         }
        //         if (!flag && count % 512 > boarder && count % 512 < 512 - boarder && count / 512 > boarder && count / 512 < 424 - boarder)
        //         {
        //             tooth2 = count;
        //         }
        //     }
        // }
        //Find3ConsectiveDots(new Vector3(0, 0, 0));
        ShowAllWhiteDots();
        dotGroupIndex = 0;
        DivideDotsIntoGroups(whiteDots, 4f);
        UpdateDotGroups();
        ShowDotGroups();
        //if any dot group is missing, use the last remembered positions
        if (dotGroup1.Count == 0 || dotGroup2.Count == 0 || dotGroup3.Count == 0 || dotGroup4.Count == 0 || dotGroup5.Count == 0)
        {
            toothPos1 = oldtoothPos1;
            toothPos2 = oldtoothPos2;
            TriDot1 = oldTriDot1;
            TriDot2 = oldTriDot2;
            TriDot3 = oldTriDot3;
        }
        else
        {
            //all 5 dot groups are there, find the next current positions
            FindTeethAndTheThree();
        }
    }

    void DivideDotsIntoGroups(List<Vector2> dots, float detectRange)
    {
        if (dots.Count == 0/* || dotGroupIndex == 5*/)
        {
            TotalNumOfDots = dotGroupIndex;
            return;
        }
        //---
        dotGroups.Add(new List<Vector2>());
        //---
        Vector2 firstDot = dots[0];
        dotGroups[dotGroupIndex].Add(firstDot);
        dots.RemoveAt(0);
        // int tmp = dots.Count;
        // for (int i = 0; i < tmp; i++)
        // {
        //     if (DistanceOf2Dots(firstDot, dots[i]) < 10f)
        //     {
        //         dotGroups[dotGroupIndex].Add(dots[i]);
        //         Debug.Log(dots.IndexOf(dots[i]) + " vs " + i);
        //         dots.RemoveAt(dots.IndexOf(dots[i]));
        //     }
        // }
        foreach (Vector2 dot in dots.ToArray())
        {
            if (DistanceOf2Dots(firstDot, dot) < detectRange)
            {

                dotGroups[dotGroupIndex].Add(dot);
                int index = dots.IndexOf(dot);
                dots.RemoveAt(index);
            }
        }
        //Debug.Log(dotGroups[0].Count);
        dotGroupIndex++;
        DivideDotsIntoGroups(dots, detectRange);
    }
    void UpdateDotGroups()
    {
        dotGroup1.Clear();
        dotGroup2.Clear();
        dotGroup3.Clear();
        dotGroup4.Clear();
        dotGroup5.Clear();
        if (dotGroupIndex == 0) return;
        for (int i = 0; i < dotGroupIndex; i++)
        {
            if (dotGroups[i].Count < 3 || dotGroups[i].Count > 10)
            {
                dotGroups.RemoveAt(i);
            }
        }
        for (int i = 0; i < dotGroupIndex; i++)
        {
            switch (i)
            {
                case 0:
                    foreach (Vector2 dot in dotGroups[0])
                        dotGroup1.Add(dot);
                    break;
                case 1:
                    foreach (Vector2 dot in dotGroups[1])
                        dotGroup2.Add(dot);
                    break;
                case 2:
                    foreach (Vector2 dot in dotGroups[2])
                        dotGroup3.Add(dot);
                    break;
                case 3:
                    foreach (Vector2 dot in dotGroups[3])
                        dotGroup4.Add(dot);
                    break;
                case 4:
                    foreach (Vector2 dot in dotGroups[4])
                        dotGroup5.Add(dot);
                    break;
            }
        }
        foreach (List<Vector2> dotGroup in dotGroups)
        {
            dotGroup.Clear();
        }
    }

    float DistanceOf2Dots(Vector2 one, Vector2 two)
    {
        return (float)Math.Sqrt((one.x - two.x) * (one.x - two.x) + (one.y - two.y) * (one.y - two.y));
    }
    void Find3ConsectiveDots(Vector2 startPoint)
    {
        //left
        if (startPoint.x > 0)
        {
            Find3ConsectiveDots(new Vector2(startPoint.x - 1, startPoint.y));
        }
        //right 
        if (startPoint.x < 512)
        {
            Find3ConsectiveDots(new Vector2(startPoint.x + 1, startPoint.y));
        }
        //top
        if (startPoint.y < 424)
        {
            Find3ConsectiveDots(new Vector2(startPoint.x, startPoint.y + 1));
        }
        //bot
        if (startPoint.y > 0)
        {
            Find3ConsectiveDots(new Vector2(startPoint.x, startPoint.y - 1));
        }
    }
    int GetIndexFromXandY(int x, int y)
    {
        return y * 512 + x;
    }


    void ShowAllWhiteDots()
    {
        whiteDots.Clear();
        GameObject[] oldDots = GameObject.FindGameObjectsWithTag("Dot");
        foreach (GameObject go in oldDots)
        {
            Destroy(go);
        }
        // foreach (ushort item in _Data)
        // {
        //     if ((byte)(item >> 8) == 255)
        //     {
        //         int count = Array.IndexOf(_Data, item);
        //         int boarder = 60;
        //         if (count % 512 > boarder && count % 512 < 512 - boarder && count / 512 > boarder && count / 512 < 424 - boarder)
        //         {
        //             whiteDots.Add(new Vector2(count % 512, count / 424));
        //             Instantiate(dot, new Vector3(-256 + count % 512, -212 + count / 424, 0), Quaternion.identity);
        //         }
        //         _Data[Array.IndexOf(_Data, item)] = 0;
        //         //_whiteDots.Add(new Vector2(count % 512, count / 424));
        //     }

        // }
        for (int i = 0; i < _Data.Length; i++)
        {
            if ((byte)(_Data[i] >> 8) == 255)
            {
                int boarder = 40;
                if (i % 512 > boarder && i % 512 < 512 - boarder && i / 512 > boarder && i / 512 < 424 - boarder)
                {
                    whiteDots.Add(new Vector2(i % 512, i / 424));
                    Instantiate(dot, new Vector3(-256 + i % 512, -212 + i / 424, 0), Quaternion.identity);
                }
                _Data[i] = 0;
            }
        }
    }
    void ShowDotGroups()
    {
        foreach (Vector2 dot in dotGroup1)
        {
            Instantiate(dot1, new Vector3(-256 + dot.x, -212 + dot.y, -1), Quaternion.identity);
        }
        foreach (Vector2 dot in dotGroup2)
        {
            Instantiate(dot2, new Vector3(-256 + dot.x, -212 + dot.y, -1), Quaternion.identity);
        }
        foreach (Vector2 dot in dotGroup3)
        {
            Instantiate(dot3, new Vector3(-256 + dot.x, -212 + dot.y, -1), Quaternion.identity);
        }
        foreach (Vector2 dot in dotGroup4)
        {
            Instantiate(dot4, new Vector3(-256 + dot.x, -212 + dot.y, -1), Quaternion.identity);
        }
        foreach (Vector2 dot in dotGroup5)
        {
            Instantiate(dot5, new Vector3(-256 + dot.x, -212 + dot.y, -1), Quaternion.identity);
        }
    }

    void DetectGrabbing()
    {
        x1 = toothPos1.x;
        y1 = toothPos1.y;
        x2 = toothPos2.x;
        y2 = toothPos2.y;
        distanceBetween2Markers = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        if (distanceBetween2Markers < 15 && distanceBetween2Markers != 0)
        {
            if (countFlag)
            {
                grabCount++;
                countFlag = false;
            }
            if (grabber != null) grabber.GetComponent<GrabberDetect>().isGrabbing = true;
        }
        if (distanceBetween2Markers > 18 && distanceBetween2Markers != 0)
        {
            countFlag = true;
            if (grabber != null) grabber.GetComponent<GrabberDetect>().isGrabbing = false;
        }

        if (grabber != null && distanceBetween2Markers != 0) grabber.GetComponent<GrabberDetect>().force = (float)distanceBetween2Markers / 200;
    }
    void CheckPositionMarker()
    {
        // for (int count = 1; count < _Data.Length - 1027; count++)
        // {
        //     if ((byte)(_Data[count] >> 8) == 255 && (byte)(_Data[count + 1] >> 8) == 255 && (byte)(_Data[count + 2] >> 8) == 255 && (byte)(_Data[count + 3] >> 8) == 255
        //     && (byte)(_Data[count + 512] >> 8) == 255 && (byte)(_Data[count + 513] >> 8) == 255 && (byte)(_Data[count + 514] >> 8) == 255 && (byte)(_Data[count + 515] >> 8) == 255
        //     && (byte)(_Data[count + 1024] >> 8) == 255 && (byte)(_Data[count + 1025] >> 8) == 255 && (byte)(_Data[count + 1026] >> 8) == 255 && (byte)(_Data[count + 1027] >> 8) == 255)
        //     {
        //         int boarder = 20;
        //         if (posFlag && count % 512 > boarder && count % 512 < 512 - boarder && count / 512 > boarder && count / 512 < 424 - boarder)
        //         {
        //             position = count;
        //             x3 = count % 512;
        //             y3 = count / 512;
        //             posFlag = false;
        //         }
        //     }
        // }

        // x3 = (TriDot1.x + TriDot2.x + TriDot3.x) / 3;
        // y3 = (TriDot1.y + TriDot2.y + TriDot3.y) / 3;
        x3 = TriDot1.x;
        y3 = TriDot1.y;
        // x3 = (toothPos1.x + toothPos2.x) / 2;
        // y3 = (toothPos1.y + toothPos2.y) / 2;


        //yaw
        midOfTridot2and3 = (TriDot3 + TriDot2) / 2;
        Vector2 verticalLine = new Vector2(0, -1);
        yaw = (float)(Math.Acos(Vector2.Dot(midOfTridot2and3 - TriDot1, verticalLine) / (midOfTridot2and3 - TriDot1).magnitude) / Math.PI * 180);
        if (midOfTridot2and3.x > TriDot1.x)
        {
            yaw = -yaw;
        }
        //roll
        //position = (int)((midOfTridot2and3.y) * 512 + TriDot1.x);CC
        position = (int)(midOfTridot2and3.y) * 512 + (int)midOfTridot2and3.x;
        // Vector2 tmpLeft = midOfTridot2and3 - 10 * (TriDot2 - midOfTridot2and3).normalized;
        // positionLeft = (int)(tmpLeft.y) * 512 + (int)tmpLeft.x;
        // Vector2 tmpRight = midOfTridot2and3 - 10 * (TriDot3 - midOfTridot2and3).normalized;
        // positionRight = (int)(tmpRight.y) * 512 + (int)tmpRight.x;
    }

    Vector2 CenterPoint(List<Vector2> dots)
    {
        float x = 0, y = 0;
        foreach (Vector2 vec in dots)
        {
            x += vec.x;
            y += vec.y;
        }
        return new Vector2(x / dots.Count, y / dots.Count);
    }

    Vector2 FindClosestDot(Vector2 OriginDot, List<Vector2> dotGroup)
    {
        //if(dotGroup.Count == 0) Debug.Log("!!!!!!!!!!!!!!!!!!!");
        float min = 100f;
        Vector2 tmp = dotGroup[0];
        foreach (Vector2 dot in dotGroup.ToArray())
        {
            if (DistanceOf2Dots(OriginDot, dot) < min)
            {
                min = DistanceOf2Dots(OriginDot, dot);
                tmp = dot;
            }
        }
        if (min > 20f)
        {
            firstDetect = true;
            markerScript.emergency = true;
        }
        dotGroup.Remove(tmp);
        return tmp;
    }

    void FindTeethAndTheThree()
    {
        if (dotGroupIndex == 0) return;
        Vector2[] dots = new Vector2[5];
        //if (dotGroup1.Count != 0)
        dots[0] = CenterPoint(dotGroup1);
        //if (dotGroup2.Count != 0)
        dots[1] = CenterPoint(dotGroup2);
        //if (dotGroup3.Count != 0)
        dots[2] = CenterPoint(dotGroup3);
        //if (dotGroup4.Count != 0)
        dots[3] = CenterPoint(dotGroup4);
        //if (dotGroup5.Count != 0)
        dots[4] = CenterPoint(dotGroup5);

        //--------------------------First Detect------------------------------------
        if (firstDetect)
        {
            // Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!");
            firstDetect = false;
            if (markerScript != null) markerScript.emergency = false;
            //Two Teeth - method1
            // float min1 = 100f;
            // Vector2 tmpTooth1 = new Vector2();
            // Vector2 tmpTooth2 = new Vector2();
            // for (int i = 0; i < dots.Length - 1; i++)
            // {
            //     for (int j = i + 1; j < dots.Length; j++)
            //     {
            //         if (DistanceOf2Dots(dots[i], dots[j]) < min1)
            //         {
            //             min1 = DistanceOf2Dots(dots[i], dots[j]);
            //             tmpTooth1 = dots[i];
            //             tmpTooth2 = dots[j];
            //         }
            //     }
            // }
            // toothPos1 = tmpTooth1;
            // toothPos2 = tmpTooth2;

            //Two Teeth - method2
            List<Vector2> leftOverDots = new List<Vector2>();
            foreach (Vector2 dot in dots)
            {
                leftOverDots.Add(dot);
            }

            List<Vector2> potentialTeeth = new List<Vector2>();
            Vector2 dot0 = dots[0];
            potentialTeeth.Add(dot0);
            leftOverDots.RemoveAt(0);
            List<Vector2> leftOverDotsCopy = new List<Vector2>();
            foreach (Vector2 dot in leftOverDots)
            {
                leftOverDotsCopy.Add(dot);
            }
            for (int i = 0; i < 4; i++)
            {
                GGG[i] = DistanceOf2Dots(dot0, leftOverDots[i]);
                if (DistanceOf2Dots(dot0, leftOverDots[i]) < 70f)
                {
                    potentialTeeth.Add(leftOverDots[i]);
                    leftOverDotsCopy.Remove(leftOverDots[i]);
                }
            }

            if (potentialTeeth.Count == 2)
            {
                toothPos1 = potentialTeeth[0];
                toothPos2 = potentialTeeth[1];
            }
            else
            {
                toothPos1 = leftOverDotsCopy[0];
                toothPos2 = leftOverDotsCopy[1];
            }
            //Tri Dots method2 -- Tri1
            List<Vector2> triDots = new List<Vector2>();
            foreach (Vector2 tmp in dots)
            {
                if (tmp != toothPos1 && tmp != toothPos2)
                {
                    triDots.Add(tmp);
                }
            }
            for (int i = 0; i < triDots.Count; i++)
            {
                Vector2 v1 = triDots[(i + 1) % triDots.Count] - triDots[i];
                Vector2 v2 = triDots[(i + 2) % triDots.Count] - triDots[i];
                if (Vector2.Dot(v1, v2) < 0)
                {
                    TriDot1 = triDots[i];
                    // TriDot2 = triDots[(i + 1) % triDots.Count];
                    // TriDot3 = triDots[(i + 2) % triDots.Count];
                    triDots.RemoveAt(i);
                }
            }
            //Tri2 and Tri3
            if (triDots[0].x < triDots[1].x)
            {
                TriDot2 = triDots[0];
                TriDot3 = triDots[1];
            }
            else
            {
                TriDot3 = triDots[0];
                TriDot2 = triDots[1];
            }
        }
        //-------------------Motion Tracking---------------------------------------------
        else
        {
            List<Vector2> dotList = new List<Vector2>();
            foreach (Vector2 dot in dots)
            {
                dotList.Add(dot);
            }
            toothPos1 = FindClosestDot(toothPos1, dotList);
            toothPos2 = FindClosestDot(toothPos2, dotList);
            TriDot1 = FindClosestDot(TriDot1, dotList);
            TriDot2 = FindClosestDot(TriDot2, dotList);
            TriDot3 = FindClosestDot(TriDot3, dotList);
        }

        //remember its 5 dots old positions
        oldtoothPos1 = toothPos1;
        oldtoothPos2 = toothPos2;
        oldTriDot1 = TriDot1;
        oldTriDot2 = TriDot2;
        oldTriDot3 = TriDot3;
    }

    void RunShader()
    {
        int kernelIndex = shader.FindKernel("CSMains");
        RenderTexture tex = new RenderTexture(512, 512, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetTexture(kernelIndex, "Result", tex);
        shader.Dispatch(kernelIndex, 512 / 8, 512 / 8, 1);
    }
}

