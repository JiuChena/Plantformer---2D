Shader "UI/UIDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // --- 溶解相关参数 ---
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0, 1)) = 0
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1) // 边缘发黄光
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.02   // 边缘宽度
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha // 标准的UI透明混合

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            fixed4 _Color;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _Threshold;
            fixed4 _EdgeColor;
            float _EdgeWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // 采样噪声图（使用和主图相同的UV，如果觉得噪声颗粒太大，可以乘以一个系数如 i.uv * 5）
                float noise = tex2D(_NoiseTex, i.uv).r;
                
                // 计算当前像素与阈值的关系
                float cutout = noise - _Threshold;

                // 核心：丢弃亮度小于阈值的像素 (产生溶解)
                clip(cutout);

                // 计算边缘发光 (当 cutout 在 0 到 _EdgeWidth 之间时，显示边缘颜色)
                float edgeFactor = smoothstep(0, _EdgeWidth, cutout);
                fixed4 finalColor = lerp(_EdgeColor, col, edgeFactor);

                // 保持原始透明度
                finalColor.a *= col.a;
                return finalColor;
            }
            ENDCG
        }
    }
}
