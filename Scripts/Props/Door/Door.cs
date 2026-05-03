using System.Collections;
using UnityEngine;

/// <summary>
/// 通关门：玩家触碰后通过 LoadPanel 加载指定场景进入下一关
/// </summary>
public class Door : ScenePropBase
{
    [Header("场景配置")]
    [Tooltip("通关后要加载的下一个场景名称")]
    public string nextSceneName;

    private bool _triggered;

    public override void PropInit()
    {
        base.PropInit();
        bc.isTrigger = true;
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        _triggered = true;

        // 封禁玩家操作，防止加载过程中继续移动
        player.DisableInput();
        
        animator.SetTrigger("Successed");

        // 弹出 LoadPanel 并启动场景加载
        PanelManager.Instance.PanelDisplay<LoadPanel>("UI/Load/", "Load Panel", UILayer.Top, (panel) =>
        {
            StartCoroutine(StartLoad());
        });
    }

    private IEnumerator StartLoad()
    {
        yield return new WaitForSeconds(1);
        PanelManager.Instance.GetPanel<LoadPanel>("Load Panel").StartLoad(nextSceneName);
    }

    public override void OnPropActivate()
    {
        // Door 通过触碰触发，不需要跳跃激活
    }
}
