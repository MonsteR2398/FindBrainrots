Shader "Custom/DynamicTiling_BuiltIn_Mobile_Ambient_Normal_Dynamic"
{
    Properties
    {
        _MainTex ("Albedo (RGB) + Alpha", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,2)) = 1.0

        _OffsetX ("Offset X", Float) = 0.0
        _OffsetY ("Offset Y", Float) = 0.0
        _Scale ("Tiling Scale", Range(0.01, 50)) = 1.0

        _Metallic ("Metallic (approx)", Range(0,1)) = 0.0
        _Smoothness ("Smoothness (0..1)", Range(0,1)) = 0.5

        _Color ("Color Tint", Color) = (1,1,1,1)
        _AmbientIntensity ("Ambient Light Intensity", Range(0,2)) = 1.0
        _NormalStrength ("Normal Influence", Range(0,1)) = 1.0

        _IsDynamic("Is Dynamic", Float) = 0
        _ObjectScale("Object Scale", Vector) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            sampler2D _MainTex;
            sampler2D _BumpMap;

            // Non-instanced — shared across all instances (set on material)
            float _BumpScale;
            float _OffsetX;
            float _OffsetY;
            float _Scale;
            float _Metallic;
            float _Smoothness;
            float _AmbientIntensity;
            float _NormalStrength;

            // Per-instance — set via MaterialPropertyBlock
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float,  _IsDynamic)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ObjectScale)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos            : SV_POSITION;
                float3 worldPos       : TEXCOORD0;
                float3 worldNormal    : TEXCOORD1;
                float3 worldTangent   : TEXCOORD2;
                float3 worldBitangent : TEXCOORD3;
                float3 viewDir        : TEXCOORD4;
                float3 localPos       : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 UnpackNormalFast(float4 nmap)
            {
                float3 n;
                n.xy = nmap.xy * 2 - 1;
                n.z = sqrt(saturate(1 - dot(n.xy, n.xy)));
                return n;
            }

            void TriplanarUVs(float3 pos, float3 normal,
                              out float2 uvX, out float2 uvY, out float2 uvZ,
                              out float3 blend)
            {
                float2 offset = float2(_OffsetX, _OffsetY);
                uvX = pos.yz * _Scale + offset;
                uvY = pos.xz * _Scale + offset;
                uvZ = pos.xy * _Scale + offset;

                float3 wa = abs(normal);
                blend = wa / max(wa.x + wa.y + wa.z, 1e-6);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float4 worldPos4 = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos   = worldPos4.xyz;
                o.pos        = UnityObjectToClipPos(v.vertex);

                o.worldNormal  = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                float3 tangentWS  = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
                float  tangentW   = v.tangent.w;
                float3 bitangentWS = cross(o.worldNormal, tangentWS) * tangentW;

                o.worldTangent    = tangentWS;
                o.worldBitangent  = bitangentWS;
                o.viewDir         = normalize(_WorldSpaceCameraPos - o.worldPos);

                float3 objectScale = UNITY_ACCESS_INSTANCED_PROP(Props, _ObjectScale).xyz;
                o.localPos = v.vertex.xyz * objectScale;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 color     = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float  isDynamic = UNITY_ACCESS_INSTANCED_PROP(Props, _IsDynamic);

                float3 posForUV = lerp(i.worldPos, i.localPos, saturate(isDynamic));

                float2 uvX, uvY, uvZ;
                float3 blend;
                TriplanarUVs(posForUV, i.worldNormal, uvX, uvY, uvZ, blend);

                float4 aX = tex2D(_MainTex, uvX);
                float4 aY = tex2D(_MainTex, uvY);
                float4 aZ = tex2D(_MainTex, uvZ);
                float3 albedo = (aX.rgb * blend.x + aY.rgb * blend.y + aZ.rgb * blend.z) * color.rgb;
                float  alpha  = aX.a;

                float4 nX = tex2D(_BumpMap, uvX);
                float4 nY = tex2D(_BumpMap, uvY);
                float4 nZ = tex2D(_BumpMap, uvZ);

                float3 nsX = UnpackNormalFast(nX) * blend.x;
                float3 nsY = UnpackNormalFast(nY) * blend.y;
                float3 nsZ = UnpackNormalFast(nZ) * blend.z;

                float3 nWorldFromX = float3(nsX.z, nsX.x, nsX.y);
                float3 nWorldFromY = float3(nsY.x, nsY.z, nsY.y);
                float3 nWorldFromZ = float3(nsZ.x, nsZ.y, nsZ.z);

                float3 normalWS = normalize(nWorldFromX + nWorldFromY + nWorldFromZ);
                normalWS = normalize(lerp(i.worldNormal, normalWS, saturate(_BumpScale)));

                float nFactor = 0.5 + 0.5 * normalWS.y;
                nFactor = lerp(1.0, nFactor, _NormalStrength);

                float3 col = albedo * _AmbientIntensity * nFactor;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
