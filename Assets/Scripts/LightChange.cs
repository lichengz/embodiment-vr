using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChange : MonoBehaviour
{
    public int lightID;
    public string lightCode;
    int stateDuration = 2;
    public Material originalColor, newColor;
    public bool isLightOn;
    public TextMesh num;
    GameObject[] lightPositions;
    float chance;
    bool isReassignNeeded;
    // Use this for initialization
    void Start()
    {
        if (gameObject.tag == "CubeLight")
        {
            if (gameObject.name.Contains("pTask"))
            {
                num.text = gameObject.name.Substring(5, gameObject.name.Length - 5);
            }
            if (gameObject.name.Contains("sTask"))
            {
                num.text = (System.Int32.Parse(gameObject.name.Substring(5, gameObject.name.Length - 5)) + 8).ToString();
            }
        }
        chance = Random.Range(0f, 1f);
        if (chance < 0.5f)
        {
            GetComponent<Renderer>().material.color = newColor.color;
            isLightOn = true;
        }
        StartCoroutine(ExecuteLightChange());
        isReassignNeeded = GameObject.Find("GameManager").GetComponent<GameManagerScript>().isReassigningNeeded;
    }

    // Update is called once per frame
    void Update()
    {
        if (isReassignNeeded)
        {
            lightPositions = GameObject.FindGameObjectsWithTag("CubeLightPos");
            foreach (GameObject go in lightPositions)
            {
                if (gameObject.name.Substring(0, 5) + "Pos" + gameObject.name.Substring(5, gameObject.name.Length - 5) == go.name)
                {
                    transform.position = go.transform.position;
                    transform.rotation = go.transform.rotation;
                    gameObject.transform.parent = go.transform;
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                    gameObject.transform.localScale = Vector3.one;
                }
            }
        }
    }
    IEnumerator ExecuteLightChange()
    {
        for (int i = 0; i < lightCode.Length; i++)
        {
            yield return new WaitForSeconds(stateDuration);
            ChangeColor(lightCode[i]);
        }
    }
    void ChangeColor(char code)
    {
        // if (code == '0')
        // {
        //     GetComponent<Renderer>().material.color = originalColor.color;
        //     isLightOn = false;
        // }
        // else
        // {
        //     GetComponent<Renderer>().material.color = newColor.color;
        //     isLightOn = true;
        // }
        if (code == '1')
        {
            if (isLightOn)
            {
                GetComponent<Renderer>().material.color = originalColor.color;
                isLightOn = false;
            }
            else
            {
                GetComponent<Renderer>().material.color = newColor.color;
                isLightOn = true;
            }
        }
    }
}
