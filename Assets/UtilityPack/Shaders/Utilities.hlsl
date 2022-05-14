#ifndef UTILITIES
#define UTILITIES

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "HLSLSupport.cginc"

uniform sampler2D _CameraDepthTexture;

//returns the screen coordinates from the clip space
fixed2 CSToScreenPos(fixed3 posCS)
{
    float2 uv = posCS/ _ScaledScreenParams.xy;
    return uv;
}

//returns the dst of the world space position to the nearClip
float WSDstFromNearClip(float3 posWS)
{
    float3 dirInPos = posWS- _WorldSpaceCameraPos;
    float3 viewDir = normalize(unity_CameraToWorld._m02_m12_m22);
    
    //take the dot product (prodotto scalare) of the viewDir and dirInPos getting the depth of the point 
    return dot(viewDir,dirInPos);
}

//returns the linear depth of the pixel's image, in short the distance from the nearplane to the opaque object at the pixel's position. 
//It's clamped between 0 (equivalent to the near clip plane) and 1 (equivalent to the far clip plane)
float SceneDepth01(float2 uv)
{
    float sceneNonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
    return LinearEyeDepth(sceneNonLinearDepth, _ZBufferParams);
}

//returns the view direction from a pixel at UV screen position
float3 ViewDirWS(float2 screenUV)
{
    //gets the view direction of from camera view to each fragment displayed in the screen
    float3 viewVector = mul(unity_CameraInvProjection, float4(screenUV * 2 - 1, 0, -1));
    
    //converts the camera space view direction in world space direction
    return mul(unity_CameraToWorld, float4(viewVector,0));
}

#endif