using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityPack;

public class CameraSettings : MonoBehaviour
{
    [System.Serializable]
    public class CullingLayer
    {
        public LayerMask layer;
        public float cullingDistance;
    }

    [SerializeField] private CullingLayer[] cullingLayers;
    [SerializeField] private Camera cam;

    private void Start() 
    {
        float[] cullingDistances = cam.layerCullDistances;
        for (int i = 0; i < cullingLayers.Length; i++)
        {
            int layerIndexTarget = Utilities.GetLayerIndex(cullingLayers[i].layer);
            Debug.Log(layerIndexTarget);
            cullingDistances[layerIndexTarget] = cullingLayers[i].cullingDistance;
        }
        cam.layerCullDistances = cullingDistances;
    }
}
