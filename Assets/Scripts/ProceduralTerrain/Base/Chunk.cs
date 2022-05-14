using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MeshUtilities;
using MEC;
using ChunkSystem.Terrain;

namespace ChunkSystem
{
    /// <summary>
    /// Interface/Support for the terrainHandler that holds information about mesh and savePath
    /// </summary>
    /// <typeparam name="T">TerrainType</typeparam>
    public class Chunk<T> where T : ITerrainHandler
    {
        ///<summary>
        /// It holds the basic components for rendering and collinding in a chunk
        ///</summary>
        public class Properties
        {
            //chunk's gameObject
            public GameObject gameObject;
            public MeshFilter meshFilter;
            public MeshCollider meshCollider;

            public Properties(MeshFilter meshFilter, MeshCollider meshCollider, GameObject gameObject)
            {
                this.meshCollider = meshCollider;
                this.meshFilter = meshFilter;
                this.gameObject = gameObject;
            }
        }

        ///<summary>
        ///It holds the main data of the terrain
        ///</summary>
        public class Data
        {
            public T terrainHandler;
            public string savePath {get; private set;}
            public int originalLOD {get; private set;}
            public int LOD {get; private set;}
            public bool mapGenerated {get; private set;}


            public Data(T terrainHandler, string savePath, int LOD)
            {
                this.terrainHandler = terrainHandler;
                this.savePath = savePath;
                this.LOD = LOD;
                this.originalLOD = LOD;
            }

            public void SetLOD(int LOD)
            {
                this.LOD = LOD;
            }

            public void SetMapState(bool isLoaded)
            {
                mapGenerated = isLoaded;
            }
        }

        //Utility data
        public Properties properties {get; private set;}
        public Data data {get; private set;}

        private Mesh mesh;

        public bool canReadMesh {get; private set;} = true;

        ///<summary>
        /// Event called when the chunk is safely destroyed through <see cref="DestroySafe"/>
        ///</summary>
        public event Action OnDestroy;

        public Chunk(IChunkGenerator<T> chunkGenerator, Vector3Int coord)
        {
            Chunk<T>.Properties chunkProperties;
            Chunk<T>.Data chunkData;
            chunkGenerator.GetChunk(out chunkProperties, out chunkData, coord.x,coord.y,coord.z);
            this.data = chunkData;
            this.properties = chunkProperties;
            mesh = new Mesh();

            //setting up each events for the coordination of the system

            //make sure everytime the terrain is done generating it reloads the mesh
            data.terrainHandler.OnGenerateDone += OnLoadMap;
        }

        public void GenerateMap()
        {
            data.SetMapState(true);
            data.terrainHandler.GenerateMap();
        }

        public void SetLod(int LOD)
        {
            if (data.LOD == LOD) return;
            data.terrainHandler.SetLod(LOD);
            data.SetLOD(LOD);
        }
        
        ///<summary>
        /// Safe way to save data from the terrain
        ///</Summary>
        public void Save(EventHandler<ITerrainHandler.LoadTerrainArgs> OnSaved)
        {
            data.terrainHandler.Save(data.savePath, OnSaved);
        }

        ///<summary>
        /// Safe way to load data from the terrain
        ///</summary>
        public void Load(EventHandler<ITerrainHandler.LoadTerrainArgs> OnLoaded)
        {
            data.terrainHandler.Load(data.savePath, OnLoaded);
        }

        //makes sure for the mesh to be written only once per frame
        IEnumerator<float> WaitToReadAgainMesh()
        {
            canReadMesh = false;
            yield return  Timing.WaitForSeconds(0f);
        
            canReadMesh = true;
        }

        ///<summary>
        ///Safely destroys the terrain mesh and disposes the references 
        ///</summary>
        public void DestroySafe()
        {
            OnDestroy?.Invoke();
            if (properties.meshFilter != null) 
                MonoBehaviour.Destroy(properties.meshFilter.gameObject);
            if (properties.meshCollider != null) 
                MonoBehaviour.Destroy(properties.meshCollider.gameObject);
            if (properties.gameObject != null) 
                MonoBehaviour.Destroy(properties.gameObject);
            
            data.terrainHandler.Dispose();
            
            data.SetMapState(false);
            OnDestroy = null;
        }

        //reloads the mesh with the given data
        private void ReloadMesh(Vector3[] vertices, int[] triangles)
        {
            if (!canReadMesh) return;
            if (properties.meshFilter == null) return;
            //calculate and create the mesh
            if (mesh == null)
                mesh = new Mesh();
            if (mesh.indexFormat != UnityEngine.Rendering.IndexFormat.UInt32)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.Clear();
            if(vertices.Length == 0)
            {
                Debug.LogFormat("no vertices");
                return;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            //assign the mesh to their respective handlers

            properties.meshFilter.sharedMesh = mesh;

            if  (properties.meshCollider != null)
                properties.meshCollider.sharedMesh = mesh;

            Timing.RunCoroutine(WaitToReadAgainMesh());
        }
        
        //reads the data when the terrain is done loading 
        private void OnLoadMap(object sender, MeshDataArgs args)
        {
            ReloadMesh(args.vertices, args.triangles);
        }
    }
}
