using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System.Diagnostics;

public class GameManagerScript : MonoBehaviour
{
    //controll which study case is in use
    public string userID;
    public bool isUsingLeapMotion;
    public bool isUsingKinect;
    public bool isUsingController;
    public bool isToolBroken;
    string gameMode;
    public GameObject scoreBoardText;
    private TextMesh announcement;
    // the word requried for the task
    //public string[] words;
    public string[] words;
    public int totalNumOfWords;
    public string randomUselessLetters = "abcdefghijklmnopqrstuvwxyz";
    string numbers = "012345678";
    private int numberOfCha;
    // spawn base and cubes
    public GameObject basePrefab;
    public GameObject cubePrefab;
    public GameObject[] cubes;
    public GameObject logger;
    public int wordIndex; // count the progress of how many words have been done

    //light controller
    public int totalNumOfLights;
    public int experimentLength = 5;
    string[] lightCodes = new string[30];
    GameObject[] handLights;
    GameObject[] toolLights;
    GameObject[] pCubeLights;
    GameObject[] sCubeLights;
    GameObject[] lightNums;
    bool isNumOn;
    //Report
    [SerializeField]
    bool isGamePaused = false;
    int panelLength = 6;
    int panelWidth = 3;
    GameObject selectedButtonObject;
    [SerializeField]
    int selectedButtonNumber;
    int newSeletedNumber;
    GameObject[] reportButtons;

    //new system
    [SerializeField]
    int taskIndex = 0;
    public bool isReassigningNeeded;

    //My Log System
    Stopwatch stopWatch = new Stopwatch();
    float logRate = 0.1f;
    float nextLog = 0;
    string dataOutputPath;
    string rawLogPath;
    string reportSummeryPath;
    public GameObject cubeLightPrefab;
    public List<GameObject> pTasks = new List<GameObject>();
    public List<GameObject> sTasks = new List<GameObject>();
    List<string[]> LightChangeData = new List<string[]>();
    bool needReassignLightGroups;
    //raw log
    List<string[]> RawData = new List<string[]>();
    int[] reportSummery = new int[30];
    List<string[]> rptSumData = new List<string[]>();

    string[] events = { "grab cube", "drop cube", "move cube", "finish letter", "finish word", "-", "report", "new word" };
    string[] commonLogHead = { "time", "event", "detail" };
    string[] rawDataHead = { "headx", "heady", "headz", "targetx", "targety", "targetz", "toolx", "tooly", "toolz", "toolyaw", "toolpitch", "toolroll", "pinchforce" };
    Dictionary<string, int> rawLogIndexDict = new Dictionary<string, int>()
    {
        {"time", 0},
        {"event", 1},
        {"detail", 2},
        {"headx", 3},
        {"heady", 4},
        {"headz", 5},
        {"targetx", 6},
        {"targety", 7},
        {"targetz", 8},
        {"toolx", 9},
        {"tooly", 10},
        {"toolz", 11},
        {"toolyaw", 12},
        {"toolpitch", 13},
        {"toolroll", 14},
        {"pinchforce", 15},
    };
    public GameObject rightController;
    Transform headsetTrans;
    Vector3 targetPos = Vector3.zero;
    Vector3 toolPos = Vector3.zero;
    Transform toolTrans;
    GameObject grabber;
    float pinchForce = 0;
    bool pauseEventToggle;
    bool resumeEventToggle;
    bool newWordToggle;
    bool reportToggle;
    string rptInputStr = "";
    void Awake()
    {
        if (!GameObject.Find("LogManager(Clone)"))
        {
            //Instantiate(logger, Vector3.zero, Quaternion.identity);
        }
        // wordIndex = GameObject.Find("LogManager(Clone)").GetComponent<TrialLogger>().currentTrialNumber - 1;
        // if (wordIndex < 0)

        // {
        //     wordIndex = 0;
        // }
        LoadWordText();
        totalNumOfWords = words.Length;
    }
    // Use this for initialization
    void Start()
    {
        if (isUsingLeapMotion) gameMode = "LM";
        if (isUsingKinect) gameMode = "KN";
        if (isUsingController) gameMode = "OT";
        announcement = scoreBoardText.GetComponent<TextMesh>();
        // numberOfCha = words[wordIndex].Length;
        // BaseSpawn(numberOfCha, words[wordIndex]);
        // CubeSpawn(numberOfCha, words[wordIndex]);
        SpawnEverything(0);
        CubeLightSpawn();
        AssignToolAndHandLights();
        AssignLights();
        //------------------ above is new system------------------------
        StackGameStartAnnouncement();
        InitReportButtons();
        reportButtons = GameObject.FindGameObjectsWithTag("ReportButton");
        //-----------------log---------------
        dataOutputPath = Application.dataPath + "/StreamingAssets" + "/output" + "/" + userID +"_" + gameMode + "_light_change" + ".csv";
        rawLogPath = Application.dataPath + "/StreamingAssets" + "/output" + "/" + userID +"_" + gameMode + "_raw_log" + ".csv";
        reportSummeryPath = Application.dataPath + "/StreamingAssets" + "/output" + "/" + userID +"_" + gameMode + "_report_summery" + ".csv";
        headsetTrans = GameObject.Find("CenterEyeAnchor").transform;
        if (isUsingController)
        {
            grabber = rightController.transform.GetChild(2).gameObject;
        }
        else
        {
            grabber = GameObject.Find("ToolWrapper").transform.GetChild(2).gameObject;

        }
        stopWatch.Start();
        for (int i = 0; i < reportSummery.Length; i++)
        {
            reportSummery[i] = 0;
        }
        lightNums = GameObject.FindGameObjectsWithTag("LightNum");
    }

