using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.IO;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading.Tasks;
using System.Threading;
using MeshUtilities;
using ChunkSystem.Terrain;

namespace MarchingCubes
{
    public class MarchingCubesTerrainHandler : ITerrainHandler
    {
        internal class BrushSystem
        {
            private MarchingCubesTerrainHandler terrain;
            
            internal ComputeShader brushCompute;
            private const string COMPUTE_SHADER_PATH = "MarchingCubes/brushCompute";

            //brush
            public static readonly int BrushLocalPointID = Shader.PropertyToID("brushLocalPoint");
            public static readonly int BrushRadiusID = Shader.PropertyToID("brushRadius");
            public static readonly int BrushAmountID = Shader.PropertyToID("brushAmount");
            uint xThreadsBrush, yThreadsBrush, zThreadsBrush;

            public BrushSystem(MarchingCubesTerrainHandler terrain)
            {
                if (terrain == null)
                {
                    Debug.LogError("Chunk not initialized for brushSystem!");
                    return;
                }
                
                brushCompute = (ComputeShader)Resources.Load(COMPUTE_SHADER_PATH);
                this.terrain = terrain;

                brushCompute.SetInt(ShaderIDStandard.DimensionXID, terrain.gridProperty.dimensions.x);
                brushCompute.SetInt(ShaderIDStandard.DimensionYID, terrain.gridProperty.dimensions.y);
                brushCompute.SetInt(ShaderIDStandard.DimensionZID, terrain.gridProperty.dimensions.z);

                brushCompute.GetKernelThreadGroupSizes(0, out xThreadsBrush, out yThreadsBrush, out zThreadsBrush);
                xThreadsBrush = (uint)(Mathf.CeilToInt(terrain.gridProperty.dimensions.x / (int)xThreadsBrush) + 1);
                yThreadsBrush = (uint)(Mathf.CeilToInt(terrain.gridProperty.dimensions.y / (int)yThreadsBrush) + 1);
                zThreadsBrush = (uint)(Mathf.CeilToInt(terrain.gridProperty.dimensions.z / (int)zThreadsBrush) + 1);
            }

            public void Brush(Vector3 localPos, float radius, float amount)
            {
                if(terrain.verticesBuffer == null) 
                {
                    Debug.LogWarning("You must first generate the world before start brushing");
                    return;
                }
                brushCompute.SetFloat(BrushRadiusID, radius);
                brushCompute.SetVector(BrushLocalPointID, localPos);
                brushCompute.SetFloat(BrushAmountID, amount);
                brushCompute.SetBuffer(0, ShaderIDStandard.VerticesVert4ID, terrain.verticesBuffer);

                brushCompute.Dispatch(0, (int)xThreadsBrush, (int)yThreadsBrush, (int)zThreadsBrush);

                terrain.GenerateMap();
            }
        }

        public class VerticesArgs : EventArgs
        {
            public VerticesData vertices;
        }

        [System.Serializable]
        public class VerticesData
        {
            public Vector4[] vert4;
        }

        public event EventHandler<MeshDataArgs> OnGenerateDone;

        //stored property data of the grid Terrain
        public TerrainGridProperty gridProperty;
        public MarchingCubesHandler.Settings marchingCubesSettings;

        public int LOD { get; private set; } = 1;
        public int seed { get; private set; } = 0;
        public int bufferSizeByte {get; private set;}

        //utility classes used to generate the terrain data
        private VerticesGridGenerator verticesGenerator;
        private INoiseGenerator noiseGenerator;
        private MarchingCubesHandler marchingCubesHandler;
        private BrushSystem brushSystem;

        //holds current information about the vertices and intensity levels
        private ComputeBuffer verticesBuffer;

        public MarchingCubesTerrainHandler(
            TerrainGridProperty terrainGridProperty,
            MarchingCubesHandler.Settings marchingCubesSettings,
            INoiseGenerator noiseGenerator, int seed)
        {
            verticesGenerator = new VerticesGridGenerator(terrainGridProperty);
            
            this.gridProperty = terrainGridProperty;
            this.marchingCubesSettings = marchingCubesSettings;
            this.noiseGenerator = noiseGenerator;
            this.seed = seed;
            this.bufferSizeByte = verticesGenerator.GetVerticesBufferSize(VerticesGridGenerator.BUFFERSTRIDETYPE.Vector4);

            marchingCubesSettings.dimensions = gridProperty.dimensions;
            
            brushSystem = new BrushSystem(this);

            marchingCubesHandler = new MarchingCubesHandler(marchingCubesSettings);
            marchingCubesHandler.OnDoneLoading += (sender, meshInfo) => { OnGenerateDone?.Invoke(this, meshInfo); };
        }

        public void SetSettings(TerrainGridProperty terrainGridProperty, MarchingCubesHandler.Settings  marchingCubesSettings)
        {
            this.gridProperty = terrainGridProperty;

            marchingCubesSettings.dimensions = gridProperty.dimensions;
            this.marchingCubesSettings = marchingCubesSettings;

            marchingCubesHandler.SetData(marchingCubesSettings);
            verticesGenerator.SetData(terrainGridProperty);
            brushSystem = new BrushSystem(this);
        }

