using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanel : PanelBase
{
    protected override void LoadInit()
    {
        
    }

    protected override void CompomentInit()
    {

    }

    protected override void OnUpdate()
    {
        
    }
    
    public void StartLoad(string sceneName)
    {
        LoadSceneManager.Instance.LoadSceneAsync(sceneName, null);
        
        PanelManager.Instance.CameraInit();
    }

    public void LoadEnd()
    {
        PanelManager.Instance.PanelHide("Load Panel");
    }
}
