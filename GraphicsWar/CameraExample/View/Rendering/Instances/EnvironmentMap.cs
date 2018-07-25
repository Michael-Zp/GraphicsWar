using System;
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
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace GraphicsWar.View.Rendering.Instances
{
    class EnvironmentMap : IUpdateResolution, IUpdateTransforms
    {
        public ITexture2D Output => _outputSurface.Texture;

        private readonly ITransformation[] _cameras = new ITransformation[6];
        private readonly Position[] _positions;
        private readonly IRenderSurface[] _mapSurfaces = new IRenderSurface[6];

        private readonly CubeMapFBO _cubeFbo;

        private readonly Deferred _deferred;
        private readonly DirectionalShadowMapping _shadowMapping;
        private readonly Lighting _lighting;


        private readonly IShaderProgram _environmentMapProgram;
        private IRenderSurface _outputSurface;



        private readonly IShaderProgram _debugShader;

        public EnvironmentMap(int size, IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes)
        {
            _positions = new Position[]
            {
                new Position(Vector3.Zero,-90, 180), //right
                new Position(Vector3.Zero,90, 180), //left
                new Position(Vector3.Zero,0, -90), //up
                new Position(Vector3.Zero,0, 90), //down
                new Position(Vector3.Zero,0, 180), //back
                new Position(Vector3.Zero,180, 180) //front
            };

            for (int i = 0; i < 6; i++)
            {
                _cameras[i] = new Camera<Position, Perspective>(_positions[i], new Perspective(farClip: 500f));
                _mapSurfaces[i] = new FBOwithDepth(Texture2dGL.Create(size, size));
            }

            _cubeFbo = new CubeMapFBO(size);

            _deferred = new Deferred(contentLoader, meshes);
            _deferred.UpdateResolution(size, size);
            _shadowMapping = new DirectionalShadowMapping(contentLoader, meshes);
            _shadowMapping.UpdateResolution(size, size);
            _lighting = new Lighting(contentLoader);
            _lighting.UpdateResolution(size, size);

            _environmentMapProgram = contentLoader.Load<IShaderProgram>("environmentMap.*");

            _debugShader = contentLoader.Load<IShaderProgram>("debugCubeMap.*");
        }

        public void CreateMap(Vector3 position, IRenderState renderState, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> textures, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<LightSource> lightSources, Vector3 ambientColor, ITransformation camera)
        {

            for (int i = 0; i < 6; i++)
            {
                _positions[i].Location = position;
                _deferred.Draw(renderState, _cameras[i], instanceCounts, textures, normalMaps, heightMaps, new List<Enums.EntityType>());
                _shadowMapping.Draw(renderState, _cameras[i], instanceCounts, _deferred.Depth, lightSources[0].Direction);
                _lighting.Draw(_cameras[i], _deferred.Color, _deferred.Normals, _deferred.Position, _shadowMapping.Output, lightSources, ambientColor, _mapSurfaces[i]);
            }

            _cubeFbo.Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, _cubeFbo.Texture.ID, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                TextureDrawer.Draw(_mapSurfaces[i].Texture);
            }
            _cubeFbo.Deactivate();
        }

        public void Draw(Matrix4x4 transform, IRenderState renderState, DefaultMesh mesh, ITransformation camera, ITexture2D depth)
        {
            VAO geometry = VAOLoader.FromMesh(mesh, _environmentMapProgram);

            geometry.SetAttribute(_environmentMapProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), new Matrix4x4[1] { transform }, true);

            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            _outputSurface.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _environmentMapProgram.Activate();
            _environmentMapProgram.ActivateTexture("cubeMap", 0, _cubeFbo.Texture);
            _environmentMapProgram.ActivateTexture("depth", 1, depth);
            _environmentMapProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _environmentMapProgram.Uniform("camPos", invert.Translation / invert.M44);

            geometry.Draw();

            _environmentMapProgram.DeactivateTexture(0, _cubeFbo.Texture);
            _environmentMapProgram.DeactivateTexture(1, depth);
            _environmentMapProgram.Deactivate();

            _outputSurface.Deactivate();
            GL.ClearColor(Color.FromArgb(0, 0, 0, 1));
        }

        public void DrawCubeMap(ITransformation camera)
        {
            var cube = Meshes.CreateSphere(20, 5).SwitchHandedness();
            VAO geom = VAOLoader.FromMesh(cube, _debugShader);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _debugShader.Activate();


            _cubeFbo.Texture.Activate();

            _debugShader.Uniform("camera", camera);
            geom.Draw();
            _cubeFbo.Texture.Deactivate();

            _debugShader.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _outputSurface = new FBO(Texture2dGL.Create(width, height));

            _environmentMapProgram.Activate();
            _environmentMapProgram.Uniform("iResolution", new Vector2(width, height));
            _environmentMapProgram.Deactivate();
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            _deferred.UpdateTransforms(transforms);
            _shadowMapping.UpdateTransforms(transforms);
        }
    }
}
