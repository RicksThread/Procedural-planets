Shader "Custom/Scoc"
{
    Properties
    {
        _GrassColor ("Grass Color", Color) = (0.2,0.7,0.2)
        _GrassTex("Grass tex", 2D)  = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"  "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 200
            ZWrite On
            AlphaToMask On
            Cull Off

            HLSLPROGRAM

            // Signal this shader requires a compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 5.0

            // Lighting and shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "HLSLSupport.cginc"
            #include "Assets/UtilityPack/Shaders/LightingHelper.hlsl"
            
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 positionHCS  : TEXCOORD0;
                float3 normalOS     : NORMAL;
                float3 normalWS     : TEXCOORD1;
                float3 positionOS   : TEXCOORD2;
                float3 positionWS   : TEXCOORD3;
                float2 uv           : TEXCOORD4;
                float4 shadowCoord  : TEXCOORD5;
            };

            struct DrawVertex
            {
                float3 pos;
                float2 uv;
            };

            struct Triangle
            {
                DrawVertex drawVertices[3];
            };

            struct Quad
            {
                float3 normal;
                float3 tangent;
                float3 bitangent;
                float3 posOS;
                Triangle triangles[2];
            };


            sampler2D _GrassTex;
            
            StructuredBuffer<Quad> folliageQuads;

            half4 _GrassColor;
            float windSpeedBase;
            float windAmplitude;
            float windPhaseSensitivity;
            float sampleScaleNoiseWind;
            float sampleScalePhaseWind;
            float dstRender;

            float4x4 _LocalToWS;
            TEXTURE2D(_NOISE_WINDTEX); SAMPLER(sampler_NOISE_WINDTEX);
            TEXTURE2D(_PHASE_WINDTEX); SAMPLER(sampler_PHASE_WINDTEX);

            Varyings vert(uint idVertex : SV_VERTEXID, uint idInstance : SV_INSTANCEID)
            {
                Varyings o = (Varyings)0;
                Quad quadTarget = folliageQuads[idInstance];
                if (quadTarget.normal.x == quadTarget.tangent.x) return o;
                float3 quadPosWs = mul(_LocalToWS, quadTarget.posOS); 
                if (length(quadPosWs - _WorldSpaceCameraPos) > dstRender) return o;
                
                
                Triangle triangleTarget = quadTarget.triangles[idVertex/3];
                float windValue = SAMPLE_TEXTURE2D_LOD(_NOISE_WINDTEX,sampler_NOISE_WINDTEX, quadTarget.posOS.xy*sampleScaleNoiseWind,0).x;
                float windPhaseValue = SAMPLE_TEXTURE2D_LOD(_PHASE_WINDTEX, sampler_PHASE_WINDTEX,  quadTarget.posOS.xy*sampleScaleNoiseWind,0).x;
                float amountWind = sin(_Time.y*windSpeedBase-windPhaseValue*windPhaseSensitivity);
                
                float3 posVertex = triangleTarget.drawVertices[idVertex%3].pos;
                float2 uv = triangleTarget.drawVertices[idVertex%3].uv;
                
                if (uv.y > 0.9f)
                {
                     posVertex += quadTarget.bitangent * amountWind* windValue * windAmplitude;
                }
                
                float3 posWS = mul(_LocalToWS,float4(posVertex,1));
                float4 posCS = mul(UNITY_MATRIX_MVP, float4(posWS,1));
                
                VertexPositionInputs vertexInputs = GetVertexPositionInputs(posVertex.xyz);
                VertexNormalInputs vertNormalInputs = GetVertexNormalInputs(float4(quadTarget.normal,1), float4(quadTarget.tangent,1));
          
                vertexInputs.positionCS = posCS;
                vertexInputs.positionWS = posWS;

                o.positionWS = posWS;
                o.positionCS = posCS;
                o.positionOS = posVertex;
                o.positionHCS = TransformObjectToHClip(posVertex.xyz);
                o.normalOS = quadTarget.normal;
                o.normalWS = mul(_LocalToWS,float4(quadTarget.normal,1));
                o.uv = uv;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                o.shadowCoord = GetShadowCoord(vertexInputs);
                #endif
                return o;
            }

            half3 GetColorPBR(Varyings IN, half4 albedo)
            {
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.alpha = 1;
                surfaceData.emission = 0;
                surfaceData.metallic = 0;
                surfaceData.occlusion = 0;
                surfaceData.smoothness = 1;
                surfaceData.specular = 0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;
                surfaceData.normalTS = 1;

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = IN.normalWS;
                lightingInput.viewDirectionWS = normalize(IN.positionWS-_WorldSpaceCameraPos);
                lightingInput.shadowCoord = IN.shadowCoord;
                return CalculateSimpleLighting(lightingInput, surfaceData);
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                half4 albedo = tex2D(_GrassTex, IN.uv) * _GrassColor;
                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    IN.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                #endif
                return half4(GetColorPBR(IN, albedo),albedo.w);
            }
            ENDHLSL
        }
    }
}