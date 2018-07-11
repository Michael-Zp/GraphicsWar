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
        private readonly ITransformation[] _cameras = new ITransformation[6];
        private readonly Position[] _positions = new Position[6];
        private readonly IRenderSurface[] _mapSurfaces = new IRenderSurface[6];

        private readonly Deferred _deferred;
        private readonly DirectionalShadowMapping _shadowMapping;
        private readonly Lighting _lighting;

        private readonly IShaderProgram _environmentMappingProgram;
        private IRenderSurface _outputSurface;


        private CubeMapFBO _cubeFbo;
        private IShaderProgram _testCubeMapProgram;
        private IShaderProgram _debugShader;

        public EnvironmentMap(int size, IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes, ICollection<Enums.EntityType> normalMapped, ICollection<Enums.EntityType> heightMapped)
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

            _deferred = new Deferred(contentLoader, meshes, normalMapped, heightMapped);
            _deferred.UpdateResolution(size, size);
            _shadowMapping = new DirectionalShadowMapping(contentLoader, meshes);
            _shadowMapping.UpdateResolution(size, size);
            _lighting = new Lighting(contentLoader);
            _lighting.UpdateResolution(size, size);

            _environmentMappingProgram = contentLoader.LoadPixelShader("environmentMapping.glsl");

            _cubeFbo = new CubeMapFBO(1024);
            _testCubeMapProgram = contentLoader.LoadPixelShader("testCubeMap.glsl");
            _debugShader = contentLoader.Load<IShaderProgram>("debugCubeMap.*");

            //CreateMap();
        }

        public void CreateMap(Vector3 position, IRenderState renderState, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<LightSource> lightSources, Vector3 ambientColor, ITransformation camera)
        {

            for (int i = 0; i < 6; i++)
            {
                _positions[i].Location = position;
                _deferred.Draw(renderState, _cameras[i], instanceCounts, normalMaps, heightMaps);
                _shadowMapping.Draw(renderState, _cameras[i], instanceCounts, _deferred.Depth, lightSources[0].Direction);
                _lighting.Draw(_cameras[i], _deferred.Color, _deferred.Normals, _deferred.Position, _shadowMapping.Output, lightSources, ambientColor, _mapSurfaces[i]);
            }

            //TextureDrawer.Draw(_deferred.Color);

            _cubeFbo.Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, _cubeFbo.Texture.ID, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                TextureDrawer.Draw(_mapSurfaces[i].Texture);
            }
            _cubeFbo.Deactivate();
        }

        public void DrawCubeMap(ITransformation camera)
        {
            var cube = Meshes.CreateSphere(20, 5).SwitchHandedness();
            VAO geom = VAOLoader.FromMesh(cube, _debugShader);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _debugShader.Activate();


            _cubeFbo.Texture.Activate();

            _debugShader.Uniform("camera", camera);
            geom.Draw();
            _cubeFbo.Texture.Deactivate();

            _debugShader.Deactivate();
        }

        public void Draw(ITexture2D normal, ITexture2D position)
        {
            _outputSurface.Activate();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _environmentMappingProgram.Activate();

            _environmentMappingProgram.ActivateTexture("normal", 0, normal);
            _environmentMappingProgram.ActivateTexture("position", 1, position);

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _environmentMappingProgram.DeactivateTexture(1, position);
            _environmentMappingProgram.DeactivateTexture(0, normal);

            _environmentMappingProgram.Deactivate();
            _outputSurface.Deactivate();
        }

        public void UpdateResolution(int width, int height)
        {
            _outputSurface = new FBO(Texture2dGL.Create(width, height));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            _deferred.UpdateTransforms(transforms);
            _shadowMapping.UpdateTransforms(transforms);
        }
    }
}
