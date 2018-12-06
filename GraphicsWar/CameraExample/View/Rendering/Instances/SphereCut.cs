using System.Drawing;
using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class SphereCut : IUpdateResolution
    {
        public ITexture2D Output => _outputSurface.Texture;

        private readonly VAO _sphereGeometry;

        private readonly IShaderProgram _sphereCutProgram;
        private IRenderSurface _outputSurface;

        public SphereCut(IContentLoader contentLoader, float size)
        {
            _sphereCutProgram = contentLoader.Load<IShaderProgram>("imageOnGeometry.*");

            var sphere = Meshes.CreateSphere(size, 5).SwitchHandedness();
            _sphereGeometry = VAOLoader.FromMesh(sphere, _sphereCutProgram);
        }

        public void Draw(ITransformation camera, ITexture2D image, ITexture2D depth)
        {
            _outputSurface.Activate();
            GL.ClearColor(Color.FromArgb(0,0,0,0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _sphereCutProgram.Activate();

            _sphereCutProgram.ActivateTexture("image", 1, image);
            _sphereCutProgram.ActivateTexture("depth", 0, depth);

            _sphereCutProgram.Uniform("camera", camera);
            _sphereGeometry.Draw();

            _sphereCutProgram.DeactivateTexture(1, image);
            _sphereCutProgram.DeactivateTexture(0, depth);

            _sphereCutProgram.Deactivate();

            _outputSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            ((FBO)_outputSurface)?.Dispose();
            _outputSurface = new FBO(Texture2dGL.Create(width, height));

            _sphereCutProgram.Activate();
            _sphereCutProgram.Uniform("iResolution", new Vector2(width, height));
            _sphereCutProgram.Deactivate();
        }
    }
}
