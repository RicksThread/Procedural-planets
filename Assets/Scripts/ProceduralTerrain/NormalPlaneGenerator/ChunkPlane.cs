using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkSystem;
using ChunkSystem.Terrain;

public class ChunkPlane : MonoBehaviour
{
    [SerializeField] private ChunkPlaneGenerator.Settings chunkSett;
    private ChunksManager<PlaneTerrainHandler> chunksManager;

    [ContextMenu("Generate world")]
    private void Generate() {
        if (true)
        {
            ChunksManager<PlaneTerrainHandler>.DelayerSettings delayerSettings = new ChunksManager<PlaneTerrainHandler>.DelayerSettings();
            delayerSettings.delaysGenerate = Vector3.zero;
            delayerSettings.delaysSave = Vector3.zero;
            delayerSettings.delaysLoad = Vector3.zero;
            
            ChunkPlaneGenerator chunkPlaneGenerator = new ChunkPlaneGenerator(chunkSett);

            chunksManager = new ChunksManager<PlaneTerrainHandler>(chunkPlaneGenerator, delayerSettings, chunkSett.dimensions);
        }
        chunksManager.GenerateMap(Vector3Int.zero, Vector3Int.one);
    }
}

public class ChunkPlaneGenerator : IChunkGenerator<PlaneTerrainHandler>
{
    [System.Serializable]
    public class Settings
    {
        public ChunkBehaviour chunkPrefab;
        public Vector3Int dimensions;
        public Transform parent;
        public float length;
        public string savePath;
        public int startLOD;
        public Material mat;
    }
    private Settings settings;

    public ChunkPlaneGenerator(Settings settings)
    {
        this.settings = settings;
    }

    public string GetChunkName(int x, int y, int z)
    {
        return string.Empty;
    }

    
    public Chunk<PlaneTerrainHandler>.Properties SetChunkObjProperties(int x, int y, int z)
    {
        ChunkBehaviour chunkBehaviour = MonoBehaviour.Instantiate(settings.chunkPrefab, settings.parent.position, settings.parent.rotation);
        chunkBehaviour.transform.SetParent(settings.parent);
        chunkBehaviour.gameObject.name = "Chunk";
        chunkBehaviour.meshRenderer.material = settings.mat;
        Chunk<PlaneTerrainHandler>.Properties properties = 
        new Chunk<PlaneTerrainHandler>.Properties
            (
                chunkBehaviour.meshFilter,
                chunkBehaviour.meshCollider,
                chunkBehaviour.gameObject
            );
        return properties;
    }

    public Chunk<PlaneTerrainHandler>.Data GetData(int x, int y, int z)
    {
        Chunk<PlaneTerrainHandler>.Data data = 
            new Chunk<PlaneTerrainHandler>.Data
            (
                GetTerrain(),
                settings.savePath,
                settings.startLOD
            );
        return data;
    }

    private PlaneTerrainHandler GetTerrain()
    {
        TerrainGridProperty gridProperty = new TerrainGridProperty(settings.dimensions, Vector3.zero, settings.length);
        return new PlaneTerrainHandler(gridProperty);
    }

    public void GetChunk(out Chunk<PlaneTerrainHandler>.Properties properties, out Chunk<PlaneTerrainHandler>.Data data, int x, int y, int z)
    {
        data = GetData(x,y,z);
        properties = SetChunkObjProperties(x,y,z);
    }

    public Vector3Int GetGridCoord(Vector3 worldPos)
    {
        Debug.LogError("GetGridCoord not setted!");
        return Vector3Int.zero;
    }
}