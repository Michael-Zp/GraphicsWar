using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    /// <summary>
    /// Takes one input and one output texture and draws a simple quad over the whole screen
    /// </summary>
    public class Add : IUpdateResolution
    {
        public ITexture2D Output => _outputSurface.Texture;

        private readonly IShaderProgram _addProgram;
        private IRenderSurface _outputSurface;
        private readonly byte _fboTexComponentCount;
        private readonly bool _fboTexFloat;

        public Add(IContentLoader contentLoader, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            _addProgram = contentLoader.LoadPixelShader("add.glsl");
            _fboTexComponentCount = fboTexComponentCount;
            _fboTexFloat = fboTexFloat;
        }

        public void Draw(ITexture2D texOne, ITexture2D texTwo, float factor = 1)
        {
            _outputSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _addProgram.Activate();
            _addProgram.ActivateTexture("image1", 0, texOne);
            _addProgram.ActivateTexture("image2", 1, texTwo);
            _addProgram.Uniform("factor", factor);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _addProgram.DeactivateTexture(1, texTwo);
            _addProgram.DeactivateTexture(0, texOne);

            _addProgram.Deactivate();

            _outputSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _outputSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));
        }
    }
}
