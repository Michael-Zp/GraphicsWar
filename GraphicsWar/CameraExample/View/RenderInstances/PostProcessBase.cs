using System.Numerics;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    public abstract class PostProcessBase : RenderInstanceBase, IUpdateResolution
    {
        public ITexture2D Output
        {
            get
            {
                return _renderSurface.Texture;
            }
        }

        protected IShaderProgram _postProcessShader;
        protected IRenderSurface _renderSurface;
        protected byte _fboTexComponentCount;
        protected bool _fboTexFloat;

        public PostProcessBase(IShaderProgram postProcessShader, RenderInstanceGroup group, byte fboTexComponentCount, bool fboTexFloat) : base(group)
        {
            _postProcessShader = postProcessShader;
            _fboTexComponentCount = fboTexComponentCount;
            _fboTexFloat = fboTexFloat;
        }

        public void UpdateResolution(int width, int height)
        {
            _renderSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));

            _postProcessShader.Uniform("iResolution", new Vector2(width, height));
        }
    }
}
