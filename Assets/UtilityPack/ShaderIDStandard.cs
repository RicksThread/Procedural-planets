using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShaderIDStandard
{
    public static readonly int VerticesVert4ID = Shader.PropertyToID("verticesVert4");
    public static readonly int VerticesVert3ID = Shader.PropertyToID("verticesVert3");
    public static readonly int PointsID = Shader.PropertyToID("points");

    public static readonly int DimensionXID = Shader.PropertyToID("dimensionX");
    public static readonly int DimensionYID = Shader.PropertyToID("dimensionY");
    public static readonly int DimensionZID = Shader.PropertyToID("dimensionZ");

    public static readonly int LOD = Shader.PropertyToID("levelOfDetail");
}
