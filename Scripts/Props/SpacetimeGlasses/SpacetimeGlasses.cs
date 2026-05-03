using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class SpacetimeGlasses : HoldablePropBase
{
    private ColorOverlay overlay;
    private bool enabledAbility = false;
    private float targetColorOverlayIntensity = 0;
    
    public List<GameObject> pastGos = new List<GameObject>();
    public List<GameObject> currentGos = new List<GameObject>();

    public override void PropInit()
    {
        base.PropInit();
        
        if (string.IsNullOrEmpty(propName))
            propName = "SpacetimeGlasses";

        overlay = Camera.main.GetComponent<ColorOverlay>();
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        base.PropOnTriggerEnter2D(other);
    }
    
    public override void OnAction()
    {
        if (Input.GetKeyDown(code))
        {
            //特殊能力，时空转换
            if (enabledAbility)
            {
                //能力关闭
                animator.SetTrigger("Hide");
                targetColorOverlayIntensity = 0;
                ps.Play();
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                
                foreach (var go in currentGos) go.SetActive(true);
                foreach (var go in pastGos) go.SetActive(false);
                
                enabledAbility = false;
            }
            else
            {
                //能力开启
                animator.SetTrigger("Use");
                targetColorOverlayIntensity = 1;
                ps.Play();
                AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
                
                foreach (var go in currentGos) go.SetActive(false);
                foreach (var go in pastGos) go.SetActive(true);
                
                enabledAbility = true;
            }
        }
        
        base.OnAction();
        overlay.overlayIntensity = Mathf.Lerp(overlay.overlayIntensity, targetColorOverlayIntensity, Time.deltaTime * 5f);
    }
}
