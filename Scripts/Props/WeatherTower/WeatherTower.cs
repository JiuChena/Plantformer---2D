using UnityEngine;

public class WeatherTower : ScenePropBase
{
    private WeatherManager weatherManager;

    public override void PropInit()
    {
        base.PropInit();

        weatherManager = WeatherManager.Instance;

        // 注册所有天气开始监听
        weatherManager.AddStartListener(WeatherType.Sunny, OnSunnyStart);
        weatherManager.AddStartListener(WeatherType.Rain, OnRainStart);
        weatherManager.AddStartListener(WeatherType.AcidRain, OnAcidRainStart);
        weatherManager.AddStartListener(WeatherType.Snow, OnSnowStart);
    }

    private void OnDestroy()
    {
        if (weatherManager == null) return;
        weatherManager.RemoveStartListener(WeatherType.Sunny, OnSunnyStart);
        weatherManager.RemoveStartListener(WeatherType.Rain, OnRainStart);
        weatherManager.RemoveStartListener(WeatherType.AcidRain, OnAcidRainStart);
        weatherManager.RemoveStartListener(WeatherType.Snow, OnSnowStart);
    }

    public override void OnPropActivate()
    {
        // 玩家在塔附近跳跃 → 循环切换天气
        var nextIndex = ((int)weatherManager.CurrentWeather + 1) % System.Enum.GetValues(typeof(WeatherType)).Length;
        weatherManager.SetWeather((WeatherType)nextIndex);
        
        AudioManager.Instance.SetAudio(triggerSound, this.gameObject, AudioClipType.Sound);
    }

    private void OnSunnyStart()    => animator?.SetTrigger("Sun");
    private void OnRainStart()     => animator?.SetTrigger("Rain");
    private void OnAcidRainStart() => animator?.SetTrigger("AcidRain");
    private void OnSnowStart()     => animator?.SetTrigger("Snow");
}
