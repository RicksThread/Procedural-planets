using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBehaviour : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public BoxCollider boxCollider;
    public FloraBehaviour floraBehaviour;

    public Vector3 pos;
    public float size;


    private void OnDrawGizmos() 
    {
        Gizmos.color = new Color(0.3f, 0.6f, 0.3f, 0.009f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(pos, Vector3.one * size); 
        Gizmos.color = new Color(0.3f, 0.6f, 0.3f, 0.1f);
        Gizmos.DrawWireCube(pos, Vector3.one * size); 
    }       
}