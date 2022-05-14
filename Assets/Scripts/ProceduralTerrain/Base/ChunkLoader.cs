using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using ChunkSystem.Terrain;

namespace ChunkSystem
{
    ///<summary>
    ///An helper class with additional functionalities that manages the chunk loading features
    ///</summary>
    public class ChunkLoader<T> where T : ITerrainHandler
    {

        internal interface IChunksLayerRegister
        {
            HashSet<Chunk<T>> GetChunkHashSet();

            ///<summary>
            /// Removes chunk from the register
            ///</summary>
            void RemoveChunk(Chunk<T> chunk);
        }

        //list of chunks in the loaded chunk layer
        internal class ChunksLayer : IChunksLayerRegister
        {
            public HashSet<Chunk<T>> chunks = new HashSet<Chunk<T>>();

            public HashSet<Chunk<T>> GetChunkHashSet()
            {
                return chunks;
            }

            public void RemoveChunk(Chunk<T> chunk)
            {
                chunks.Remove(chunk);
            }
        }

        internal class ChunksLODLayer : IChunksLayerRegister
        {
            public HashSet<Chunk<T>> chunks = new HashSet<Chunk<T>>();
            public Dictionary<Chunk<T>, int> ChunkPriorityDic;

            public HashSet<Chunk<T>> GetChunkHashSet()
            {
                return chunks;
            }

            public void RemoveChunk(Chunk<T> chunk)
            {
                if (chunks.Contains(chunk))
                {
                    chunks.Remove(chunk);
                    ChunkPriorityDic.Remove(chunk);
                }
            }
        }

        public class LoadingLayerInfo
        {
            public int id {get; private set; }
            public int layer {get; private set; }
            private bool _isClearing = false;

            public bool isClearing 
            {
                get
                {
                    return _isClearing;
                }
            }

            public LoadingLayerInfo(int layer, int id)
            {
                this.layer = layer;
                this.id = id;
            }

            public void SetClear()
            {
                _isClearing = true;
            }

            public void ChangeLayer(int layer)
            {
                this.layer = layer;
            }

        }

        //interface to the chunk world
        private ChunksManager<T> chunksManager;

        ///<summary>
        /// plane load mode that specify the orientation of the loaded chunk plane in the chunk grid
        ///</summary>
        public enum PlaneMode{ XY, ZY, XZ}
        
        ///<summary>
        ///List of chunkLayers, which are list of loaded chunks with different LOD than default
        ///</summary>
        private List<ChunksLODLayer> modifiedChunkLayers = new List<ChunksLODLayer>();

        ///<summary>
        ///List of chunkLayers which are list of loaded chunks from the ChunkLoader
        ///</summary>
        private List<ChunksLayer> LoadedChunks = new List<ChunksLayer>();

        private List<LoadingLayerInfo> loadingLayersGenerateInfo = new List<LoadingLayerInfo>();
        private List<LoadingLayerInfo> loadingLayersLODInfo = new List<LoadingLayerInfo>();

        ///<summary>
        ///Max chunkLayers that can be loaded
        ///</summary>
        public int maxLayers {get; private set; }

        public delegate void ChunkAction(Chunk<T> chunk);
        public delegate void ChunkGridAction(Vector3Int gridChunk);
        public delegate bool TimerGridSkipCondition(Vector3Int chunk);

        //converts an 2d offset of a plane in a 3d one, for different kind of orientation (EX: the face of the plane is oriented towards the x axis or z axis)
        public delegate Vector3Int GridOffSetPlane(Vector2Int offSet);

        internal ChunkLoader(ChunkWorld<T> ChunkWorld, int maxLayers)
        {
            Initialize(ChunkWorld.GetChunksManager(), maxLayers);
        }

        public ChunkLoader(ChunksManager<T> chunksManager, int maxLayers)
        {
            Initialize(chunksManager, maxLayers);
        }

        private void Initialize(ChunksManager<T> chunkManager, int maxLayers)
        {
            if (maxLayers <= 0)
            {
                Debug.LogError("Num layers can't be less or equal to 0");
                return;
            }
            this.chunksManager = chunkManager;
            this.maxLayers = maxLayers;

            for (int i = 0; i < maxLayers; i++)
            {
                PushLayersLOD();
                PushLayersLoadedChunk();
            }
        }

