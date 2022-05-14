using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using System;
using System.Threading.Tasks;
using System.Threading;
using MEC;
using MeshUtilities;

namespace MarchingCubes
{
    public class MarchingCubesHandler : IDisposable
    {
        public struct Settings
        {
            public Vector3Int dimensions;
            public float surfaceLevel;
            public int LOD;
        }
        
        /// <summary>
        /// Called whenever the loading of the mesh data is done
        /// </summary>
        public event EventHandler<MeshDataArgs> OnDoneLoading;

        //triangles
        public static readonly int SurfaceLevelID = Shader.PropertyToID("surfaceLevel");
        public static readonly int TrianglesID = Shader.PropertyToID("triangles");

        public Settings settings;

        private const string COMPUTE_SHADER_PATH = "MarchingCubes/MarchingCubeAlgorithm";
        private ComputeShader trianglesCreateCompute = (ComputeShader)Resources.Load(COMPUTE_SHADER_PATH);
        private ComputeShader trianglesCreateComputeInstanced;
        private ComputeBuffer trianglesBuffer;
        private int numTriangles = 0;
        private uint xThreads,yThreads,zThreads;
        private int kernelTriangleIndex = 0;
        
        private bool isOccupied = false;
        private NativeArray<Triangle> triangles;

        public MarchingCubesHandler(Settings settings)
        {
            trianglesCreateComputeInstanced = MonoBehaviour.Instantiate(trianglesCreateCompute);
            SetData(settings);
            OnDoneLoading+= (obj, args)=>
            {
                trianglesBuffer.Dispose();
                isOccupied = false;
            };
        }

        /// <summary>
        /// It sets and prepares the properties of the chunk to build based on its dimension
        /// </summary>
        public void SetData(Settings settings)
        {
            //set the settings
            this.settings = settings;

            //get the kernel index
            kernelTriangleIndex = trianglesCreateComputeInstanced.FindKernel("GenerateTriangles");

            //get thread groupsize
            trianglesCreateComputeInstanced.GetKernelThreadGroupSizes(kernelTriangleIndex, out xThreads, out yThreads, out zThreads);
            
            //calculate the amount of groups needed to best operate the marching cubes calculation
            xThreads = (uint)(Mathf.CeilToInt((settings.dimensions.x/ settings.LOD/ (int)xThreads) + 1));
            yThreads = (uint)(Mathf.CeilToInt((settings.dimensions.y/ settings.LOD/ (int)yThreads) + 1));
            zThreads = (uint)(Mathf.CeilToInt((settings.dimensions.z/ settings.LOD/ (int)zThreads) + 1));
            
            //set the properties and buffers of the triangle compute shaders
            trianglesCreateComputeInstanced.SetInt(ShaderIDStandard.DimensionXID, settings.dimensions.x);
            trianglesCreateComputeInstanced.SetInt(ShaderIDStandard.DimensionYID, settings.dimensions.y);
            trianglesCreateComputeInstanced.SetInt(ShaderIDStandard.DimensionZID, settings.dimensions.z);
            trianglesCreateComputeInstanced.SetInt(ShaderIDStandard.LOD, settings.LOD);
            trianglesCreateComputeInstanced.SetFloat(SurfaceLevelID, settings.surfaceLevel);

            //calculate the maximum amount of possible triangles
            numTriangles = settings.dimensions.x * settings.dimensions.y * settings.dimensions.z * 5;
        }

