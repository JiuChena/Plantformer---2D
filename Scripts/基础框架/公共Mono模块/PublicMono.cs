using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PublicMono : MonoBehaviour
{
    private static PublicMono instance;

    public static PublicMono Instance
    {
        get
        {
            if (instance is null)
            {
                GameObject obj = new GameObject("PublicMono");
                obj.AddComponent<PublicMono>();  
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private UnityAction actions;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        actions?.Invoke();
    }

    public void AddActionToMono(UnityAction action)
    {
        actions += action;
    }

    public void RemoveActionFromMono(UnityAction action)
    {
        actions -= action;
    }

    public void ClearActionsInMono()
    {
        actions = null;
    }
}
