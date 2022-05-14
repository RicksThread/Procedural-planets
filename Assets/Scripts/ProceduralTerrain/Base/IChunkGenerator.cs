using UnityEngine;
using ChunkSystem.Terrain;

namespace ChunkSystem
{
    /// <summary>
    /// It is used to generate the chunk from start to end
    /// <para>A standardized process which is both versitale and modular </para>
    /// </summary>
    public interface IChunkGenerator<T> where T : ITerrainHandler
    {

        ///<summary>
        ///Calculates the properties and data of the chunk given the position on the grid
        ///</summary>
        /// <param name="x">x coordinate in the chunk web</param>
        /// <param name="y">y coordinate in the chunk web</param>
        /// <param name="z">z coordinate in the chunk web</param>
        void GetChunk(out Chunk<T>.Properties properties, out Chunk<T>.Data data, int x,int y, int z);

        ///<summary>
        /// Returns the coordinate in the chunk grid of the nearest chunk to the given world position
        ///</summary>
        Vector3Int GetGridCoord(Vector3 worldPos);

        /// <summary>
        /// Returns the name of the current chunk
        /// </summary>
        /// <param name="x">x coordinate in the chunk web</param>
        /// <param name="y">y coordinate in the chunk web</param>
        /// <param name="z">z coordinate in the chunk web</param>
        /// <returns></returns>
        string GetChunkName(int x, int y, int z);
    }

}
