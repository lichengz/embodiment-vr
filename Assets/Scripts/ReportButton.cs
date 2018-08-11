using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportButton : MonoBehaviour
{


    public Color defaultColor = Color.Lerp(Color.black, Color.white, 0.1F);

    public Color pressedColor = Color.white;

    private Material _material;

    public bool isSelected = false;
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = GetComponentInChildren<Renderer>();
        }
        if (renderer != null)
        {
            _material = renderer.material;
        }
    }

    void Update()
    {
        if (_material != null)
        {

            // The target color for the Interaction object will be determined by various simple state checks.
            Color targetColor = defaultColor;
            if (isSelected)
            {
                targetColor = pressedColor;
            }
            else
            {
                targetColor = defaultColor;
            }
            // Lerp actual material color to the target color.
            _material.color = Color.Lerp(_material.color, targetColor, 30F * Time.deltaTime);
        }
    }

}
