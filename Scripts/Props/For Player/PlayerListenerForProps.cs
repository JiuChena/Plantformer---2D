using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.Events;

public class PlayerListenerForProps : MonoBehaviour
{
    public PlayerController player;
    public UnityAction propEvents;

    private void Update()
    {
        if(player._stats.InputEnable) propEvents?.Invoke();
    }
}
