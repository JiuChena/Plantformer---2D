using UnityEngine;

public class SnowGround : MonoBehaviour
{
    private Material snowMaterial;
    public float accumulationSpeed = 0.01f; // 积雪速度

    void Start()
    {
        snowMaterial = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
        // 如果游戏里在下雪，就增加积雪量
        if (IsSnowing()) // 你可以自己写个判断，比如根据天气系统
        {
            // 获取当前积雪量，缓慢增加，最大不超过 1
            float currentSnow = snowMaterial.GetFloat("_SnowAmount");
            snowMaterial.SetFloat("_SnowAmount", Mathf.Clamp(currentSnow + accumulationSpeed * Time.deltaTime, 0, 1));
        }
    }

    bool IsSnowing()
    {
        // 简单写死为 true，实际应通过事件总线 GameEvent 获取天气状态
        return true; 
    }
}