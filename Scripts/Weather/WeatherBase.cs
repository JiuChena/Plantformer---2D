using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherBase : MonoBehaviour
{
    public Vector2 offset;
    public Vector2 size;

    private void Update()
    {
        WeatherFollow();
        OnUpdate();
    }

    private void WeatherFollow()
    {
        if (Camera.main == null) return;
        Vector3 camPos = Camera.main.transform.position;
        transform.position = new Vector3(camPos.x + offset.x, camPos.y + offset.y, transform.position.z);
    }

    public abstract void OnStart();
    public abstract void OnUpdate();
    public abstract void OnEnd();
}
