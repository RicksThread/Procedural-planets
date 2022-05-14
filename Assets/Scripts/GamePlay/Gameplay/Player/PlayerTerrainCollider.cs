using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTerrainCollider : MonoBehaviour
{
    [Header("Dummy collider settings")]
    [SerializeField] private float radiusCollider = 0.3f;
    [SerializeField] private SphereCollider dummyCollider;

    [Header("Terrain detection settings")]
    [SerializeField] private Transform startRayPoint;
    [SerializeField] private Transform orientation;
    [SerializeField] private float lengthRay = 2;
    [SerializeField] private LayerMask terrainLayer;

    private void Start()
    {
        dummyCollider.radius = radiusCollider;
    }

    private void FixedUpdate()
    {
        RaycastHit rayHitTerrain; 
        if (Physics.Raycast(startRayPoint.position, -orientation.up, out rayHitTerrain, lengthRay, terrainLayer, QueryTriggerInteraction.Ignore))
        {
            Vector3 dirToPointTerrain = rayHitTerrain.point - startRayPoint.position;    
            dummyCollider.transform.position = startRayPoint.position + dirToPointTerrain.normalized * (dirToPointTerrain.magnitude + dummyCollider.radius);
        }
    }

}
