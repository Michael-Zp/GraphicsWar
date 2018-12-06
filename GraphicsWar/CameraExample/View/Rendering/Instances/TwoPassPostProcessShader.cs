using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class TwoPassPostProcessShader : IUpdateResolution
    {
        public ITexture2D Output => PassTwoSurface.Texture;

        protected readonly IShaderProgram PassOne;
        protected readonly IShaderProgram PassTwo;
        protected IRenderSurface PassOneSurface;
        protected IRenderSurface PassTwoSurface;
        protected readonly byte FboTexComponentCount;
        protected readonly bool FboTexFloat;

        public TwoPassPostProcessShader(IShaderProgram blurPassOne, IShaderProgram blurPassTwo, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            PassOne = blurPassOne;
            PassTwo = blurPassTwo;
            FboTexComponentCount = fboTexComponentCount;
            FboTexFloat = fboTexFloat;
        }

        public void Draw(ITexture2D inputTexture)
        {
            DrawPass(inputTexture, PassOneSurface, PassOne);
            DrawPass(PassOneSurface.Texture, PassTwoSurface, PassTwo);
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
            ((FBO)PassOneSurface)?.Dispose();
            PassOneSurface = new FBO(Texture2dGL.Create(width, height, FboTexComponentCount, FboTexFloat));
            ((FBO)PassTwoSurface)?.Dispose();
            PassTwoSurface = new FBO(Texture2dGL.Create(width, height, FboTexComponentCount, FboTexFloat));
        }
    }
}
