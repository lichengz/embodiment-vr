using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrialLogger : MonoBehaviour
{

    public int currentTrialNumber = 0;
    List<string> header;
    List<string> codeAndReportList;
    List<string> menu;
    [HideInInspector]
    public Dictionary<string, string> trial;
    [HideInInspector]
    public string outputFolder;

    bool trialStarted = false;
    string ppid;
    string dataOutputPath;
    List<string> output;

    // Use this for initialization
    void Awake()
    {
        outputFolder = Application.dataPath + "/StreamingAssets" + "/output";
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
    }

    void Start()
    {
        StartTrial();
    }

    // Update is called once per frame
    void Update()
    {
        // if (currentTrialNumber < GameObject.Find("GameManager").GetComponent<GameManagerScript>().totalNumOfWords)
        // {
        //     if (Input.GetKeyDown(KeyCode.Space)) ReloadScene();
        // }
        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     trial[menu[0]] = "100";
        // }
    }

    public void Initialize(string participantID, List<string> customCodeAndReportList, List<string> customHeader)
    {
        ppid = participantID;
        header = customHeader;
        codeAndReportList = customCodeAndReportList;
        menu = customHeader;
        InitHeader();
        InitDict();
        output = new List<string>();
        output.Add(string.Join(",", header.ToArray()));
        dataOutputPath = outputFolder + "/" + participantID + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
    }

    private void InitHeader()
    {
        header.Insert(0, "number");
        header.Insert(1, "ppid");
        header.Insert(2, "start_time");
        header.Insert(3, "end_time");
    }

    private void InitDict()
    {
        trial = new Dictionary<string, string>();
        foreach (string value in header)
        {
            trial.Add(value, "");
        }
    }

    public void StartTrial()
    {
        trialStarted = true;
        currentTrialNumber += 1;
        InitDict();
        trial["number"] = currentTrialNumber.ToString();
        trial["ppid"] = ppid;
        trial["start_time"] = Time.time.ToString();
        // for (int i = 0; i < menu.Count; i++)
        // {
        //     trial[menu[i]] = codeAndReportList[i];
        // }
        Debug.Log("Start Trial");
    }

    public void EndTrial()
    {
        if (output != null && dataOutputPath != null)
        {
            if (trialStarted)
            {
                trial["end_time"] = Time.time.ToString();
                output.Add(FormatTrialData());
                trialStarted = false;
            }
            else Debug.LogError("Error ending trial - Trial wasn't started properly");

        }
        else Debug.LogError("Error ending trial - TrialLogger was not initialsed properly");

        Debug.Log("End Trial");
    }

    private string FormatTrialData()
    {
        List<string> rowData = new List<string>();
        foreach (string value in header)
        {
            rowData.Add(trial[value]);
        }
        return string.Join(",", rowData.ToArray());
    }

    private void OnApplicationQuit()
    {
        EndTrial();
        if (output != null && dataOutputPath != null)
        {
            File.WriteAllLines(dataOutputPath, output.ToArray());
            Debug.Log(string.Format("Saved data to {0}.", dataOutputPath));
        }
        else Debug.LogError("Error saving data - TrialLogger was not initialsed properly");
        //reset counter
        currentTrialNumber = 0;
    }

    public void ReloadScene()
    {
        EndTrial();
        //SceneManager.LoadScene(0);
        StartTrial();
    }
}
