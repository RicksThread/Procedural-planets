using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;
using System.IO;
using MEC;
using System;
using ChunkSystem.Terrain;

namespace ChunkSystem
{
    public class ChunksManager<T> where T : ITerrainHandler
    {
        public class DelayerSettings
        {
            public Vector3 delaysGenerate;
            public Vector3 delaysSave;
            public Vector3 delaysLoad;
        }

        public class Register
        {
            public OrderedDictionary<Transform, Chunk<T>> chunksOrderedTransf = new OrderedDictionary<Transform, Chunk<T>>();
            public Dictionary<Vector3Int, Chunk<T>> chunksPos = new Dictionary<Vector3Int, Chunk<T>>();
            public Dictionary<Chunk<T>, Vector3Int> posChunk = new Dictionary<Chunk<T>, Vector3Int>();
        }

        ///<summary>
        /// Holds the dictionaries of the map's chunks
        ///</summary>
        public Register register;

        //object's global settings common to the whole map
        #region Properties
        
        public Vector3Int chunksNumber { get; private set; }
        
        ///<summary>
        ///the limits of the grid of chunks (lowest point)
        ///</summary>
        public Vector3Int chunksLimitLower {get; private set; }

        ///<summary>
        ///the limits of the grid of chunks (highest point)
        ///</summary>
        public Vector3Int chunksLimitUpper {get; private set; }

        public int seed { get; private set; }
        public string commonDirectory { get; private set; }
        public DelayerSettings delayerSettings { get; private set; }

        #endregion

        private IChunkGenerator<T> chunkGenerator;

        public ChunksManager(
            IChunkGenerator<T> chunkGenerator,
            DelayerSettings delayerSettings,
            Vector3Int chunksNumber
            )
        {
            
            SetGridDimension(chunksNumber);
            this.chunkGenerator = chunkGenerator;


            //setting up the delay settings
            if (delayerSettings == null)
            {
                this.delayerSettings = new DelayerSettings()
                {
                    delaysGenerate = Vector3.zero,
                    delaysSave = Vector3.zero,
                    delaysLoad = Vector3.zero
                };
            }
            else
                this.delayerSettings = delayerSettings;
            register = new Register();
        }

        ///<summary>
        /// Returns the nearest cell in the grid chunk based on worldPos
        ///</summary>
        public Vector3Int GetChunkGrid(Vector3 worldPos)
        {
            return chunkGenerator.GetGridCoord(worldPos);
        }

        #region BASIC_MAPCHUNK_FUNCS
        
        ///<summary>
        ///It generates the map with size chunkNumber and center a cell in the chunk grid
        ///</summary>
        public void GenerateMap(Vector3Int center, Vector3Int chunksNumber)
        {
            ClearMap();
            this.chunksNumber = chunksNumber;
            Timing.RunCoroutine(LoadAsync(center,chunksNumber, delayerSettings.delaysGenerate, (chunk)=> chunk.GenerateMap()));
        }
        
        ///<summary>
        ///Loads a map of chunks with size chunkNumber and center a cell in the chunk grid, at the end of each chunk loading the action is the invoked
        ///<para> NOTE: it loads the vertices buffer of the chunks but it doesn't generate the chunk's map</para>
        ///</summary>
        public void LoadMap(Vector3Int center, Vector3Int chunksNumber, Action<Chunk<T>> OnLoad)
        {
            Timing.RunCoroutine(LoadAsync(center,chunksNumber, delayerSettings.delaysLoad, OnLoad));
        }

        private IEnumerator<float> LoadAsync(Vector3Int center,Vector3Int chunksNumber, Vector3 delayer, Action<Chunk<T>> OnLoad)
        {
            int startX = center.x - (int)((float)chunksNumber.x * 0.5f);
            int startY = center.y - (int)((float)chunksNumber.y * 0.5f);
            int startZ = center.z - (int)((float)chunksNumber.z * 0.5f);

            for (int z = startZ; z < startZ+chunksNumber.z; z++)
            {
                for (int y = startY; y < startY+chunksNumber.y; y++)
                {
                    for (int x = startX; x < startX+chunksNumber.x; x++)
                    {
                        Chunk<T> chunk = CreateChunk(x,y,z);
                        chunk.Load((obj, args)=>OnLoad?.Invoke(chunk));
                        if (delayer.x != 0) yield return Timing.WaitForSeconds(delayer.x);
                    }
                    if (delayer.y != 0) yield return Timing.WaitForSeconds(delayer.y);
                }
                if (delayer.z != 0) yield return Timing.WaitForSeconds(delayer.z);
            }
        }

