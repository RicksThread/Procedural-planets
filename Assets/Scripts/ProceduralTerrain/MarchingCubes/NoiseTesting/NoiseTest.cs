using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseTest : MonoBehaviour
{
    [System.Serializable]
    public class NoiseLayer
    {
        [Header("'Donut' Influence")]
        public float dstFromCenter;
        public float radiusInfluence;

        [Header("General")]
        public float strength;
        public float scale;
        public float blending;

        [Header("Distortion")]
        public float distortion;
        public float distortionScale;
    }

    [SerializeField] private NoiseLayer[] noiseLayers;

    public int width = 256;
    public int height = 256;

    public float scale = 1;

    [Range(0.1f,1)]
    public float radiusTerrain;
    public float blendCenterDst;


    private void LateUpdate()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float perlinWeight = CalculatePerlin(x,y);
                Color colorPixel = new Color(perlinWeight,perlinWeight,perlinWeight);
                texture.SetPixel(x,y,colorPixel);

            }
        }

        texture.Apply();
        return texture;
    }

    public float CalculatePerlin(int x, int y)
    {
        float widthX = (float)x/width;
        float heightY = (float)y/height;

        Vector2 center = Vector2.one * 0.5f * scale;
        Vector2 samplePos = new Vector2(widthX,heightY) * scale;
        
        float terrainLevel = 0;
        for (int i = 0; i < noiseLayers.Length; i++)
        {
            float dstFromCenter = (samplePos - center).magnitude / (scale*0.5f);
            float minDstCenter = noiseLayers[i].dstFromCenter - noiseLayers[i].radiusInfluence;
            float maxDstCenter = noiseLayers[i].dstFromCenter + noiseLayers[i].radiusInfluence;

            if (dstFromCenter > minDstCenter || dstFromCenter < maxDstCenter)
            {
               
                float density = 1-Mathf.Clamp01(Mathf.Abs(dstFromCenter-noiseLayers[i].dstFromCenter) / noiseLayers[i].radiusInfluence);
                terrainLevel +=
                    Mathf.Clamp01(
                    Mathf.Pow
                    ( 
                        Mathf.PerlinNoise
                            (
                            samplePos.x * noiseLayers[i].scale + 
                                Mathf.PerlinNoise
                                    (
                                    samplePos.x * noiseLayers[i].distortionScale , samplePos.y * noiseLayers[i].distortionScale
                                    ) * noiseLayers[i].distortion,
                            samplePos.y * noiseLayers[i].scale +
                                Mathf.PerlinNoise
                                    (
                                        samplePos.y * noiseLayers[i].distortionScale , samplePos.x * noiseLayers[i].distortionScale
                                    ) * noiseLayers[i].distortion
                            ), 
                        noiseLayers[i].blending
                    ) * density) * noiseLayers[i].strength;
            }
        }

        return Mathf.Clamp01(terrainLevel);
    }
}