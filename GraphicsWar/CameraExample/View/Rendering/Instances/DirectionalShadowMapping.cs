using System;
using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class DirectionalShadowMapping : IUpdateTransforms, IUpdateResolution
    {
        private readonly IShaderProgram _depthShader;
        private readonly IShaderProgram _shadowShader;
        private IRenderSurface _depthSurface;
        private IRenderSurface _outputSurface;

        private readonly Dictionary<Enums.EntityType, VAO> _geometriesDepth = new Dictionary<Enums.EntityType, VAO>();
        private readonly Dictionary<Enums.EntityType, VAO> _geometriesShadow = new Dictionary<Enums.EntityType, VAO>();

        public ITexture2D Output => _outputSurface.Texture;

        public DirectionalShadowMapping(IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes)
        {
            _depthShader = contentLoader.Load<IShaderProgram>("depth.*");
            _shadowShader = contentLoader.Load<IShaderProgram>("shadow.*");

            foreach (var meshContainer in meshes)
            {
                _geometriesDepth.Add(meshContainer.Key, VAOLoader.FromMesh(meshContainer.Value, _depthShader));
            }

            foreach (var meshContainer in meshes)
            {
                _geometriesShadow.Add(meshContainer.Key, VAOLoader.FromMesh(meshContainer.Value, _shadowShader));
            }
        }

        public void UpdateResolution(int width, int height)
        {
            ((FBO)_depthSurface)?.Dispose();
            _depthSurface = new FBOwithDepth(Texture2dGL.Create(width * 4, height * 4, 1, true));
            ((FBO)_outputSurface)?.Dispose();
            _outputSurface = new FBOwithDepth(Texture2dGL.Create(width, height, 1));

            _depthShader.Uniform("iResolution", new Vector2(width, height));
            _shadowShader.Uniform("iResolution", new Vector2(width, height));
        }

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, ITexture2D sceneDepth, Vector3 lightDirection, List<Enums.EntityType> disableBackFaceCulling)
        {
            renderState.Set(new DepthTest(true));

            lightDirection = Vector3.Normalize(-lightDirection);
            lightDirection.X *= -1;
            var azimuth = Math.Atan2(lightDirection.X, lightDirection.Z) - Math.Atan2(0, 1);
            var basedVector = new Vector3(lightDirection.X, 0, lightDirection.Z);
            if (basedVector != Vector3.Zero)
            {
                basedVector = (Vector3.Normalize(basedVector));
            }
            var elevation = Math.Acos(Vector3.Dot(lightDirection, basedVector));
            ITransformation lightCamera = new Camera<Orbit, Ortographic>(new Orbit(6, MathHelper.RadiansToDegrees((float)azimuth), MathHelper.RadiansToDegrees((float)elevation)), new Ortographic(10, 10));

            DrawDepthSurface(renderState, lightCamera, instanceCounts, disableBackFaceCulling);


            lightDirection.X *= -1;

            DrawShadowSurface(renderState, lightDirection, lightCamera, camera, instanceCounts, disableBackFaceCulling);

            renderState.Set(new DepthTest(false));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            foreach (var type in _geometriesDepth.Keys)
            {
                _geometriesDepth[type].SetAttribute(_depthShader.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type], true);
            }

            foreach (var type in _geometriesShadow.Keys)
            {
                _geometriesShadow[type].SetAttribute(_shadowShader.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type], true);
            }
        }

        private void DrawDepthSurface(IRenderState renderState, ITransformation lightCamera, Dictionary<Enums.EntityType, int> instanceCounts, List<Enums.EntityType> disableBackFaceCulling)
        {
            _depthSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 10000 });

            _depthShader.Activate();
            _depthShader.Uniform("camera", lightCamera);

            foreach (var type in _geometriesDepth.Keys)
            {
                if (disableBackFaceCulling.Contains(type))
                {
                    renderState.Set(new BackFaceCulling(false));
                }
                _geometriesDepth[type].Draw(instanceCounts[type]);
                renderState.Set(new BackFaceCulling(true));
            }

            _depthShader.Deactivate();

            _depthSurface.Deactivate();
        }

        private void DrawShadowSurface(IRenderState renderState, Vector3 lightDirection, ITransformation lightCamera, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, List<Enums.EntityType> disableBackFaceCulling)
        {
            _outputSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shadowShader.Activate();

            _depthSurface.Texture.Activate();
            _shadowShader.Uniform("lightDirection", lightDirection);
            _shadowShader.Uniform("lightCamera", lightCamera);
            _shadowShader.Uniform("camera", camera);

            GL.Uniform1(_shadowShader.GetResourceLocation(ShaderResourceType.Uniform, "lightDepth"), 0);

            foreach (var type in _geometriesDepth.Keys)
            {
                if (disableBackFaceCulling.Contains(type))
                {
                    renderState.Set(new BackFaceCulling(false));
                }
                _geometriesShadow[type].Draw(instanceCounts[type]);
                renderState.Set(new BackFaceCulling(true));
            }

            _depthSurface.Texture.Deactivate();

            _shadowShader.Deactivate();

            _outputSurface.Deactivate();
        }
    }
}
