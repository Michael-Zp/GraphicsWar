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
    public class EnvironmentMap : IUpdateResolution, IUpdateTransforms
    {
        private Dictionary<Enums.EntityType, DefaultMesh> _meshes;
        private ViewEntity _entity;
        private ITransformation _camera;

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
            _meshes = meshes;

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

        public void CreateMap(ViewEntity entity, IRenderState renderState, int index, Dictionary<Enums.EntityType, List<Matrix4x4>> transforms, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> textures, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<Enums.EntityType> disableBackFaceCulling, List<LightSource> lightSources, Vector3 ambientColor, ITransformation camera)
        {
            _entity = entity;
            _camera = camera;

            Vector3 position = new Vector3(entity.Transform.M41, entity.Transform.M42,
                entity.Transform.M43);
            transforms[entity.Type].Remove(entity.Transform);
            instanceCounts[entity.Type]--;

            _deferred.UpdateTransforms(transforms);

            for (int i = 0; i < 6; i++)
            {
                _positions[i].Location = position;
                _deferred.Draw(renderState, _cameras[i], instanceCounts, textures, normalMaps, heightMaps, disableBackFaceCulling);
                _shadowMapping.Draw(renderState, _cameras[i], instanceCounts, _deferred.Depth, lightSources[0].Direction);
                _lighting.Draw(_cameras[i], _deferred.Color, _deferred.Normal, _deferred.Position, _shadowMapping.Output, lightSources, ambientColor, _mapSurfaces[i]);
            }

            transforms[entity.Type].Insert(index, entity.Transform);
            instanceCounts[entity.Type]++;

            _cubeFbo.Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, _cubeFbo.Texture.ID, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                TextureDrawer.Draw(_mapSurfaces[i].Texture);
            }
            _cubeFbo.Deactivate();

            _cubeFbo.Texture.Activate();
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
            _cubeFbo.Texture.Deactivate();
        }

        public void Draw(IRenderState renderState, ITexture2D depth, float mipmapLevel)
        {
            VAO geometry = VAOLoader.FromMesh(_meshes[_entity.Type], _environmentMapProgram);

            geometry.SetAttribute(_environmentMapProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), new Matrix4x4[1] { _entity.Transform }, true);

            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            _outputSurface.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _environmentMapProgram.Activate();
            _environmentMapProgram.ActivateTexture("cubeMap", 0, _cubeFbo.Texture);
            _environmentMapProgram.ActivateTexture("depth", 1, depth);
            _environmentMapProgram.Uniform("camera", _camera);
            Matrix4x4.Invert(_camera.Matrix, out var invert);
            _environmentMapProgram.Uniform("camPos", invert.Translation / invert.M44);
            _environmentMapProgram.Uniform("mipmapLevel", mipmapLevel);

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
            _shadowMapping.UpdateTransforms(transforms);
        }
    }
}