        ///<summary>
        ///It pushes the first of the list out (index: 0), reverting all the modifications of its chunks, and creates another layer on the last position (index: maxLayers-1)
        ///<para> FIRST-IN-LAST-OUT </para>
        ///</summary>
        public void PushLayersLOD()
        {
            if (modifiedChunkLayers.Count < maxLayers)
                modifiedChunkLayers.Add(new ChunksLODLayer());
            else
            {
                ClearLayer<ChunksLODLayer>(modifiedChunkLayers, (chunk)=> chunk.SetLod(chunk.data.originalLOD), 0, loadingLayersLODInfo);
                modifiedChunkLayers.RemoveAt(0);
                modifiedChunkLayers.Add(new ChunksLODLayer());
            }
            PushLoadingInfoLayers(loadingLayersLODInfo);
        }

        ///<summary>
        ///It pushes the first of the list out (index: 0), reverting all the modifications of its chunks, and creates another layer on the last position (index: maxLayers-1)
        ///<para> FIRST-IN-LAST-OUT </para>
        ///</summary>
        public void PushLayersLoadedChunk()
        {
            if (LoadedChunks.Count < maxLayers)
                LoadedChunks.Add(new ChunksLayer());
            else
            {
                ClearLayer<ChunksLayer>(LoadedChunks, (chunk)=> chunk.DestroySafe(), 0, loadingLayersGenerateInfo);
                LoadedChunks.RemoveAt(0);
                LoadedChunks.Add(new ChunksLayer());
            }
            PushLoadingInfoLayers(loadingLayersGenerateInfo);
        }
        
        #region LOADING-CHUNK_FUNCTIONS_INTERFACE

        ///<summary>
        /// Generates a cube, with given radius and center, of chunks 
        ///</summary>
        public void GenerateAreaCube(
            Vector3Int center,
            int radius,
            bool loadMap,
            bool generateMap,
            Vector3Int timerDelaysFrame,
            int layer,
            TimerGridSkipCondition timerConditionDelay)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersGenerateInfo.Add(loadingLayerInfo);
            
            SetAreaCube(center, radius, GetChunkGenerateAction(loadMap, generateMap, layer), timerDelaysFrame, timerConditionDelay, loadingLayerInfo);
        }

