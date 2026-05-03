using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class Rain : WeatherBase
{
    [Header("检测配置")]
    public float raycastDistance = 10f;

    private PlayerController player;
    private float originalGroundDeceleration;
    private bool _decelerationRestored;
    
    public override void OnStart()
    {
        player = FindObjectOfType<PlayerController>();
        _decelerationRestored = false;
        if (player != null && player.Stats != null)
        {
            originalGroundDeceleration = player.Stats.GroundDeceleration;
            player.Stats.GroundDeceleration = originalGroundDeceleration * 0.8f;
        }
    }

    public override void OnUpdate()
    {
        if (player == null || _decelerationRestored) return;

        var hit = Physics2D.Raycast(player.transform.position, Vector2.up, raycastDistance, ~player.Stats.PlayerLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            player.Stats.GroundDeceleration = originalGroundDeceleration;
            _decelerationRestored = true;
        }
    }

    public override void OnEnd()
    {
        if (player != null && player.Stats != null)
        {
            player.Stats.GroundDeceleration = originalGroundDeceleration;
        }
    }
}
