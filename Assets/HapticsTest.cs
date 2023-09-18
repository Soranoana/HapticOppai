using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using uOSC;
using System;
using ContactGloveSDK;
using Valve.VR;

public class HapticsTest : MonoBehaviour
{
    [SerializeField] private GameObject contactGloveObj;
    private IContactGloveManager cgManager;
    // Start is called before the first frame update
    void Start()
    {
        this.cgManager = contactGloveObj.GetComponent<IContactGloveManager>();
        Debug.Log("cgManager: " + cgManager);

    }

    void Update()
    {
        // public void SetHaptics(HandSides hand, ColliderFinger_e section, bool enable)
        // SetHaptics(HandSides.Left, ColliderFinger_e.IndexDistal, true);
        // cgManager.SetVibration(HandSides.Left, 0.5f, 160, 0.1f);
        cgManager.SetHaptics(HandSides.Left, ColliderFinger_e.IndexDistal, true);
        Debug.Log("run");

    }
}
