using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    private void Start()
    {
        if (PanelManager.Instance.GetPanel<NoticePanel>("Notice Panel") == null)
        {
            PanelManager.Instance.PanelDisplay<NoticePanel>("UI/Gaming/", "Notice Panel", UILayer.Bot);
        }
    }

    private void Update()
    {
        MenuPanelHandler();
    }

    private void MenuPanelHandler()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PanelManager.Instance.GetPanel<GamingMenuPanel>("Gaming Menu Panel") == null)
            {
                PanelManager.Instance.PanelDisplay<GamingMenuPanel>("UI/Gaming/", "Gaming Menu Panel", UILayer.Mid);
            }
            else
            {
                PanelManager.Instance.PanelHide("Gaming Menu Panel");
            }
        }
    }
}
