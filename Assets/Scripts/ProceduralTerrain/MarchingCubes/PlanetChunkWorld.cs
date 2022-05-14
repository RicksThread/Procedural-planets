using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;
using UtilityPack;
using System;
using System.IO;
using MeshUtilities;
using ChunkSystem;
using ChunkSystem.Terrain;

//manages the chunk of the planet system
public class PlanetChunkWorld : MonoBehaviour, ChunkWorld<MarchingCubesTerrainHandler>
{
    [System.Serializable]
    public class PlanetImageSettings
    {
        public int chunkDimension = 32;
        public float unitLength = 6;
        public TerrainShaderProperty materialProperty;
        public ChunkBehaviour chunkPrefab;
        public LayerMask chunkImageLayer;
    }
    
    [Header("General settings")]
    //the radius of the planet
    [SerializeField] private float radiusPlanet;

    //chunks over the ones that envelop the planet
    [SerializeField] private int nChunksOverRadius;

    [Header("Editor settings")]
    [SerializeField] private bool showRadius; 
    [SerializeField] private Color radiusWireGizmosColor;
    [SerializeField] private Color radiusGizmosColor;

    [Header("Chunk generation settings")]
    [SerializeField] private ChunkGenerator.Settings chunkGeneratorSettingsProper;
    [SerializeField] private PlanetImageSettings planetImageSettings;

    private ChunksManager<MarchingCubesTerrainHandler> chunksManager;
    private ChunksManager<MarchingCubesTerrainHandler> chunksManagerImage;

    private ChunkLoader<MarchingCubesTerrainHandler> imageChunkLoader;
    ///<summary>
    ///The n of chunks present in each side
    ///<para> the total number of chunks is the n^3</para>
    ///</summary>
    public int chunksDimension
    {
        get
        {
            return Mathf.RoundToInt(radiusPlanet/chunkSize)*2 +1 + nChunksOverRadius*2;
        }
    }

    ///<summary>
    /// The physical scale of each chunk
    ///</summary>
    public float chunkSize
    {
        get
        {
            return chunkGeneratorSettingsProper.chunkDimension * chunkGeneratorSettingsProper.unitLength;
        }
    }
    
    ///<summary>
    ///The register of the chunks
    ///</summary>
    public ChunksManager<MarchingCubesTerrainHandler>.Register chunkRegister
    {
        get
        {
            return chunksManager.register;
        }
    }

    //chunk generator of the proper terrain
    private ChunkGenerator chunkGeneratorProper;

    //chunk generator of the image terrain of the planet
    private ChunkGenerator chunkGeneratorImage;

    private void Awake()
    {
        InitializeChunkManager();
    }

    private void InitializeChunkManager()
    {
        //define the heightdata 
        HeightData heightData = new HeightData();
        //the planet has origin at point 0 and has the radius given
        heightData.heightType = HeightData.HeightType.Planet;
        heightData.startPos = Vector3.zero;
        heightData.maxHeight = radiusPlanet;

        //a planet cannot have negative distance from the center
        heightData.minHeight = 0;

        //Create the "image" layer of the planet
        ChunkGenerator.Settings chunkImageSettings = GetChunkImageSettings(chunkGeneratorSettingsProper, planetImageSettings);
        chunkGeneratorImage = new ChunkGenerator(chunkImageSettings, heightData);
        
        int chunksImageDimensions = Mathf.CeilToInt(radiusPlanet/(chunkImageSettings.unitLength * chunkImageSettings.chunkDimension));
        Debug.Log(radiusPlanet + "; " + (chunkImageSettings.unitLength * chunkImageSettings.chunkDimension));
        chunksManagerImage = new ChunksManager<MarchingCubesTerrainHandler>
        (
            chunkGeneratorImage,
            null,
            Vector3Int.one * (chunksImageDimensions*2+1)
        );

        //instantiating the loader for the image chunks
        imageChunkLoader = new ChunkLoader<MarchingCubesTerrainHandler>(chunksManagerImage,1);
        
        //load the "image" of the planet
        imageChunkLoader.PushLayersLoadedChunk();
        for (int i = chunksImageDimensions-1; i >= 0; i--)
        {
            Debug.Log(i);
            imageChunkLoader.GenerateAreaCube(Vector3Int.zero, i, true, true, Vector3Int.one * 20, 0, (chunk)=>true);
        }

        //setting up the chunk generator of the real brushable planet terrain
        chunkGeneratorProper = new ChunkGenerator(chunkGeneratorSettingsProper, heightData);
        chunksManager = new ChunksManager<MarchingCubesTerrainHandler>
        (
            chunkGeneratorProper,
            null,
            Vector3Int.one * chunksDimension
        );


        
        //creates the folder where to store the chunks information
        if (!File.Exists(Application.persistentDataPath+ "/" + chunkGeneratorSettingsProper.basePath))
        {
            Directory.CreateDirectory(Application.persistentDataPath+ "/" + chunkGeneratorSettingsProper.basePath);
        }
    }
    

