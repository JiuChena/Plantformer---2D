Shader "Custom/ColorOverlay"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader
    {
        // 后处理 Shader 的标准配置：关闭深度写入、剔除等
        ZTest Always Cull Off ZWrite Off Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 声明变量
            sampler2D _MainTex;
            float _Brightness;
            float3 _OverlayColor;
            float _OverlayIntensity;
            
            // 噪声参数
            float _NoiseIntensity;      // 噪声强度
            float _NoiseScale;          // 噪声缩放
            float _NoiseSpeed;          // 噪声动画速度
            float _NoiseOpacity;        // 噪声不透明度

            // 顶点着色器输入结构体
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 顶点着色器输出结构体
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 noiseUV : TEXCOORD1;  // 用于噪声的UV
            };

            // Simplex 2D 噪声函数（产生斑驳感）
            // 基于 Ashima Arts 的噪声函数
            float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float3 permute(float3 x) { return mod289(((x*34.0)+1.0)*x); }

            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187, 0.366025403784439,
                                       -0.577350269189626, 0.024390243902439);
                float2 i  = floor(v + dot(v, C.yy));
                float2 x0 = v -   i + dot(i, C.xx);
                float2 i1;
                i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;
                i = mod289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0))
                                        + i.x + float3(0.0, i1.x, 1.0));
                float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy),
                                             dot(x12.zw,x12.zw)), 0.0);
                m = m*m;
                m = m*m;
                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;
                m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
                float3 g;
                g.x  = a0.x  * x0.y  - h.x  * x0.x;
                g.yz = a0.yz * x12.xz - h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }

            // 分形噪声（多层叠加，更自然的斑驳效果）
            float fbm(float2 p, float time)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                // 叠加4层噪声
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * snoise(p * frequency + time * 0.1);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            // 顶点着色器
            v2f vert (appdata v)
            {
                v2f o;
                // 将顶点转换到裁剪空间
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.noiseUV = v.uv * _NoiseScale;  // 应用噪声缩放
                return o;
            }

            // 片元着色器
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 采样原始屏幕图像
                fixed4 col = tex2D(_MainTex, i.uv);

                // 2. 修改屏幕亮度 (直接与颜色相乘)
                col.rgb *= _Brightness;

                // 3. 计算 Overlay 叠加效果
                // 公式原理 (等同于 Photoshop 的 Overlay 混合模式)：
                // 如果底色(原屏幕) < 0.5，结果 = 2 * 底色 * 叠加色 (正片叠底效果，变暗)
                // 如果底色(原屏幕) >= 0.5，结果 = 1 - 2 * (1 - 底色) * (1 - 叠加色) (滤色效果，变亮)
                float3 baseColor = col.rgb;
                float3 blendColor = _OverlayColor;
                
                float3 overlayResult = float3(0,0,0);
                overlayResult.r = (baseColor.r < 0.5) ? (2.0 * baseColor.r * blendColor.r) : (1.0 - 2.0 * (1.0 - baseColor.r) * (1.0 - blendColor.r));
                overlayResult.g = (baseColor.g < 0.5) ? (2.0 * baseColor.g * blendColor.g) : (1.0 - 2.0 * (1.0 - baseColor.g) * (1.0 - blendColor.g));
                overlayResult.b = (baseColor.b < 0.5) ? (2.0 * baseColor.b * blendColor.b) : (1.0 - 2.0 * (1.0 - baseColor.b) * (1.0 - blendColor.b));

                // 4. 根据叠加强度，将原画面与叠加后的画面进行线性插值
                // 简单粗暴的相乘混合，即使纯黑也能看出颜色倾向
                col.rgb *= lerp(float3(1,1,1), _OverlayColor, _OverlayIntensity);

                // 5. 应用噪声效果（斑驳感）
                // 计算时间因子用于动画
                float time = _Time.y * _NoiseSpeed;
                
                // 生成分形噪声值
                float noise = fbm(i.noiseUV, time);
                
                // 将噪声从 [-1, 1] 映射到 [0, 1]
                noise = noise * 0.5 + 0.5;
                
                // 创建斑驳的颜色变化
                float3 noiseColor = float3(noise, noise, noise);
                
                // 应用噪声强度和不透明度
                // noiseColor 在 [0.5, 1.5] 范围内变化，产生明暗斑驳效果
                float noiseEffect = (noise - 0.5) * 2.0 * _NoiseIntensity;
                
                // 将噪声效果混合到最终颜色
                col.rgb += noiseEffect * _NoiseOpacity;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
