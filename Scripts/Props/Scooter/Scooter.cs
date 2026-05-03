using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scooter : HoldablePropBase
{
    public override void PropInit()
    {
        base.PropInit();
        if (string.IsNullOrEmpty(propName))
            propName = "Scooter";
    }

    public override void PropUpdate()
    {
        base.PropUpdate();
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        base.PropOnTriggerEnter2D(other);
    }

    private bool abilityEnabled = false;
    public override void OnAction()
    {
        if (Input.GetKeyDown(code))
        {
            if (abilityEnabled)
            {
                //禁用
                player._col.offset = new Vector2(player._col.offset.x, player._col.offset.y + 0.1f);
                player._col.size = new Vector2(player._col.size.x, player._col.size.y - 0.2f);
                
                animator.SetTrigger("Hide");
                ps.Play();
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                
                player._stats.MaxSpeed /= 1.5f;
                player._stats.AllowAirJump = true;
                player._stats.AllowWallInteraction = true;
                
                abilityEnabled = false;
            }
            else
            {
                //使用
                player._col.offset = new Vector2(player._col.offset.x, player._col.offset.y - 0.1f);
                player._col.size = new Vector2(player._col.size.x, player._col.size.y + 0.2f);
                
                animator.SetTrigger("Use");
                ps.Play();
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                
                player._stats.MaxSpeed *= 1.5f;
                player._stats.AllowAirJump = false;
                player._stats.AllowWallInteraction = false;
                
                abilityEnabled = true;
            }
        }
        
        base.OnAction();
    }
}
