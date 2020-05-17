using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerVibrationManager : MonoBehaviour
{
    public static ControllerVibrationManager Instance;

    private void Awake()
    {
        if(Instance && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void TriggerVibration(AudioClip vibrationAudio, OVRInput.Controller controller)
    {
        OVRHapticsClip clip = new OVRHapticsClip(vibrationAudio);
        
        if (controller == OVRInput.Controller.LTouch)
        {
            //Left Controller
            OVRHaptics.LeftChannel.Preempt(clip);
        }
        else if (controller == OVRInput.Controller.RTouch)
        {
            //Right Controller
            OVRHaptics.RightChannel.Preempt(clip);
        }
    }
    
    public void TriggerVibration(int iteration, int frequency, int strength, OVRInput.Controller controller)
    {
        OVRHapticsClip clip = new OVRHapticsClip();

        for (int i = 0; i < iteration; i++)
        {
            clip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        }

        if (controller == OVRInput.Controller.LTouch)
        {
            //Left Controller
            OVRHaptics.LeftChannel.Preempt(clip);
        }
        else if (controller == OVRInput.Controller.RTouch)
        {
            //Right Controller
            OVRHaptics.RightChannel.Preempt(clip);
        }
    }
}
