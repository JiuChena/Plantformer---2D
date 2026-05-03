using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 边界触发器：玩家进入后通过 LoadPanel 重新加载当前场景
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RestartEdge : MonoBehaviour
{
    [Header("触发设置")]
    [Tooltip("延迟触发时间（秒），避免玩家刚进入场景就触发")]
    public float triggerDelay = 0.2f;

    private Collider2D m_Collider;
    private bool m_Triggered;
    private float m_EnableTime;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
        m_EnableTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 通过 Layer 匹配玩家
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        // 防止短时间内重复触发
        if (m_Triggered) return;

        // 启动后短暂延迟防止误触
        if (Time.time - m_EnableTime < triggerDelay) return;

        m_Triggered = true;

        string currentScene = SceneManager.GetActiveScene().name;

        PanelManager.Instance.PanelDisplay<LoadPanel>("UI/Load/", "Load Panel", UILayer.Top, (panel) =>
        {
            StartCoroutine(StartLoad());
        });
    }

    private IEnumerator StartLoad()
    {
        yield return new WaitForSeconds(1);
        PanelManager.Instance.GetPanel<LoadPanel>("Load Panel").StartLoad(SceneManager.GetActiveScene().name);
    }
}
