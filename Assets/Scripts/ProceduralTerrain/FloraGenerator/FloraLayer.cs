using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshUtilities;
using UtilityPack;

public class FloraLayer
{
    private FloraLayerSettings settings;
    private Transform transform;
    private Material instancedMaterial;

    public ComputeBuffer argsBuffer;
    public ComputeBuffer quadsBuffer;
    public ComputeBuffer indicesBuffer;
    public ComputeBuffer verticesBuffer;

    public ComputeShader computeGrassInstanced;     
    private bool isInitialized = false;
    private int numTriangles;
    
    private HeightData heightData;
    private Bounds bounds;


    public FloraLayer(FloraLayerSettings settings, HeightData heightData, Bounds bounds, Transform transformObject)
    {
        this.settings = settings;
        this.transform = transformObject;
        this.heightData = heightData;
        this.bounds = bounds;

        this.instancedMaterial = MonoBehaviour.Instantiate(settings.mat);
        this.computeGrassInstanced = MonoBehaviour.Instantiate(settings.grassGenerator);
    }

    public void GenerateFlora(Vector3[] vertices, int[] triangles)
    {
        //reset the buffers so that they're ready to be reused
        ResetBuffers();

        //if the vertices and triangles are less than a quad return 
        if (vertices.Length < 4 || triangles.Length < 6) return;
        isInitialized = true;

        numTriangles = triangles.Length/3 * 2 * settings.numFolliageQuads;
        int numVertices = vertices.Length;
        int numIndices = triangles.Length;
        int kernelIndex = GetKernelIndex();
        
        //create the buffers
        verticesBuffer = new ComputeBuffer(numVertices, sizeof(float)*3);
        indicesBuffer = new ComputeBuffer(numIndices,sizeof(int));
        quadsBuffer = new ComputeBuffer(numTriangles, (sizeof(float)*3 + sizeof(float)*2)*6+(sizeof(float)*3*4), ComputeBufferType.Append);
        quadsBuffer.SetCounterValue(0);
        argsBuffer = new ComputeBuffer(1,sizeof(uint)*4, ComputeBufferType.IndirectArguments);

        //recreate the indirectArgs
        IndirectArgs[] args = new IndirectArgs[] {IndirectArgs.GetDefault()};
        args[0].numVerticesPerInstance = 6;

        //set the buffers
        verticesBuffer.SetData(vertices);
        indicesBuffer.SetData(triangles);

        argsBuffer.SetData(args);

        //settings the buffers
        computeGrassInstanced.SetBuffer(kernelIndex, FloraLayerSettings.IndirectArgsID, argsBuffer);
        computeGrassInstanced.SetBuffer(kernelIndex, FloraLayerSettings.QuadsFolliageID, quadsBuffer);
        computeGrassInstanced.SetBuffer(kernelIndex, FloraLayerSettings.MeshVerticesID, verticesBuffer);
        computeGrassInstanced.SetBuffer(kernelIndex, FloraLayerSettings.MeshIndicesID, indicesBuffer);
        
        //settings the main settings
        computeGrassInstanced.SetInt(FloraLayerSettings.NumMeshTrianglesID, triangles.Length/3);
        computeGrassInstanced.SetInt(FloraLayerSettings.NumTrianglesID, numTriangles);
        computeGrassInstanced.SetInt(FloraLayerSettings.NumFolliageID, settings.numFolliageQuads);
        computeGrassInstanced.SetFloat(FloraLayerSettings.ProbabilitySpawnID, settings.probabilitySpawning);
        computeGrassInstanced.SetMatrix(FloraLayerSettings.LocalToWSID, transform.localToWorldMatrix);
        computeGrassInstanced.SetMatrix(FloraLayerSettings.WSToLocalID, transform.worldToLocalMatrix);
        computeGrassInstanced.SetVector(FloraLayerSettings.FolliageDimensionsID, settings.folliageQuadDimensions);
        computeGrassInstanced.SetFloat(FloraLayerSettings.OffSetCenterFolliagesID, settings.quadsOffsetFromCenterPlane);
        computeGrassInstanced.SetFloat(FloraLayerSettings.AngleTwistFolliageID, settings.maxAngleTwist);

        computeGrassInstanced.SetFloat(HeightData.maxHeightID, heightData.maxHeight);
        computeGrassInstanced.SetFloat(HeightData.minHeightID, heightData.minHeight);
        computeGrassInstanced.SetVector(HeightData.startPosID, heightData.startPos);
        computeGrassInstanced.SetVector(HeightData.dirHeightID, heightData.dirHeight);

        computeGrassInstanced.SetVector(FloraLayerSettings.HeightRangeID,settings.heightRange.GetVector2());
        computeGrassInstanced.SetVector(FloraLayerSettings.SteepnessRangeID,settings.steepnessRange.GetVector2());

        instancedMaterial.SetFloat(FloraLayerSettings.WindSpeedBaseID, settings.windSpeedBase);
        instancedMaterial.SetFloat(FloraLayerSettings.WindAmplitudeID, settings.windAmplitude);
        instancedMaterial.SetFloat(FloraLayerSettings.WindPhaseSensitivityID, settings.windPhaseSensitivity);
        instancedMaterial.SetFloat(FloraLayerSettings.SampleScaleNoiseWindID, settings.sampleScaleNoiseWind);
        instancedMaterial.SetFloat(FloraLayerSettings.SampleScalePhaseWindID, settings.sampleScalePhaseWind);
        instancedMaterial.SetFloat(FloraLayerSettings.dstRenderID, settings.dstRender);
        
        //settings the textures for the wind
        instancedMaterial.SetTexture(FloraLayerSettings.NoiseWindID, settings.noiseWind);
        instancedMaterial.SetTexture(FloraLayerSettings.PhaseWindID, settings.noisePhaseWind);

        //setting the buffer of the folliage triangles
        instancedMaterial.SetBuffer(FloraLayerSettings.QuadsFolliageID, quadsBuffer);
        computeGrassInstanced.Dispatch(kernelIndex, (numTriangles/128)+1,1,1);
    }

