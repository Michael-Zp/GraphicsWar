using GraphicsWar.ExtensionMethods;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using System;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class AddWithDepthTest : IUpdateResolution
    {
        public ITexture2D Depth => _outputSurface.Textures[0];
        public ITexture2D Color => _outputSurface.Textures[1];
        public ITexture2D Normal => _outputSurface.Textures[2];
        public ITexture2D Position => _outputSurface.Textures[3];
        public ITexture2D IntensityMap => _outputSurface.Textures[4];

        private readonly IShaderProgram _addWithDepthTestProgram;
        private IRenderSurface _outputSurface;

        private static readonly DrawBuffersEnum[] _drawBuffers = new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 };

        public AddWithDepthTest(IContentLoader contentLoader)
        {
            _addWithDepthTestProgram = contentLoader.LoadPixelShader("addWithDepthTest.glsl");
        }

        public void Draw(ITexture2D depth1, ITexture2D depth2, ITexture2D one1, ITexture2D one2, ITexture2D two1, ITexture2D two2, ITexture2D three1, ITexture2D three2, ITexture2D four1, ITexture2D four2, float offset = 0)
        {
            _outputSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _addWithDepthTestProgram.Activate();
            _addWithDepthTestProgram.ActivateTexture("depth1", 0, depth1);
            _addWithDepthTestProgram.ActivateTexture("depth2", 1, depth2);
            _addWithDepthTestProgram.Uniform("offset", offset);
            _addWithDepthTestProgram.ActivateTexture("bufferOne1", 2, one1);
            _addWithDepthTestProgram.ActivateTexture("bufferOne2", 3, one2);
            _addWithDepthTestProgram.ActivateTexture("bufferTwo1", 4, two1);
            _addWithDepthTestProgram.ActivateTexture("bufferTwo2", 5, two2);
            _addWithDepthTestProgram.ActivateTexture("bufferThree1", 6, three1);
            _addWithDepthTestProgram.ActivateTexture("bufferThree2", 7, three2);
            _addWithDepthTestProgram.ActivateTexture("bufferFour1", 8, four1);
            _addWithDepthTestProgram.ActivateTexture("bufferFour2", 9, four2);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(_drawBuffers.Length, _drawBuffers);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _addWithDepthTestProgram.DeactivateTexture(9, four2);
            _addWithDepthTestProgram.DeactivateTexture(8, four1);
            _addWithDepthTestProgram.DeactivateTexture(7, three2);
            _addWithDepthTestProgram.DeactivateTexture(6, three1);
            _addWithDepthTestProgram.DeactivateTexture(5, two2);
            _addWithDepthTestProgram.DeactivateTexture(4, two1);
            _addWithDepthTestProgram.DeactivateTexture(3, one2);
            _addWithDepthTestProgram.DeactivateTexture(2, one1);
            _addWithDepthTestProgram.DeactivateTexture(1, depth2);
            _addWithDepthTestProgram.DeactivateTexture(0, depth1);

            _addWithDepthTestProgram.Deactivate();

            _outputSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            ((FBO)_outputSurface)?.Dispose();
            _outputSurface = new FBO(Texture2dGL.Create(width, height, 1, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 4, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 4, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 4, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 4, true));
        }
    }
}
