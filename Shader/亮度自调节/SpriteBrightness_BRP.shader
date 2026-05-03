Shader "Custom/SpriteBrightness_BRP"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        // 亮度参数：范围0~5，允许过曝发白
        _Brightness ("Brightness", Range(0, 5)) = 1.0 
    }

    SubShader
    {
        // 下面这些Tags是Unity 2D渲染和排序层正常工作的核心！缺一不可
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Sprite" 
            "CanUseSpriteAtlas"="True" 
        }

        // 2D Sprite 标准设置
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha // 标准的透明混合模式

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // 包含Sprite专用的批处理宏，防止打断合批
            #include "UnityCG.cginc" 
            #include "UnitySprites.cginc"

            struct appdata_t1
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f1
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 声明变量
            float _Brightness;

            v2f1 vert(appdata_t1 v)
            {
                v2f1 o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                // 乘以内部_Tint颜色（由Unity SpriteRenderer的Color控制）
                o.color = v.color * _RendererColor;
                return o;
            }

            fixed4 frag(v2f1 i) : SV_Target
            {
                // 采样贴图
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                
                // 核心逻辑：仅对RGB通道乘以亮度系数，不影响Alpha透明度
                c.rgb *= _Brightness;
                
                // 处理预乘Alpha（Unity默认Sprite开启预乘，必须加上这句否则颜色会发黑）
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    // 回退到Unity默认的Sprite Shader
    Fallback "Sprites/Default"
}
