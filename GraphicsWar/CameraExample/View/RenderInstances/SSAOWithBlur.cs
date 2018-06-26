using Zenseless.HLGL;

namespace GraphicsWar.View.RenderInstances
{
    public class SSAOWithBlur : IRenderInstance, IUpdateResolution
    {
        public ITexture2D Output {
            get {
                return _blur.Output;
            }
        }

        private readonly OnePassPostProcessShader _ssao;
        private readonly Blur _blur;

        public SSAOWithBlur(IContentLoader contentLoader, float blurKernelSize)
        {
            _ssao = new OnePassPostProcessShader(contentLoader.LoadPixelShader("SSAO.glsl"));
            _blur = new Blur(contentLoader.LoadPixelShader("BlurGausPass1"), contentLoader.LoadPixelShader("BlurGausPass2"), blurKernelSize);
        }
        
        public void Draw(ITexture2D inputTexture)
        {
            _ssao.Draw(inputTexture);
            _blur.Draw(_ssao.Output);
        }

        public void UpdateResolution(int width, int height)
        {
            _ssao.UpdateResolution(width, height);
            _blur.UpdateResolution(width, height);
        }
    }
}
