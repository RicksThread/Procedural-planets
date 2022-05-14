using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct TerrainGridProperty
{
    public Vector3Int dimensions;
    public Vector3 centerPos;
    public Vector3 up;
    public Vector3 right;
    public Vector3 foward;
    public float length;

    #region ConstructorOverloads
    public TerrainGridProperty(Vector3Int dimensions, Vector3 centerPos, Vector3 up, Vector3 right, float length)
    {
        this.dimensions = dimensions;
        this.centerPos = centerPos;
        this.up = up;
        this.right = right;
        this.foward = -Vector3.Cross(up,right);
        this.length = length;
    }

    public TerrainGridProperty(Vector3Int dimensions, Vector3 centerPos, float length)
    {
        this.dimensions = dimensions;
        this.centerPos = centerPos;
        this.length = length;

        //default assignment for the dir values
        this.up = Vector3.up;
        this.right = Vector3.right;
        this.foward = -Vector3.Cross(up,right);
    }

    public TerrainGridProperty(Vector3Int dimensions, float length)
    {
        this.dimensions = dimensions;
        this.length = length;
        this.centerPos = Vector3.zero;

        //default assignment for the dir values
        this.up = Vector3.up;
        this.right = Vector3.right;
        this.foward = -Vector3.Cross(up,right);
    }
    #endregion

}

public class VerticesGridGenerator : IDisposable
{

    public static readonly string VERTICES_GENERATOR_COMPUTE_PATH = "Procedural/VerticesGenerator";
    //vertices
    public static readonly int PointsDstID = Shader.PropertyToID("pointsDst");
    public static readonly int StartPointID = Shader.PropertyToID("startPoint");
    public static readonly int UpwardDirID = Shader.PropertyToID("upwardDir");
    public static readonly int rightDirID = Shader.PropertyToID("rightDir");
    public static readonly int fowardDirID = Shader.PropertyToID("fowardDir");
    //vertices || general
    public static readonly int OffSetBufferID = Shader.PropertyToID("offSetBuffer");

    public enum BUFFERSTRIDETYPE { Vector3, Vector4}

    private TerrainGridProperty property;

    private ComputeShader verticesCreateCompute;

    private int kernelVerticesIndex = 0; //mod of constructing the vertices
    private uint xThreads, yThreads, zThreads;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="property">Generation settings</param>
    public VerticesGridGenerator(TerrainGridProperty property)
    {
        verticesCreateCompute = (ComputeShader) Resources.Load(VERTICES_GENERATOR_COMPUTE_PATH);
        SetData(property);

    }

    /// <summary>
    /// It sets the data that will be used to generate the vertices
    /// </summary>
    /// <param name="data"></param>
    public void SetData(TerrainGridProperty property)
    {
        this.property = property;
        //set the parameters
        verticesCreateCompute.SetFloat(PointsDstID, property.length);
        verticesCreateCompute.SetVector(StartPointID, CenterToStartPoint());
        verticesCreateCompute.SetInt(ShaderIDStandard.DimensionXID, property.dimensions.x);
        verticesCreateCompute.SetInt(ShaderIDStandard.DimensionYID, property.dimensions.y);
        verticesCreateCompute.SetInt(ShaderIDStandard.DimensionZID, property.dimensions.z);
        verticesCreateCompute.SetVector(UpwardDirID, property.up);
        verticesCreateCompute.SetVector(rightDirID, property.right);
        verticesCreateCompute.SetVector(fowardDirID, property.foward);
    }

    /// <summary>
    /// It creates and loads a grid array of vertices in to the compute buffer
    /// </summary>
    /// <param name="verticesBuffer">compute buffer where the vertices will be loaded</param>
    /// <param name="strideType">size of each element of the array</param>
    /// <param name="offSetBuffer">start index of the buffer array</param>
    /// <param name="createBuffer">create new buffer or use the one that has been passed</param>
    public void GenerateVertices(ref ComputeBuffer verticesBuffer,
        BUFFERSTRIDETYPE strideType = BUFFERSTRIDETYPE.Vector3, int offSetBuffer = 0, bool createBuffer = true) //optional parameters
    {

        //initialize the settings of the buffer and the parameters to pass to the buffer
        int numVertices = (property.dimensions.x + 1) * (property.dimensions.y + 1) * (property.dimensions.z + 1);

        int numFloats = 3;  //stride dimensions
        int VerticesID = 0;
        switch (strideType)
        {
            case BUFFERSTRIDETYPE.Vector3:
                kernelVerticesIndex = verticesCreateCompute.FindKernel("GenerateVerticesVert3");
                VerticesID = ShaderIDStandard.VerticesVert3ID;
                numFloats = 3;
                break;
            case BUFFERSTRIDETYPE.Vector4:
                kernelVerticesIndex = verticesCreateCompute.FindKernel("GenerateVerticesVert4");
                VerticesID = ShaderIDStandard.VerticesVert4ID;
                numFloats = 4;
                break;
        }

        //manage the vertices
        verticesCreateCompute.GetKernelThreadGroupSizes(kernelVerticesIndex, out xThreads, out yThreads, out zThreads);
        xThreads = (uint)(Mathf.CeilToInt(property.dimensions.x / (int)xThreads) + 1);
        yThreads = (uint)(Mathf.CeilToInt(property.dimensions.y / (int)yThreads) + 1);
        zThreads = (uint)(Mathf.CeilToInt(property.dimensions.z / (int)zThreads) + 1);

        if (createBuffer)
        {
            verticesBuffer = new ComputeBuffer(numVertices, sizeof(float) * numFloats, ComputeBufferType.Structured);
        }

        verticesCreateCompute.SetBuffer(kernelVerticesIndex, VerticesID, verticesBuffer);
        
        verticesCreateCompute.SetInt(OffSetBufferID, offSetBuffer);

        //run the compute shader
        verticesCreateCompute.Dispatch(kernelVerticesIndex, (int)xThreads, (int)yThreads, (int)zThreads);
        
    }

    public int GetVerticesBufferSize(BUFFERSTRIDETYPE strideType = BUFFERSTRIDETYPE.Vector3)
    {
        int numVertices = (property.dimensions.x + 1) * (property.dimensions.y + 1) * (property.dimensions.z + 1);

        int numFloats = 3;  //stride dimensions
        switch (strideType)
        {
            case BUFFERSTRIDETYPE.Vector3:
                numFloats = 3;
                break;
            case BUFFERSTRIDETYPE.Vector4:
                numFloats = 4;
                break;
        }

        return numVertices * numFloats;
    }

    private Vector3 CenterToStartPoint()
    {
        Vector3 startPos = property.centerPos + (
            (-property.foward * property.dimensions.z) +
            (-property.up * property.dimensions.y) +
            (-property.right * property.dimensions.x)
            ) * property.length * 0.5f;

        return startPos;
    }

    public void Dispose()
    {
        Resources.UnloadUnusedAssets();
    }
}
