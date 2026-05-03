using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : PanelBase
{
    public float residentTime = 2f;
    private float timer = 0;
    
    private Animator animator;
    private Queue<NoticeMessage> messages = new Queue<NoticeMessage>();
    
    private NoticeMessage currentMessage;

    private TMP_Text text;
    private Image icon;
    private Image keyCodeIcon;
    
    private bool panelState = false;
    
    protected override void LoadInit()
    {
        
    }

    protected override void CompomentInit()
    {
        animator = GetComponent<Animator>();
        
        text = this.transform.Find("Text").GetComponent<TMP_Text>();
        icon = this.transform.Find("Sprite").GetComponent<Image>();
        keyCodeIcon = this.transform.Find("KeyIcon").GetComponent<Image>();
    }

    protected override void OnUpdate()
    {
        if (messages.Count > 0 || timer > 0)
        {
            if (!panelState)
            {
                animator.SetBool("Display", true);
                panelState = true;
            }
            
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                if (messages.Count > 0)
                {
                    currentMessage = messages.Dequeue();
                    timer = residentTime;
                
                    text.text = currentMessage.text;
                    icon.sprite = currentMessage.icon;
                    icon.color = currentMessage.iconColor;
                    keyCodeIcon.sprite = currentMessage.keyCodeIcon;
                    keyCodeIcon.color = currentMessage.keyCodeIconColor;
                }
            }
        }
        else
        {
            if (panelState)
            {
                animator.SetBool("Display", false);
                panelState = false;
            }
        }
    }

    public void PushMessage(NoticeMessage message)
    {
        messages.Enqueue(message);
    }
}

public class NoticeMessage
{
    public Sprite icon;
    public Color iconColor;
    public string text;
    public Sprite keyCodeIcon;
    public Color keyCodeIconColor;
}