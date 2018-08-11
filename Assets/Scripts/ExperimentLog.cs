using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentLog : MonoBehaviour
{
    TrialLogger trialLogger;
    public string participantID = "player";
    public int numOfLights = 18;
    public List<string> columnList = new List<string>();
    public List<string> codeAndReportList = new List<string>();
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < numOfLights; i++)
        {
            columnList.Add("led" + (i + 1).ToString() + "-code");
            columnList.Add("led" + (i + 1).ToString() + "-rpt");
        }

        // initialise trial logger
        trialLogger = GetComponent<TrialLogger>();
        trialLogger.Initialize(participantID, codeAndReportList, columnList);

        // here we start the first trial immediately, you can start it at any time
        //trialLogger.StartTrial();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //trialLogger.StartTrial();
        }
    }
}
