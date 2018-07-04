using Zenseless.HLGL;

namespace GraphicsWar.View.RenderInstances
{
    public class SSAOWithBlur : IRenderInstance, IUpdateResolution
    {
        public ITexture2D Output {
            get {
                return _applySSAO.Output;
            }
        }

        private readonly OnePassPostProcessShader _ssao;
        private readonly Blur _blur;
        private readonly ApplySSAO _applySSAO;

        public SSAOWithBlur(IContentLoader contentLoader, float blurKernelSize)
        {
            _ssao = new OnePassPostProcessShader(contentLoader.LoadPixelShader("SSAO.glsl"));
            _blur = new Blur(contentLoader.LoadPixelShader("BlurGausPass1"), contentLoader.LoadPixelShader("BlurGausPass2"), blurKernelSize);
            _applySSAO = new ApplySSAO(contentLoader.LoadPixelShader("ApplySSAOToImage.glsl"));
        }
        
        public void Draw(ITexture2D dephtTexture, ITexture2D image)
        {
            _ssao.Draw(dephtTexture);
            _blur.Draw(_ssao.Output);
            _applySSAO.Draw(_blur.Output, image);
        }

        public void UpdateResolution(int width, int height)
        {
            _ssao.UpdateResolution(width, height);
            _blur.UpdateResolution(width, height);
            _applySSAO.UpdateResolution(width, height);
        }
    }
}
