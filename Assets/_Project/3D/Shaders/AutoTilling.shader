Shader "Custom/AutoTilling"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _Tiling("Tiling", Float) = 1.0
        _Offset("Offset", Vector) = (0, 0, 0)

        // Lit properties
        [Toggle(_NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0

        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5

        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
        _EmissionMap("Emission Map", 2D) = "white" {}
        _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)

        [HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        [HideInInspector] _Surface("Surface", Float) = 0.0
        [HideInInspector] _Blend("Blend", Float) = 0.0
        [HideInInspector] _Cull("Cull", Float) = 2.0
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0.0
        [HideInInspector] _Cutoff("Cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _SrcBlend("Src Blend", Float) = 1.0
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0.0
        [HideInInspector] _QueueOffset("Queue Offset", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM

            #pragma target 4.5

            // Material keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ALPHAMODULATE_ON

            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile _ NORMALMAP
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_COOKIES

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 lightmapUV : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Tiling;
                float3 _Offset;
                float _BumpScale;
                float _Metallic;
                float _Smoothness;
                half4 _EmissionColor;
                float _Cutoff;
                float _Surface;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                real sign = IN.tangentOS.w * GetOddNegativeScale();
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), sign);

                OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // World-space tri-planar UVs
                float3 worldPos = IN.positionWS + _Offset;
                float3 worldNormal = normalize(IN.normalWS);
                float3 blendWeights = abs(worldNormal);
                blendWeights = max(blendWeights, 0.001);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z);

                float2 uvX = worldPos.zy * _Tiling;
                float2 uvY = worldPos.xz * _Tiling;
                float2 uvZ = worldPos.xy * _Tiling;

                // Sample base map from all 3 projections
                half4 colorX = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvX);
                half4 colorY = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvY);
                half4 colorZ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvZ);
                half4 albedo = colorX * blendWeights.x + colorY * blendWeights.y + colorZ * blendWeights.z;
                albedo *= _BaseColor;

                // Sample normal map from all 3 projections if enabled
                float3 normalTS = float3(0, 0, 1);
                #if defined(_NORMALMAP)
                {
                    half4 normalX = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvX);
                    half4 normalY = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvY);
                    half4 normalZ = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvZ);
                    half4 normalSample = normalX * blendWeights.x + normalY * blendWeights.y + normalZ * blendWeights.z;
                    normalTS = UnpackNormalScale(normalSample, _BumpScale);
                }
                #endif

                // Sample metallic/smoothness per projection (use _BaseMap alpha for smoothness by default)
                half metallic = _Metallic;
                half smoothness = _Smoothness;

                // Sample emission
                half3 emission = 0;
                #if defined(_EMISSION)
                {
                    half4 emissionX = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uvX);
                    half4 emissionY = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uvY);
                    half4 emissionZ = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uvZ);
                    half4 emissionSample = emissionX * blendWeights.x + emissionY * blendWeights.y + emissionZ * blendWeights.z;
                    emission = emissionSample.rgb * _EmissionColor.rgb;
                }
                #endif

                // Build surface data
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalize(IN.normalWS);
                
                #if defined(_NORMALMAP)
                {
                    // Transform tangent-space normal to world-space using tri-planar tangent basis
                    float3 worldNormalInput = inputData.normalWS;
                    
                    // Construct tangent and bitangent using derivatives of world position for tri-planar
                    float3 dpdx = ddx(IN.positionWS);
                    float3 dpdy = ddy(IN.positionWS);
                    float3 tangent = normalize(dpdx);
                    float3 bitangent = normalize(cross(worldNormalInput, tangent));

                    float3x3 tangentToWorld = float3x3(tangent, bitangent, worldNormalInput);
                    inputData.normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));
                }
                #endif

                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = 0;

                #if defined(LIGHTMAP_ON)
                    inputData.bakedGI = SampleLightmap(IN.lightmapUV, inputData.normalWS);
                #else
                    inputData.bakedGI = SampleSH(inputData.normalWS);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);

                // Build surface properties
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.alpha = albedo.a;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = 1;
                surfaceData.specular = half3(0, 0, 0);
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                // Handle alpha
                #if defined(_ALPHATEST_ON)
                    clip(surfaceData.alpha - _Cutoff);
                #endif

                // Compute lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                // Apply fog
                color.rgb = MixFog(color.rgb, inputData.fogCoord);

                #if defined(_SURFACE_TYPE_TRANSPARENT)
                    color.a = surfaceData.alpha;
                #else
                    color.a = 1;
                #endif

                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _SURFACE_TYPE_TRANSPARENT

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Tiling;
                float3 _Offset;
                float _Cutoff;
            CBUFFER_END

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 ShadowPassFragment(Varyings IN) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                    clip(texColor.a * _BaseColor.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        // Depth only pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _SURFACE_TYPE_TRANSPARENT

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Tiling;
                float3 _Offset;
                float _Cutoff;
            CBUFFER_END

            Varyings DepthOnlyVertex(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 DepthOnlyFragment(Varyings IN) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                    clip(texColor.a * _BaseColor.a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}