        /// <summary>
        /// It creates the vertices and indices, it calls the <see cref="OnDoneLoading"/> when done
        /// </summary>
        /// <param name="verticesBuffer">buffer that contains and array of vector4 vertices that describe 3d position (x,y,z) and density (w) respectively</param>
        public void GenerateTriangles(ref ComputeBuffer verticesBuffer)
        {
            if(isOccupied) 
                return;
            isOccupied = true;

            if (triangles != null && triangles.IsCreated)
                triangles.Dispose();
            if (trianglesBuffer != null)
                trianglesBuffer.Dispose();
            
            if (verticesBuffer.count < 8)
            {
                Debug.LogError("Not enough vertices to make a cube for the marching cube algorithm to work!");
                return;
            }

            trianglesBuffer = new ComputeBuffer(numTriangles, (sizeof(float) * 3) * 3);
            
            //calculate the number of threads used by the compute shader
            trianglesCreateComputeInstanced.SetBuffer(kernelTriangleIndex, TrianglesID, trianglesBuffer);
            trianglesCreateComputeInstanced.SetBuffer(kernelTriangleIndex, ShaderIDStandard.VerticesVert4ID, verticesBuffer);
            trianglesCreateComputeInstanced.Dispatch(kernelTriangleIndex, (int)xThreads, (int)yThreads, (int)zThreads);
            
            SafeThreadEventHandler<MeshDataArgs> OnDoneLoadingSafeThread = new SafeThreadEventHandler<MeshDataArgs>(this, OnDoneLoading);
            //ask the gpu to read back the data from the buffer, creating an async operation (this drastically improves performance and efficiency)
            
            AsyncGPUReadback.Request(trianglesBuffer, (request)=>
            {
                //unpack and optimize vertices and indices
                //**NOTE** this algorithm makes sure that there's no duplicate.
                //Calculate the least number of vertices and indices possible to reduce memory usage and increase performance

                triangles = request.GetData<Triangle>();
                //create a BackGround task to eliminate the duplicated vertices
                Task.Run(()=>
                {
                    Triangle[] trianglesArray = triangles.ToArray();
                    Dictionary<Vector3, int> verticesDic = new Dictionary<Vector3, int>();
                    List<int> indicesList = new List<int>();
                    int i, cont;

                    //re-ordering the vertices and indices removing any copy of the positions
                    for (i = 0, cont = 0; i < trianglesArray.Length; i++)
                    {
                        if (trianglesArray[i].pointA != trianglesArray[i].pointB)
                        {
                            if (!verticesDic.ContainsKey(trianglesArray[i].pointA))
                            {
                                verticesDic.Add(trianglesArray[i].pointA, cont);
                                cont++;
                            }
                            if (!verticesDic.ContainsKey(trianglesArray[i].pointB))
                            {
                                verticesDic.Add(trianglesArray[i].pointB, cont);
                                cont++;
                            }
                            if (!verticesDic.ContainsKey(trianglesArray[i].pointC))
                            {
                                verticesDic.Add(trianglesArray[i].pointC, cont);
                                cont++;
                            }
                            indicesList.Add(verticesDic[trianglesArray[i].pointC]);
                            indicesList.Add(verticesDic[trianglesArray[i].pointB]);
                            indicesList.Add(verticesDic[trianglesArray[i].pointA]);

                        }
                    }
                    
                    //preparing the data struct
                    MeshDataArgs meshDataArgs = new MeshDataArgs();
                    meshDataArgs.triangles = indicesList.ToArray();
                    meshDataArgs.vertices = new Vector3[verticesDic.Count];
                    verticesDic.Keys.CopyTo(meshDataArgs.vertices, 0);

                    triangles.Dispose();


                    //call the events and set the flag to not occupied
                    OnDoneLoadingSafeThread.Execute(meshDataArgs);
                }
                
                );
                isOccupied = false;
                trianglesBuffer.Dispose();
            }
            );
            trianglesBuffer.Dispose();

        }

        /// <summary>
        /// It sets the lod. The lod, in this case, indicates how many vertices it jumps from each point to another
        /// </summary>
        /// <param name="LOD"></param>
        public void SetLOD(int LOD)
        {
            settings.LOD = LOD;
            SetData(settings);
        }
        
        public void Dispose()
        {
            if (trianglesBuffer != null) trianglesBuffer.Dispose();
            OnDoneLoading = null;
            if (Application.isPlaying)
            {
                MonoBehaviour.Destroy(trianglesCreateComputeInstanced);
            }
            else
            {
                MonoBehaviour.DestroyImmediate(trianglesCreateComputeInstanced);
            }
            Resources.UnloadUnusedAssets();
        }

    }

}