Shader "Custom/2D_SnowAccumulation_Corrected"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowAmount ("Snow Amount", Range(0,1)) = 0 
        _SnowDepth ("Snow Depth", Float) = 0.3      
        _SnowColor ("Snow Color", Color) = (1,1,1,1) 
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _SnowAmount;
            float _SnowDepth;
            float4 _SnowColor;
            static const float kAlphaThreshold = 0.1;
            static const int kSurfaceSearchSteps = 128;
            static const int kSurfaceRefineSteps = 5;

            // 生成柔和的噪声函数
            float snowWaveNoise(float x)
            {
                return sin(x * 6.0) * 0.4 + 
                       sin(x * 14.0) * 0.2 + 
                       sin(x * 3.0) * 0.6;
            }

            float findTopSurface(float x)
            {
                float stepSize = 1.0 / kSurfaceSearchSteps;
                float transparentY = 1.0;

                [unroll]
                for (int step = kSurfaceSearchSteps - 1; step >= 0; step--)
                {
                    float y = (step + 0.5) / kSurfaceSearchSteps;
                    float alpha = tex2D(_MainTex, float2(x, y)).a;

                    if (alpha > kAlphaThreshold)
                    {
                        float opaqueY = y;
                        float low = opaqueY;
                        float high = transparentY;

                        [unroll]
                        for (int refine = 0; refine < kSurfaceRefineSteps; refine++)
                        {
                            float mid = (low + high) * 0.5;
                            float midAlpha = tex2D(_MainTex, float2(x, mid)).a;

                            if (midAlpha > kAlphaThreshold)
                            {
                                low = mid;
                            }
                            else
                            {
                                high = mid;
                            }
                        }

                        return low;
                    }

                    transparentY = y + stepSize * 0.5;
                }

                return -1.0;
            }

            float surfaceSupport(float x, float topSurfaceY, float highestSurfaceY)
            {
                float sampleOffset = max(_MainTex_TexelSize.x * 3.0, 0.01);
                float leftTop = findTopSurface(saturate(x - sampleOffset));
                float rightTop = findTopSurface(saturate(x + sampleOffset));

                float sideDelta = max(abs(topSurfaceY - leftTop), abs(topSurfaceY - rightTop));
                float flatnessSupport = 1.0 - smoothstep(0.015, 0.12, sideDelta);

                // 只有接近整体最高点的顶部区域才保留积雪，避免圆弧两侧像墙面一样大面积挂雪。
                float heightBand = max(_SnowAmount * 1.15 + _SnowDepth * 0.06, 0.08);
                float heightSupport = smoothstep(highestSurfaceY - heightBand, highestSurfaceY - heightBand * 0.2, topSurfaceY);

                return saturate(flatnessSupport * heightSupport);
            }

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 groundColor = texColor * i.color;

                if (texColor.a <= 0.001)
                {
                    return groundColor;
                }
                
                if (_SnowAmount <= 0.0)
                {
                    return groundColor;
                }

                float topSurfaceY = findTopSurface(i.uv.x);
                if (topSurfaceY < 0.0)
                {
                    return groundColor;
                }

                float highestSurfaceY = findTopSurface(0.5);
                float support = surfaceSupport(i.uv.x, topSurfaceY, highestSurfaceY);
                float effectiveSnowAmount = _SnowAmount * support;
                float snowLine = 1.0 - effectiveSnowAmount;
                float noise = snowWaveNoise(i.uv.x * 100.0) * (0.02 + _SnowDepth * 0.05) * support;
                float edgeSoftness = 0.01 + _SnowDepth * 0.03;
                float snowFactor = smoothstep(snowLine - edgeSoftness + noise, snowLine + edgeSoftness + noise, i.uv.y);
                snowFactor *= step(kAlphaThreshold, texColor.a);

                fixed4 snowCol = fixed4(_SnowColor.rgb, groundColor.a);
                return lerp(groundColor, snowCol, snowFactor);
            }

            ENDCG
        }
    }

    FallBack "Sprites/Default"

}
