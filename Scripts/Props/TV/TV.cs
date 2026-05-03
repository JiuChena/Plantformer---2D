using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TV : ScenePropBase
{
    public GameObject frame;
    public Transform arrow;
    public TV target;
    
    private TVGlitchEffect tvGlitchEffect;

    public override void PropInit()
    {
        base.PropInit();

        
        tvGlitchEffect = Camera.main.GetComponent<TVGlitchEffect>();
    }

    public override void PropUpdate()
    {
        arrow.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, (target.transform.position - arrow.position).normalized));
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        animator.SetTrigger("Enter");
        frame.SetActive(true);
        target.frame.SetActive(true);
    }

    public override void PropOnTriggerExit2D(Collider2D other)
    {
        animator.SetTrigger("Exit");
        frame.SetActive(false);
        target.frame.SetActive(false);
    }

    public override void OnPropActivate()
    {
        animator.SetTrigger("Use");
        tvGlitchEffect.enabled = true;
        AudioManager.Instance.SetAudio(triggerSound, this.gameObject, AudioClipType.Sound);

        StartCoroutine(Teleport());
    }

    private IEnumerator Teleport()
    {
        //封禁玩家操作
        player.DisableInput();
        
        while (player.transform.localScale.x > 0.5f)
        {
            player.transform.localScale = Vector3.one * Mathf.Lerp(player.transform.localScale.x, 0.4f, Time.deltaTime * 5f);
            yield return null;
        }
        
        player.transform.position = target.transform.position + Vector3.up * 0.3f;
        
        yield return new WaitForSeconds(0.5f);
        
        while (player.transform.localScale.x < 0.9f)
        {
            player.transform.localScale = Vector3.one * Mathf.Lerp(player.transform.localScale.x, 1f, Time.deltaTime * 5f);
            yield return null;
        }
        
        player.transform.localScale = Vector3.one;
        
        //解除操作限制
        player.EnableInput();
    }
}
