#ifndef LIGHTINGHELPER_INCLUDED
#define LIGHTINGHELPER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

//NOTE: I know this is not the correct formula but a gross estimation of what
//the perceived brithness will be otherwise. However i wouldn't mind since it's 
// far more performant
float PerceivedBrightness(half3 colours){
    return (0.213*colours.x + 0.715*colours.y + 0.072*colours.z);
}

half3 CalculateSimpleLighting(InputData lightingInput,SurfaceData surfaceData){
    
    SurfaceData surfDataLight = surfaceData;
    surfDataLight.albedo = half4(1,1,1,1);
    //get the absolute value of the light by inserting an albedo of white (so that the illuminated parts are 1 and black are 0)
    half3 light = UniversalFragmentBlinnPhong(lightingInput,surfDataLight).xyz;
    //calculate the perceived light intensity
    float lightIntensity = PerceivedBrightness(light.xyz);

    //calculate the light color
    half3 lightColor = UniversalFragmentBlinnPhong(lightingInput,surfaceData).xyz;
    
    //get the light color and its intensity
    half3 ambientColor = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    float intensityAmbientColor = PerceivedBrightness(ambientColor);
    
    //return the light color plus the shadowcolor which intensity is inversely proportional to the lightIntensity
    return lerp(ambientColor * surfaceData.albedo,lightColor,saturate(max(lightIntensity,intensityAmbientColor)));
}
#endif