    ///<summary>
    /// It draws the folliage
    ///</summary>
    public void Draw()
    {
        if (!isInitialized) return;
        
        instancedMaterial.SetMatrix(FloraLayerSettings.LocalToWSID, transform.localToWorldMatrix);
        
        if (argsBuffer != null)
        {
            Graphics.DrawProceduralIndirect
            (
                instancedMaterial,
                bounds,
                MeshTopology.Triangles,
                argsBuffer,
                0, null, null,
                UnityEngine.Rendering.ShadowCastingMode.On,
                true,
                Utilities.GetLayerIndex(settings.layerCulling)
            );
        }
    }

    ///<summary>
    /// Dispose the buffers and reset the counter buffer
    ///</summary>
    public void ResetBuffers()
    {
        if (isInitialized)
        {
            if (quadsBuffer != null)
            {
                quadsBuffer.Dispose();
            }
            if (argsBuffer != null)
                argsBuffer.Dispose();
            if (verticesBuffer != null)
                verticesBuffer.Dispose();
            if (indicesBuffer != null)
            indicesBuffer.Dispose();
        }
    }

    ///<summary>
    ///Clears the parameters that are to be displaced by the garbage collector
    ///</summary>
    public void Dispose()
    {

        ResetBuffers();
        if (Application.isPlaying)
        {
            MonoBehaviour.Destroy(instancedMaterial);
            MonoBehaviour.Destroy(computeGrassInstanced);
        }
        else
        {
            MonoBehaviour.DestroyImmediate(instancedMaterial);
            MonoBehaviour.DestroyImmediate(computeGrassInstanced);
        }
    }

    private int GetKernelIndex()
    {
        switch(heightData.heightType)
        {
            case HeightData.HeightType.Plane:
                return 0;
            case HeightData.HeightType.Planet:
                return 1;
        }
        return -1;
    }
}