    void AssignUselessLetter()
    {
        foreach (char letter in randomUselessLetters)
        {
            foreach (char wordletter in words[taskIndex])
            {
                if (letter == wordletter)
                {
                    randomUselessLetters = randomUselessLetters.Replace(letter.ToString(), string.Empty);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isReassigningNeeded)
        {
            isReassigningNeeded = false;
        }
        //Pause
        // if (isGamePaused)
        // {
        //     Time.timeScale = 0;
        // }
        // else
        // {
        //     Time.timeScale = 1;
        // }
        //has to run this after all the existing lights are destroyed, which is one frame later
        // if (needReassignLightGroups)
        // {
        //     needReassignLightGroups = false;
        //     //divide lights into primary and secondary tasks
        //     pTasks.Clear();
        //     sTasks.Clear();
        //     GameObject[] cubeLights = GameObject.FindGameObjectsWithTag("CubeLight");
        //     foreach (GameObject go in cubeLights)
        //     {
        //         if (go.name.Contains("pTask")) pTasks.Add(go);
        //         if (go.name.Contains("sTask")) sTasks.Add(go);
        //     }
        //     totalNumOfLights = pTasks.Count + sTasks.Count;
        // }
        //Collecting data for raw log=======================
        if (isUsingController)
        {
            toolPos = rightController.transform.position;
            toolTrans = rightController.transform;
        }
        else
        {
            toolPos = GameObject.Find("ToolWrapper").transform.position;
            toolTrans = GameObject.Find("ToolWrapper").transform;
        }
        //=================================================
        UpdateAnnouncement();
        PauseGame();
        ReceiveReportInput();
        //ReportPanelController();
        UpdateLog();
    }

    void ReceiveReportInput()
    {
        if (!reportToggle) return;
        foreach (char c in Input.inputString)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), @"^\d+$")) rptInputStr += c;
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            reportToggle = false;
            announcement.text = rptInputStr + " is reported\nready to continue?";
        }
    }

    void AssignToolAndHandLights()
    {
        toolLights = GameObject.FindGameObjectsWithTag("ToolLight");
        for (int i = 0; i < toolLights.Length; i++)
        {
            toolLights[i].GetComponent<LightChange>().num.text = (System.Int32.Parse(toolLights[i].name[toolLights[i].name.Length - 1].ToString()) + 18).ToString();
        }
        handLights = GameObject.FindGameObjectsWithTag("HandLight");
        for (int i = 0; i < handLights.Length; i++)
        {
            handLights[i].GetComponent<LightChange>().num.text = (System.Int32.Parse(handLights[i].name[handLights[i].name.Length - 1].ToString()) + 27).ToString();
        }
        //cube lights
        pTasks.Clear();
        sTasks.Clear();
        GameObject[] cubeLights = GameObject.FindGameObjectsWithTag("CubeLight");
        foreach (GameObject go in cubeLights)
        {
            if (go.name.Contains("pTask")) pTasks.Add(go);
            if (go.name.Contains("sTask")) sTasks.Add(go);
        }
        totalNumOfLights = pTasks.Count + sTasks.Count;
        pCubeLights = pTasks.ToArray();
        sCubeLights = sTasks.ToArray();
    }

    void SpawnEverything(int index)
    {
        numbers = "012345678";
        randomUselessLetters = "abcdefghijklmnopqrstuvwxyz";
        AssignUselessLetter();
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (GameObject go in cubes) Destroy(go);
        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        foreach (GameObject go in bases) Destroy(go);

        numberOfCha = words[index].Length;
        BaseSpawn(numberOfCha, words[index]);
        CubeSpawn(numberOfCha, words[index]);
        //AssignLights();
        needReassignLightGroups = true;
        newWordToggle = true;
        isReassigningNeeded = true;
    }

    void LoadWordText()
    {
        var sr = new StreamReader(Application.dataPath + "/StreamingAssets/" + "4 letters words 1-1.txt");
        var fileContents = sr.ReadToEnd();
        sr.Close();

        string[] lines = fileContents.Split("\n"[0]);
        List<string> wordList = new List<string>();
        foreach (string word in lines)
        {
            //string tmp = word.Remove(word.Length - 1, 1);
            string tmp = "";
            if (word.Length == 5) tmp = word.Remove(word.Length - 1, 1);
            wordList.Add(tmp);
        }
        words = wordList.ToArray();
        //randomize the words array
        System.Random rand = new System.Random();
        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < words.Length - 1; i++)
        {
            int j = rand.Next(i, words.Length);
            string temp = words[i];
            words[i] = words[j];
            words[j] = temp;
        }
    }
    void InitReportButtons()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("ReportButton");
        foreach (GameObject button in buttons)
        {
            string idStr;
            if (button.name[button.name.Length - 2] == '0')
            {
                idStr = button.name[button.name.Length - 1].ToString();
            }
            else
            {
                idStr = button.name.Substring(button.name.Length - 2, 2).ToString();
            }
            button.transform.GetChild(1).GetComponent<TextMesh>().text = idStr;
            //set button1 to be the first selected
            selectedButtonNumber = 1;
            newSeletedNumber = 1;
            if (button.name.Contains("01"))
            {
                selectedButtonObject = button;
                selectedButtonObject.GetComponent<ReportButton>().isSelected = true;
            }
        }

    }
    void BaseSpawn(int num, string word)
    {
        for (int i = 0; i < num; i++)
        {
            string baseName = "BaseSpawn" + i.ToString();
            Vector3 spawnPos = GameObject.Find(baseName).transform.position;
            GameObject newBase = (GameObject)Instantiate(basePrefab, spawnPos, Quaternion.Euler(0, -40, 0));
            newBase.name = "Base_" + word[i].ToString();
            Transform newBaseTextTransform = newBase.transform.GetChild(0);
            GameObject newBaseText = newBaseTextTransform.gameObject;
            newBaseText.GetComponent<TextMesh>().text = word[i].ToString();
        }
    }
    void CubeSpawn(int num, string word)
    {
        for (int i = 0; i < num; i++)
        {
            int spawnIndex = Random.Range(0, numbers.Length - 1);
            string cubeName = "CubeSpawn" + numbers[spawnIndex];
            numbers = numbers.Remove(spawnIndex, 1);
            Vector3 spawnPos = GameObject.Find(cubeName).transform.position;
            GameObject newCube = (GameObject)Instantiate(cubePrefab, spawnPos, Quaternion.Euler(0, 40, 0));
            int index = Random.Range(0, word.Length - 1);
            newCube.name = "Cube_" + word[index].ToString();
            Transform newCubeTextTransform = newCube.transform.GetChild(0);
            GameObject newCubeText = newCubeTextTransform.gameObject;
            newCubeText.GetComponent<TextMesh>().text = word[index].ToString();
            word = word.Remove(index, 1);
            //assign primary task name
            GameObject light1 = newCube.transform.GetChild(1).gameObject;
            GameObject light2 = newCube.transform.GetChild(2).gameObject;
            light1.name = "pTaskPos" + (2 * (i + 1) - 1).ToString();
            //light1.GetComponent<LightChange>().num.text = (2 * (i + 1) - 1).ToString();
            light2.name = "pTaskPos" + (2 * (i + 1)).ToString();
            //light2.GetComponent<LightChange>().num.text = (2 * (i + 1)).ToString();
        }
        for (int i = 0; i < 9 - num; i++)
        {
            int spawnIndex = Random.Range(0, numbers.Length - 1);
            string cubeName = "CubeSpawn" + numbers[spawnIndex];
            numbers = numbers.Remove(spawnIndex, 1);
            Vector3 spawnPos = GameObject.Find(cubeName).transform.position;
            GameObject newCube = (GameObject)Instantiate(cubePrefab, spawnPos, Quaternion.Euler(0, 40, 0));
            int index = Random.Range(0, randomUselessLetters.Length - 1);
            newCube.name = "Cube_" + randomUselessLetters[index].ToString();
            Transform newCubeTextTransform = newCube.transform.GetChild(0);
            GameObject newCubeText = newCubeTextTransform.gameObject;
            newCubeText.GetComponent<TextMesh>().text = randomUselessLetters[index].ToString();
            randomUselessLetters = randomUselessLetters.Remove(index, 1);
            //assign primary task name
            GameObject light1 = newCube.transform.GetChild(1).gameObject;
            GameObject light2 = newCube.transform.GetChild(2).gameObject;
            light1.name = "sTaskPos" + (2 * (i + 1) - 1).ToString();
            //light1.GetComponent<LightChange>().num.text = (2 * (i + 1) - 1 + num * 2).ToString();
            light2.name = "sTaskPos" + (2 * (i + 1)).ToString();
            //light2.GetComponent<LightChange>().num.text = (2 * (i + 1) + num * 2).ToString();
        }
    }

    void CubeLightSpawn()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject cubeLight = (GameObject)Instantiate(cubeLightPrefab, Vector3.zero, Quaternion.identity);
            cubeLight.name = "pTask" + (i + 1).ToString();
        }
        for (int i = 0; i < 10; i++)
        {
            GameObject cubeLight = (GameObject)Instantiate(cubeLightPrefab, Vector3.zero, Quaternion.identity);
            cubeLight.name = "sTask" + (i + 1).ToString();
        }
    }

    void AssignLights()
    {
        // GameObject[] lights = GameObject.FindGameObjectsWithTag("CubeLight");
        // for (int i = 0; i < lights.Length; i++)
        // {
        //     string codeString = "";
        //     for (int j = 0; j < experimentLength; j++)
        //     {
        //         int code = Random.Range(0, 2);
        //         codeString += code.ToString();
        //     }
        //     lights[i].GetComponent<LightChange>().lightCode = codeString;
        //     GameObject num1 = lights[i].transform.parent.gameObject.transform.GetChild(1).gameObject;
        //     GameObject num2 = lights[i].transform.parent.gameObject.transform.GetChild(2).gameObject;
        //     if (lights[i].name[lights[i].name.Length - 1] == '1')
        //     {
        //         num1.GetComponent<TextMesh>().text = (i + 1).ToString();
        //     }
        //     if (lights[i].name[lights[i].name.Length - 1] == '2')
        //     {
        //         num2.GetComponent<TextMesh>().text = (i + 1).ToString();
        //     }
        // }
        string ulti = "";
        for (int i = 0; i < 180; i++)
        {
            ulti += "1";
        }
        for (int i = 0; i < lightCodes.Length; i++)
        {
            lightCodes[i] = "";
        }
        List<int> nums = new List<int>();
        for (int i = 0; i < 30; i++)
        {
            nums.Add(i);
        }
        System.Random rnd = new System.Random();
        for (int i = 0; i < 180; i++)
        {
            //int index = Random.Range(0, nums.Count - 1);
            int index = rnd.Next(0, nums.Count);
            //UnityEngine.Debug.Log(nums.Count);
            int numAtIndex = nums[index];
            for (int j = 0; j < 30; j++)
            {
                //UnityEngine.Debug.Log(nums[index]);
                if (j == numAtIndex)
                {
                    lightCodes[j] += "1";
                    int tmp = 0;
                    foreach (char c in lightCodes[j])
                    {
                        if (c == '1') tmp++;
                    }
                    if (tmp == 6)
                    {
                        //nums.Remove(j);
                        for (int k = 0; k < nums.Count; k++)
                        {
                            if (nums[k] == j) nums.RemoveAt(k);
                        }
                    }
                }
                else
                {
                    lightCodes[j] += "0";
                }
            }
        }
        //Start assigning light codes to cube, tool and hand
        // GameObject[] cubeLights = GameObject.FindGameObjectsWithTag("CubeLight");
        GameObject[] lights = GameObject.FindGameObjectsWithTag("CubeLight");
        //UnityEngine.Debug.Log(pCubeLights.Length + " " + sCubeLights.Length + " " + toolLights.Length + " " + handLights.Length + " " + lights.Length);
        foreach (GameObject go in pCubeLights)
        {
            go.GetComponent<LightChange>().lightCode = lightCodes[System.Int32.Parse(go.name[go.name.Length - 1].ToString()) - 1];
        }
        foreach (GameObject go in sCubeLights)
        {
            go.GetComponent<LightChange>().lightCode = lightCodes[System.Int32.Parse(go.name[go.name.Length - 1].ToString()) + 8 - 1];
        }
        foreach (GameObject go in toolLights)
        {
            go.GetComponent<LightChange>().lightCode = lightCodes[System.Int32.Parse(go.name[go.name.Length - 1].ToString()) + 18 - 1];
        }
        foreach (GameObject go in handLights)
        {
            go.GetComponent<LightChange>().lightCode = lightCodes[System.Int32.Parse(go.name[go.name.Length - 1].ToString()) + 27 - 1];
        }
    }

    //once all cubes are in the required position, update the broadcast board
    void UpdateAnnouncement()
    {
        bool flag = true;
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Base");
        foreach (GameObject go in gos)
        {
            flag = flag && go.GetComponent<BaseScript>().done;
        }
        if (flag || Input.GetKeyDown(KeyCode.Space))
        {
            // if (wordIndex == words.Length - 1)
            // {
            //     announcement.text = "YOU WON!";
            // }
            // else
            // {
            //     GameObject.Find("LogManager(Clone)").GetComponent<TrialLogger>().ReloadScene();
            // }
            taskIndex++;
            SpawnEverything(taskIndex);
        }
    }

    void StackGameStartAnnouncement()
    {
        announcement.text = "Welcome!";
    }

    //==================Pause and report====================
    void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isGamePaused)
            {
                announcement.text = "Game running";
                Time.timeScale = 1;
                isGamePaused = false;
                resumeEventToggle = true;
            }
            else
            {
                announcement.text = "Game paused\nTime for report";
                //Time.timeScale = 0;
                StartCoroutine(SetTimeScaleTo0());
                isGamePaused = true;
                pauseEventToggle = true;
                reportToggle = true;
                rptInputStr = "";
            }
            if (isNumOn)
            {
                foreach (GameObject go in lightNums)
                {
                    go.GetComponent<MeshRenderer>().enabled = false;
                }
                isNumOn = false;
            }
            else
            {
                foreach (GameObject go in lightNums)
                {
                    go.GetComponent<MeshRenderer>().enabled = true;
                }
                isNumOn = true;
            }

        }
    }


    IEnumerator SetTimeScaleTo0()
    {
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 0;
    }

    void ReportPanelController()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            newSeletedNumber = selectedButtonNumber - 1;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            newSeletedNumber = selectedButtonNumber + 1;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            int oldNum = selectedButtonNumber;
            newSeletedNumber = selectedButtonNumber - panelLength;
            if (newSeletedNumber < 1) newSeletedNumber = oldNum;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            int oldNum = selectedButtonNumber;
            newSeletedNumber = selectedButtonNumber + panelLength;
            if (newSeletedNumber > reportButtons.Length) newSeletedNumber = oldNum;
        }
        if (newSeletedNumber < 1) newSeletedNumber = 1;
        if (newSeletedNumber > reportButtons.Length) newSeletedNumber = reportButtons.Length;
        if (newSeletedNumber != selectedButtonNumber)
        {
            ChangeSeletedButton();
        }
    }
    void ChangeSeletedButton()
    {
        string seletedNumStr;
        if (selectedButtonNumber < 10)
        {
            seletedNumStr = "0" + selectedButtonNumber.ToString();
        }
        else
        {
            seletedNumStr = selectedButtonNumber.ToString();
        }
        string newSeletedNumStr;
        if (newSeletedNumber < 10)
        {
            newSeletedNumStr = "0" + newSeletedNumber.ToString();
        }
        else
        {
            newSeletedNumStr = newSeletedNumber.ToString();
        }
        selectedButtonNumber = newSeletedNumber;
        foreach (GameObject button in reportButtons)
        {
            if (button.name.Contains(seletedNumStr))
            {
                button.GetComponent<ReportButton>().isSelected = false;
            }
            if (button.name.Contains(newSeletedNumStr))
            {
                button.GetComponent<ReportButton>().isSelected = true;
            }
        }
    }

    //==================================LOGGING===========================
    void UpdateLog()
    {
        if (Time.time > nextLog)
        //if (stopWatch.ElapsedMilliseconds > nextLog)
        {
            nextLog += logRate;
            //UpdateLightChange();
            UpdateRawData();
        }
        RaycastHit hit;
        Physics.Raycast(headsetTrans.position, headsetTrans.rotation * Vector3.forward, out hit, 3f);
        //Debug.DrawRay(headsetTrans.position, headsetTrans.rotation * Vector3.forward * 3f);
        targetPos = hit.point;
    }
    //=========================Light Change and Report====================
    void UpdateLightChange()
    {
        string[] rowDataTemp = new string[totalNumOfLights + 2];
        rowDataTemp[0] = Time.time.ToString();
        rowDataTemp[1] = words[taskIndex];
        for (int i = 0; i < pTasks.Count; i++)
        {
            if (pTasks[i].GetComponent<LightChange>().isLightOn)
            {
                rowDataTemp[i + 2] = "1";
            }
            else
            {
                rowDataTemp[i + 2] = "0";
            }
        }
        for (int i = 0; i < sTasks.Count; i++)
        {
            if (sTasks[i].GetComponent<LightChange>().isLightOn)
            {
                rowDataTemp[i + 2 + pTasks.Count] = "1";
            }
            else
            {
                rowDataTemp[i + 2 + pTasks.Count] = "0";
            }
        }
        LightChangeData.Add(rowDataTemp);
    }
    void FinishLightChangeLog()
    {
        string[] rowDataTemp = new string[totalNumOfLights + 2];
        rowDataTemp[0] = "time";
        rowDataTemp[1] = "word";
        for (int i = 0; i < pTasks.Count; i++)
        {
            rowDataTemp[i + 2] = pTasks[i].name;
        }
        for (int i = 0; i < sTasks.Count; i++)
        {
            rowDataTemp[i + 2 + pTasks.Count] = sTasks[i].name;
        }
        LightChangeData.Insert(0, rowDataTemp);

        string[][] output = new string[LightChangeData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = LightChangeData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        StreamWriter outStream = System.IO.File.CreateText(dataOutputPath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    //======================================RAW DATA==================================
    void UpdateRawData()
    {
        string[] rowDataTemp = new string[commonLogHead.Length + rawDataHead.Length];
        rowDataTemp[rawLogIndexDict["time"]] = (stopWatch.ElapsedMilliseconds / 1000f).ToString();
        rowDataTemp[rawLogIndexDict["event"]] = "-";
        rowDataTemp[rawLogIndexDict["detail"]] = "-";
        if (pauseEventToggle)
        {
            rowDataTemp[rawLogIndexDict["event"]] = "pause";
            pauseEventToggle = false;
            //UnityEngine.Debug.Log(stopWatch.ElapsedMilliseconds / 1000f);
        }
        if (resumeEventToggle)
        {
            rowDataTemp[rawLogIndexDict["event"]] = "resume";
            resumeEventToggle = false;
            if (rptInputStr != "")
            {
                rowDataTemp[rawLogIndexDict["detail"]] = rptInputStr;
                int index = System.Int32.Parse(rptInputStr) - 1;
                reportSummery[index]++;
            }
        }
        if (newWordToggle)
        {
            rowDataTemp[rawLogIndexDict["event"]] = "new word";
            rowDataTemp[rawLogIndexDict["detail"]] = words[taskIndex];
            newWordToggle = false;
        }
        rowDataTemp[rawLogIndexDict["headx"]] = headsetTrans.position.x.ToString();
        rowDataTemp[rawLogIndexDict["heady"]] = headsetTrans.position.y.ToString();
        rowDataTemp[rawLogIndexDict["headz"]] = headsetTrans.position.z.ToString();
        rowDataTemp[rawLogIndexDict["targetx"]] = targetPos.x.ToString();
        rowDataTemp[rawLogIndexDict["targety"]] = targetPos.y.ToString();
        rowDataTemp[rawLogIndexDict["targetz"]] = targetPos.z.ToString();
        rowDataTemp[rawLogIndexDict["toolx"]] = toolPos.x.ToString();
        rowDataTemp[rawLogIndexDict["tooly"]] = toolPos.y.ToString();
        rowDataTemp[rawLogIndexDict["toolz"]] = toolPos.z.ToString();
        rowDataTemp[rawLogIndexDict["toolyaw"]] = toolTrans.rotation.y.ToString();
        rowDataTemp[rawLogIndexDict["toolpitch"]] = toolTrans.rotation.x.ToString();
        rowDataTemp[rawLogIndexDict["toolroll"]] = toolTrans.rotation.z.ToString();
        rowDataTemp[rawLogIndexDict["pinchforce"]] = GetGrabForce().ToString();
        if (!(isGamePaused && rowDataTemp[rawLogIndexDict["event"]] == "-")) RawData.Add(rowDataTemp);
    }

    float GetGrabForce()
    {
        float force = 0.1f - grabber.GetComponent<GrabberDetect>().force;
        if (force > 0.1) force = 0.1f;
        if (force < 0) force = 0;
        force *= 10f;
        return force;
    }
    void FinishRawDataLog()
    {
        string[] rowDataTemp = new string[commonLogHead.Length + rawDataHead.Length];
        for (int i = 0; i < commonLogHead.Length; i++)
        {
            rowDataTemp[i] = commonLogHead[i];
        }
        for (int i = 0; i < rawDataHead.Length; i++)
        {
            rowDataTemp[i + commonLogHead.Length] = rawDataHead[i];
        }
        RawData.Insert(0, rowDataTemp);

        string[][] output = new string[RawData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = RawData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        StreamWriter outStream = System.IO.File.CreateText(rawLogPath);
        outStream.WriteLine(sb);
        outStream.Close();
    }
    void FinishReportSummery()
    {
        string[] sumHead = new string[2];
        sumHead[0] = "light#";
        sumHead[1] = "report times";
        rptSumData.Add(sumHead);
        for (int i = 0; i < pCubeLights.Length + sCubeLights.Length + toolLights.Length + handLights.Length; i++)
        {
            string[] tmp = new string[2];
            if (i < 8)
            {
                tmp[0] = "pTask" + (i + 1).ToString();
            }
            if (i >= 8 && i < 18)
            {
                tmp[0] = "sTask" + (i + 1 - 8).ToString();
            }
            if (i >= 18 && i < 27)
            {
                tmp[0] = "tool" + (i + 1 - 18).ToString();
            }
            if (i >= 27 && i < 30)
            {
                tmp[0] = "hand" + (i + 1 - 27).ToString();
            }
            tmp[1] = reportSummery[i].ToString();
            rptSumData.Add(tmp);
        }
        string[][] output = new string[rptSumData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rptSumData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        StreamWriter outStream = System.IO.File.CreateText(reportSummeryPath);
        outStream.WriteLine(sb);
        outStream.Close();
    }
    //=====================Finish log and Generate File=====================
    void FinishLog()
    {
        //FinishLightChangeLog();
        FinishRawDataLog();
        FinishReportSummery();
    }

    void OnApplicationQuit()
    {
        FinishLog();
    }
}
