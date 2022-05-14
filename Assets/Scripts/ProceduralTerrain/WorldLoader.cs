using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;
using ChunkSystem;
using ChunkSystem.Terrain;

public class WorldLoader : MonoBehaviour
{
    [System.Serializable]
    internal class ChunkLayerLoadingSettings
    {
        public int frameDelay;
        public int radiusLoadChunkCube;
    }

    [SerializeField] private int layersLoadedQueue;

    [SerializeField] private PlanetChunkWorld world;
    [SerializeField] private ChunkLayerLoadingSettings[] chunkLayerLoadingSettings;

    private Vector3Int currentChunkGridPosition = Vector3Int.zero;
    private ChunkLoader<MarchingCubesTerrainHandler> chunkLoadingInferace;
    private ChunkLoader<MarchingCubesTerrainHandler>.TimerGridSkipCondition delayChunkLoading;

    private void Start()
    {
        chunkLoadingInferace = new ChunkLoader<MarchingCubesTerrainHandler>(world, layersLoadedQueue);
    
        delayChunkLoading = (chunkGridPos)=>
        {
            //if the chunk is present and already loaded skip the timer
            if (world.chunkRegister.chunksPos.ContainsKey(chunkGridPos))
                return !world.chunkRegister.chunksPos[chunkGridPos].data.mapGenerated;
            //if the chunk is yet to be created don't skip
            return true;
        };
    }

    private void Update()
    {
        Vector3Int chunkGridPos =  world.GetChunkGridPos(transform.position);
        if (chunkGridPos != currentChunkGridPosition)
        {
            chunkLoadingInferace.PushLayersLoadedChunk();

            for (int i = 0; i < chunkLayerLoadingSettings.Length; i++)
            {
                for (int j = chunkLayerLoadingSettings[i].radiusLoadChunkCube; j >=0; j--)
                {
                    chunkLoadingInferace.GenerateAreaCube(chunkGridPos, j, true, true, Vector3Int.one * chunkLayerLoadingSettings[i].frameDelay, layersLoadedQueue-1, delayChunkLoading);
                }
            }
            // chunkLoadingInferace.GenerateAreaCube(chunkGridPos, 3, true, true,  Vector3Int.one * 12, 3, delayChunkLoading);
            currentChunkGridPosition = chunkGridPos;
        }
    }

    IEnumerator DelayGenerate(Chunk<MarchingCubesTerrainHandler> chunk)
    {
        yield return new WaitForSecondsRealtime(0.2f);
    }

    public void ClearLayer()
    {
        chunkLoadingInferace.PushLayersLoadedChunk();
    }
}
