using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


//NOTE: to be studied later
public class Blit : ScriptableRendererFeature {
 
    public class BlitPass : ScriptableRenderPass {
        
        public enum RenderTarget 
        {
            Color,
            RenderTexture,
        }
 
        public Material blitMaterial = null;            //the material used
        public int blitShaderPassIndex = 0;             //the pass used for the shader
        public FilterMode filterMode { get; set; }      //linear - bilinear - trilinear filtering
 
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
 
        private RenderTargetHandle m_TemporaryColorTexture;
        private string m_ProfilerTag;
         
        public BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag) 
        {
            //store the given data
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;

            //initializes the shader property with given name for the temporary texture
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }
         
        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination) 
        {
            this.source = source;
            this.destination = destination;
        }
    
        //called in every draw call
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) 
        {
            //instantiate a command buffer with given name
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            //gets the renderTexture definer instance from the camera descriptor
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            
            // Can't read and write to same color target, use a TemporaryRT
            if (destination == RenderTargetHandle.CameraTarget) 
            {
                //creates a temporary renderTexture through the use of the RenderTextureDescriptor and filterMode 
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);

                //copies the renderTexture data source to the temporary renderTexture handle with the effects given by the blitmaterial
                Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, blitShaderPassIndex);

                //returns back the modified texture renderer to the source
                Blit(cmd, m_TemporaryColorTexture.Identifier(), source);
            } else 
            {
                Blit(cmd, source, destination.Identifier(), blitMaterial, blitShaderPassIndex);
            }

            //execute the command and release the command buffer
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
         
        public override void FrameCleanup(CommandBuffer cmd) 
        {
            if (destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }
 
    [System.Serializable]
    public class BlitSettings {
        public RenderPassEvent renderEventTiming = RenderPassEvent.AfterRenderingOpaques;
 
        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public Target destination = Target.Camera;

        //id of the texture in the shader that needs to be handled as a destination
        //it is valued only when the target is a texture
        public string textureId = "_BlitPassTexture";
        public string tagCamera = "BlitEffectRenderer";
        public bool allCameras = false;
    }
 
    public enum Target 
    {
        Camera,
        Texture
    }
    
    //handles instance of the blit settings
    public BlitSettings settings = new BlitSettings();
    RenderTargetHandle m_RenderTextureHandle;
 
    BlitPass blitPass;
 
    //setup the rendererFeatures
    public override void Create() 
    {
        int maxPassIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;

        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, maxPassIndex);

        blitPass = new BlitPass(settings.renderEventTiming, settings.blitMaterial, settings.blitMaterialPassIndex, name);

        m_RenderTextureHandle.Init(settings.textureId);
    }
    
    //adds the renderPass to the scriptableRenderer
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        
        if (!settings.allCameras)
        {
            if (!renderingData.cameraData.camera.CompareTag(settings.tagCamera))
                return;   
        }
        //settings the source to the camera renderer texture
        RenderTargetIdentifier src = renderer.cameraColorTarget;
        RenderTargetHandle dest = (settings.destination == Target.Camera) ? RenderTargetHandle.CameraTarget : m_RenderTextureHandle;

        if (settings.blitMaterial == null) {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
 
        //setting up the pass
        blitPass.Setup(src, dest);

        //register the pass to the scriptableRenderer
        renderer.EnqueuePass(blitPass);
    }
}

/*
    DOCS ABOUT EACH OF THE CLASSES/STRUCTS USED
    
    ScriptableRenderer              //implements the rendering mode of the objects through renderPass
    RenderingData                   // ? 
    ScriptableRendererFeature       //holds the information and initialization process of the renderPass that'll be injected in the scriptableRenderer
    ScriptableRenderPass            //implements logical rendering pass to the scriptableRenderer    
    RenderTargetIdentifier          //handles an opaque renderTexture
    renderTexture                   //is texture that can be rendered directly to the screen
    RenderTargetHandle              //a wrapper class used to get the renderTargetIdentifier and holds the identity of the property (so you can reference it by many means)

    RenderTextureDescriptor         //contains all the information required to create a renderTexture
*/