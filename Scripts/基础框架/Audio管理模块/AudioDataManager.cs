using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDataManager
{
    //音乐，音效
    private static AudioDataManager instance;

    public static AudioDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AudioDataManager();
                instance.LoadData();
            }
            
            return instance;
        }
    }
    
    private AudioDataManager(){ }

    private AudioData data;

    public AudioData Data
    {
        get
        {
            if(data == null) LoadData();
            
            return data;
        }
    }
    
    private List<AudioSource> sources = new List<AudioSource>();
    private Queue<AudioSource> misses = new Queue<AudioSource>();
    //事件订阅与监听
    public void AddAudioListener(AudioSource source)
    {
        if(!sources.Contains(source)) sources.Add(source);
    }
    
    public void RemoveAudioListener(AudioSource source)
    {
        if(sources.Contains(source)) sources.Remove(source);
    }

    public void PushData(AudioData data)
    {
        if(this.Data.musicEnabled != data.musicEnabled) this.Data.musicEnabled = data.musicEnabled;
        if(this.Data.musicVolume != data.musicVolume) this.Data.musicVolume = data.musicVolume;
        if(this.Data.soundEnabled != data.soundEnabled) this.Data.soundEnabled = data.soundEnabled;
        if(this.Data.soundVolume != data.soundVolume) this.Data.soundVolume = data.soundVolume;

        foreach (AudioSource source in sources)
        {
            if (source == null)
            {
                misses.Enqueue(source);
                continue;
            }
            
            source.mute = !data.soundEnabled;
            source.volume = data.soundVolume;
        }

        while (misses.Count > 0)
        {
            sources.Remove(misses.Dequeue());
        }
        
        SaveData();
    }

    public void SaveData()
    {
        BinaryDataManager.Instance.SaveDataToFile("PlayerData/Setting/", "GlobalAudio", data);
    }
    
    public void LoadData()
    {
        data = BinaryDataManager.Instance.LoadDataFromFile<AudioData>("PlayerData/Setting/", "GlobalAudio") ?? new AudioData();
    }
}
