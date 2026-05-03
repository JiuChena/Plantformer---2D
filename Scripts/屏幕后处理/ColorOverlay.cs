using UnityEngine;

[ExecuteInEditMode] // 让你在编辑器模式下也能实时预览调色效果
public class ColorOverlay : MonoBehaviour
{
    public Shader postShader;
    private Material postMaterial;

    [Header("基础设置")]
    [Range(0f, 3f)]
    public float brightness = 1.0f; // 亮度

    [Header("颜色叠加")]
    public Color overlayColor = Color.white; // 叠加的颜色
    [Range(0f, 1f)]
    public float overlayIntensity = 0f; // 叠加强度 (0为原画面，1为完全叠加)

    [Header("噪声效果 (斑驳感)")]
    [Tooltip("噪声强度，控制斑驳的明显程度")]
    [Range(0f, 1f)]
    public float noiseIntensity = 0.1f; // 噪声强度
    
    [Tooltip("噪声缩放，值越小噪声颗粒越大")]
    [Range(0.1f, 100f)]
    public float noiseScale = 10f; // 噪声缩放
    
    [Tooltip("噪声动画速度，0为静态噪声")]
    [Range(0f, 5f)]
    public float noiseSpeed = 0.5f; // 噪声动画速度
    
    [Tooltip("噪声不透明度，控制噪声对画面的影响程度")]
    [Range(0f, 1f)]
    public float noiseOpacity = 1f; // 噪声不透明度

    // 初始化材质
    void CheckShaderAndCreateMaterial()
    {
        if (postMaterial == null && postShader != null)
        {
            postMaterial = new Material(postShader);
            postMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void Start()
    {
        CheckShaderAndCreateMaterial();
        // 如果当前平台不支持该后处理，禁用此脚本
        if (!SystemInfo.supportsImageEffects || postShader == null || !postShader.isSupported)
        {
            enabled = false;
            return;
        }
    }

    // BRP 实现后处理的核心函数
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CheckShaderAndCreateMaterial();
        if (postMaterial == null)
        {
            Graphics.Blit(source, destination); // 出错时直接绘制原图
            return;
        }

        // 传递参数给 Shader
        postMaterial.SetFloat("_Brightness", brightness);
        postMaterial.SetColor("_OverlayColor", overlayColor);
        postMaterial.SetFloat("_OverlayIntensity", overlayIntensity);
        
        // 传递噪声参数
        postMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        postMaterial.SetFloat("_NoiseScale", noiseScale);
        postMaterial.SetFloat("_NoiseSpeed", noiseSpeed);
        postMaterial.SetFloat("_NoiseOpacity", noiseOpacity);

        // 使用 Shader 处理 source 纹理，并输出到 destination
        Graphics.Blit(source, destination, postMaterial);
    }

    // 清理内存
    void OnDisable()
    {
        if (postMaterial != null)
        {
            DestroyImmediate(postMaterial);
        }
    }
}