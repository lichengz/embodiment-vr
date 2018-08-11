using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LmHandLightScript : MonoBehaviour
{
    public Transform handLightTrans;

    // Use this for initialization
    void Awake()
    {
        if (!GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingLeapMotion)
        {
            gameObject.SetActive(false);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = handLightTrans.position;
        transform.rotation = handLightTrans.rotation;
    }
}
