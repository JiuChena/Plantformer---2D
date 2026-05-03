using System.Collections;
using System.Collections.Generic;
using TarodevController;
using Unity.VisualScripting;
using UnityEngine;

public abstract class HoldablePropBase : MonoBehaviour
{
    //可继承
    [Header("基础配置")]
    public KeyCode code = KeyCode.Tab;
    public AudioClip audioClip;
    public Vector2 propPosition = new Vector2(0.0f, 0.5f);
    [Tooltip("道具名称，用于在 PlayerController 道具字典中注册")]
    public string propName = "";

    //继承
    [Header("消息通知配置")] 
    public Sprite icon;
    public Color iconOverlapColor = Color.black;
    public string text;
    public Sprite keyCodeIcon;
    public Color keyCodeIconOverlapColor = Color.black;
    
    //继承
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D bc;
    [HideInInspector] public ParticleSystem ps;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public SpriteRenderer sr;
    protected float posX = 0;
    
    private void Start()
    {
        PropInit();
    }

    private void Update()
    {
        PropUpdate();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        PropOnTriggerEnter2D(other);
    }
    

    public virtual void PropInit()
    {
        //继承
        animator = this.GetComponent<Animator>();
        bc = this.GetComponent<BoxCollider2D>();
        ps = this.transform.Find("Particle").GetComponent<ParticleSystem>();
        sr = this.transform.Find("Sprite").GetComponent<SpriteRenderer>();
        
        posX = propPosition.x;
    }

    public virtual void PropUpdate()
    {
        
    }

    public virtual void PropOnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        Transform trans = player.transform.Find("Visual/Sprite/Prop");
        trans.GetComponent<PlayerListenerForProps>().propEvents += OnAction;
        
        this.transform.SetParent(trans);
        this.transform.localPosition = propPosition;
        ps.Play();
        animator.SetTrigger("Hide");
        
        PanelManager.Instance.GetPanel<NoticePanel>("Notice Panel").PushMessage(new NoticeMessage()
        {
            icon = this.icon,
            iconColor = this.iconOverlapColor,
            text = this.text,
            keyCodeIcon = this.keyCodeIcon,
            keyCodeIconColor = this.keyCodeIconOverlapColor,
        });
        
        bc.enabled = false;

        // 注册到玩家道具字典
        if (!string.IsNullOrEmpty(propName))
            player.AddProp(propName, this);
    }

    public virtual void OnAction()
    {
        //继承
        //截取Player输入改变localpos
        if (player.FrameInput.x > 0)
        {
            this.transform.localPosition = new Vector2(posX, this.transform.localPosition.y);
            sr.flipX = false;
        }
        else if (player.FrameInput.x < 0)
        {
            this.transform.localPosition = new Vector2(-posX, this.transform.localPosition.y);
            sr.flipX = true;
        }
    }
}
