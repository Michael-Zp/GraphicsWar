﻿using System.Numerics;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    /// <summary>
    /// Takes one input and one output texture and draws a simple quad over the whole screen
    /// </summary>
    public class OnePassPostProcessShader : IUpdateResolution
    {
        public ITexture2D Output => _renderSurface.Texture;

        private readonly IShaderProgram _postProcessShader;
        private IRenderSurface _renderSurface;
        private readonly byte _fboTexComponentCount;
        private readonly bool _fboTexFloat;

        public OnePassPostProcessShader(IShaderProgram postProcessShader, byte fboTexComponentCount = 4, bool fboTexFloat = false)
        {
            _postProcessShader = postProcessShader;
            _fboTexComponentCount = fboTexComponentCount;
            _fboTexFloat = fboTexFloat;
        }

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

        public void UpdateResolution(int width, int height)
        {
            _renderSurface = new FBO(Texture2dGL.Create(width, height, _fboTexComponentCount, _fboTexFloat));

            _postProcessShader.Uniform("iResolution", new Vector2(width, height));
        }
    }
}