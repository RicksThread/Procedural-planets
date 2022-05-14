using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UtilityPack;

public class Atmosphere : MonoBehaviour
{
    [Header("General components")]
    [SerializeField] private ComputeShader opticalDepthCompute;
    [SerializeField] private Material materialAtmoSphere;
    [SerializeField] private Light lightSource;

    [Header("Main settings")]
    [SerializeField] private float atmopshereRadius;
    [SerializeField] private float planetRadius;
    [SerializeField] private float densityFallOff;

    [Header("Color")]
    [SerializeField] private float scatteringStrength;
    [SerializeField] private Vector3 waveLengths = new Vector3(700,530,440);

    [Header("Space sampling settings")]
    [SerializeField] private int opticalDepthTextureSize = 1;
    [SerializeField] private int opticalDepthSamples;
    [SerializeField] private int scatteringSamples;

    public static readonly int OpticalDepthTextureID = Shader.PropertyToID("_BakedOpticalDepthTexture");
    public static readonly int OpticalDepthTextureSizeID = Shader.PropertyToID("_OpticalDepthTextureSize");
    public static readonly int OpticalDepthSamplesID = Shader.PropertyToID("_OpticalDepthSamples");
    public static readonly int AtmosphereRadiusID = Shader.PropertyToID("_AtmosphereRadius");
    public static readonly int DensityFalloffID = Shader.PropertyToID("_DensityFallOff");
    public static readonly int ScatteringCoefficientsID = Shader.PropertyToID("_ScatteringCoefficients");
    public static readonly int PlanetCentreID = Shader.PropertyToID("_PlanetCentre");
    public static readonly int DirFromSunID = Shader.PropertyToID("_DirFromSun");
    public static readonly int PlanetRadiusID = Shader.PropertyToID("_PlanetRadius");
    public static readonly int ScatteringLightSamplesID = Shader.PropertyToID("_ScatteringSamples");

    private RenderTexture opticalDepthTexture; 

    private void OnValidate()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();    
    }

    private void LateUpdate() 
    {
        materialAtmoSphere.SetVector(PlanetCentreID, transform.position);
        materialAtmoSphere.SetVector(DirFromSunID, -(transform.position- lightSource.transform.position).normalized);
    }

    [ContextMenu("Create Texture")]
    public void CreateTexture()
    {
        if (opticalDepthTexture != null)
        {
            opticalDepthTexture.Release();
        }
        opticalDepthTexture = new RenderTexture(opticalDepthTextureSize, opticalDepthTextureSize,1);
        opticalDepthTexture.enableRandomWrite = true;
        opticalDepthTexture.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        opticalDepthTexture.filterMode = FilterMode.Bilinear;
        opticalDepthTexture.Create();

        opticalDepthCompute.SetTexture(0,  OpticalDepthTextureID, opticalDepthTexture);
        opticalDepthCompute.SetInt(OpticalDepthTextureSizeID, opticalDepthTextureSize);
        opticalDepthCompute.SetInt(OpticalDepthSamplesID, opticalDepthSamples);
        opticalDepthCompute.SetFloat(AtmosphereRadiusID, atmopshereRadius);
        opticalDepthCompute.SetFloat(DensityFalloffID, densityFallOff);

        ComputeHelper.Run(opticalDepthCompute, 0, opticalDepthTextureSize, opticalDepthTextureSize, 1);
    }

    public void Refresh() {
        if (materialAtmoSphere != null){
            //texture of the optical depth
            CreateTexture();

            float scatterR = Mathf.Pow(400 / waveLengths.x,4) * scatteringStrength;
            float scatterG = Mathf.Pow(400 / waveLengths.y,4) * scatteringStrength;
            float scatterB = Mathf.Pow(400 / waveLengths.z,4) * scatteringStrength;

            Vector3 scatteringCoefficients = new Vector3(scatterR,scatterG,scatterB);
            
            materialAtmoSphere.SetTexture(OpticalDepthTextureID, opticalDepthTexture);
            
            materialAtmoSphere.SetVector(PlanetCentreID, transform.position);
            materialAtmoSphere.SetVector(DirFromSunID, -(transform.position - lightSource.transform.position).normalized);
            
            materialAtmoSphere.SetFloat(AtmosphereRadiusID, atmopshereRadius);
            materialAtmoSphere.SetFloat(PlanetRadiusID, planetRadius);
            
            materialAtmoSphere.SetInt(ScatteringLightSamplesID, scatteringSamples);
            materialAtmoSphere.SetFloat(DensityFalloffID, densityFallOff);
            materialAtmoSphere.SetVector(ScatteringCoefficientsID, scatteringCoefficients);
            
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Atmosphere))]
public class AtmosphereEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        Atmosphere atmopshere = (Atmosphere)target;
        if (GUILayout.Button("Refresh")){
            atmopshere.Refresh();
        }
    }
}
#endif