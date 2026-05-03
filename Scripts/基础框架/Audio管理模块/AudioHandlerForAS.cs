using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandlerForAS : MonoBehaviour
{
    public AudioClipType type;
    private AudioSource source;
    
    public bool lerp = false;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        AudioDataManager.Instance.AddAudioListener(source);

        if (lerp) source.volume = 0;

        switch (type)
        {
            case AudioClipType.Music:
                source.mute = !AudioDataManager.Instance.Data.musicEnabled;
                source.volume = AudioDataManager.Instance.Data.musicVolume;
                break;
            case AudioClipType.Sound:
                source.mute = !AudioDataManager.Instance.Data.soundEnabled;
                source.volume = AudioDataManager.Instance.Data.soundVolume;
                break;
        }
    }

    private void Update()
    {
        if (lerp)
        {
            switch (type)
            {
                case AudioClipType.Music:
                    source.volume = Mathf.Lerp(source.volume, AudioDataManager.Instance.Data.musicVolume, Time.deltaTime);
                    break;
                case AudioClipType.Sound:
                    source.volume = Mathf.Lerp(source.volume, AudioDataManager.Instance.Data.soundVolume, Time.deltaTime);
                    break;
            }
        }
    }
}
