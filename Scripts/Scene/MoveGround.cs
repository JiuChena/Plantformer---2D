using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可移动地面：沿路径点列表顺序移动，可配置停留时间和移动速度。
/// 玩家站在该地面上时会跟随移动（需配合PlayerController的移动平台支持）。
/// 建议为挂载此脚本的物体添加Kinematic类型的Rigidbody2D以获得更好的物理性能。
/// </summary>
public class MoveGround : MonoBehaviour
{
    [Header("路径配置")]
    [Tooltip("移动路径点列表（世界坐标），物体将按顺序在这些点之间移动。启动后物体会先传送到第一个路径点")]
    public List<Vector2> waypoints = new List<Vector2>();

    [Header("速度配置")]
    [Tooltip("移动速度（单位/秒）")]
    public float moveSpeed = 3f;

    [Tooltip("在每个路径点的停留时间（秒）")]
    public float stayDuration = 1f;

    [Header("循环配置")]
    [Tooltip("是否循环移动。到达最后一个点后继续循环；否则停在最后一个点")]
    public bool loop = true;

    [Tooltip("循环模式：true为往返移动（PingPong），false为到达末尾后回到第一个点重新开始")]
    public bool pingPong = true;

    /// <summary>
    /// 平台当前帧的移动速度（单位/秒），供PlayerController读取以实现玩家跟随
    /// </summary>
    public Vector2 PlatformVelocity { get; private set; }

    private int _currentTargetIndex = 1;
    private int _direction = 1;
    private float _stayTimer;
    private bool _isStaying;
    private Vector3 _previousPosition;

    private void Awake()
    {
        if (waypoints.Count > 0)
            transform.position = new Vector3(waypoints[0].x, waypoints[0].y, transform.position.z);
        _previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (waypoints.Count < 2) return;

        _previousPosition = transform.position;

        // 停留阶段：在当前路径点等待 stayDuration 秒
        if (_isStaying)
        {
            _stayTimer += Time.fixedDeltaTime;
            if (_stayTimer >= stayDuration)
            {
                _isStaying = false;
                _stayTimer = 0f;
            }
            PlatformVelocity = Vector3.zero;
            return;
        }

        // 移动阶段：向目标路径点移动
        Vector2 target = waypoints[_currentTargetIndex];
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 toTarget = target - currentPos2D;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
        {
            transform.position = new Vector3(target.x, target.y, transform.position.z);
            ArriveAtWaypoint();
        }
        else
        {
            float step = moveSpeed * Time.fixedDeltaTime;
            if (step >= distance)
            {
                transform.position = new Vector3(target.x, target.y, transform.position.z);
                ArriveAtWaypoint();
            }
            else
            {
                Vector2 movement = toTarget.normalized * step;
                transform.position = new Vector3(transform.position.x + movement.x, transform.position.y + movement.y, transform.position.z);
            }
        }

        PlatformVelocity = new Vector2(transform.position.x - _previousPosition.x, transform.position.y - _previousPosition.y) / Time.fixedDeltaTime;
    }

    private void ArriveAtWaypoint()
    {
        _isStaying = true;
        _stayTimer = 0f;
        PlatformVelocity = Vector3.zero;
        AdvanceToNextWaypoint();
    }

    private void AdvanceToNextWaypoint()
    {
        if (_direction > 0)
        {
            // 正向移动到达末尾
            if (_currentTargetIndex >= waypoints.Count - 1)
            {
                if (!loop) return; // 不循环，停在最后一个点

                if (pingPong)
                {
                    _direction = -1;
                    _currentTargetIndex = Mathf.Max(0, _currentTargetIndex - 1);
                }
                else
                {
                    _currentTargetIndex = 0;
                }
            }
            else
            {
                _currentTargetIndex++;
            }
        }
        else
        {
            // 反向移动到达起点
            if (_currentTargetIndex <= 0)
            {
                if (pingPong)
                {
                    _direction = 1;
                    _currentTargetIndex = Mathf.Min(waypoints.Count - 1, _currentTargetIndex + 1);
                }
                else
                {
                    _currentTargetIndex = waypoints.Count - 1;
                }
            }
            else
            {
                _currentTargetIndex--;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // 绘制路径点与连线
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 wp = new Vector3(waypoints[i].x, waypoints[i].y, 0);
            Gizmos.DrawWireSphere(wp, 0.15f);
            if (i > 0)
            {
                Vector3 prev = new Vector3(waypoints[i - 1].x, waypoints[i - 1].y, 0);
                Gizmos.DrawLine(prev, wp);
            }
        }

        // 非PingPong循环模式下绘制末尾到首位的回环线
        if (loop && !pingPong && waypoints.Count > 1)
        {
            Vector3 last = new Vector3(waypoints[waypoints.Count - 1].x, waypoints[waypoints.Count - 1].y, 0);
            Vector3 first = new Vector3(waypoints[0].x, waypoints[0].y, 0);
            Gizmos.DrawLine(last, first);
        }
    }
#endif
}
