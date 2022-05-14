using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraBehaviour : MonoBehaviour
{
    public bool isInitialize {get; private set;}  = false;
    
    private FloraGenerator floraGenerator;

    void Update()
    {
        if (!isInitialize) return;
        floraGenerator.Tick();
    }

    public void SetFlora(FloraGenerator floraGenerator)
    {
        isInitialize = true;
        this.floraGenerator = floraGenerator;
    }

    public void UpdateFlora(Mesh mesh)
    {
        floraGenerator.GenerateFlora(mesh);
    }

    public void UpdateFlora(Vector3[] vertices, int[] triangles)
    {
        floraGenerator.GenerateFlora(vertices,triangles);
    }

    private void OnDestroy() {
        if (floraGenerator != null)
            floraGenerator.Dispose();
    }

}
