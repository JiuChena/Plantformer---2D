using System.Collections;
using System.Collections.Generic;
using TarodevController;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraProp : HoldablePropBase
{
    public RawImage rawImage;
    public TMP_Text posText; 
    public Material material;
    public float speed = 1;
    public float cooldown = 2;
    public AudioClip takePhotoSound;
    public AudioClip usePhotoSound;
    private float cooldownTimer = 0;

    private Vector2 playerPosition;
    private GaussianBlur gaussianBlur;
    private int cameraState = 0;
    
    private bool handling = false;
    private Transform parent;
    
    
    public override void PropInit()
    {
        base.PropInit();
        
        gaussianBlur = Camera.main.GetComponent<GaussianBlur>();
    }

    public override void PropUpdate()
    {
        switch (cameraState)
        {
            case 0:
                material.SetFloat("_Threshold", Mathf.Clamp(Mathf.Lerp(material.GetFloat("_Threshold"), 1, speed * Time.deltaTime), 0, 1));
                break;
            case 1:
                material.SetFloat("_Threshold", Mathf.Clamp(Mathf.Lerp(material.GetFloat("_Threshold"), 0, speed * Time.deltaTime), 0, 1));
                break;
        }
        
        if(handling) cooldownTimer += Time.deltaTime;
        if(cooldownTimer >= cooldown)
        {
            handling = false;
            cooldownTimer = 0;
            this.transform.SetParent(parent);
        }

        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
    }

    public override void PropOnTriggerEnter2D(Collider2D other)
    {
        base.PropOnTriggerEnter2D(other);
        
        player = other.GetComponent<PlayerController>();
        animator.SetTrigger("HoldNormal");
        AudioManager.Instance.SetAudio(audioClip, this.gameObject, AudioClipType.Sound);
        parent = this.transform.parent;
    }

    public override void OnAction()
    {
        if (Input.GetKeyDown(code))
        {
            if(handling) return;
            
            this.transform.SetParent(null);
            
            switch (cameraState)
            {
                case 0:
                    //拍照
                    AudioManager.Instance.SetAudio(takePhotoSound, this.gameObject, AudioClipType.Sound);
                    animator.SetTrigger("TakePhoto");
                    handling = true;
                    break;
                case 1:
                    //使用相片
                    AudioManager.Instance.SetAudio(usePhotoSound, this.gameObject, AudioClipType.Sound);
                    animator.SetTrigger("UsePhoto");
                    handling = true;
                    break;
            }
        }
    }

    public void FocuseStart()
    {
        //开启高斯模糊,lerp
        gaussianBlur.enabled = true;
    }

    public void FocuseEnd()
    {
        //关闭高斯模糊,lerp
        gaussianBlur.enabled = false;
    }

    public void TakePhoto()
    {
        StartCoroutine(TakePhotoCoroutine());
    }

    private IEnumerator TakePhotoCoroutine()
    {
        //拍照获取当前屏幕图片赋值给RawImage
        yield return new WaitForEndOfFrame();
        
        rawImage.texture = ScreenCapture.CaptureScreenshotAsTexture();
        playerPosition = player.transform.position;
        posText.text = $"Position : ({playerPosition.x.ToString("F1")}, {playerPosition.y.ToString("F1")})";
        
        cameraState = 1;
    }

    public void UsePhoto()
    {
        player.transform.position = playerPosition;
        cameraState = 0;
    }
}