        ///<summary>
        /// It saves all the registered chunks
        ///</summary>
        public void SaveMap()
        {
            Timing.RunCoroutine(SaveDelayed());
        }

        IEnumerator<float> SaveDelayed()
        {
            for (int z = 0; z < chunksNumber.z ; z++)
            {
                for (int y = 0; y < chunksNumber.y ; y++)
                {
                    for (int x = 0; x < chunksNumber.x ; x++)
                    {
                        int i = z*chunksNumber.y*chunksNumber.x  + y*chunksNumber.x +x;
                        register.chunksOrderedTransf[i].Value.Save(null);
                        if (delayerSettings.delaysSave.x != 0)
                            yield return Timing.WaitForSeconds(delayerSettings.delaysSave.x);
                    }    
                    if (delayerSettings.delaysSave.y != 0)
                        yield return Timing.WaitForSeconds(delayerSettings.delaysSave.y);
                }
                if (delayerSettings.delaysSave.y != 0)
                    yield return Timing.WaitForSeconds(delayerSettings.delaysSave.y);
            }
        }

        ///<summary>
        ///It clears all the chunks registered
        ///</summary>
        public void ClearMap()
        {
            chunksNumber = Vector3Int.zero;
            for (int i = register.chunksOrderedTransf.Count-1; i >= 0; i--)
            {
                ClearChunk(register.chunksOrderedTransf[i].Value);
            }
        }
        

        #endregion

        #region BASIC_CHUNK_FUNCS

        ///<summary>
        ///It creates and loads the chunk in to the dictionaries (DOESN'T generate the chunk's map)
        ///</summary>
        public Chunk<T> CreateChunk(int x,int y,int z)
        {
            if (x > chunksLimitUpper.x || y > chunksLimitUpper.y || z > chunksLimitUpper.z)
                return null;
            if (x < chunksLimitLower.x || y < chunksLimitLower.y ||  z < chunksLimitLower.z)
                return null;

            //instantiating the chunk
            Chunk <T> chunk =
                new Chunk<T>(
                    chunkGenerator, new Vector3Int(x,y,z)
                    );
            register.chunksOrderedTransf.Add(chunk.properties.gameObject.transform, chunk);
            register.chunksPos.Add(new Vector3Int(x,y,z), chunk);
            register.posChunk.Add(chunk, new Vector3Int(x, y, z));

            //remove the chunk once it has been destroyed
            chunk.OnDestroy += 
            ()=>
            {
                register.chunksOrderedTransf.Remove(chunk.properties.gameObject.transform);
                register.chunksPos.Remove(new Vector3Int(x,y,z));
                register.posChunk.Remove(chunk);
            };
            return chunk;
        }

        ///<summary>
        ///It creates and loads the chunk in to the dictionaries (DOESN'T generate the chunk's map)
        ///</summary>
        public Chunk<T> CreateChunk(Vector3Int positionChunkGrid)
        {
            return CreateChunk(positionChunkGrid.x, positionChunkGrid.y, positionChunkGrid.z);
        }

        ///<sumary>
        /// Safely removes the chunk from the register and destroys it
        ///</summary>
        public void ClearChunk(Chunk<T> chunk)
        {
            chunk.DestroySafe();
        }

        #endregion
        
        //sets the bounding of the chunks grid over the which chunks cannot be loaded
        private void SetGridDimension(Vector3Int chunksNumber)
        {
            this.chunksNumber = chunksNumber;

            //removing the central chunk 
            chunksNumber -= Vector3Int.one;

            //calculate the extension
            chunksLimitLower = -Vector3Int.CeilToInt((Vector3)chunksNumber * 0.5f);
            chunksLimitUpper = chunksNumber + chunksLimitLower;
        }

        ~ChunksManager()
        {
            ClearMap();
        }
    }

}