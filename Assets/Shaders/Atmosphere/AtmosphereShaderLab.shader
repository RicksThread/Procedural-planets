Shader "Custom/Atmosphere"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Assets/UtilityPack/Shaders/Utilities.hlsl"
            #include "Assets/UtilityPack/Shaders/MathHelper.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewVector : TEXCOORD1;
            };

            float4 _MainTex_ST;
            sampler2D _MainTex;
            sampler2D _BakedOpticalDepthTexture;
            
            float3 _PlanetCentre;
            float3 _DirFromSun;
            float _AtmosphereRadius;
            float _PlanetRadius;

            int _ScatteringSamples;

            int _OpticalDepthPoints;
            
            float _DensityFallOff;
            float3 _ScatteringCoefficients;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz,1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex).xy;
            
                //gets the view direction of from camera view to each fragment displayed in the screen
                float3 viewVector = mul(unity_CameraInvProjection, float4(o.uv * 2 - 1, 0, -1));

                //converts the camera space view direction in world space direction
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }

            float DensityAtPoint(float3 densitySamplePoint)
            {
                //gets the distance from the surface
                float heightAboveSurface = length(densitySamplePoint -  _PlanetCentre) - _PlanetRadius;
                //clamps the distance from 0 to 1
                float height01 = heightAboveSurface / (_AtmosphereRadius - _PlanetRadius);
                
                //returns the local density by elevating the exponential value by the negative height and forcing the end point to be 0
                //in short, the higher the altitude the lower the local density
                float localDensity = exp(-height01 * _DensityFallOff) * (1-height01);
                return localDensity;    
            }

            //gets the total density value that the ray goes through
            float OpticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (_OpticalDepthPoints - 1);
                float opticalDepth = 0;

                for(int i = 0; i < _OpticalDepthPoints; i++)
                {
                    float localDensity = DensityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }
                return opticalDepth;
            }

			float OpticalDepthBaked(float3 rayOrigin, float3 rayDir) 
            {
				float height = length(rayOrigin - _PlanetCentre) - _PlanetRadius;
				float height01 = saturate(height / (_AtmosphereRadius - _PlanetRadius));

                // x uv coord is equivalent to the angulation,
                // 0 = looking towards the center of the planet, 
                // 1 = looking towards the sky and above
				float uvX = 1 - (dot(normalize(rayOrigin - _PlanetCentre), rayDir) * .5 + .5);
				return tex2Dlod(_BakedOpticalDepthTexture, float4(uvX, height01,0,0));
			}

			float OpticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 endPoint = rayOrigin + rayDir * rayLength;
				float d = dot(rayDir, normalize(rayOrigin-_PlanetCentre));
				float opticalDepth = 0;

				const float blendStrength = 1.5;
				float w = saturate(d * blendStrength + .5);
				
                //calculate the difference in optical depth between the sample distance

                //d1: viewing the sky from the surface
				float d1 = OpticalDepthBaked(rayOrigin, rayDir) - OpticalDepthBaked(endPoint, rayDir);
				
                //d2: viewing the sky from space
                float d2 = OpticalDepthBaked(endPoint, -rayDir) - OpticalDepthBaked(rayOrigin, -rayDir);

                //lerping the value based on the angulation value
                //w = 0 view from space towards the atmosphere
                //w = 1 view from surface towards the sky
				opticalDepth = lerp(d2, d1, w);
				return opticalDepth;
			}

            float3 CalculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalColor)
            {
				
				float3 inScatterPoint = rayOrigin;
				float stepSize = rayLength / (_ScatteringSamples - 1);
				float3 inScatteredLight = 0;
				float viewRayOpticalDepth = 0;

				for (int i = 0; i < _ScatteringSamples; i ++) {
					float sunRayLength = raySphere(_PlanetCentre, _AtmosphereRadius, inScatterPoint, _DirFromSun).y;
					float sunRayOpticalDepth = OpticalDepthBaked(inScatterPoint + _DirFromSun * 1, _DirFromSun);
					float localDensity = DensityAtPoint(inScatterPoint);
					viewRayOpticalDepth = OpticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
					float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _ScatteringCoefficients);
					
					inScatteredLight += localDensity * transmittance;
					inScatterPoint += rayDir * stepSize;
				}
				inScatteredLight *= _ScatteringCoefficients * 1 * stepSize;


                // //set the in scattered point to the origin which is the start of the atmosphere
                // float3 inScatteredPoint = rayOrigin;

                // //samples distance between each other
                // float stepSize = rayLength / (_ScatteringSamples - 1);
                
                // //stores the values used for calculating the light through each of the sampling
                // float3 inScatteredLight = 0;
                // float viewRayOpticalDepth = 0;
                
                // for(int i = 0; i < _ScatteringSamples; i++)
                // {
                //     //gets the length dst from the sun to the scattered point
                //     float sunRayLength = raySphere(_PlanetCentre, _AtmosphereRadius, inScatteredPoint, _DirFromSun).y;
                    
                //     //gets the atmosphere density (density of the atmosphere through a ray) from the sun to the atmosphere point
                //     float sunRayOpticalDepth = OpticalDepth(inScatteredPoint, _DirFromSun, sunRayLength);
                    
                //     //gets the atmosphere density (density of the atmosphere through a ray) from the startPoint of the atmosphere
                //     viewRayOpticalDepth = OpticalDepth(inScatteredPoint, -rayDir,stepSize*i);

                //     //sum the OpticalDepth values from the sun and startPoint and set them negative and as power to the exponential 
                //     //the higher the density of the atmosphere the light had to go through the lower is the trasmittance
                //     float3 trasmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _ScatteringCoefficients);
                //     float localDensity =  DensityAtPoint(inScatteredPoint);
                    

                //     inScatteredLight += localDensity * trasmittance;
                //     inScatteredPoint += rayDir * stepSize;
                // }

                // inScatteredLight *= _ScatteringCoefficients * stepSize/_AtmosphereRadius;
                // float originalColTrasmittance = exp(-viewRayOpticalDepth);
                return originalColor  + inScatteredLight;

                
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //get the original color of the view
                fixed4 originalColor = tex2D(_MainTex, i.uv);
                //get the scene depth in units
                float sceneDepth = SceneDepth01(i.uv) * length(i.viewVector);
                //compute the ray direction and origin
                float3  rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                //get the hitInfo
                float2 hitInfo = raySphere(_PlanetCentre, _AtmosphereRadius, rayOrigin, rayDir);
                
                float dstToAtmosphere = hitInfo.x;

                //get the dst through the atmosphere by accounting for the surface of the planet described in the scenedepth
                float dstThroughAtmosphere = min(hitInfo.y, sceneDepth - dstToAtmosphere);
                //if the view hits the atmosphere
                if (dstThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    //calculates the exact start point of the atmosphere
                    float3 pointInAtmosphere = rayOrigin + rayDir * (dstToAtmosphere+epsilon);

                    //elaborate the light for the atmosphere
                    float3 light = CalculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere-epsilon*2, originalColor);
                    return fixed4(light,0);
                }
                return originalColor;
            }
            ENDHLSL
        }
    }
}
