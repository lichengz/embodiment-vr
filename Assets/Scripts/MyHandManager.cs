using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;


namespace Leap.Unity
{
    public class MyHandManager : Detector
    {
		public IHandModel HandModel = null;  
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
			//Finger finger = HandModel.GetLeapHand().Fingers[0];
			//Debug.Log(finger.TipPosition.ToVector3());
        }

        // public Finger.FingerType FingerName = Finger.FingerType.TYPE_INDEX;
        // private int selectedFingerOrdinal()
        // {
        //     switch (FingerName)
        //     {
        //         case Finger.FingerType.TYPE_INDEX:
        //             return 1;
        //         case Finger.FingerType.TYPE_MIDDLE:
        //             return 2;
        //         case Finger.FingerType.TYPE_PINKY:
        //             return 4;
        //         case Finger.FingerType.TYPE_RING:
        //             return 3;
        //         case Finger.FingerType.TYPE_THUMB:
        //             return 0;
        //         default:
        //             return 1;
        //     }
        // }
    }
}