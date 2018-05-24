using System;
using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View
{
    public class MainView
    {
        private readonly IRenderState _renderState;
        private readonly IShaderProgram _shaderProgram;
        private readonly IShaderProgram _copyShaderProgram;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private IRenderSurface _deferredSurface;
        private readonly List<IRenderSurface> _postProcessingSurfaces = new List<IRenderSurface>();
        private readonly List<IShaderProgram> _postProcessShaders = new List<IShaderProgram>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new BackFaceCulling(true));

            _shaderProgram = contentLoader.Load<IShaderProgram>("shader.*");
            _copyShaderProgram = contentLoader.LoadPixelShader("Copy.frag");

            //var mesh = contentLoader.Load<DefaultMesh>("suzanne");
            var mesh = Meshes.CreateSphere();

            _geometries.Add(Enums.EntityType.Type1, VAOLoader.FromMesh(mesh, _shaderProgram));
            _geometries.Add(Enums.EntityType.Type2, VAOLoader.FromMesh(mesh, _shaderProgram));
            _postProcessShaders.Add(contentLoader.LoadPixelShader("normal"));
        }

        public void Render(IEnumerable<ViewEntity> entities, float time, ITransformation camera)
        {
            if (_shaderProgram is null) return;
            foreach (var shader in _postProcessShaders)
            {
                if (shader is null) return;
            }

            UpdateInstancing(entities);
            UpdateAttributes();

            DrawGeometry(time, camera);

            if (_postProcessShaders.Count > 0)
            {
                _postProcessingSurfaces[0].Activate();
            }

            DrawTexture(_deferredSurface.Texture, _copyShaderProgram, time);

            if (_postProcessShaders.Count > 0)
            {
                _postProcessingSurfaces[0].Deactivate();
                ApplyPostProcessing(time);
            }
        }

        public void Resize(int width, int height)
        {
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _postProcessingSurfaces.Clear();
            foreach (var shader in _postProcessShaders)
            {
                _postProcessingSurfaces.Add(new FBO(Texture2dGL.Create(width, height)));
            }
        }

        private void UpdateInstancing(IEnumerable<ViewEntity> entities)
        {
            _transforms.Clear();
            _instanceCounts.Clear();

            foreach (var type in _geometries.Keys)
            {
                _instanceCounts.Add(type, 0);
                _transforms.Add(type, new List<Matrix4x4>());
            }

            foreach (var entity in entities)
            {
                _instanceCounts[entity.Type]++;
                _transforms[entity.Type].Add(entity.Transform);
            }
        }

        private void DrawGeometry(float time, ITransformation camera)
        {
            _deferredSurface.Activate();
            _renderState.Set(new DepthTest(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shaderProgram.Activate();
            _shaderProgram.Uniform("time", time);
            _shaderProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _shaderProgram.Uniform("camPos", invert.Translation / invert.M44);
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].Draw(_instanceCounts[type]);
            }
            _shaderProgram.Deactivate();

            _renderState.Set(new DepthTest(false));
            _deferredSurface.Deactivate();
        }

        private void DrawTexture(ITexture2D texture, IShaderProgram shader, float time)
        {
            texture.Activate();

            shader.Activate(); //activate post processing shader
            shader.Uniform("iGlobalTime", time);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4); //draw quad
            shader.Deactivate();

            texture.Deactivate();
        }

        private void DrawTextures(Dictionary<string, ITexture2D> namedTextures, IShaderProgram shader, float time)
        {
            var textures = namedTextures.Values.ToArray();
            var names = namedTextures.Keys.ToArray();

            shader.Activate(); //activate post processing shader

            for (int i = 0; i < namedTextures.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                textures[i].Activate();
                GL.Uniform1(shader.GetResourceLocation(ShaderResourceType.Uniform, names[i]), i);
            }

            shader.Uniform("iGlobalTime", time);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4); //draw quad
            shader.Deactivate();

            foreach (var texture in textures)
            {
                texture.Deactivate();
            }
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void ApplyPostProcessing(float time)
        {
            Dictionary<string, ITexture2D> namedTextures = new Dictionary<string, ITexture2D>();
            namedTextures.Add("color", null);
            namedTextures.Add("normal", _deferredSurface.Textures[1]);
            namedTextures.Add("depth", _deferredSurface.Textures[2]);

            for (int i = 0; i < _postProcessShaders.Count - 1; i++)
            {
                namedTextures["color"] = _postProcessingSurfaces[i].Texture;

                _postProcessingSurfaces[i + 1].Activate();

                DrawTextures(namedTextures, _postProcessShaders[i], time);

                _postProcessingSurfaces[i + 1].Deactivate();
            }

            namedTextures["color"] = _postProcessingSurfaces[_postProcessShaders.Count - 1].Texture;
            DrawTextures(namedTextures, _postProcessShaders[_postProcessShaders.Count - 1], time);
        }

        private void UpdateAttributes()
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_shaderProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), _transforms[type].ToArray(), true);
            }
        }
    }
}
