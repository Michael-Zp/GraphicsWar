using Zenseless.HLGL;

namespace GraphicsWar.View.RenderInstances
{
    public class DeferredLighting : PostProcessBase
    {
        public DeferredLighting(IShaderProgram postProcessShader, byte fboTexComponentCount = 4, bool fboTexFloat = false) : base(postProcessShader, fboTexComponentCount, fboTexFloat)
        {
        }
    }
}
