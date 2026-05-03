using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 箭头飞行脚本，挂载在箭头预制体上，接收轨迹点列表并沿轨迹飞行。
/// 对象池复用：通过 Init 重置状态，避免频繁 Instantiate/Destroy。
/// </summary>
public class BowArrow : MonoBehaviour
{
    public AudioClip fireSound;
    public AudioClip boomAC;
    
    // ── 运行时状态 ──────────────────────────────────────────

    /// <summary>预计算的轨迹点列表</summary>
    private List<Vector3> trajectoryPoints;

    /// <summary>飞行速度（单位/秒）</summary>
    private float speed;

    /// <summary>轨迹末端是否命中了障碍物</summary>
    private bool hitObstacle;

    /// <summary>障碍物检测层级</summary>
    private LayerMask obstacleLayer;

    /// <summary>是否超出最大模拟距离</summary>
    private bool exceededMaxDistance;

    /// <summary>当前目标轨迹点索引</summary>
    private int currentPointIndex;

    /// <summary>是否已到达最后一个轨迹点，进入"惯性飞行"阶段</summary>
    private bool isFreeFlying;

    /// <summary>惯性飞行时的方向向量（单位向量）</summary>
    private Vector3 freeFlyDirection;

    /// <summary>惯性飞行计时器（秒）</summary>
    private float freeFlyTimer;

    /// <summary>惯性飞行持续时间（秒）</summary>
    private const float FREE_FLY_DURATION = 2f;

    private ParticleSystem ps;

    /// <summary>是否已执行归还，防止重复归还</summary>
    private bool isReturned;

    // ── 公开初始化方法 ──────────────────────────────────────

    /// <summary>
    /// 初始化箭头状态，对象池取出后必须调用此方法。
    /// </summary>
    /// <param name="trajectoryPoints">预计算的轨迹点列表</param>
    /// <param name="speed">飞行速度（maxSpeed）</param>
    /// <param name="hitObstacle">轨迹末端是否命中了障碍物</param>
    /// <param name="maxSimulationDistance">最大模拟距离</param>
    public void Init(List<Vector3> trajectoryPoints, float speed, bool hitObstacle, float maxSimulationDistance, float minSpeed = 0f, float maxSpeed = 10f, LayerMask obstacleLayer = default)
    {
        this.trajectoryPoints = trajectoryPoints;
        this.speed            = speed;
        this.hitObstacle      = hitObstacle;
        this.obstacleLayer    = obstacleLayer;

        // 计算轨迹总长度，判断是否超出最大模拟距离
        float totalLength = 0f;
        for (int i = 1; i < trajectoryPoints.Count; i++)
        {
            totalLength += Vector3.Distance(trajectoryPoints[i - 1], trajectoryPoints[i]);
        }
        exceededMaxDistance = totalLength >= maxSimulationDistance;

        // 重置所有运行时状态，确保对象池复用时不受上次状态影响
        currentPointIndex = 0;
        isFreeFlying      = false;
        freeFlyDirection  = Vector3.zero;
        freeFlyTimer      = 0f;
        isReturned        = false;

        // 将箭头初始位置设为第一个轨迹点
        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            transform.position = trajectoryPoints[0];
        }
        
        if(ps == null) ps = GetComponent<ParticleSystem>();
        
