using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物体激活按钮：触发后激活指定物体列表并隐藏指定物体列表。
/// </summary>
public class SceneButtonObjectActived : SceneButton
{
    [Header("目标物体")]
    [Tooltip("触发后激活的物体列表")]
    public List<GameObject> activateList = new List<GameObject>();

    [Tooltip("触发后隐藏的物体列表")]
    public List<GameObject> hideList = new List<GameObject>();

    public override void OnTrigger()
    {
        foreach (var obj in activateList)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (var obj in hideList)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}
