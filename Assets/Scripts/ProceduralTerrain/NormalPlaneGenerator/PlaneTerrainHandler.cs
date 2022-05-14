using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshUtilities;
using ChunkSystem;
using ChunkSystem.Terrain;

public class PlaneTerrainHandler : ITerrainHandler
{
    
    public event EventHandler<MeshDataArgs> OnGenerateDone;
    private TerrainGridProperty gridProperty;
    private VerticesGridGenerator verticesGenerator;
    private ComputeBuffer verticesBuffer;

    public PlaneTerrainHandler(TerrainGridProperty gridProperty)
    {
        verticesGenerator = new VerticesGridGenerator(gridProperty);
        this.gridProperty = gridProperty;
    }

    public void GenerateMap()
    {
        if (verticesBuffer == null)
            verticesGenerator.GenerateVertices(ref verticesBuffer, VerticesGridGenerator.BUFFERSTRIDETYPE.Vector3, 0, true);

        Vector3[] vertices = new Vector3[verticesBuffer.count];
        verticesBuffer.GetData(vertices);

        int nSquares = (gridProperty.dimensions.x) * (gridProperty.dimensions.z);
        int[] triangles = new int[nSquares*6];
        int a = 0, z=0;
        
        for (int i = 0; i < nSquares; i++)
        {

            //draw a square for each 6 vertices
            triangles[a] = i+z+1;
            triangles[a+1] = i+z;
            triangles[a+2] = i+z + gridProperty.dimensions.x+1;
            triangles[a+3] = i+z + gridProperty.dimensions.x+1;
            triangles[a+4] = i+z + gridProperty.dimensions.x+2;
            triangles[a+5] = i+z+1;

            if ((i+1)%(gridProperty.dimensions.x) == 0) 
                z++;
            a+=6;
        }
        
        MeshDataArgs meshArgs = new MeshDataArgs();
        meshArgs.vertices = vertices;
        meshArgs.triangles = triangles;

        OnGenerateDone?.Invoke(this,meshArgs);
    }

    public void Load(string path, EventHandler<ITerrainHandler.LoadTerrainArgs> OnLoaded)
    {
    }

    public void Save(string path, EventHandler<ITerrainHandler.LoadTerrainArgs>  OnSaved)
    {

    }

    public void SetLod(int LOD)
    {

    }

    public void Dispose()
    {
        verticesBuffer.Dispose();
    }
}