        // 根据发射速度计算音量缩放比例：最大速度时为1，最小速度时最小
        float speedRatio = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        
        //添加发射音效
        AudioManager.Instance.SetAudio(fireSound, this.gameObject, AudioClipType.Sound, (source) =>
        {
            source.volume *= speedRatio;
        });
    }

    // ── Unity 生命周期 ──────────────────────────────────────

    private void Update()
    {
        // 尚未初始化则跳过
        if (trajectoryPoints == null || trajectoryPoints.Count == 0) return;

        if (isFreeFlying)
        {
            // 惯性飞行阶段：保持最后方向和速度继续前进
            UpdateFreeFly();
        }
        else
        {
            // 轨迹跟随阶段：沿预计算轨迹点依次移动
            UpdateTrajectory();
        }
    }

    // ── 私有方法 ────────────────────────────────────────────

    /// <summary>
    /// 轨迹跟随阶段逻辑：逐点移动并更新朝向。
    /// </summary>
    private void UpdateTrajectory()
    {
        // 目标点索引不能超出列表末尾
        int targetIndex = currentPointIndex + 1;
        if (targetIndex >= trajectoryPoints.Count)
        {
            // 已到达最后一个轨迹点，执行到达处理
            OnReachedEnd();
            return;
        }

        Vector3 current = transform.position;
        Vector3 target  = trajectoryPoints[targetIndex];

        // 朝目标点移动
        Vector3 newPos = Vector3.MoveTowards(current, target, speed * Time.deltaTime);

        // 实时射线检测：从当前位置到新位置之间检测碰撞体
        // 非触发器始终可命中；携带 IArrowAttacked 接口的触发器也可命中
        RaycastHit2D hit = Physics2D.Linecast(current, newPos, obstacleLayer);
        if (hit.collider != null && IsHittable(hit.collider))
        {
            OnHitCollider(hit.collider.gameObject, hit.point);
            return;
        }

        transform.position = newPos;

        // 更新箭头朝向（指向移动方向）
        Vector3 dir = (target - current).normalized;
        if (dir != Vector3.zero)
        {
            float angle = Vector2.SignedAngle(Vector2.up, dir);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // 到达目标点后推进到下一个点
        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            currentPointIndex++;
        }
    }

    /// <summary>
    /// 惯性飞行阶段逻辑：保持最后方向持续飞行，2 秒后归还对象池。
    /// </summary>
    private void UpdateFreeFly()
    {
        Vector3 current = transform.position;
        Vector3 newPos = current + freeFlyDirection * speed * Time.deltaTime;

        // 惯性飞行阶段也进行实时碰撞检测
        // 非触发器始终可命中；携带 IArrowAttacked 接口的触发器也可命中
        RaycastHit2D hit = Physics2D.Linecast(current, newPos, obstacleLayer);
        if (hit.collider != null && IsHittable(hit.collider))
        {
            OnHitCollider(hit.collider.gameObject, hit.point);
            return;
        }

        transform.position = newPos;

        freeFlyTimer += Time.deltaTime;
        if (freeFlyTimer >= FREE_FLY_DURATION && !isReturned)
        {
            isReturned = true;
            AudioManager.Instance.SetAudio(boomAC, this.gameObject, AudioClipType.Sound);
            ps.Play();
            
            // 惯性飞行时间到，归还对象池
            ObjectsPool.Instance.ReturnObjectToPool(gameObject, 1);
        }
    }

    /// <summary>
    /// 实时碰撞到障碍物时的处理逻辑。
    /// </summary>
    private void OnHitCollider(GameObject target, Vector2 hitPoint)
    {
        if (isReturned) return;
        isReturned = true;

        transform.position = hitPoint;

        AudioManager.Instance.SetAudio(boomAC, this.gameObject, AudioClipType.Sound);
        ps.Play();

        // 检测目标是否实现了IArrowAttacked接口，若实现则执行Attacked
        target.GetComponent<IArrowAttacked>()?.Attacked();

        // 处理完毕后归还对象池
        ObjectsPool.Instance.ReturnObjectToPool(gameObject, 1);
    }

    /// <summary>
    /// 判断碰撞体是否可被箭头命中：非触发器始终可命中；触发器仅当携带 IArrowAttacked 接口时才可命中。
    /// </summary>
    private bool IsHittable(Collider2D collider)
    {
        if (!collider.isTrigger) return true;
        return collider.GetComponent<IArrowAttacked>() != null;
    }

    /// <summary>
    /// 到达最后一个轨迹点后的处理逻辑。
    /// </summary>
    private void OnReachedEnd()
    {
        if (isReturned) return;

        if (hitObstacle)
        {
            // 预测轨迹命中了障碍物但箭头飞行途中未实时碰到（目标已移走），转为惯性飞行
            int lastIndex       = trajectoryPoints.Count - 1;
            int secondLastIndex = Mathf.Max(lastIndex - 1, 0);
            freeFlyDirection = (trajectoryPoints[lastIndex] - trajectoryPoints[secondLastIndex]).normalized;

            isFreeFlying = true;
            freeFlyTimer = 0f;
        }
        else if (exceededMaxDistance)
        {
            isReturned = true;
            // TODO: 超出最大模拟距离处理逻辑（用户自定义）
            AudioManager.Instance.SetAudio(boomAC, this.gameObject, AudioClipType.Sound);
            ps.Play();

            // 处理完毕后归还对象池
            ObjectsPool.Instance.ReturnObjectToPool(gameObject, 1);
        }
        else
        {
            // 没有碰到障碍物：保持最后的飞行方向和速度惯性前进，2 秒后回收
            // 计算最后两个轨迹点之间的方向作为惯性方向
            int lastIndex       = trajectoryPoints.Count - 1;
            int secondLastIndex = Mathf.Max(lastIndex - 1, 0);
            freeFlyDirection = (trajectoryPoints[lastIndex] - trajectoryPoints[secondLastIndex]).normalized;

            isFreeFlying = true;
            freeFlyTimer = 0f;
        }
    }
}
