using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class Skybox : IUpdateResolution
    {
        private static readonly string[] Endings = { "_ft", "_bk", "_up", "_dn", "_lf", "_rt" };

        public ITexture2D Output => _outputSurface.Texture;

        private readonly CubeMapFBO _cubeFbo;

        private readonly VAO _sphereGeometry;

        private readonly IShaderProgram _skyboxProgram;
        private IRenderSurface _outputSurface;

        public Skybox(IContentLoader contentLoader, float size, string textureName)
        {
            _skyboxProgram = contentLoader.Load<IShaderProgram>("sky.*");

            ITexture2D[] textures = new ITexture2D[6];

            for (int i = 0; i < 6; i++)
            {
                textures[i] = contentLoader.Load<ITexture2D>(textureName + Endings[i]);
            }

            _cubeFbo = new CubeMapFBO(textures[0].Width);

            CreateMap(textures);

            var sphere = Meshes.CreateSphere(size).SwitchHandedness();
            _sphereGeometry = VAOLoader.FromMesh(sphere, _skyboxProgram);
        }

        private void CreateMap(IReadOnlyList<ITexture2D> textures)
        {
            _cubeFbo.Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, _cubeFbo.Texture.ID, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                TextureDrawer.Draw(textures[i]);
            }
            _cubeFbo.Deactivate();

            _cubeFbo.Texture.Activate();
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
            _cubeFbo.Texture.Deactivate();
        }

        public void Draw(ITransformation camera, float mipmapLevel = 0)
        {
            _outputSurface.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _skyboxProgram.Activate();

            _cubeFbo.Texture.Activate();


            _skyboxProgram.Uniform("mipmapLevel", mipmapLevel);
            _skyboxProgram.Uniform("camera", camera);
            _sphereGeometry.Draw();

            _cubeFbo.Texture.Deactivate();

            _skyboxProgram.Deactivate();

            _outputSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            ((FBO)_outputSurface)?.Dispose();
            _outputSurface = new FBO(Texture2dGL.Create(width, height));
        }
    }
}
