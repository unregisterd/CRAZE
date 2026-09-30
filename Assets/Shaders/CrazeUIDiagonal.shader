// P5 风格 UGUI 斜切角着色器
// 用途：给 UI Image 赋予「尖锐斜切角」+「对角切割」，复刻女神异闻录的大色块卡片感。
// 用法：新建 Material 使用此 Shader，赋给 Image.material；调节 _Bevel（斜切角大小）与 _CutAmount（对角切割）。
Shader "Craze/UI/Diagonal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shape)]
        _Bevel ("Corner Bevel (斜切角)", Range(0,0.5)) = 0.08
        _CutAmount ("Diagonal Cut (对角切割)", Range(0,1)) = 0
        _CutDirection ("Cut Slide (切割滑移)", Range(-1,1)) = 0

        [Header(Masking)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_CLIP_RECT)] _UseClipRect ("Use Clip Rect", Float) = 0
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }

        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Bevel;
            float _CutAmount;
            float _CutDirection;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 判断 uv 是否落在「斜切角」外侧；落在即返回真(应丢弃)
            bool OutOfBevel(float2 uv, float be)
            {
                // 左上角
                if (uv.x < be && uv.y > 1 - be)
                {
                    float lx = uv.x / be;
                    float ly = (1 - uv.y) / be;
                    if (lx + ly > 1.01) return true;
                }
                // 右上角
                if (uv.x > 1 - be && uv.y > 1 - be)
                {
                    float lx = (1 - uv.x) / be;
                    float ly = (1 - uv.y) / be;
                    if (lx + ly > 1.01) return true;
                }
                // 左下角
                if (uv.x < be && uv.y < be)
                {
                    float lx = uv.x / be;
                    float ly = uv.y / be;
                    if (lx + ly > 1.01) return true;
                }
                // 右下角
                if (uv.x > 1 - be && uv.y < be)
                {
                    float lx = (1 - uv.x) / be;
                    float ly = uv.y / be;
                    if (lx + ly > 1.01) return true;
                }
                return false;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                float2 uv = IN.texcoord;

                // ① 四角斜切（P5 尖锐卡片）
                if (_Bevel > 0.001)
                {
                    if (OutOfBevel(uv, _Bevel)) discard;
                }

                // ② 对角大切割（把卡片斜切成两部分，可做扫入动效）
                if (_CutAmount > 0.001)
                {
                    float d = uv.x + uv.y;
                    float offset = _CutDirection * 0.5;
                    float threshold = _CutAmount * 2.0 + offset;
                    if (d > threshold) discard;
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
