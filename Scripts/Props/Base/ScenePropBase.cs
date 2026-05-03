using TarodevController;
using UnityEngine;

public abstract class ScenePropBase : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D bc;
    [HideInInspector] public ParticleSystem ps;
    [HideInInspector] public PlayerController player;
    [HideInInspector] public SpriteRenderer sr;
    
    public AudioClip triggerSound;

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
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Jumped += OnPlayerJumped;
            player.AirJumped += OnPlayerJumped;
        }
        
        PropOnTriggerEnter2D(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        if (player != null)
        {
            player.Jumped -= OnPlayerJumped;
            player.AirJumped -= OnPlayerJumped;
        }
        
        PropOnTriggerExit2D(other);
    }
    
    public virtual void PropOnTriggerEnter2D(Collider2D other){ }
    
    public virtual void PropOnTriggerExit2D(Collider2D other){ }

    public virtual void PropInit()
    {
        animator = GetComponent<Animator>();
        bc = GetComponent<BoxCollider2D>();
    }

    public virtual void PropUpdate() { }

    /// <summary>
    /// 玩家在触发器范围内跳跃时调用，子类必须实现具体道具功能。
    /// </summary>
    public abstract void OnPropActivate();

    private void OnPlayerJumped()
    {
        OnPropActivate();
    }
}
