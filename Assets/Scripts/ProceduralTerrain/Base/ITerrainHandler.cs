using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshUtilities;
using System;

namespace ChunkSystem
{
    namespace Terrain
    {
        public interface ITerrainHandler : IDisposable
        {
            public class LoadTerrainArgs : EventArgs
            {
                public bool success;
            }
            
            /// <summary>
            /// this action is called whenever the system has done drawing the data of the terrain
            /// </summary>
            event EventHandler<MeshDataArgs> OnGenerateDone;

            /// <summary>
            /// It creates the map with the current information
            /// </summary>
            void GenerateMap();

            /// <summary>
            /// It sets the level of detail of the map
            /// </summary>
            void SetLod(int LOD);

            /// <summary>
            /// Save the current information of the terrain
            /// </summary>
            /// <param name="path"> file path where to save the terrain</param>
            /// <param name="OnSaved"> The event that'll be called when the saving is completed</param>
            void Save(string path, EventHandler<LoadTerrainArgs> OnSaved);

            /// <summary>
            /// Load and stores the information of the terrain
            /// </summary>
            /// <param name="path">file path where to load the terrain</param>
            /// <param name="OnSaved"> The event that'll be called when the loading is completed</param>
            void Load(string path, EventHandler<LoadTerrainArgs> OnLoaded);
        }

    }
}
