using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ContactGloveSDK;
using Valve.VR;

public class HowToUse : MonoBehaviour
{
    [SerializeField] private GameObject contactGloveObj;

    // You can communicate with the glove by using IContactGloveManager
    private IContactGloveManager cgManager;
    
    [SerializeField] private GameObject sliderFingerValueSetPrefab;
    private SliderFingerValueSet[] sliderFingerValueSets = new SliderFingerValueSet[15];

    [SerializeField] private bool showFingerValue = false;

    void Start()
    {
        this.cgManager = contactGloveObj.GetComponent<IContactGloveManager>();
        
        // You can set the action invoked when A button is on pressed.
        this.cgManager.AddOnControllerInputHandler(HandSides.Left, ControllerBoolInputType.A, () =>
        {
            cgManager.SetVibration(HandSides.Left, 0.5f, 160, 0.1f);
        });
        
        SetupUi();
    }
    
    void Update()
    {
        // You can set the vibration of the glove by using SetVibration
        // With the current hardware, the frequency is limited to 160Hz
        if (Input.GetKeyDown(KeyCode.F))
        {
            cgManager.SetVibration(HandSides.Left, 0.2f, 160.0f, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            cgManager.SetVibration(HandSides.Right, 0.2f, 160.0f, 0.5f);
        }

        UpdateUi();
    }

    // Here, we will define the UI components
    void SetupUi()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        for (int i = 0; i < 15; i++)
        {
            var sliderFingerValueSet = Instantiate(sliderFingerValueSetPrefab,canvas.transform);
            sliderFingerValueSet.transform.SetParent(canvas.transform);
            sliderFingerValueSets[i] = sliderFingerValueSet.GetComponent<SliderFingerValueSet>();
            
            // We are only going to show the values of the left hand
            sliderFingerValueSets[i].SetTitle("Left " + ((FingerRotationAmplitude_e)i).ToString());
            sliderFingerValueSet.transform.position += new Vector3(0, i * 1.2f, 0);
        }
    }

    void UpdateUi()
    {
        string debugText = "";
        for (int i = 0; i < 15; i++)
        {
            // Here, we will get the finger rotation amplitude
            float flex = cgManager.GetFingerRotationAmplitude(HandSides.Left, (FingerRotationAmplitude_e)i);
            sliderFingerValueSets[i].SetSliderValue(flex);
            
            debugText += "Finger " + (FingerRotationAmplitude_e)i + ": " + flex + "\n";
        }
        if(showFingerValue)
            Debug.Log(debugText);
    }
}
