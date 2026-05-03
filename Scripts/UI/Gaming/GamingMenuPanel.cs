using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GamingMenuPanel : PanelBase
{
    [Header("基础设置")]
    public Button bkButton;
    public Button exit;
    public Button resume;
    public Button set;
    [Space(10)]
    public Toggle musicToggle;
    public Slider musicSlider;
    public Toggle soundToggle;
    public Slider soundSlider;
    
    [Header("其他")]
    public AudioClip popSound;
    public AudioClip clickSound;
    
    private GaussianBlur gaussianBlur;
    private PlayerController player;
    
    private bool setting = false;
    
    protected override void LoadInit()
    {
        
    }

    protected override void CompomentInit()
    {
        musicToggle.isOn = AudioDataManager.Instance.Data.musicEnabled;
        musicSlider.value = AudioDataManager.Instance.Data.musicVolume;
        soundToggle.isOn = AudioDataManager.Instance.Data.soundEnabled;
        soundSlider.value = AudioDataManager.Instance.Data.soundVolume;
        
        bkButton.onClick.AddListener(() =>
        {
            if (!setting)
            {
                PanelManager.Instance.PanelHide("Gaming Menu Panel");
            }
            else
            {
                animator.SetTrigger("Menu");
                setting = false;
            }
        });
        
        exit.onClick.AddListener(() =>
        {
            //切换到选关界面
            AudioManager.Instance.SetAudio(clickSound, exit.gameObject, AudioClipType.Sound);
        });
        
        resume.onClick.AddListener(() =>
        {
            PanelManager.Instance.PanelHide("Gaming Menu Panel");
            AudioManager.Instance.SetAudio(clickSound, resume.gameObject, AudioClipType.Sound);
        });
        
        set.onClick.AddListener(() =>
        {
            //调出设置面板
            animator.SetTrigger("Set");
            AudioManager.Instance.SetAudio(clickSound, set.gameObject, AudioClipType.Sound);
            setting = true;
        });
        
        musicToggle.onValueChanged.AddListener(value =>
        {
            AudioDataManager.Instance.PushData(new AudioData { musicEnabled = value });
            AudioManager.Instance.SetAudio(popSound, musicToggle.gameObject, AudioClipType.Sound);
        });
        
        musicSlider.onValueChanged.AddListener(value =>
        {
            AudioDataManager.Instance.PushData(new AudioData { musicVolume = value });
            AudioManager.Instance.SetAudio(popSound, musicSlider.gameObject, AudioClipType.Sound);
        });
        
        soundToggle.onValueChanged.AddListener(value =>
        {
            AudioDataManager.Instance.PushData(new AudioData { soundEnabled = value });
            AudioManager.Instance.SetAudio(popSound, soundToggle.gameObject, AudioClipType.Sound);
        });
        
        soundSlider.onValueChanged.AddListener(value =>
        {
            AudioDataManager.Instance.PushData(new AudioData { soundVolume = value });
            AudioManager.Instance.SetAudio(popSound, soundSlider.gameObject, AudioClipType.Sound);
        });
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!setting)
            {
                //如果不是在设置面板直接退出
                PanelManager.Instance.PanelHide("Gaming Menu Panel");
            }
            else
            {
                //如果在设置面板则返回菜单
                animator.SetTrigger("Menu");
                setting = false;
            }
        }
    }
    
    public override void DisplayPanel()
    {
        this.transform.localPosition = Vector3.zero;
        gaussianBlur = Camera.main.GetComponent<GaussianBlur>();
        gaussianBlur.enabled = true;
        
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player._stats.InputEnable = false;
    }

    public override void HidePanel()
    {
        gaussianBlur.enabled = false;
        player._stats.InputEnable = true;
    }
}
