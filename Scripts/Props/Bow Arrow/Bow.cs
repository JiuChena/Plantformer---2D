using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : HoldablePropBase
{
    [Header("玩家中心位置偏移")] 
    public Vector2 center = Vector2.zero;
    public float offset = 0.2f;
    
    public float minSpeed = 3f;
    public float maxSpeed = 10f;
    public float acceleration = 5f;

    public int precision = 10;
    public float trajectoryTime = 5f;
    public float maxSimulationDistance = 100f;

    [Header("障碍物检测")]
    public LayerMask obstacleLayer;
    public GameObject hitMarkerPrefab;
    public GameObject arrowPrefab;

    private Transform arrow;
    private LineRenderer lr;
    private GameObject currentHitMarker;
    private List<Vector3> cachedTrajectoryPoints;
    private bool cachedHitObstacle;
    private float currentSpeed;        // 当前蓄力速度
    private float cachedSpeed;          // 缓存松手时的速度，用于传给箭头
    
    public override void PropInit()
    {
        base.PropInit();

        if (string.IsNullOrEmpty(propName))
            propName = "Bow";

        arrow = this.transform.Find("Sprite");
        lr = this.GetComponent<LineRenderer>();
    }

    public override void PropUpdate()
    {
        base.PropUpdate();
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        base.PropOnTriggerEnter2D(other);
    }
    
    public override void OnAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Use");
            // 按下时重置蓄力速度为最低阈值
            currentSpeed = minSpeed;
        }
        else if (Input.GetMouseButton(0))
        {
            //根据鼠标位置和玩家位置得出射箭方向

            // 计算玩家中心世界坐标
            Vector2 centerWorld = (Vector2)transform.position + center;

            // 获取鼠标世界坐标
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 计算从中心点指向鼠标的方向
            Vector2 dir = (mouseWorld - centerWorld).normalized;

            // arrow绕center中心点旋转，offset为距离中心点的偏移距离
            arrow.localPosition = (Vector2)center + dir * offset;
            arrow.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, dir));

            // 抛物线起始点：从arrow的世界位置开始，沿dir方向再偏移0.3f
            Vector2 startPos = (Vector2)arrow.position + dir * 0.3f;

            // 按住时间越久初速度越高，按加速度递增，不超过最大速度
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // 计算初始速度：方向为dir，大小为当前蓄力速度
            Vector2 velocity = dir * currentSpeed;

            // 轨迹显示的总时间范围
            float totalTime = trajectoryTime;

            // 失活上一帧的打击标记
            if (currentHitMarker != null) currentHitMarker.SetActive(false);

            // 设置 LineRenderer 点数
            lr.positionCount = precision + 1;

            // 记录上一个描绘点位置，用于相邻点间的射线检测
            Vector2 prevPosition = startPos;

            // 初始化缓存轨迹点列表，记录当前蓄力速度
            cachedTrajectoryPoints = new List<Vector3>();
            cachedHitObstacle = false;
            cachedSpeed = currentSpeed;
            float totalDistance = 0f;  // 累计模拟距离

            // 使用抛物线公式 P(t) = startPos + velocity * t + 0.5 * gravity * t^2 逐点计算轨迹
            for (int i = 0; i <= precision; i++)
            {
                float t = i * (totalTime / precision);
                Vector2 position = startPos + velocity * t + 0.5f * Physics2D.gravity * t * t;

                // 累加与上一点的距离（i==0 时 prevPosition == startPos，同样需要累加）
                totalDistance += Vector2.Distance(prevPosition, position);

                // 超出最大模拟距离：截断轨迹并退出循环
                if (totalDistance >= maxSimulationDistance)
                {
                    lr.SetPosition(i, position);
                    lr.positionCount = i + 1;
                    cachedTrajectoryPoints.Add(position);
                    break;
                }

                // 从第2个点开始，对上一点到当前点之间做射线检测
                if (i >= 1)
                {
                    RaycastHit2D hit = Physics2D.Linecast(prevPosition, position, obstacleLayer);
                    if (hit.collider != null && IsHittable(hit.collider))
                    {
                        // 将当前点设为命中点，截断轨迹线
                        lr.SetPosition(i, hit.point);
                        lr.positionCount = i + 1;

                        // 在命中点显示打击点标记
                        if (hitMarkerPrefab != null)
                        {
                            if (currentHitMarker == null)
                                currentHitMarker = Instantiate(hitMarkerPrefab);
                            else
                                currentHitMarker.SetActive(true);

                            // 将打击点标记设在命中点沿打击方向偏移0.2f的位置
                            Vector2 hitDir = ((Vector2)position - prevPosition).normalized;
                            currentHitMarker.transform.position = (Vector3)((Vector2)hit.point + hitDir * 0.2f);
                        }

                        // 缓存命中点、目标物体并标记命中障碍物
                        cachedTrajectoryPoints.Add((Vector3)hit.point);
                        cachedHitObstacle = true;

                        break;
                    }
                }

                lr.SetPosition(i, position);
                cachedTrajectoryPoints.Add(position);

                // 更新上一个点位置
                prevPosition = position;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetTrigger("Hide");
            
            // 松开鼠标时清除轨迹线
            lr.positionCount = 0;

            // 失活打击标记
            if (currentHitMarker != null) currentHitMarker.SetActive(false);

            // 从对象池获取箭头并发射
            if (arrowPrefab != null && cachedTrajectoryPoints != null && cachedTrajectoryPoints.Count > 1)
            {
                GameObject arrowObj = ObjectsPool.Instance.GetObjectFromPool(arrowPrefab);
                BowArrow bowArrow = arrowObj.GetComponent<BowArrow>();
                bowArrow.Init(cachedTrajectoryPoints, cachedSpeed, cachedHitObstacle, maxSimulationDistance, minSpeed, maxSpeed, obstacleLayer);
            }
        }
    }

    /// <summary>
    /// 判断碰撞体是否可被箭头命中：非触发器始终可命中；触发器仅当携带 IArrowAttacked 接口时才可命中。
    /// </summary>
    private bool IsHittable(Collider2D collider)
    {
        if (!collider.isTrigger) return true;
        return collider.GetComponent<IArrowAttacked>() != null;
    }
}
