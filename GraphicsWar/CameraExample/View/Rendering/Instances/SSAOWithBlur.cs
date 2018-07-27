using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class SSAOWithBlur : IUpdateResolution
    {
        public ITexture2D Output => _renderSurface.Texture;

        private readonly OnePassPostProcessShader _ssao;
        private readonly Blur _blur;

        private IRenderSurface _renderSurface;
        private readonly IShaderProgram _shader;

        public SSAOWithBlur(IContentLoader contentLoader, float blurKernelSize)
        {
            _ssao = new OnePassPostProcessShader(contentLoader.LoadPixelShader("SSAO.glsl"));
            _blur = new Blur(contentLoader, blurKernelSize);
            _shader = contentLoader.LoadPixelShader("saturationMap.glsl");
        }

        public void Draw(ITexture2D dephtTexture, ITexture2D image)
        {
            _ssao.Draw(dephtTexture);
            _blur.Draw(_ssao.Output);

            _renderSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Activate();

            _shader.ActivateTexture("saturation", 0, _blur.Output);
            _shader.ActivateTexture("image", 1, image);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _shader.DeactivateTexture(1, image);
            _shader.DeactivateTexture(0, _blur.Output);

            _shader.Deactivate();

            _renderSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _ssao.UpdateResolution(width, height);
            _blur.UpdateResolution(width, height);

            _renderSurface = new FBO(Texture2dGL.Create(width, height));

            _shader.Uniform("iResolution", new Vector2(width, height));
        }
    }
}
