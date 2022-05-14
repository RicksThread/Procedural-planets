using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereShell : MonoBehaviour
{
    [SerializeField] private MeshRenderer shellMeshRenderer;
    [SerializeField] private Material shellMaterial;
    [SerializeField] private Transform lightTransform;

    [SerializeField] private Color AtmShellColor;
    [SerializeField] private float radiusAtmShell;
    [SerializeField] private float radiusPlanet;
    [SerializeField] private float blendingColorHeightSurface;
    [SerializeField] private float startDstShellFadeColor;
    [SerializeField] private float blendingFadeColorNearShell;
    [SerializeField] private float lightOffSet;
    [SerializeField] private float blendingShadow;

    public static readonly int AtmShellColorID = Shader.PropertyToID("_AtmShellColor");
    public static readonly int DirToSunID = Shader.PropertyToID("_DirToSun");
    public static readonly int RadiusAtmShellID = Shader.PropertyToID("_RadiusAtmShell");
    public static readonly int RadiusPlanetID = Shader.PropertyToID("_RadiusPlanet");
    public static readonly int BlendingColorHeightSurfaceID = Shader.PropertyToID("_BlendingColorHeightSurface");
    public static readonly int StartDstShellFadeColorID = Shader.PropertyToID("_StartDstShellFadeColor");
    public static readonly int BlendingFadeColorNearShellID = Shader.PropertyToID("_BlendingFadeColorNearShell");
    public static readonly int LightOffSetID = Shader.PropertyToID("_LightOffSet");
    public static readonly int BlendingShadowID = Shader.PropertyToID("_BlendingShadow");

    private Material shellMaterialInstanced;
    private bool isInitialised = false;
    private Vector3 dirToSun;
    
    private void OnValidate()
    {
        dirToSun = GetDirToSun();
        Initialize();    
    }

    private void Start()
    {
        dirToSun = GetDirToSun();
        Initialize();
    }

    private void LateUpdate()
    {
        dirToSun = GetDirToSun();
        shellMaterialInstanced.SetVector(DirToSunID, dirToSun);
    }

    private void Initialize()
    {
        if (isInitialised)
        {
            Reset();
        }

        shellMaterialInstanced = Instantiate(shellMaterial);
        shellMeshRenderer.sharedMaterial = shellMaterialInstanced;
        
        shellMaterialInstanced.SetColor(AtmShellColorID, AtmShellColor);

        shellMaterialInstanced.SetVector(DirToSunID, dirToSun);
        shellMaterialInstanced.SetFloat(RadiusAtmShellID, radiusAtmShell);
        shellMaterialInstanced.SetFloat(RadiusPlanetID, radiusPlanet);
        shellMaterialInstanced.SetFloat(BlendingColorHeightSurfaceID, blendingColorHeightSurface);
        shellMaterialInstanced.SetFloat(StartDstShellFadeColorID, startDstShellFadeColor);
        shellMaterialInstanced.SetFloat(BlendingFadeColorNearShellID, blendingFadeColorNearShell);
        shellMaterialInstanced.SetFloat(LightOffSetID, lightOffSet);
        shellMaterialInstanced.SetFloat(BlendingShadowID, blendingShadow);
    }

    private void OnDestroy()
    {
        Reset();    
    }

    private void Reset()
    {
        isInitialised = true;
        
        if (Application.isPlaying)
        {
            Destroy(shellMaterialInstanced);
        }
        else
        {
            DestroyImmediate(shellMaterialInstanced);
        }
    }

    private Vector3 GetDirToSun()
    {
        return  (lightTransform.position - transform.position).normalized;
    }

}
