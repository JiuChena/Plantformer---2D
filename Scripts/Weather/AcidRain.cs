using System;
using System.Collections;
using TarodevController;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AcidRain : WeatherBase
{
    [Header("检测配置")]
    public float checkInterval = 0.5f;
    public float raycastDistance = 10f;
    [Tooltip("天气开始后的宽限期（秒），此期间不进行检测")]
    public float gracePeriod = 2f;

    private PlayerController player;
    private float nextCheckTime;
    private float _startTime;
    private bool _triggered;

    public override void OnStart()
    {
        player = FindObjectOfType<PlayerController>();
        nextCheckTime = Time.time + checkInterval;
        _startTime = Time.time;
        _triggered = false;
    }

    public override void OnUpdate()
    {
        if (player == null) return;
        if (Time.time < nextCheckTime) return;

        // 宽限期内不检测
        if (Time.time < _startTime + gracePeriod)
        {
            nextCheckTime = Time.time + checkInterval;
            return;
        }

        nextCheckTime = Time.time + checkInterval;

        // 1. 雨伞打开 → 始终安全（优先级最高，不依赖射线检测）
        var umbrella = player.GetProp<Umbrella>("Umbrella");
        if (umbrella != null && umbrella.IsOpen) return;

        // 2. 检测头顶上方是否有障碍物（过滤触发器）
        var hit = Physics2D.Raycast(player.transform.position, Vector2.up, raycastDistance, ~player.Stats.PlayerLayer);
        bool hasObstacle = hit.collider != null && !hit.collider.isTrigger;
        if (hasObstacle) return;

        // 3. 无遮挡且雨伞未打开 → 判定失败
        TriggerRestart();
    }

    public override void OnEnd()
    {
        player = null;
    }

    /// <summary>
    /// 触发重新开始，防止重复调用。
    /// </summary>
    private void TriggerRestart()
    {
        if (_triggered) return;
        _triggered = true;

        string currentScene = SceneManager.GetActiveScene().name;

        PanelManager.Instance.PanelDisplay<LoadPanel>("UI/Load/", "Load Panel", UILayer.Top, (panel) =>
        {
            StartCoroutine(StartLoad(panel));
        });
    }

    private IEnumerator StartLoad(LoadPanel panel)
    {
        yield return new WaitForSeconds(1);
        panel.StartLoad(SceneManager.GetActiveScene().name);
    }
}
