﻿using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class ShadowBlur : TwoPassPostProcessShader
    {
        private readonly float _blurKernelSize;

        public ShadowBlur(IContentLoader contentLoader, float blurKernelSize = 20, byte fboTexComponentCount = 4, bool fboTexFloat = false)
            : base(contentLoader.LoadPixelShader("ShadowBlurGausPass1.glsl"), contentLoader.LoadPixelShader("ShadowBlurGausPass2.glsl"), fboTexComponentCount, fboTexFloat)
        {
            _blurKernelSize = blurKernelSize;
        }

        public new void Draw(ITexture2D inputTexture)
        {
            DrawPass(inputTexture, PassOneSurface, PassOne);
            DrawPass(PassOneSurface.Texture, PassTwoSurface, PassTwo);
        }

        private new void DrawPass(ITexture2D inputTexture, IRenderSurface surface, IShaderProgram shader)
        {
            surface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Activate();
            shader.Uniform("GaussSize", _blurKernelSize);

            inputTexture.Activate();

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            inputTexture.Deactivate();

            shader.Deactivate();

            surface.Deactivate();
        }
    }
}
