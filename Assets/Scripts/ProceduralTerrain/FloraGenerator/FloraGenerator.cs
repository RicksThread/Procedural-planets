using UnityEngine;
using MeshUtilities;
using MEC;

///<summary>
///It handles the generation of the flora layers the dispose of the shader buffers
///</summary>
public class FloraGenerator
{
    //handles the various layers of flora settings and their height data
    private FloraLayerSettings[] floraLayersSettings;
    private HeightData heightData;


    private FloraLayer[] floraLayers;
    
    public FloraGenerator(FloraLayerSettings[] floraLayersSettings, HeightData heightData, Bounds bounds, Transform transformLocal)
    {

        this.floraLayersSettings = floraLayersSettings;

        //set a common height data for all the flora layers
        this.heightData = heightData;
        floraLayers = new FloraLayer[floraLayersSettings.Length];

        //initialize the floraLayers and set their respective settings and data
        for (int i = 0; i < floraLayers.Length; i++)
        {
            floraLayers[i] = new FloraLayer(floraLayersSettings[i], heightData, bounds, transformLocal);
        }
    }

    ///<summary>
    ///Draw the flora on top of a mesh with the given settings
    ///</summary>
    public void GenerateFlora(Mesh mesh)
    {
        Debug.Log("sdus");

        foreach(FloraLayer floraGenerator in floraLayers)
        {
            floraGenerator.GenerateFlora(mesh.vertices, mesh.triangles);
        }
    }

    ///<summary>
    ///Draw the flora on top of a mesh reconstructed with the vertices and triangles with the given settings
    ///</summary>
    public void GenerateFlora(Vector3[] vertices, int[] triangles)
    {

        foreach(FloraLayer floraGenerator in floraLayers)
        {
            floraGenerator.GenerateFlora(vertices, triangles);
        }
    }

    public void Tick() {
        if (floraLayers == null || floraLayers.Length == 0) return;
        for (int i = 0; i < floraLayers.Length; i++)
        {
            floraLayers[i].Draw();
        }
    }

    public void Dispose()
    {
        if (floraLayers != null)
        {
            for (int i = 0; i < floraLayers.Length; i++)
            {
                if(floraLayers[i] != null) floraLayers[i].Dispose();
            }
        }
    }
}
