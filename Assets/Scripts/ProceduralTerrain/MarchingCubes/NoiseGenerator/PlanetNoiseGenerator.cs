using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshUtilities;

[CreateAssetMenu(fileName ="NoiseGenerator", menuName ="NoiseGenerators/DefaultNoiseGenerator")]
public class PlanetNoiseGenerator : ScriptableObject, INoiseGenerator
{

    [System.Serializable]
    public struct NoiseLayer
    {
        [Header("'Donut' Influence")]
        public float dstFromCenter;
        public float radiusInfluence;
        public float radiusInfluenceBlending;

        [Header("General")]
        public float strength;
        public float scale;
        
        public float blending;

        [Header("Octaves settings")]
        public int octaveLayers;

        //how noisy will the final product be, the lower the more linear it'll look
        public float roughness;     
        //how strong is the noisyness
        public float steepness;

        [Header("Distortion")]
        public float distortion;
        public float distortionScale;
    }

    [Header("Layers")]
    [SerializeField] private NoiseLayer[] noiseLayers;

    [Header("Compute Shader")]
    [SerializeField] private ComputeShader densityCreateCompute;

    [Header("GlobalSettings")]
    [SerializeField] private float globalScale = 0.3f;
    [SerializeField] private float globalBlending = 1; 

    public float radius {get; private set; }

    public static readonly int CenterID = Shader.PropertyToID("center");
    public static readonly int OffSetID = Shader.PropertyToID("offSet");
    public static readonly int NumLayersID = Shader.PropertyToID("numLayers");

    public static readonly int RadiusTerrainID = Shader.PropertyToID("radius");
    public static readonly int GlobalScaleNoiseID = Shader.PropertyToID("globalScale");
    public static readonly int GlobalBlendingID = Shader.PropertyToID("globalBlending");
    public static readonly int NoiseLayerID = Shader.PropertyToID("noiseLayers");

    public static readonly int SteepnessID = Shader.PropertyToID("steepness");

    public static readonly int SizeNoiseLayer = sizeof(float) *10 + sizeof(int);

    private ComputeBuffer noiseLayersBuffer;
    private bool noiseBufferInitialized = false;

    public void Initialize(HeightData heightData)
    {
        this.radius = heightData.maxHeight;
    }

    public void GenerateNoiseValues(ref ComputeBuffer vertBuffer, Vector3Int dimensions, int seed)
    {
        //get settings of the compute shader
        int kernelDensityIndex = densityCreateCompute.FindKernel("GenerateDensity");

        //manage the density
        uint xThreads, yThreads, zThreads;
        densityCreateCompute.GetKernelThreadGroupSizes(kernelDensityIndex, out xThreads, out yThreads, out zThreads);
        xThreads = (uint)(Mathf.CeilToInt(dimensions.x / (int)xThreads) + 1);
        yThreads = (uint)(Mathf.CeilToInt(dimensions.y / (int)yThreads) + 1);
        zThreads = (uint)(Mathf.CeilToInt(dimensions.z / (int)zThreads) + 1);

        //initialize and send the data of the noiselayer buffer
        noiseLayersBuffer = new ComputeBuffer(noiseLayers.Length, SizeNoiseLayer);
        noiseLayersBuffer.SetData(noiseLayers);
        densityCreateCompute.SetBuffer(kernelDensityIndex, NoiseLayerID, noiseLayersBuffer);
        
        //manage the density
        densityCreateCompute.SetBuffer(kernelDensityIndex, ShaderIDStandard.VerticesVert4ID, vertBuffer);

        densityCreateCompute.SetInt(ShaderIDStandard.DimensionXID, dimensions.x);
        densityCreateCompute.SetInt(ShaderIDStandard.DimensionYID, dimensions.y);
        densityCreateCompute.SetInt(ShaderIDStandard.DimensionZID, dimensions.z);

        densityCreateCompute.SetVector(CenterID, Vector3.zero);
        densityCreateCompute.SetVector(OffSetID, new Vector3(seed, seed, seed));
        densityCreateCompute.SetInt(NumLayersID, noiseLayers.Length);
        
        densityCreateCompute.SetFloat(GlobalScaleNoiseID, globalScale);
        densityCreateCompute.SetFloat(RadiusTerrainID, radius);
        densityCreateCompute.SetFloat(GlobalBlendingID, globalBlending);

        //the number of threads are the same of the vertices
        densityCreateCompute.Dispatch(kernelDensityIndex, (int)xThreads, (int)yThreads, (int)zThreads);
        noiseLayersBuffer.Release();
        noiseLayersBuffer.Dispose();
    }

    public void Dispose()
    {
        if (noiseBufferInitialized)
        {
            if (noiseLayersBuffer != null)
            {
                noiseLayersBuffer.Dispose();
            }
        }
    }
}