        /// <summary>
        /// It generates the map
        /// <para> If the buffer of the vertices is null it'll create one </para>
        /// <para> otherwise it'll use the already existing one </para>
        /// </summary>
        public void GenerateMap()
        {
            if (verticesBuffer == null)
            {
                //generates the vertices grid
                verticesGenerator.GenerateVertices(ref verticesBuffer, VerticesGridGenerator.BUFFERSTRIDETYPE.Vector4);

                //generates the density array
                noiseGenerator.GenerateNoiseValues(ref verticesBuffer, gridProperty.dimensions, seed);
            }
            
            
            //generates the triangles through the marching cube algorithm
            marchingCubesHandler.GenerateTriangles(ref verticesBuffer);
        }


        public void ClearBuffer()
        {
            verticesBuffer.Dispose();
        }

        public void SetBuffer(ComputeBuffer buffer)
        {
            ClearBuffer();
            verticesBuffer = buffer;
        }

        public void Brush(Vector3 localPos, float radius, float amount)
        {
            brushSystem.Brush(localPos,radius,amount);
        }

        public void Dispose()
        {
            if(verticesBuffer != null) verticesBuffer.Release();
            OnGenerateDone = null;
            marchingCubesHandler.Dispose();
            noiseGenerator.Dispose();
            Resources.UnloadUnusedAssets();
        }

        public void SetLod(int LOD)
        {
            LOD = Mathf.Clamp(LOD,0, gridProperty.dimensions.x);
            if (gridProperty.dimensions.x%LOD != 0) return;

            marchingCubesHandler.SetLOD(LOD);
            GenerateMap();
        }

        public void Load(string path, EventHandler<ITerrainHandler.LoadTerrainArgs> OnLoaded)
        {
            //gets the formatter to deserialize the data from the data
            BinaryFormatter formatter = BinarySerializer.GetFormatter();

            FileStream fileStream;

            //if the path + the file exists load the data
            if (File.Exists(path))
            {
                //creates a safeThreadAction that sets, once the loading is terminated, the vertices data to the buffer
                SafeThreadEventHandler<VerticesArgs> OnLoadedDefault = 
                new SafeThreadEventHandler<VerticesArgs>
                (
                    this, //sender

                    (sender, vertsArgs)=> 
                    {
                        verticesBuffer = new ComputeBuffer(vertsArgs.vertices.vert4.Length, sizeof(float)*4, ComputeBufferType.Structured);
                        verticesBuffer.SetData(vertsArgs.vertices.vert4);
                        OnLoaded = null;
                    }
                );

                //creates a safeThread action that has the passed action as delegate to invoke when the task is completed
                SafeThreadEventHandler<ITerrainHandler.LoadTerrainArgs> OnLoadedSafeThread = new SafeThreadEventHandler<ITerrainHandler.LoadTerrainArgs>(this, OnLoaded);
                
                Task task = Task.Run(
                    ()=>
                    {
                        fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                        VerticesData vec4sData = (VerticesData)formatter.Deserialize(fileStream);
                        fileStream.Dispose();
                        fileStream.Close();
                        
                        VerticesArgs verticesArgs = new VerticesArgs();
                        verticesArgs.vertices = vec4sData;
                        OnLoadedDefault.Execute(verticesArgs);
                        ITerrainHandler.LoadTerrainArgs args = new ITerrainHandler.LoadTerrainArgs
                        {
                            success = true
                        };
                        OnLoadedSafeThread.Execute(args);

                    }
                );
            }
            else
            {
                ITerrainHandler.LoadTerrainArgs args = new ITerrainHandler.LoadTerrainArgs
                {
                    success = false
                };
                OnLoaded?.Invoke(this, args);
            }

        }
        
        public void Save(string path, EventHandler<ITerrainHandler.LoadTerrainArgs> OnSaved)
        {
            AsyncGPUReadback.Request(verticesBuffer, (rqst) =>
            {
                SafeThreadEventHandler<ITerrainHandler.LoadTerrainArgs> OnSaveSafeThread = new SafeThreadEventHandler<ITerrainHandler.LoadTerrainArgs>(this, OnSaved);
                VerticesData data = new VerticesData();
                data.vert4 = rqst.GetData<Vector4>().ToArray();
                
                var task = Task.Run(
                    ()=>
                    {
                        BinaryFormatter formatter = BinarySerializer.GetFormatter();

                        FileStream fileStream;
                        fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        formatter.Serialize(fileStream, data);

                        fileStream.Dispose();
                        fileStream.Close();

                        ITerrainHandler.LoadTerrainArgs args = new
                        ITerrainHandler.LoadTerrainArgs
                        {
                            success = true
                        };
                        OnSaveSafeThread.Execute(args);

                    }
                );
            });

        }

    }

}