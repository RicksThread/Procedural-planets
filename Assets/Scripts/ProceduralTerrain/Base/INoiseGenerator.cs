using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface INoiseGenerator : IDisposable
{
    /// <summary>
    /// takes a buffer array composed of vector4 where in the w values will be inserted the noiseValue
    /// </summary>
    /// <param name="vertBuffer">the buffer to take out</param>
    /// <param name="dimensions"> the dimensions of the amount of vertices for each axis</param>
    void GenerateNoiseValues(ref ComputeBuffer vertBuffer, Vector3Int dimensions, int seed);
}
