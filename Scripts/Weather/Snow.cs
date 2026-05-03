using System;
using TarodevController;
using UnityEngine;

public class Snow : WeatherBase
{
    [Header("积雪配置")]
    [Tooltip("使用 2D_SnowAccumulation Shader 的材质")]
    public Material snowMaterial;
    [Tooltip("开始积雪前的延迟时间（秒）")]
    public float snowDelay = 1f;
    [Tooltip("积雪厚度累积速度（单位/秒）")]
    public float snowSpeed = 0.1f;

    private float _startTime;
    private float _maxSnowAmount;

    private PlayerController player;
    private float _originalGroundDeceleration;
    private bool _decelerationOnSnow;

    public override void OnStart()
    {
        _startTime = Time.time;
        player = FindObjectOfType<PlayerController>();
        if (player != null && player.Stats != null)
        {
            _originalGroundDeceleration = player.Stats.GroundDeceleration;
        }
        _decelerationOnSnow = false;
        if (snowMaterial != null)
        {
            _maxSnowAmount = snowMaterial.GetFloat("_SnowDepth");
            snowMaterial.SetFloat("_SnowAmount", 0f);
        }
    }

    public override void OnUpdate()
    {
        if (snowMaterial != null)
        {
            // 积雪厚度累积
            if (Time.time >= _startTime + snowDelay)
            {
                float current = snowMaterial.GetFloat("_SnowAmount");
                if (current < _maxSnowAmount)
                {
                    float next = Mathf.Min(current + snowSpeed * Time.deltaTime, _maxSnowAmount);
                    snowMaterial.SetFloat("_SnowAmount", next);
                }
            }
        }

        // 地面材质检测：脚下是否为雪地 → 调整减速度
        if (player == null || player.Stats == null) return;

        bool onSnowGround = false;
        var hit = Physics2D.Raycast(player.transform.position, Vector2.down, 2f, ~player.Stats.PlayerLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            var renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.shader != null)
            {
                onSnowGround = renderer.sharedMaterial.shader.name.Contains("2D_SnowAccumulation");
            }
        }

        if (onSnowGround && !_decelerationOnSnow)
        {
            player.Stats.GroundDeceleration = _originalGroundDeceleration * 0.6f;
            _decelerationOnSnow = true;
        }
        else if (!onSnowGround && _decelerationOnSnow)
        {
            player.Stats.GroundDeceleration = _originalGroundDeceleration;
            _decelerationOnSnow = false;
        }
    }

    public override void OnEnd()
    {
        if (snowMaterial != null)
        {
            snowMaterial.SetFloat("_SnowAmount", 0f);
        }
        if (player != null && player.Stats != null && _decelerationOnSnow)
        {
            player.Stats.GroundDeceleration = _originalGroundDeceleration;
            _decelerationOnSnow = false;
        }
    }

    private void OnDisable()
    {
        OnEnd();
    }

    private void OnDestroy()
    {
        OnEnd();
    }
}
