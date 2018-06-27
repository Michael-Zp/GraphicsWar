using System.Numerics;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    public class DeferredLighting : IRenderInstance, IUpdateResolution
    {
        public ITexture2D Output
        {
            get
            {
                return _renderSurface.Texture;
            }
        }

        private IShaderProgram _shader;
        private IRenderSurface _renderSurface;
        private byte _fboTexComponentCount;
        private bool _fboTexFloat;
        
        public DeferredLighting(IShaderProgram postProcessShader, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            _shader = postProcessShader;
            _fboTexComponentCount = fboTexComponentCount;
            _fboTexFloat = fboTexFloat;
        }

        public void UpdateResolution(int width, int height)
        {
            _renderSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));

            _shader.Uniform("iResolution", new Vector2(width, height));
        }
    }
}
