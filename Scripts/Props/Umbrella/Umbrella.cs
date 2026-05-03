using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Umbrella : HoldablePropBase
{
    public bool IsOpen => open;
    private bool open = false;
    
    public override void PropInit()
    {
        base.PropInit();
        if (string.IsNullOrEmpty(propName))
            propName = "Umbrella";
    }

    public override void PropUpdate()
    {
        base.PropUpdate();
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        base.PropOnTriggerEnter2D(other);
    }
    
    public override void OnAction()
    {
        base.OnAction();

        if (Input.GetKeyDown(code))
        {
            if (!open)
            {
                //开伞
                animator.SetTrigger("Use");
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                ps.Play();
                
                open = true;
            }
            else
            {
                //收伞
                animator.SetTrigger("Hide");
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                ps.Play();
                
                open = false;
            }
        }
    }
}
