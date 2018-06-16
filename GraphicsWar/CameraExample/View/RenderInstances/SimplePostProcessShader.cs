using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;

namespace GraphicsWar.View.RenderInstances
{
    /// <summary>
    /// Takes one input and one output texture and draws a simple quad over the whole screen
    /// </summary>
    public class SimplePostProcessShader : PostProcessBase
    {
        public SimplePostProcessShader(IShaderProgram postProcessShader, byte fboTexComponentCount, bool fboTexFloat, RenderInstanceGroup group) : base(postProcessShader, fboTexComponentCount, fboTexFloat, group)
        {}

        public void Draw(ITexture2D inputTexture)
        {
            _renderSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _postProcessShader.Activate();

            inputTexture.Activate();

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            inputTexture.Deactivate();

            _postProcessShader.Deactivate();

            _renderSurface.Deactivate();
        }
    }
}
