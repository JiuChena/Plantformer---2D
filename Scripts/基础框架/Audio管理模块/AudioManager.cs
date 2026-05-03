using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum StopAudioMode
{
    ClipPause,
    ClipStop,
    ClipMute,
    ClipClear,
}

public enum RemoveAudioMode
{
    RemoveAudioSource,
    RemoveGameObject,
}

public class AudioManager
{
    private static AudioManager instance = new AudioManager();
    public static AudioManager Instance => instance;
    
    private AudioManager() { }

    /// <summary>
    /// 设置(播放)音乐
    /// </summary>
    /// <param name="clipName">音乐切片名称(地址)</param>
    /// <param name="obj">要加音乐的物体</param>
    /// <param name="callback">回调函数</param>
    /// <param name="open3D">是否开启3D音效</param>
    /// <param name="rolloffMode">3D音效衰减模式</param>
    public void SetAudio(string clipName, GameObject obj, AudioClipType type, UnityAction<AudioSource> callback = null, bool open3D = false, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if(audioSource == null) audioSource = obj.AddComponent<AudioSource>();
        
        ResourcesLoadManager.Instance.LoadSync<AudioClip>("AudioClips/" + clipName, (clip) =>
        {
            audioSource.clip = clip;

            switch (type)
            {
                case AudioClipType.Music:
                    audioSource.mute = !AudioDataManager.Instance.Data.musicEnabled;
                    audioSource.volume = AudioDataManager.Instance.Data.musicVolume;
                    break;
                case AudioClipType.Sound:
                    audioSource.mute = !AudioDataManager.Instance.Data.soundEnabled;
                    audioSource.volume = AudioDataManager.Instance.Data.soundVolume;
                    break;
            }
            
            audioSource.playOnAwake = false;
            audioSource.Play();
            if (open3D) audioSource.spatialBlend = 1;
            else audioSource.spatialBlend = 0;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            callback?.Invoke(audioSource);
        });
    }
    
    /// <summary>
    /// 设置(播放)音乐
    /// </summary>
    /// <param name="clipName">音乐切片名称(地址)</param>
    /// <param name="obj">要加音乐的物体</param>
    /// <param name="callback">回调函数</param>
    /// <param name="open3D">是否开启3D音效</param>
    /// <param name="rolloffMode">3D音效衰减模式</param>
    public void SetAudio(AudioClip clip, GameObject obj, AudioClipType type, UnityAction<AudioSource> callback = null, bool open3D = false, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear)
    {
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if(audioSource == null) audioSource = obj.AddComponent<AudioSource>();
        
        audioSource.clip = clip;
        
        switch (type)
        {
            case AudioClipType.Music:
                audioSource.mute = !AudioDataManager.Instance.Data.musicEnabled;
                audioSource.volume = AudioDataManager.Instance.Data.musicVolume;
                break;
            case AudioClipType.Sound:
                audioSource.mute = !AudioDataManager.Instance.Data.soundEnabled;
                audioSource.volume = AudioDataManager.Instance.Data.soundVolume;
                break;
        }
        
        audioSource.playOnAwake = false;
        audioSource.Play();
        if (open3D) audioSource.spatialBlend = 1;
        else audioSource.spatialBlend = 0;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        callback?.Invoke(audioSource);
    }

    /// <summary>
    /// 移除(停止)音乐
    /// </summary>
    /// <param name="audioSource">音源组件</param>
    /// <param name="mode">移除(停止)模式</param>
    /// <param name="callback">回调函数</param>
    public void RemoveAudio(AudioSource audioSource, StopAudioMode mode = StopAudioMode.ClipClear, UnityAction<AudioSource> callback = null)
    {
        if (mode == StopAudioMode.ClipPause)
        {
            audioSource.Pause();
        }
        else if (mode == StopAudioMode.ClipStop)
        {
            audioSource.Stop();
        }
        else if (mode == StopAudioMode.ClipMute)
        {
            audioSource.mute = true;
        }
        else if (mode == StopAudioMode.ClipClear)
        {
            audioSource.clip = null;
        }
        
        callback?.Invoke(audioSource);
        
    }
    
    //无参回调函数重载
    public void RemoveAudio(AudioSource audioSource, RemoveAudioMode mode = RemoveAudioMode.RemoveAudioSource, UnityAction callback = null)
    {
        if (mode == RemoveAudioMode.RemoveAudioSource)
        {
            GameObject.Destroy(audioSource);
        }
        else
        {
            GameObject.Destroy(audioSource.gameObject);
        }
        
        callback?.Invoke();
        
    }
}
