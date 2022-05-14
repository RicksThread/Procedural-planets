using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshUtilities;
using System;

[CreateAssetMenu(fileName = "ShaderProperties/Terrain", menuName = "TerrainProperties")]
public class TerrainShaderProperty : ScriptableObject, IDisposable
{
    [System.Serializable]
    public class HeightTerrainLayer
    {
        [System.Serializable]
        public class SteepnessLayer
        {
            [Range(-1f,1f)]
            public float steepness;
            public Texture2D terrainTex;
            public float scale = 1;
        }

        [Range(0f,1f)]
        public float height;
        public float blendingSteepness;
        public SteepnessLayer[] placementSettings;
    }

    //the struct data that is present in the shader
    public struct ShaderLayerTerrain
    {
        public int countHeightLayer;
        public float steepness;
        public float height;
        public float blendingSteepness;
        public float scale;
    }

    [SerializeField] private Material mat;

    [Header("Layer settings")] 
    [SerializeField] private HeightTerrainLayer[] heightLayers;
    [SerializeField] private float blendingHeight;
    [Header("Positional settings")]
    private HeightData heightData;

    [Header("Camera settings")]
    [SerializeField] private float dstCamForCulling = 0; 

    public Material material {get{return mat; }}

    //properties ID of the for the terrainShader
    public static readonly int LayersTerrainID = Shader.PropertyToID("terrainLayers");
    public static readonly int BlendingHeightID = Shader.PropertyToID("blendingHeight");
    public static readonly int TexArrayID = Shader.PropertyToID("terrainTexArray");
    
    public static readonly int minMaxHeightID = Shader.PropertyToID("minMaxHeight");
    public static readonly int isPlanetID = Shader.PropertyToID("isPlanet");
    public static readonly int startPosID = Shader.PropertyToID("startPos");
    public static readonly int heightDirID = Shader.PropertyToID("heightDir");
    public static readonly int heightLayersCountID = Shader.PropertyToID("heightLayersCount");
    public static readonly int dstCamForCullingID = Shader.PropertyToID("dstCamForCulling");

    private ComputeBuffer layersBuffer;
    private bool isInitialized = false;
    private Texture2DArray tex2DArray;

    private void OnValidate()
    {
        Initialize(this.heightData);
    }

    public void Initialize(HeightData _heightData)
    {
        //initial checks to make sure everything is set up correctly
        if (mat == null || heightLayers == null || heightLayers.Length == 0) return;
        if (!CheckLayersReady()) return;
        
        //setting the height data
        if (_heightData == null && this.heightData == null) return;
        if (_heightData != null)
            this.heightData = _heightData;

        //if it was already initialized then reset them
        ResetBuffers();
        isInitialized = true;

        //due to the difference in structure of the data type in the shader
        //the height layers and their respective steepness layers must be converted
        //in to many different layers
        List<ShaderLayerTerrain> ShaderDatalayerTerrains = new List<ShaderLayerTerrain>();

        //creating the data layers for the shader to elaborate
        for (int i = 0; i < heightLayers.Length; i++)
        {
            for (int k = 0; k < heightLayers[i].placementSettings.Length; k++)
            {
                ShaderLayerTerrain shaderLayerData = new ShaderLayerTerrain();
                shaderLayerData.countHeightLayer = heightLayers[i].placementSettings.Length;
                shaderLayerData.height = heightLayers[i].height;
                shaderLayerData.steepness = heightLayers[i].placementSettings[k].steepness;
                shaderLayerData.blendingSteepness = heightLayers[i].blendingSteepness;
                shaderLayerData.scale = heightLayers[i].placementSettings[k].scale;
                ShaderDatalayerTerrains.Add(shaderLayerData);
            }
        }

        layersBuffer = new ComputeBuffer(ShaderDatalayerTerrains.Count, sizeof(int) + sizeof(float)*4);

        //store the formatted layers in to the buffer
        layersBuffer.SetData(ShaderDatalayerTerrains.ToArray());

        //since the textures in the shader is made by a single array
        //it's important to store the 
        tex2DArray = new Texture2DArray(512,512,ShaderDatalayerTerrains.Count, TextureFormat.RGB24,false);

        //setup the texture array with the specified textures
        int j = 0;
        for (int i = 0; i < heightLayers.Length; i++)
        {
            for (int k = 0; k < heightLayers[i].placementSettings.Length; k++)
            {
                tex2DArray.SetPixels(heightLayers[i].placementSettings[k].terrainTex.GetPixels(), j);
                j++;
            }
        }
        
        //apply the textures
        tex2DArray.Apply();

        //apply the data to the buffers
        mat.SetTexture("terrainTexArray",tex2DArray);
        mat.SetBuffer(LayersTerrainID,layersBuffer);
        mat.SetFloat(BlendingHeightID,blendingHeight);
    

        mat.SetVector(minMaxHeightID, new Vector2(heightData.minHeight, heightData.maxHeight));

        //check if the height data type is planet
        int isPlanet = 0;
        if (heightData.heightType == HeightData.HeightType.Planet)
            isPlanet = 1;

        mat.SetInt(isPlanetID, isPlanet);
        mat.SetVector(startPosID, heightData.startPos);
        mat.SetVector(heightDirID, heightData.dirHeight);
        mat.SetInt(heightLayersCountID, heightLayers.Length);
        mat.SetFloat(dstCamForCullingID, dstCamForCulling);
    }

    public void Dispose()
    {
        ResetBuffers();
    }

    private bool CheckLayersReady()
    {
        for (int i = 0; i < heightLayers.Length; i++)
        {
            if (heightLayers[i].placementSettings == null) return false; 
        }
        return true;
    }

    private void ResetBuffers()
    {

        if (isInitialized)
        {
            if (tex2DArray != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(tex2DArray);
                }
                else
                {
                    DestroyImmediate(tex2DArray);
                }

            }
            if (layersBuffer != null)
                layersBuffer.Dispose();
        }
    }

    ~TerrainShaderProperty()
    {
        ResetBuffers();
    }
}
    
