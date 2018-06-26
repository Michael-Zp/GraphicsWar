using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    public class TwoPassPostProcessShader : IRenderInstance, IUpdateResolution
    {
        public ITexture2D Output {
            get {
                return _passTwoSurface.Texture;
            }
        }

        protected readonly IShaderProgram _passOne;
        protected readonly IShaderProgram _passTwo;
        protected IRenderSurface _passOneSurface;
        protected IRenderSurface _passTwoSurface;
        protected readonly byte _fboTexComponentCount;
        protected readonly bool _fboTexFloat;

        public TwoPassPostProcessShader(IShaderProgram blurPassOne, IShaderProgram blurPassTwo, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            _passOne = blurPassOne;
            _passTwo = blurPassTwo;
            _fboTexComponentCount = fboTexComponentCount;
            _fboTexFloat = fboTexFloat;
        }

        public void Draw(ITexture2D inputTexture)
        {
            DrawPass(inputTexture, _passOneSurface, _passOne);
            DrawPass(_passOneSurface.Texture, _passTwoSurface, _passTwo);
        }

        protected void DrawPass(ITexture2D inputTexture, IRenderSurface surface, IShaderProgram shader)
        {
            surface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Activate();

            inputTexture.Activate();

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            inputTexture.Deactivate();

            shader.Deactivate();

            surface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _passOneSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));
            _passTwoSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));
        }
    }
}
