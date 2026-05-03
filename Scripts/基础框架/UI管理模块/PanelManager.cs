using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum UILayer
{
    Bot,
    Mid,
    Top,
    System,
}

public class PanelManager
{
    private static PanelManager instance = new PanelManager();
    public static PanelManager Instance => instance;
    
    private Dictionary<string, PanelBase> panelsDic = new Dictionary<string, PanelBase>();

    private Canvas canvas;
    public Transform RectTrans_Canvas;
    private Transform bot;
    private Transform mid;
    private Transform top;
    private Transform system;

    public PanelManager()
    {
        GameObject obj = ResourcesLoadManager.Instance.Load<GameObject>("Prefabs/UI/Base/Canvas");
        RectTrans_Canvas = obj.transform;
        GameObject.DontDestroyOnLoad(obj);

        bot = RectTrans_Canvas.Find("Bot");
        mid = RectTrans_Canvas.Find("Mid");
        top = RectTrans_Canvas.Find("Top");
        system = RectTrans_Canvas.Find("System");

        canvas = obj.GetComponent<Canvas>();
        
        if(canvas.renderMode == RenderMode.ScreenSpaceCamera) canvas.worldCamera = Camera.main;
    }

    public void CameraInit()
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="name">面板名字</param>
    /// <param name="layer">显示层级</param>
    /// <param name="callback">有参回调函数</param>
    /// <typeparam name="T">参数类型</typeparam>
    public void PanelDisplay<T>(string path, string name, UILayer layer, UnityAction<T> callback = null) where T : PanelBase
    {
        if (panelsDic.ContainsKey(name)) return;
        
        GameObject panel = ResourcesLoadManager.Instance.LoadSync<GameObject>("Prefabs/" + path + name, (obj) =>
        {
            //找到根对象，设置在哪一层
            Transform trans_Root = system;
            switch (layer)
            {
                case UILayer.Bot:
                    trans_Root = bot;
                    break;
                case UILayer.Mid:
                    trans_Root = mid;
                    break;
                case UILayer.Top:
                    trans_Root = top;
                    break;
            }
            
            obj.transform.SetParent(trans_Root);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
        });
        
        T panelScript = panel.GetComponent<T>();
        panelsDic.Add(panel.name, panelScript);
        
        panelScript.DisplayPanel();
        
        callback?.Invoke(panelScript);
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="panelName">面板名字(场景)</param>
    /// <param name="callback">无参回调函数</param>
    public void PanelHide(string panelName, UnityAction callback = null)
    {
        if (panelsDic.ContainsKey(panelName))
        {
            panelsDic[panelName].HidePanel();
            
            GameObject.Destroy(panelsDic[panelName].gameObject);
            panelsDic.Remove(panelName);
        }
        else return;
        
        callback?.Invoke();
    }

    /// <summary>
    /// 获取场景上已存在的面板(挂载的脚本)
    /// </summary>
    /// <param name="panelName">面板名字</param>
    /// <typeparam name="T">面板脚本类型</typeparam>
    /// <returns></returns>
    public T GetPanel<T>(string panelName) where T : PanelBase
    {
        if(panelsDic.ContainsKey(panelName)) return panelsDic[panelName] as T;
        else return null;
    }
}
