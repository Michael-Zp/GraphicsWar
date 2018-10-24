using GraphicsWar.View.Rendering.Management;
using Zenseless.HLGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class Bloom : IUpdateResolution
    {
        public ITexture2D Output => _add.Output;

        OnePassPostProcessShader _extract;
        AvgBlur _blur;
        Addition _add;

        public Bloom(IContentLoader contentLoader)
        {
            _extract = new OnePassPostProcessShader(contentLoader.LoadPixelShader("Extract.glsl"));
            _blur = new AvgBlur(contentLoader);
            _add = new Addition(contentLoader);
        }

        public void Draw(ITexture2D inputTexture)
        {
            _extract.Draw(inputTexture);
            _blur.Draw(_extract.Output);
            _add.Draw(inputTexture, _blur.Output);
        }

        public void UpdateResolution(int width, int height)
        {
            _extract.UpdateResolution(width, height);
            _blur.UpdateResolution(width, height);
            _add.UpdateResolution(width, height);
        }
    }
}
