using UnityEngine;
using UtilityPack;
using MeshUtilities;
using ChunkSystem;
using ChunkSystem.Terrain;

namespace MarchingCubes
{
    public class ChunkGenerator : IChunkGenerator<MarchingCubesTerrainHandler>
    {
        [System.Serializable]
        public class Settings
        {
            [Header("chunk settings")]
            public int chunkDimension = 16;//
            public float unitLength = 1;//
            public int LODStart;
            public int seed;

            [Header("Marching cubes settings")]
            public float surfaceLevel = 0.3f;
            public TerrainShaderProperty materialProperty;//
            public PlanetNoiseGenerator noiseGenerator;

            [Header("Chunks gameObject")]
            public LayerMask chunkLayer;
            public ChunkBehaviour chunkPrefab;//
            public Transform parent;

            [Header("Vegetation")]
            public FloraLayerSettings[] floraSettings;//
            
            [Header("Storage location")]
            public string basePath;

            public Settings GetClone()
            {
                return (Settings)this.MemberwiseClone();
            }
        }

        private Settings settings;
        private HeightData heightData;

        public ChunkGenerator(Settings settings, HeightData heightData)
        {
            this.settings = settings;
            this.heightData = heightData;
            
            settings.noiseGenerator.Initialize(heightData);
            
            if (settings.materialProperty != null)
                settings.materialProperty.Initialize(heightData);
        }

        public void GetChunk(out Chunk<MarchingCubesTerrainHandler>.Properties properties, out Chunk<MarchingCubesTerrainHandler>.Data data, int x, int y, int z)
        {
            //calculate size and position
            float sizeChunk = (float)settings.chunkDimension * settings.unitLength;
            Vector3 chunkLocalPos = ((Vector3.right * sizeChunk * x) + (Vector3.up * sizeChunk * y) +
                (Vector3.forward * sizeChunk * z));
            
            //instantiate gameobject
            GameObject chunkObj = MonoBehaviour.Instantiate(settings.chunkPrefab.gameObject, settings.parent.position, settings.parent.rotation);
            
            //set gameobject scene settings
            chunkObj.name = GetChunkName(x,y,z);
            chunkObj.transform.SetParent(settings.parent);
            chunkObj.layer = Utilities.GetLayerIndex(settings.chunkLayer);

            //Set the parameters of the chunk object
            ChunkBehaviour chunkBehaviour = chunkObj.GetComponent<ChunkBehaviour>();
            if (chunkBehaviour.boxCollider != null)
            {
                chunkBehaviour.boxCollider.isTrigger = true;
                chunkBehaviour.boxCollider.size = new Vector3(sizeChunk, sizeChunk, sizeChunk);
                chunkBehaviour.boxCollider.center = chunkLocalPos;
            }


            chunkBehaviour.meshRenderer.sharedMaterial = settings.materialProperty.material;
            chunkBehaviour.pos = chunkLocalPos;
            chunkBehaviour.size = sizeChunk;

            //creates the settings of the flora
            if (chunkBehaviour.floraBehaviour != null)
                chunkBehaviour.floraBehaviour.SetFlora(GetFloraGenerator(chunkBehaviour.transform, chunkLocalPos, sizeChunk));
            
            //load the main properties of the chunk in the scene
            properties =
                new Chunk<MarchingCubesTerrainHandler>.Properties
                (
                    chunkBehaviour.meshFilter,
                    chunkBehaviour.meshCollider,
                    chunkBehaviour.gameObject
                );

            //gets the terrain
            MarchingCubesTerrainHandler terrainHandler = GetTerrain(x,y,z);
            
            //adds to the terrainHandler's event: OnGenerateDone, the refresh function of the flora 
            terrainHandler.OnGenerateDone += (sender, meshArgs)=>
            {
                if(chunkBehaviour.floraBehaviour != null)
                {
                    chunkBehaviour.floraBehaviour.UpdateFlora(meshArgs.vertices, meshArgs.triangles);
                }
            };  

            //set the data
            data = new Chunk<MarchingCubesTerrainHandler>.Data
                (
                    terrainHandler,
                    Application.persistentDataPath+ "/" + settings.basePath + "/" + GetChunkName(x,y,z),
                    settings.LODStart
                );
        }

        private FloraGenerator GetFloraGenerator(Transform chunkTransform, Vector3 localPosCenterChunk, float chunkSize)
        {
            Bounds bounds = new Bounds(chunkTransform.InverseTransformPoint(localPosCenterChunk), Vector3.one * chunkSize);

            FloraGenerator floraGenerator = 
                new FloraGenerator
                (
                    settings.floraSettings,
                    this.heightData,
                    bounds,
                    chunkTransform
                );

            return floraGenerator;
        }

        public string GetChunkName(int x, int y, int z)
        {
            return "Chunk_" + new Vector3Int(x, y, z);
        }

        private MarchingCubesTerrainHandler GetTerrain(int x,int y, int z)
        {
            float sizeChunk = (float)settings.chunkDimension * settings.unitLength;
            Vector3 chunkLocalPos = ((Vector3.right * sizeChunk * x) + (Vector3.up * sizeChunk * y) +
                (Vector3.forward * sizeChunk * z));

            //Creating the grid information dimensions, center and unit length 
            TerrainGridProperty terrainGridProperty =
                new TerrainGridProperty(
                    new Vector3Int(settings.chunkDimension, settings.chunkDimension, settings.chunkDimension),
                    chunkLocalPos,
                    settings.unitLength
                    );

            MarchingCubesHandler.Settings marchingCubesSettings;
            marchingCubesSettings.dimensions = terrainGridProperty.dimensions;
            marchingCubesSettings.surfaceLevel = settings.surfaceLevel;
            marchingCubesSettings.LOD = settings.LODStart;
            
            MarchingCubesTerrainHandler terrain = 
                new MarchingCubesTerrainHandler
                (
                    terrainGridProperty, 
                    marchingCubesSettings, 
                    settings.noiseGenerator,
                    settings.seed
                );
            return terrain;
        }

        public Vector3Int GetGridCoord(Vector3 worldPos)
        {
            Vector3 localPos = settings.parent.InverseTransformPoint(worldPos);
            float chunkSize = settings.chunkDimension * settings.unitLength;
            Vector3 centerReference = -(new Vector3(chunkSize,chunkSize,chunkSize) * 0.5f);
            localPos = localPos - centerReference;
            return Vector3Int.FloorToInt(localPos/(chunkSize));
        }
    }
}