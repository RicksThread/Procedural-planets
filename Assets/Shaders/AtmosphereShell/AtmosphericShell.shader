Shader "Custom/AtmosphericShell"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0.2,0.2,0.9)
    }
    SubShader
    {
        Tags {  "Queue" = "Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline"}

        Cull Back 
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
           
            #include "Assets/UtilityPack/Shaders/Utilities.hlsl"
            #include "Assets/UtilityPack/Shaders/MathHelper.hlsl"
            #include "HLSLSupport.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                float3 normalWS : NORMAL;
                float4 posCS : SV_POSITION;
            };

            uniform sampler2D _CameraOpaqueTexture;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half4 _AtmShellColor;
            float3 _DirToSun;
            float _RadiusAtmShell;
            float _RadiusPlanet;
            float _BlendingColorHeightSurface;
            float _StartDstShellFadeColor;
            float _BlendingFadeColorNearShell;
            float _LightOffSet;
            float _BlendingShadow;

            v2f vert (appdata v)
            {
                v2f o;
                o.posCS = mul(UNITY_MATRIX_MVP, v.vertex);
                o.posWS = mul(unity_ObjectToWorld, v.vertex);
                o.normalWS = normalize(mul(unity_ObjectToWorld, (v.normalOS + v.vertex.xyz)) - o.posWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                //get the uv equivalent to the screen
                float2 uv = CSToScreenPos(i.posCS);

                half4 originalColor = tex2D(_CameraOpaqueTexture, uv);

                //get the distance from surface and shell
                float dstShell = WSDstFromNearClip(i.posWS);
                float sceneDepth = SceneDepth01(uv);

                if (sceneDepth >= _ProjectionParams.z - _ProjectionParams.y) 
                    sceneDepth = 100000000;

                float dstAtmosphereSurface = (sceneDepth - dstShell);

                float d = pow( saturate( dstShell / _StartDstShellFadeColor ), _BlendingFadeColorNearShell);
                float t =  pow(saturate(dstAtmosphereSurface / (_RadiusAtmShell- _RadiusPlanet)), _BlendingColorHeightSurface);

                float sunLightIntensity = saturate(pow(dot(_DirToSun, i.normalWS)*0.5f+0.5f + _LightOffSet, _BlendingShadow));

                if (dstAtmosphereSurface > _RadiusAtmShell ) 
                {
                    return originalColor;
                }
                else
                {
                    return lerp(originalColor, _AtmShellColor, t*d*sunLightIntensity);
                }

            }
            ENDHLSL
        }
    }
}
