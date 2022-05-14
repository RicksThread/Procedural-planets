using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkSystem.Terrain;

namespace ChunkSystem
{
    internal interface ChunkWorld<T> where T : ITerrainHandler
    {
        ChunksManager<T> GetChunksManager();

        Vector3Int GetChunkGridPos(Vector3 worldPos);
    }
}
