using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloraLayerSettings
{
    [Header("Base components")]
    public Material mat; 
    public ComputeShader grassGenerator;

    [Header("Grass Unit Settings")]
    public int numFolliageQuads;
    public Vector2 folliageQuadDimensions;
    public float quadsOffsetFromCenterPlane;
    public float maxAngleTwist;

    [Header("Wind")]
    public float windSpeedBase = 1;
    public float windAmplitude = 1;
    public float windPhaseSensitivity;
    public float sampleScaleNoiseWind;
    public float sampleScalePhaseWind;
    public Texture2D noisePhaseWind;
    public Texture2D noiseWind;

    [Header("Displacement Settings")]
    [Range(0f,1f)]
    public float probabilitySpawning;

    public Range2 heightRange;
    public Range2 steepnessRange;

    [Header("Renderer")]
    public float dstRender;
    public LayerMask layerCulling;

    [System.Serializable]
    public struct Range2
    {
        [Range(-1f,1f)]
        public float low;
        [Range(-1f,1f)]
        public float high;

        public Vector2 GetVector2()
        {
            return new Vector2(low,high);
        }
    }

    // property that automatically realoads 
    // the modifications of the folliages

    //main buffers ID
    public static readonly int QuadsFolliageID = Shader.PropertyToID("folliageQuads");
    public static readonly int IndirectArgsID = Shader.PropertyToID("indirectArgs");
    public static readonly int MeshVerticesID = Shader.PropertyToID("meshVertices");
    public static readonly int MeshIndicesID = Shader.PropertyToID("meshIndices");

    //main settings shader ID
    public static readonly int NumTrianglesID = Shader.PropertyToID("numTriangles");
    public static readonly int NumMeshTrianglesID = Shader.PropertyToID("numMeshTriangles");
    public static readonly int NumFolliageID = Shader.PropertyToID("numFolliage");
    public static readonly int ProbabilitySpawnID = Shader.PropertyToID("probabilySpawnFolliage");
    public static readonly int FolliageDimensionsID = Shader.PropertyToID("folliageDimensions");
    public static readonly int OffSetCenterFolliagesID = Shader.PropertyToID("offSetCenterFolliage");
    public static readonly int AngleTwistFolliageID = Shader.PropertyToID("angleTwistModifier");
    public static readonly int WindSpeedBaseID = Shader.PropertyToID("windSpeedBase");
    public static readonly int WindAmplitudeID = Shader.PropertyToID("windAmplitude");
    public static readonly int WindPhaseSensitivityID = Shader.PropertyToID("windPhaseSensitivity");
    public static readonly int SampleScaleNoiseWindID = Shader.PropertyToID("sampleScaleNoiseWind");
    public static readonly int SampleScalePhaseWindID = Shader.PropertyToID("sampleScalePhaseWind");
    
    //textures id buffer
    public static readonly int NoiseWindID = Shader.PropertyToID("_NOISE_WINDTEX");
    public static readonly int PhaseWindID = Shader.PropertyToID("_PHASE_WINDTEX");
    
    //matrices
    public static readonly int LocalToWSID = Shader.PropertyToID("_LocalToWS");
    public static readonly int WSToLocalID = Shader.PropertyToID("_WSToLocal");

    public static readonly int HeightRangeID = Shader.PropertyToID("heightRange");
    public static readonly int SteepnessRangeID = Shader.PropertyToID("steepnessRange");

    public static readonly int dstRenderID = Shader.PropertyToID("dstRender");
}
