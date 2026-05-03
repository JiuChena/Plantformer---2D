using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TVGlitchEffect : MonoBehaviour
{
    public bool enable = false;
    public Shader glitchShader;
    private Material glitchMaterial;

    [Range(0, 1)]
    public float intensity = 0f;

    private float timeOffset;

    void Start()
    {
        if (glitchShader != null)
            glitchMaterial = new Material(glitchShader);
    }

    void Update()
    {
        timeOffset += Time.deltaTime * 100;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enable || glitchMaterial == null || intensity <= 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture tempRT = source;
        if (source.antiAliasing > 1)
        {
            // 如果当前画面是多重采样的，先把它降采样成普通纹理，Shader 才能正常读取
            tempRT = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            Graphics.Blit(source, tempRT);
        }

        glitchMaterial.SetFloat("_Intensity", intensity);
        glitchMaterial.SetFloat("_TimeOffset", timeOffset);
        
        // 使用处理好的 tempRT 去渲染
        Graphics.Blit(tempRT, destination, glitchMaterial);

        // 释放临时纹理，防止内存泄漏
        if (tempRT != source)
        {
            RenderTexture.ReleaseTemporary(tempRT);
        }
    }

    // 触发一次短暂花屏的方法保留着
    public void TriggerGlitch(float duration = 0.3f, float peakIntensity = 0.8f)
    {
        StartCoroutine(GlitchRoutine(duration, peakIntensity));
    }

    System.Collections.IEnumerator GlitchRoutine(float duration, float peak)
    {
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            intensity = Mathf.Lerp(peak, 0, timer / duration) * (Random.value > 0.3f ? 1f : 0.1f);
            yield return null;
        }
        intensity = 0;
    }
}