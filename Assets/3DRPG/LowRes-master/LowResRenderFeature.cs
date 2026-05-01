using UnityEngine.Rendering.Universal;

public class LowResRenderFeature : ScriptableRendererFeature
{
    LowResPass lowResPass;

    public override void Create()
    {
        lowResPass = new LowResPass(RenderPassEvent.BeforeRenderingPostProcessing);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        lowResPass.Setup(renderer.cameraColorTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Setup() は上記の SetupRenderPasses で呼ばれるため、ここでは追加するだけ
        renderer.EnqueuePass(lowResPass);
    }
}
