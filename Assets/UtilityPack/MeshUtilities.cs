using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace MeshUtilities
{
    public struct IndirectArgs
    {
        public uint numVerticesPerInstance;
        public uint numInstances;
        public uint startVertexIndex;
        public uint startInstanceIndex;

        public static IndirectArgs GetDefault()
        {
            IndirectArgs args;
            args.numVerticesPerInstance = 0;
            args.numInstances = 1;
            args.startVertexIndex = 0;
            args.startInstanceIndex = 0;
            return args;
        }
    }

    public struct Triangle
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
    }

    public class MeshDataArgs : EventArgs
    {
        public Vector3[] vertices;
        public int[] triangles;
    }

    ///<summary>
    ///Contains the height data used for many cases of terrain generation
    ///</summary>
    public class HeightData
    {
        public float maxHeight;
        public float minHeight;

        public Vector3 startPos;
        public Vector3 dirHeight;

        public enum HeightType {Planet, Plane};
        public HeightType heightType;
        
        public static readonly int maxHeightID = Shader.PropertyToID("maxHeight");
        public static readonly int minHeightID = Shader.PropertyToID("minHeight");
        public static readonly int startPosID = Shader.PropertyToID("startPos");
        public static readonly int dirHeightID = Shader.PropertyToID("dirHeight");
    }
}
