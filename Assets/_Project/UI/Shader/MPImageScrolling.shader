Shader "UI/ScrollingRawImage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _ScrollSpeedX ("Скорость X", Float) = 0.5
        _ScrollSpeedY ("Скорость Y", Float) = 0
        _BlurAmount ("Размытие", Range(0, 0.05)) = 0.01
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata_t
            {
                float4 vertex: POSITION;
                float4 color: COLOR;
                float2 texcoord: TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                float2 texcoord: TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            float _ScrollSpeedX;
            float _ScrollSpeedY;
            float _BlurAmount;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                #ifdef UNITY_HALF_TEXEL_OFFSET
                    OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1.0, 1.0);
                #endif
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN): SV_Target
            {
                // Прокрутка текстуры
                float2 scrolledUV = IN.texcoord;
                scrolledUV.x += _ScrollSpeedX * _Time.y;
                scrolledUV.y += _ScrollSpeedY * _Time.y;
                scrolledUV = frac(scrolledUV);
                
                fixed4 texColor = tex2D(_MainTex, scrolledUV);
                
                // Простое боксовое размытие
                if (_BlurAmount > 0)
                {
                    float2 blurOffset = float2(_BlurAmount, 0);
                    texColor += tex2D(_MainTex, scrolledUV + blurOffset);
                    texColor += tex2D(_MainTex, scrolledUV - blurOffset);
                    texColor += tex2D(_MainTex, scrolledUV + float2(0, _BlurAmount));
                    texColor += tex2D(_MainTex, scrolledUV - float2(0, _BlurAmount));
                    texColor /= 5;
                }
                
                half4 color = texColor * IN.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif
                
                return color;
            }
            ENDCG
        }
    }
}