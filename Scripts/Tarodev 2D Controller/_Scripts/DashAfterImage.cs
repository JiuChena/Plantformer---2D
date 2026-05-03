using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// 冲刺残影效果组件。
    /// 挂载在与 PlayerAnimator 相同的 GameObject 上（拥有 SpriteRenderer 的那个子物体）。
    /// 冲刺触发时，会在 DashDuration 期间按 DashAfterImageInterval 间隔生成残影，每个残影在 DashAfterImageLifetime 内淡出。
    /// </summary>
    public class DashAfterImage : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("角色的 SpriteRenderer，用于获取当前帧的 Sprite、朝向和排序信息")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Tooltip("ScriptableStats 配置资产，包含残影相关参数。留空时自动从父级 PlayerController 获取")]
        [SerializeField] private ScriptableStats _stats;

        // 玩家控制器接口，用于订阅 Dashed 事件
        private IPlayerController _player;

        // 内部对象池：存储可复用的残影 GameObject
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        // 初始预热对象池的数量
        private const int InitialPoolSize = 5;

        // 当前正在运行的残影生成协程（避免重叠启动）
        private Coroutine _spawnCoroutine;

        // 当前所有已激活（正在淡出）的残影 GameObject 列表，用于组件禁用时统一强制回收
        private readonly List<GameObject> _activeAfterImages = new List<GameObject>();

        private void Awake()
        {
            _player = GetComponentInParent<IPlayerController>();

            // 若未在 Inspector 中手动赋值，则从父级 PlayerController 获取 Stats
            if (_stats == null)
                _stats = GetComponentInParent<PlayerController>().Stats;

            // 若未在 Inspector 中手动赋值 SpriteRenderer，则尝试获取本物体的
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            // 预热对象池
            for (int i = 0; i < InitialPoolSize; i++)
            {
                _pool.Enqueue(CreatePooledObject());
            }

            // 防御性校验：确保必要引用均已获取
            if (_player == null)
                Debug.LogError($"[DashAfterImage] 未找到父级 IPlayerController，请确认挂载层级。", this);
            if (_stats == null)
                Debug.LogError($"[DashAfterImage] 未找到 ScriptableStats，请检查父级 PlayerController 配置。", this);
            if (_spriteRenderer == null)
                Debug.LogError($"[DashAfterImage] 未找到 SpriteRenderer，请确认组件挂载在正确的子物体上。", this);

            if (_player == null || _stats == null || _spriteRenderer == null)
                enabled = false;
        }

        private void OnEnable()
        {
            if (_player != null)
                _player.Dashed += OnDashed;
        }

        private void OnDisable()
        {
            if (_player != null)
                _player.Dashed -= OnDashed;

            // 停止所有协程，防止协程持有对已禁用组件的引用
            StopAllCoroutines();
            _spawnCoroutine = null;

            // 将所有仍在场景中的活跃残影强制回收到对象池
            foreach (GameObject afterImageObj in _activeAfterImages)
            {
                if (afterImageObj != null)
                    ReturnToPool(afterImageObj);
            }
            _activeAfterImages.Clear();
        }

        /// <summary>
        /// 冲刺事件回调：停止旧的生成协程后启动新的残影生成协程。
        /// </summary>
        private void OnDashed()
        {
            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);

            _spawnCoroutine = StartCoroutine(SpawnAfterImages());
        }

        /// <summary>
        /// 在冲刺持续时间内，每隔 DashAfterImageInterval 秒生成一个残影。
        /// </summary>
        private IEnumerator SpawnAfterImages()
        {
            float elapsed = 0f;

            while (elapsed < _stats.DashDuration)
            {
                SpawnSingleAfterImage();
                yield return new WaitForSeconds(_stats.DashAfterImageInterval);
                elapsed += _stats.DashAfterImageInterval;
            }

            _spawnCoroutine = null;
        }

        /// <summary>
        /// 从对象池取出一个残影，设置其位置/外观，并启动淡出协程。
        /// </summary>
        private void SpawnSingleAfterImage()
        {
            GameObject afterImageObj = GetFromPool();
            SpriteRenderer sr = afterImageObj.GetComponent<SpriteRenderer>();

            // 同步当前角色的 Sprite 外观
            sr.sprite = _spriteRenderer.sprite;
            sr.flipX = _spriteRenderer.flipX;

            // 设置残影颜色与初始透明度
            Color c = _stats.DashAfterImageColor;
            c.a = _stats.DashAfterImageInitialAlpha;
            sr.color = c;

            // 排序：显示在角色后面
            sr.sortingLayerID = _spriteRenderer.sortingLayerID;
            sr.sortingOrder = _spriteRenderer.sortingOrder - 1;

            // 同步世界变换
            Transform t = afterImageObj.transform;
            t.position = transform.position;
            t.rotation = transform.rotation;
            t.localScale = transform.lossyScale;

            afterImageObj.SetActive(true);
            _activeAfterImages.Add(afterImageObj);

            StartCoroutine(FadeAndReturn(sr, afterImageObj));
        }

        /// <summary>
        /// 在 DashAfterImageLifetime 内将残影 alpha 从初始值线性插值到 0，完成后回收到对象池。
        /// </summary>
        private IEnumerator FadeAndReturn(SpriteRenderer sr, GameObject afterImageObj)
        {
            float elapsed = 0f;
            float lifetime = _stats.DashAfterImageLifetime;
            float initialAlpha = _stats.DashAfterImageInitialAlpha;
            Color baseColor = _stats.DashAfterImageColor;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(initialAlpha, 0f, elapsed / lifetime);
                Color c = baseColor;
                c.a = alpha;
                sr.color = c;
                yield return null;
            }

            ReturnToPool(afterImageObj);
            _activeAfterImages.Remove(afterImageObj);
        }

        // ─── 对象池辅助方法 ───────────────────────────────────────────────────────

        /// <summary>
        /// 创建一个新的残影池对象（默认不激活）。
        /// </summary>
        private GameObject CreatePooledObject()
        {
            GameObject obj = new GameObject("AfterImage");
            obj.AddComponent<SpriteRenderer>();
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 从对象池中取出一个对象；池空时自动创建新的。
        /// </summary>
        private GameObject GetFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            return CreatePooledObject();
        }

        /// <summary>
        /// 将对象回收到池中（隐藏但不销毁）。
        /// </summary>
        private void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
