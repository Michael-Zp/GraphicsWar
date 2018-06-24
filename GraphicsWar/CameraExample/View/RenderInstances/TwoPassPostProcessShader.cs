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

        private readonly IShaderProgram _passOne;
        private readonly IShaderProgram _passTwo;
        private IRenderSurface _passOneSurface;
        private IRenderSurface _passTwoSurface;
        private readonly byte _fboTexComponentCount;
        private readonly bool _fboTexFloat;

        public TwoPassPostProcessShader(IShaderProgram blurPassOne, IShaderProgram blurPassTwo, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            _passOne = blurPassOne;
            _passTwo = blurPassTwo;
        }

        public void Draw(ITexture2D inputTexture)
        {
            DrawPass(inputTexture, _passOneSurface, _passOne);
            DrawPass(_passOneSurface.Texture, _passTwoSurface, _passTwo);
        }

        private void DrawPass(ITexture2D inputTexture, IRenderSurface surface, IShaderProgram shader)
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