    public Vector3Int GetChunkGridPos(Vector3 worldPos)
    {
        return chunksManager.GetChunkGrid(worldPos);
    }

    ///<summary>
    ///Used to brush and modify the topology of the world
    ///</summary>
    public void Brush(Vector3 worldPos, float radius, float amount)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        LayerMask layer = chunkGeneratorSettingsProper.chunkLayer;

        Collider[] colliders = Physics.OverlapSphere(worldPos, radius, chunkGeneratorSettingsProper.chunkLayer, QueryTriggerInteraction.Collide);
        HashSet<Transform> transfHashSet = new HashSet<Transform>();

        foreach(Collider collider in colliders)
        {
            if (chunksManager.register.chunksOrderedTransf.ContainsKey(collider.transform))
                transfHashSet.Add(collider.transform);
        }
        Debug.Log("N colliders chunk: " + colliders.Length);

        foreach(Transform transformTarget in transfHashSet)
        {
            Debug.Log("Brush chunk: " + transformTarget.name);
            chunksManager.register.chunksOrderedTransf[transformTarget].data.terrainHandler.Brush(localPos,radius, amount);
        }
    }

    [ContextMenu("Save")]
    private void SaveMap()
    {
        chunksManager.SaveMap();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        chunksManager.ClearMap();
        chunksManagerImage.ClearMap();
    }

    private void OnDestroy()
    {
        if (chunksManager != null) chunksManager.ClearMap();
    }

    //sets the chunk generator settings for the planet image
    private ChunkGenerator.Settings GetChunkImageSettings(ChunkGenerator.Settings settingsProper, PlanetImageSettings planetImageSettings)
    {
        ChunkGenerator.Settings imageSettings = settingsProper.GetClone();
        
        imageSettings.chunkDimension = planetImageSettings.chunkDimension;
        imageSettings.unitLength = planetImageSettings.unitLength;
        imageSettings.materialProperty = planetImageSettings.materialProperty;
        imageSettings.chunkPrefab = planetImageSettings.chunkPrefab;
        imageSettings.floraSettings = null;
        imageSettings.chunkLayer = planetImageSettings.chunkImageLayer;
        return imageSettings;
    }

    ChunksManager<MarchingCubesTerrainHandler> ChunkWorld<MarchingCubesTerrainHandler>.GetChunksManager()
    {
        return chunksManager;
    }

    private void OnDrawGizmos() {
        if (!showRadius) return;

        //draws the radius of the planet so that is easily managable from the editor
        Gizmos.color = radiusWireGizmosColor;
        Gizmos.DrawWireSphere(transform.position, radiusPlanet);
        Gizmos.color = radiusGizmosColor;
        Gizmos.DrawSphere(transform.position, radiusPlanet);
    }
}
