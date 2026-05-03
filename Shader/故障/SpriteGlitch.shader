Shader "Sprite/SpriteGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        // 故障参数
        _GlitchIntensity ("故障强度", Range(0, 1)) = 0
        _GlitchSpeed ("故障速度", Range(1, 100)) = 10
        _ColorShift ("色彩分离强度", Range(0, 1)) = 0.02
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
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AlphaSplitEnabled;

            // 故障参数
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _ColorShift;

            // 随机函数
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // 核心故障：在顶点着色器里做 UV 水平撕裂，保证边缘是硬切的
                float2 uv = IN.texcoord;
                float time = _Time.y * _GlitchSpeed;

                // 生成随机横条
                float sliceLine = step(0.95 - _GlitchIntensity * 0.5, rand(float2(floor(uv.y * 50.0), floor(time))));
                
                // 偏移横条的 X 坐标
                uv.x += sliceLine * (rand(float2(time, uv.y)) - 0.5) * 0.5 * _GlitchIntensity;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = uv;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y * _GlitchSpeed;

                // 核心故障：在片段着色器里做 RGB 色彩分离
                float shiftAmount = step(0.9 - _GlitchIntensity * 0.5, rand(float2(floor(uv.y * 30.0), floor(time))));
                shiftAmount *= _GlitchIntensity * _ColorShift;

                // 红色通道向右偏，蓝色通道向左偏
                float r = tex2D(_MainTex, uv + float2(shiftAmount, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(shiftAmount, 0)).b;
                float a = tex2D(_MainTex, uv).a;

                fixed4 col = float4(r, g, b, a);
                col.rgb *= IN.color.rgb;
                col.a *= IN.color.a;

                #ifdef ETC1_EXTERNAL_ALPHA
                    col.a *= tex2D(_AlphaSplitEnabled, uv).r;
                #endif

                // 标准 Sprite 的预乘 Alpha 处理
                col.rgb *= col.a;
                return col;
            }
        ENDCG
        }
    }
}
