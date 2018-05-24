using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
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

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private readonly List<IRenderSurface> _renderSurfaces = new List<IRenderSurface>();
        private readonly List<IShaderProgram> _postProcessShaders = new List<IShaderProgram>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new BackFaceCulling(true));

            _shaderProgram = contentLoader.Load<IShaderProgram>("shader.*");

            //var mesh = contentLoader.Load<DefaultMesh>("suzanne");
            var mesh = Meshes.CreateSphere();

            _geometries.Add(Enums.EntityType.Type1, VAOLoader.FromMesh(mesh, _shaderProgram));
            _geometries.Add(Enums.EntityType.Type2, VAOLoader.FromMesh(mesh, _shaderProgram));
            _postProcessShaders.Add(contentLoader.LoadPixelShader("vignette"));
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

            if (_postProcessShaders.Count > 0)
            {
                _renderSurfaces[0].Activate();
            }

            DrawGeometry(time, camera);

            if (_postProcessShaders.Count > 0)
            {
                _renderSurfaces[0].Deactivate();
                ApplyPostProcessing(time);
            }
        }

        public void Resize(int width, int height)
        {
            _renderSurfaces.Clear();
            foreach (var shader in _postProcessShaders)
            {
                _renderSurfaces.Add(new FBOwithDepth(Texture2dGL.Create(width, height)));
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
            _renderState.Set(new DepthTest(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shaderProgram.Activate();
            _shaderProgram.Uniform("time", time);
            _shaderProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _shaderProgram.Uniform("camPos", invert.Translation / invert.M44);
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].Draw(_instanceCounts[type]);
            }
            _shaderProgram.Deactivate();

            _renderState.Set(new DepthTest(false));
        }

        private void ApplyPostProcessing(float time)
        {
            for (int i = 0; i < _postProcessShaders.Count - 1; i++)
            {
                _renderSurfaces[i + 1].Activate();
                _renderSurfaces[i].Texture.Activate();

                _postProcessShaders[i].Activate(); //activate post processing shader
                _postProcessShaders[i].Uniform("iGlobalTime", time);
                GL.DrawArrays(PrimitiveType.Quads, 0, 4); //draw quad
                _postProcessShaders[i].Deactivate();

                _renderSurfaces[i].Texture.Deactivate();
                _renderSurfaces[i + 1].Deactivate();
            }

            _renderSurfaces[_postProcessShaders.Count - 1].Texture.Activate();

            _postProcessShaders[_postProcessShaders.Count - 1].Activate(); //activate post processing shader
            _postProcessShaders[_postProcessShaders.Count - 1].Uniform("iGlobalTime", time);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4); //draw quad
            _postProcessShaders[_postProcessShaders.Count - 1].Deactivate();

            _renderSurfaces[_postProcessShaders.Count - 1].Texture.Deactivate();
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
