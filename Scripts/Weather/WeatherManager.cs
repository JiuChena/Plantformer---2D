using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeatherType
{
    Sunny,
    Rain,
    AcidRain,
    Snow
}

public class WeatherManager : MonoBehaviour
{
    private static WeatherManager instance;
    public static WeatherManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;
        InitEventDicts();
    }

    public GameObject rainObject;
    public GameObject acidRainObject;
    public GameObject snowObject;

    private WeatherType _currentWeather = WeatherType.Sunny;
    public WeatherType CurrentWeather => _currentWeather;

    private Dictionary<WeatherType, UnityEvent> _onWeatherStart = new Dictionary<WeatherType, UnityEvent>();
    private Dictionary<WeatherType, UnityEvent> _onWeatherEnd = new Dictionary<WeatherType, UnityEvent>();

    private void InitEventDicts()
    {
        var types = (WeatherType[])System.Enum.GetValues(typeof(WeatherType));
        foreach (var t in types)
        {
            _onWeatherStart[t] = new UnityEvent();
            _onWeatherEnd[t] = new UnityEvent();
        }
    }

    /// <summary>
    /// 注册天气开始事件的监听。
    /// </summary>
    public void AddStartListener(WeatherType type, UnityAction action)
    {
        _onWeatherStart[type].AddListener(action);
    }

    /// <summary>
    /// 移除天气开始事件的监听。
    /// </summary>
    public void RemoveStartListener(WeatherType type, UnityAction action)
    {
        _onWeatherStart[type].RemoveListener(action);
    }

    /// <summary>
    /// 注册天气结束事件的监听。
    /// </summary>
    public void AddEndListener(WeatherType type, UnityAction action)
    {
        _onWeatherEnd[type].AddListener(action);
    }

    /// <summary>
    /// 移除天气结束事件的监听。
    /// </summary>
    public void RemoveEndListener(WeatherType type, UnityAction action)
    {
        _onWeatherEnd[type].RemoveListener(action);
    }

    /// <summary>
    /// 根据枚举类型切换天气。
    /// </summary>
    public void SetWeather(WeatherType type)
    {
        if (type == _currentWeather) return;

        var previousType = _currentWeather;
        _currentWeather = type;

        // 结束上一天气
        EndWeather(previousType);

        // 开始新天气
        StartWeather(type);
    }

    private void EndWeather(WeatherType type)
    {
        var go = GetWeatherObject(type);
        if (go != null)
        {
            var weatherBase = go.GetComponent<WeatherBase>();
            if (weatherBase != null) weatherBase.OnEnd();
            go.SetActive(false);
        }
        _onWeatherEnd[type]?.Invoke();
    }

    private void StartWeather(WeatherType type)
    {
        if (type == WeatherType.Sunny)
        {
            _onWeatherStart[type]?.Invoke();
            return;
        }

        var go = GetWeatherObject(type);
        if (go != null)
        {
            go.SetActive(true);
            var weatherBase = go.GetComponent<WeatherBase>();
            if (weatherBase != null) weatherBase.OnStart();
        }
        _onWeatherStart[type]?.Invoke();
    }

    private GameObject GetWeatherObject(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Rain:     return rainObject;
            case WeatherType.AcidRain: return acidRainObject;
            case WeatherType.Snow:     return snowObject;
            default:                   return null;
        }
    }
}
