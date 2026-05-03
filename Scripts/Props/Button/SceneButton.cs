using System.Collections;
using TarodevController;
using UnityEngine;

/// <summary>
/// 场景按钮抽象基类：玩家进入触发范围或被箭矢命中时执行抽象触发方法。
/// 子类重写 OnTrigger 来实现不同的按钮效果（开门、升降台、机关等）。
/// </summary>
public abstract class SceneButton : ScenePropBase, IArrowAttacked
{
    [Header("按钮配置")]
    [Tooltip("是否允许重复触发。为false时按钮只能触发一次；为true时触发后经过冷却时间自动重置，可再次触发")]
    public bool repeat = false;

    [Tooltip("重复触发的冷却时间（秒）。按钮触发后等待此时间再重置状态，仅在repeat为true时生效")]
    public float repeatCooldown = 1f;

    private bool _triggered;

    /// <summary>
    /// 按钮被触发时执行的抽象方法，子类必须重写以实现具体效果。
    /// </summary>
    public abstract void OnTrigger();

    /// <summary>
    /// 尝试触发按钮。未触发时执行并锁定；若repeat为true则启动冷却计时器自动重置。
    /// </summary>
    public void TryTrigger()
    {
        if (_triggered) return;
        _triggered = true;
        animator.SetTrigger("Triggered");
        AudioManager.Instance.SetAudio(triggerSound, this.gameObject, AudioClipType.Sound);
        OnTrigger();

        if (repeat)
            StartCoroutine(CooldownReset());
    }

    private IEnumerator CooldownReset()
    {
        yield return new WaitForSeconds(repeatCooldown);
        ResetTrigger();
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        TryTrigger();
    }

    /// <summary>
    /// IArrowAttacked 接口实现：箭矢命中时触发按钮
    /// </summary>
    public void Attacked()
    {
        TryTrigger();
    }

    /// <summary>
    /// 实现 ScenePropBase 的跳跃激活，统一转发到 TryTrigger。
    /// 使得玩家在按钮范围内跳跃也能触发按钮。
    /// </summary>
    public override void OnPropActivate()
    {
        TryTrigger();
    }

    /// <summary>
    /// 重置按钮状态，允许再次触发。
    /// </summary>
    public void ResetTrigger()
    {
        _triggered = false;
        animator.SetTrigger("Idle");
    }
}
