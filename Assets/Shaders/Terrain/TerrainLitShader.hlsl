#define MAX_HEIGHT_LAYERS_COUNT 5
#define MAX_STEEPNESS_LAYERS_COUNT 5

//TERMINOLOGY:
//HeightLayers = array of steepness layers that are present at some heights
//SteepnessLayer = concept described by a texture and steepness, the latter describes at what terrain angulation the first is more present

struct LayerTerrain
{
    int countHeightLayer;       //the number of steepness layers in the group
    float steepness;            //steepness of the layer
    float height;               //height of the group of layers
    float blendingSteepness;    //blending of each terrain's steepness value
    float scale;
};

//input attributes
struct Attributes
{
    float3 positionOS : POSITION;
    float4 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
    float2 uv         : TEXCOORD0;
};
            
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

float _BlendSharpness = 1;
float blendingHeight;    

//new settings
float2 minMaxHeight;   
bool isPlanet;          
float3 startPos;        //if it's a planet then the starposition acts like the center, while if it isn't it acts as the zero height 
float3 heightDir;       //the direction the vertical axis is point to, it is usually float3(0,1,0)
int heightLayersCount;  //number of heightLayers
float dstCamForCulling;

UNITY_DECLARE_TEX2DARRAY(terrainTexArray);

StructuredBuffer<LayerTerrain> terrainLayers;



Varyings vert(Attributes IN)
{
    Varyings OUT = (Varyings)0;
    OUT.positionWS = startPos;
    OUT.positionCS = mul(UNITY_MATRIX_MVP,float4(0,0,0,1));
    VertexPositionInputs vertexInputs = GetVertexPositionInputs(IN.positionOS.xyz);
    VertexNormalInputs vertNormalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                
    OUT.positionCS = vertexInputs.positionCS;
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.normalOS = IN.normalOS;
    OUT.normalWS = vertNormalInputs.normalWS;
    OUT.positionOS = IN.positionOS;
    OUT.positionWS = vertexInputs.positionWS;
    OUT.uv = IN.uv;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    OUT.shadowCoord = GetShadowCoord(vertexInputs);
    #endif
    return OUT;
}

//returns the color with all the additional lighting calculation
half4 GetColorPBR(Varyings IN, half4 albedo)
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
    return half4(CalculateSimpleLighting(lightingInput, surfaceData),1);
}

//calculates the texture correctly for a non uv mesh without strange stretched parts
half4 TriplanarMapping(int texIndex, float3 positionOS, float3 normalWS, float scale)
{
    half4 texZ = UNITY_SAMPLE_TEX2DARRAY(terrainTexArray, float3(positionOS.xy * scale,texIndex));
    half4 texY = UNITY_SAMPLE_TEX2DARRAY(terrainTexArray, float3(positionOS.xz * scale,texIndex));
    half4 texX = UNITY_SAMPLE_TEX2DARRAY(terrainTexArray, float3(positionOS.zy * scale,texIndex));

    half3 blendWeight = pow(abs(normalWS), 1);
    
    blendWeight /= dot(blendWeight,1);

    return texX * blendWeight.x + texY * blendWeight.y + texZ * blendWeight.z;
}

float CalculateSteepness(float3 normal, float3 heightDir)
{
    return dot(normal,heightDir);
}

//returns the distance of a value from a focus point within a certain range
float GetValueDstFromValue01(float _value, float focus, float minValue, float maxValue)
{
    return abs((_value-focus)/(maxValue-minValue));
}

half3 frag(Varyings IN) : SV_TARGET
{
    float steepness = 1;
    float height01 = 1;

    if (isPlanet)
    {
        float3 heightDir = IN.positionOS-startPos;
        steepness = CalculateSteepness(IN.normalOS.xyz, normalize(heightDir));
        height01 = length(heightDir)/minMaxHeight.y;
    }
    else
    {
        steepness = CalculateSteepness(IN.normalOS.xyz, normalize(heightDir));
        height01 = dot(IN.positionOS-startPos, heightDir) / (minMaxHeight.y-minMaxHeight.x);
        if (height01 < 0)
        {
            height01 = -1;
        }
    }

    //initialize the array of color, each of these colors will be relative to their height
    half3 colorHeightLayers[MAX_HEIGHT_LAYERS_COUNT];
    int countHeightLayers[MAX_HEIGHT_LAYERS_COUNT];

    int currentLayerIndex = 0;

    //calculate the length of the array of each heightLayer
    for(int i = 0; i < heightLayersCount; i++)
    {
        countHeightLayers[i] = terrainLayers[currentLayerIndex].countHeightLayer;
        currentLayerIndex += terrainLayers[currentLayerIndex].countHeightLayer;
    }

    currentLayerIndex = 0;
    //calculate the colors of each heightLayer
    for(int i = 0; i < heightLayersCount; i++)
    {
        
        float totalDensity = 0;

        //each element describes how influencial is each of the steepness colors 
        float steepnessColorWeight[MAX_STEEPNESS_LAYERS_COUNT];
        float currentLayerIndexTemp = currentLayerIndex;

        //calculate the weight of each of the steepness layers of the current height layer
        for(int k = 0; k < countHeightLayers[i]; k++)
        {   
        
            steepnessColorWeight[k] = 
            pow
            (
                //calculate the inverse distance the current layer's steepness from the pixel steepness clamped between 0 and 1, 
                (1-GetValueDstFromValue01(terrainLayers[currentLayerIndexTemp].steepness, steepness, -1, 1))+0.001f,
                terrainLayers[currentLayerIndexTemp].blendingSteepness
            );
            totalDensity += steepnessColorWeight[k];
            currentLayerIndexTemp++;
        }

        colorHeightLayers[i] = half3(0,0,0);
        //apply the pixel colour
        for(int k = 0; k < countHeightLayers[i]; k++ )
        {
            //add the color of the steepness layer depending on how near the current pixel's steepness is to the give layer (value density)
            //it is then  divided by the totaldensity of all the steepness layer in the group (height layer)
            //to make sure that the sum of each of the density is equal to 1
            colorHeightLayers[i] += 
                TriplanarMapping(currentLayerIndex, IN.positionOS *  terrainLayers[currentLayerIndex].scale, IN.normalWS, 0.3f)       //texture's pixel color
                * steepnessColorWeight[k] / totalDensity;
            currentLayerIndex++;
        } 
    }

    //same logic of the steepness is applied here but with the heightLayers 
    float totalDensityColors = 0;
    float densityColorsHeight[MAX_HEIGHT_LAYERS_COUNT];
    half3 albedo = half3(0,0,0);

    currentLayerIndex = 0;
    for(int i = 0; i < heightLayersCount; i++)
    {
        densityColorsHeight[i] = 
        pow
        (
            1-GetValueDstFromValue01(terrainLayers[currentLayerIndex].height, height01, 0, 1) + 0.001f,
            blendingHeight
        );
        totalDensityColors += densityColorsHeight[i];
        currentLayerIndex += countHeightLayers[i];
    }

    for(int i = 0; i < heightLayersCount; i++)
    {
        albedo += colorHeightLayers[i] * densityColorsHeight[i] / totalDensityColors;
    }
    
    //exp. to calculate the density of a certain texture
    // d = 1 - pow(abs(S-sl + noiseAmplitude * perlinNoise(pos)), hardness);
    // S = the current steepness level 
    // sl = steepness of the target layer

#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    IN.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
#endif
    return saturate(GetColorPBR(IN, half4(albedo,1)));

}