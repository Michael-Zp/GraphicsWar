using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using GraphicsWar.ExtensionMethods;

namespace GraphicsWar.View.RenderInstances
{
    public class ApplySSAO : IRenderInstance, IUpdateResolution
    {

        public ITexture2D Output {
            get {
                return _renderSurface.Texture;
            }
        }

        private IRenderSurface _renderSurface;
        private IShaderProgram _shader;

        public ApplySSAO(IShaderProgram shader)
        {
            _shader = shader;
        }

        public void Draw(ITexture2D ssao, ITexture2D image)
        {
            _renderSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Activate();

            _shader.ActivateOneOfMultipleTextures("ssao", 0, ssao);
            _shader.ActivateOneOfMultipleTextures("image", 1, image);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _shader.DeativateOneOfMultipleTextures(1, image);
            _shader.DeativateOneOfMultipleTextures(0, ssao);

            _shader.Deactivate();

            _renderSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _renderSurface = new FBO(Texture2dGL.Create(width, height, 4, false));

            _shader.Uniform("iResolution", new Vector2(width, height));
        }
    }


}
