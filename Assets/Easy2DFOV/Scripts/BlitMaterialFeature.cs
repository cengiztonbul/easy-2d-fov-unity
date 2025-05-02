using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Easy2DFOV
{
    public class BlitMaterialFeature : ScriptableRendererFeature {
        class RenderPass : ScriptableRenderPass {

            private readonly string profilingName;
            private readonly Material material;
            private readonly int materialPassIndex;
            private RenderTargetIdentifier sourceID;
            private RenderTargetHandle tempTextureHandle;

            public RenderPass(string profilingName, Material material, int passIndex) : base() {
                this.profilingName = profilingName;
                this.material = material;
                this.materialPassIndex = passIndex;
                tempTextureHandle.Init("_TempBlitMaterialTexture");
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                sourceID = renderingData.cameraData.renderer.cameraColorTarget;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                CommandBuffer cmd = CommandBufferPool.Get(profilingName);

                RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
                cameraTextureDesc.depthBufferBits = 0;

                cmd.GetTemporaryRT(tempTextureHandle.id, cameraTextureDesc, FilterMode.Bilinear);
                Blit(cmd, sourceID, tempTextureHandle.Identifier(), material, materialPassIndex);
                Blit(cmd, tempTextureHandle.Identifier(), sourceID);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd) {
                cmd.ReleaseTemporaryRT(tempTextureHandle.id);
            }
        }

        [System.Serializable]
        public class Settings {
            public Material material;
            public int materialPassIndex = -1; // -1 means render all passes
            public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        [SerializeField]
        private Settings settings = new Settings();

        private RenderPass renderPass;

        public Material Material {
            get => settings.material;
        }

        public override void Create() {
            renderPass = new RenderPass(name, settings.material, settings.materialPassIndex);
            renderPass.renderPassEvent = settings.renderEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (renderingData.cameraData.camera.CompareTag("MainCamera"))
            {
                renderer.EnqueuePass(renderPass);
            }
        }
    }
}
