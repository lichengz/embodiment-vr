/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using Leap.Unity;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleInterationGlow : MonoBehaviour
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
