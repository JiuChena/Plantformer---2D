Shader "Hidden/TVGlitchEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Intensity;
            float _TimeOffset;

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 水平撕裂
                float lineNoise = step(0.99 - _Intensity * 0.3, random(floor(uv.y * 50) + _TimeOffset));
                uv.x += lineNoise * 0.1 * _Intensity;

                // 色彩分离
                float colorShift = lineNoise * 0.02 * _Intensity;
                float r = tex2D(_MainTex, uv + float2(colorShift, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(colorShift, 0)).b;

                // 随机方块噪点
                float blockNoise = random(floor(i.uv * 200) + _TimeOffset);
                blockNoise = step(0.99 - _Intensity * 0.15, blockNoise);

                // 扫描线
                float scanline = sin(i.uv.y * 600) * 0.04;

                // 组合最终颜色
                fixed4 col = float4(r, g, b, 1.0);
                col.rgb += blockNoise * _Intensity; 
                
                // 【已修复】：扫描线效果现在受 Intensity 控制，调为0时画面不再变暗
                col.rgb -= scanline * _Intensity;               

                return col;
            }
            ENDCG
        }
    }
}
