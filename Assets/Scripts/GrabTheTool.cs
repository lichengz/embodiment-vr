using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTheTool : MonoBehaviour
{
    bool isUsingLeapMotion;

    // Use this for initialization
    void Start()
    {
        isUsingLeapMotion = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isUsingLeapMotion;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "TiggerOfTool" && isUsingLeapMotion)
        {
            GameObject.Find("ToolWrapper").GetComponent<Tool>().start = true;
            GameObject.Find("ToolWrapper").GetComponent<Tool>().isAboveTable = true;
        }
    }
}