        ///<summary>
        /// Generates a plane, with given radius and center, of chunks 
        ///</summary>
        public void GenerateAreaPlane(Vector3Int center, PlaneMode orientation, int radius, bool loadMap, bool generateMap, Vector3Int timerDelaysFrame, int layer, TimerGridSkipCondition timerConditionDelay = null)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersGenerateInfo.Add(loadingLayerInfo);
            SetPlane(center, radius, orientation, GetChunkGenerateAction(loadMap, generateMap, layer), timerDelaysFrame, timerConditionDelay, loadingLayerInfo);
        }

        ///<summary>
        /// Destroys a cube, with given radius and center, of chunks 
        ///</summary>
        public void DestroyAreaCube(Vector3Int center, int radius, bool loadMap, bool generateMap, Vector3Int timerDelaysFrame, int layer, TimerGridSkipCondition timerConditionDelay = null)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersGenerateInfo.Add(loadingLayerInfo);
            SetAreaCube(center, radius, GetChunkUnloadAction(layer), timerDelaysFrame, timerConditionDelay, loadingLayerInfo);
        }

        ///<summary>
        /// Destroys a plane, with given radius, orientation and center, of chunks
        ///</summary>
        public void DestroyAreaPlane(Vector3Int center, PlaneMode orientation, int radius, Vector3Int timerDelaysFrame, int layer, TimerGridSkipCondition timerConditionDelay = null)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersGenerateInfo.Add(loadingLayerInfo);
            SetPlane(center, radius, orientation, GetChunkUnloadAction(layer), timerDelaysFrame, timerConditionDelay, loadingLayerInfo);
        }

        #endregion

        #region LOD-CHUNK_FUNCTIONS_INTERFACE

        ///<summary>
        /// Generates a plane, with given radius and center, of chunks 
        ///</summary>
        public void SetAreaCubeLod(Vector3Int center, int radius, int LOD, int priorityIndex, Vector3Int timerDelaysFrame, int layer, TimerGridSkipCondition timerConditionDelay = null)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersLODInfo.Add(loadingLayerInfo);
            SetAreaCube(center, radius, GetChunkLODAction(LOD, priorityIndex, layer), timerDelaysFrame, timerConditionDelay,loadingLayerInfo);
        }

        public void SetPlaneLod(Vector3Int center, int radius, PlaneMode orientation, int LOD, int priorityIndex, Vector3Int timerDelaysFrame, int layer, TimerGridSkipCondition timerConditionDelay = null)
        {
            LoadingLayerInfo loadingLayerInfo = new LoadingLayerInfo(layer, Time.frameCount);
            loadingLayersLODInfo.Add(loadingLayerInfo);
            SetPlane(center, radius, orientation, GetChunkLODAction(LOD, priorityIndex, layer), timerDelaysFrame, timerConditionDelay,loadingLayerInfo);
        }

        #endregion

        #region AREAL_FUNCTIONS_SETCHUNK
        
        public void SetAreaCube
        (
            Vector3Int center,
            int radiusGrid,
            ChunkGridAction chunkGridAction,
            Vector3Int timerDelaysFrame,
            TimerGridSkipCondition timerConditionDelay,
            LoadingLayerInfo loadingInfo
        )
        {
            Timing.RunCoroutine(SetAreaCubeDelay(center, radiusGrid, chunkGridAction, timerDelaysFrame, timerConditionDelay,loadingInfo));
        }

        private IEnumerator<float> SetAreaCubeDelay
            (
                Vector3Int center, 
                int radiusGrid, 
                ChunkGridAction chunkGridAction, 
                Vector3Int timerDelaysFrame, 
                TimerGridSkipCondition timerConditionDelay, 
                LoadingLayerInfo loadingInfo
                )
        {
            if (!chunksManager.register.chunksPos.ContainsKey(center))
            {
                WarningChunkNotPresent(center);
            }
            
            int startY = center.y - radiusGrid;
            int endY = center.y + radiusGrid;
            
            for (int y = startY; y <= endY; y++)
            {
                if (y == startY)
                {
                    SetPlane(new Vector3Int(center.x,y,center.z), radiusGrid, PlaneMode.XZ, chunkGridAction, timerDelaysFrame, timerConditionDelay, loadingInfo);
                }
                else if (y == endY)
                {
                    SetPlane(new Vector3Int(center.x,y,center.z), radiusGrid, PlaneMode.XZ, chunkGridAction, timerDelaysFrame, timerConditionDelay, loadingInfo);
                }
                else
                {
                    SetPlaneVoid(new Vector3Int(center.x,y,center.z), PlaneMode.XZ, radiusGrid, chunkGridAction, timerDelaysFrame, timerConditionDelay, loadingInfo);
                }
                if (timerDelaysFrame.y != 0)
                {
                    yield return Timing.WaitForSeconds(Time.deltaTime * (float)timerDelaysFrame.y);
                }
                LogLayerInfo(loadingInfo);
                if (loadingInfo.isClearing)
                    break;
            }
        }


        public void SetPlaneVoid(
            Vector3Int center,
            PlaneMode orientation,
            int radius,
            ChunkGridAction chunkGridAction,
            Vector3Int timerDelaysFrame,
            TimerGridSkipCondition timerConditionDelay,
            LoadingLayerInfo loadingInfo
            )
        {
            Timing.RunCoroutine(SetPlaneVoidDelay(center, orientation, radius, chunkGridAction, timerDelaysFrame, timerConditionDelay,loadingInfo));
        }

        private IEnumerator<float> SetPlaneVoidDelay(Vector3Int center, PlaneMode orientation, int radius, ChunkGridAction chunkGridAction, Vector3Int timerDelaysFrame, TimerGridSkipCondition timerConditionDelay, LoadingLayerInfo loadingLayerInfo)
        {
            Vector3Int[] dir = null;
            Vector3Int pointTarget = Vector3Int.zero;
            float[] delays = null;

            if (timerConditionDelay == null)
                timerConditionDelay = (chunkGridPos) => true;
            switch(orientation)
            {
                //right-up plane
                case PlaneMode.XY:
                    dir = new Vector3Int[]
                    {
                        Vector3Int.up,Vector3Int.right,
                        Vector3Int.down, Vector3Int.left
                    };
                    delays = new float[]
                    {
                        timerDelaysFrame.y, timerDelaysFrame.x,
                        timerDelaysFrame.y, timerDelaysFrame.x
                    };

                    pointTarget = center - new Vector3Int(radius,radius,0);
                break;

                //foward-up plane
                case PlaneMode.ZY:
                    dir = new Vector3Int[]
                    {
                        Vector3Int.up,Vector3Int.back,
                        Vector3Int.down, Vector3Int.forward
                    };

                    delays = new float[]
                    {
                        timerDelaysFrame.y, timerDelaysFrame.z,
                        timerDelaysFrame.y, timerDelaysFrame.z
                    };
                    pointTarget = center - new Vector3Int(0,radius,radius);
                break;

                case PlaneMode.XZ:
                    dir = new Vector3Int[]
                    {
                        Vector3Int.forward, Vector3Int.right,
                        Vector3Int.back, Vector3Int.left
                    };

                    delays = new float[]
                    {
                        timerDelaysFrame.z, timerDelaysFrame.x,
                        timerDelaysFrame.z, timerDelaysFrame.x
                    };
                    pointTarget = center - new Vector3Int(radius,0,radius);
                break;

                default:
                break;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < radius*2; j++)
                {
                    if (delays[i] != 0 && timerConditionDelay(pointTarget))
                    {
                        yield return Timing.WaitForSeconds(Time.deltaTime * (float)delays[i]);
                    }
                    LogLayerInfo(loadingLayerInfo);
                    if (loadingLayerInfo.isClearing)
                    {
                        goto end; //only type of case i'll use goto
                    }
                    chunkGridAction?.Invoke(pointTarget);

                    pointTarget+= dir[i];
                }
            }
            end:;
        }


        public void SetPlane(Vector3Int center, int radius, PlaneMode orientation, ChunkGridAction chunkGridAction, Vector3Int timerDelaysFrame, TimerGridSkipCondition timerCondition, LoadingLayerInfo loadingLayerInfo)
        {
            Timing.RunCoroutine(SetPlaneDelay(center, radius, orientation, chunkGridAction, timerDelaysFrame, timerCondition, loadingLayerInfo));
        }

        private IEnumerator<float> SetPlaneDelay(Vector3Int center, int radius, PlaneMode orientation, ChunkGridAction chunkGridAction, Vector3Int timerDelaysFrame, TimerGridSkipCondition timerCondition, LoadingLayerInfo loadingLayerInfo)
        {
            GridOffSetPlane offSetConverter = null;
            Vector2Int timer = Vector2Int.zero;
            
            switch(orientation)
            {
                //right-up plane
                case PlaneMode.XY:
                offSetConverter = (offSet2D) =>
                {
                    return new Vector3Int(offSet2D.x, offSet2D.y,0);
                };
                timer = new Vector2Int(timerDelaysFrame.x, timerDelaysFrame.y);
                break;

                //foward-up plane
                case PlaneMode.ZY:

                offSetConverter = (offSet2D) =>
                {
                    return new Vector3Int(0, offSet2D.y, offSet2D.x);
                };
                timer = new Vector2Int(timerDelaysFrame.z, timerDelaysFrame.y);
                break;
                case PlaneMode.XZ:

                offSetConverter = (offSet2D) =>
                {
                    return new Vector3Int(offSet2D.x, 0, offSet2D.y);
                };
                timer = new Vector2Int(timerDelaysFrame.x, timerDelaysFrame.z);
                break;

                default:
                break;
            }

            Timing.RunCoroutine(SetPlaneGrid(center, radius, offSetConverter, chunkGridAction, timer, timerCondition, loadingLayerInfo));
            yield return 0;
        }

        private IEnumerator<float> SetPlaneGrid(Vector3Int center, int radius, GridOffSetPlane gridOffSetPlane, ChunkGridAction chunkGridAction, Vector2Int timerDelaysFrame, TimerGridSkipCondition timerCondition, LoadingLayerInfo loadingLayerInfo)
        {
            if (timerCondition == null)
                timerCondition = (gridPos)=> true;
            Vector3Int posGrid = Vector3Int.zero;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= +radius; x++)
                {
                    posGrid = center + gridOffSetPlane(new Vector2Int(x,y));
                    if (timerDelaysFrame.x != 0 && timerCondition(posGrid))
                    {
                        yield return Timing.WaitForSeconds((float)timerDelaysFrame.x * Time.deltaTime);
                        if (loadingLayerInfo.isClearing)
                            goto end;
                    }
                    chunkGridAction?.Invoke(posGrid);
                }
                
                if (timerDelaysFrame.y != 0 && timerCondition(posGrid))
                {
                    yield return Timing.WaitForSeconds((float)timerDelaysFrame.y * Time.deltaTime);
                    LogLayerInfo(loadingLayerInfo);
                    if (loadingLayerInfo.isClearing)
                        break;
                }
            }
            end:;
        }

        #endregion

        #region BASE_CHUNK_FUNCTIONS
        ///<summary>
        /// Set the lod of an already loaded chunk to the given one
        ///</summary>
        public Chunk<T> SetLodChunkAtPos(Vector3Int positionChunkGrid, int LOD, int priorityIndex, int layer)
        {
            if (!chunksManager.register.chunksPos.ContainsKey(positionChunkGrid))
            {
                WarningChunkNotPresent(positionChunkGrid);
                return null;
            }
            Chunk<T> chunk = chunksManager.register.chunksPos[positionChunkGrid];
        
            int modifiedInLayer = -1;
            if (IsModified<ChunksLODLayer>(chunk, modifiedChunkLayers, -1, out modifiedInLayer))
            {
                if (modifiedChunkLayers[modifiedInLayer].ChunkPriorityDic[chunk] <= priorityIndex)
                {
                    chunk.SetLod(LOD);
                }
            }
            else
            {
                chunk.SetLod(LOD);
            }

            if (!modifiedChunkLayers[layer].GetChunkHashSet().Contains(chunk))
            {
                modifiedChunkLayers[layer].chunks.Add(chunk);
                modifiedChunkLayers[layer].ChunkPriorityDic.Add(chunk, priorityIndex);
            }

            return chunk;
        }

        ///<summary>
        ///It instantiate and generate the chunk
        ///</summary>
        ///<param name="loadChunk"> load the saved chunk data</param>
        public Chunk<T> LoadChunk(Vector3Int positionChunkGrid, bool loadChunk, bool generateMap, int layer)
        {
            Chunk<T> chunk;

            if (chunksManager.register.chunksPos.ContainsKey(positionChunkGrid)) 
            {
                chunk = chunksManager.register.chunksPos[positionChunkGrid];
            }
            else
            {
                //create the chunk and add it to the register of the chunk manager (createchunk func)
                chunk = chunksManager.CreateChunk(positionChunkGrid);
            }

            if (chunk == null) return null;
            LoadedChunks[layer].chunks.Add(chunk);

            if (!chunk.data.mapGenerated)
            {
                if (generateMap && !chunk.data.mapGenerated)
                {
                    if (loadChunk)
                    {
                        //Loads the chunk
                        chunk.Load
                        (
                            (sender, argsLoad)=>
                            {
                                chunk.GenerateMap();
                            }
                        );
                    }
                    else
                    {
                        chunk.GenerateMap();
                    }
                }
            }


            return chunk;
        }

        ///<summary>
        ///Unload and destroy chunks
        ///</summary>
        public void UnLoadChunk(Vector3Int positionChunkGrid, int layer, out bool success)
        {
            //checks if it has been loaded by looking in the chunksManager register
            if (!chunksManager.register.chunksPos.ContainsKey(positionChunkGrid))
            {
                Debug.LogWarning("The described chunk at grid position wasn't loaded in layer: " + layer);
                success = false;
                return;
            }

            //takes the loaded chunk from the chunkManager
            Chunk<T> chunk = chunksManager.register.chunksPos[positionChunkGrid];
            
            //checks if this chunk was loaded in the current layer
            if (!LoadedChunks[layer].chunks.Contains(chunk)) 
            {
                Debug.LogWarning("Chunk: " + chunk.properties.gameObject.name + " wasn't loaded in layer: " + layer);
                success = false;
                return;
            } 

            success = true;

            LoadedChunks[layer].chunks.Remove(chunk);

            if (!IsModified<ChunksLayer>(chunk, LoadedChunks, layer, out _))
            {
                chunksManager.ClearChunk(chunk);
            }
        }
        #endregion

        #region CHUNK-GRID_ACTIONS
        private ChunkGridAction GetChunkLODAction(int LOD, int priorityIndex, int layer)
        {
            ChunkGridAction chunkGridLodAction =
            (positionChunkGrid)=>
            {
                SetLodChunkAtPos(positionChunkGrid, LOD, priorityIndex, layer);
            };

            return chunkGridLodAction;
        }

        private ChunkGridAction GetChunkGenerateAction(bool loadChunk, bool generateChunk, int layer)
        {
            ChunkGridAction chunkGridGenerateAction =
            (positionChunkGrid)=>
            {
                LoadChunk(positionChunkGrid, loadChunk, generateChunk, layer);
            };

            return chunkGridGenerateAction;
        }

        private ChunkGridAction GetChunkUnloadAction(int layer)
        {
            ChunkGridAction chunkGridUnLoadAction =
            (positionChunkGrid)=>
            {
                UnLoadChunk(positionChunkGrid, layer, out _);
            };

            return chunkGridUnLoadAction;
        }

        #endregion
        
        #region GLOBAL_LAYER_FUNCTIONS

        //checks if the current chunk has been modified in every layer but one
        private bool IsModified<I>(Chunk<T> chunk, List<I> chunksLayers, int layerIgnore, out int layerFound) where I : IChunksLayerRegister
        {
            bool isContainedInOtherLayers = false;
            layerFound = -1;

            //iterate through the layers and check if the given chunk is present in at least on of them, with the exception of the layerIgnore
            for (int i = 0; i < chunksLayers.Count; i++)
            {
                //if the layers is the one of the layer ignore then 
                if (i == layerIgnore)
                    i++;
                if (i == chunksLayers.Count) break;

                isContainedInOtherLayers = chunksLayers[i].GetChunkHashSet().Contains(chunk);
                
                if (isContainedInOtherLayers)
                {
                    layerFound = i;
                    return true;
                }
            }

            return isContainedInOtherLayers;
        }

        ///<summary>
        ///Removes from the layer each element and, if given element is not present in other layers, invokes an action 
        ///(EX: reset the lod of each chunk in the layer)
        ///</summary>
        private void ClearLayer<I>(List<I> chunksLayers, ChunkAction OnClearChunk, int layer, List<LoadingLayerInfo> loadingLayersInfo) where I : IChunksLayerRegister
        {
            for (int i =  loadingLayersInfo.Count-1; i >=0 ; i--)
            {
                if (loadingLayersInfo[i].layer == layer)
                {
                    loadingLayersInfo[i].SetClear();
                    loadingLayersInfo.RemoveAt(i);
                }
            }
            foreach(Chunk<T> chunk in chunksLayers[layer].GetChunkHashSet())
            {
                if (!IsModified<I>(chunk, chunksLayers, layer, out _))
                {
                    OnClearChunk?.Invoke(chunk);
                }
            }
            chunksLayers[layer].GetChunkHashSet().Clear();
        }

        private void PushLoadingInfoLayers(List<LoadingLayerInfo> loadingLayersInfo)
        {
            for (int i = 0; i < loadingLayersInfo.Count; i++)
            {
                loadingLayersInfo[i].ChangeLayer(loadingLayersInfo[i].layer-1);
            }
        }

        #endregion
        

        private void LogLayerInfo(LoadingLayerInfo loadingInfo)
        {
            //Debug.Log("LOADING INFO ISCLEARING: " + loadingInfo.id + " ; LAYER: " + loadingInfo.layer + "; "+ loadingInfo.isClearing);
        }

        private void WarningChunkNotPresent(Vector3Int positionGrid)
        {
            Debug.LogWarning("Error! The current cell of the chunks grind is not currently generated (NOT PRESENT IN THE REGISTER); PosGrid: " + positionGrid);
        }
    }

}