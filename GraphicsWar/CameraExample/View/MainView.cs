using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using GraphicsWar.View.RenderInstances;
using GraphicsWar.Shared;

namespace GraphicsWar.View
{
    public class MainView
    {
        private readonly IRenderState _renderState;
        private readonly IShaderProgram _copyShaderProgram;

        private readonly Dictionary<Enums.EntityType, Mesh> _meshes = new Dictionary<Enums.EntityType, Mesh>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private readonly List<IRenderSurface> _postProcessingSurfaces = new List<IRenderSurface>();
        private readonly List<IShaderProgram> _postProcessShaders = new List<IShaderProgram>();

        private Vector2 _resolution;

        private RenderInstanceGroup _renderInstanceGroup = new RenderInstanceGroup();
        private Deferred _deferred;
        private DirectionalShadowMapping _directShadowMap;

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            _meshes.Add(Enums.EntityType.Type1,Meshes.CreateSphere());
            _meshes.Add(Enums.EntityType.Type2, Meshes.CreateCornellBox());

            _deferred = new Deferred(contentLoader, _meshes);
            _directShadowMap = new DirectionalShadowMapping(contentLoader, _meshes);

            _renderInstanceGroup.RenderInstances.Add(_deferred);
            _renderInstanceGroup.RenderInstances.Add(_directShadowMap);

            _copyShaderProgram = contentLoader.LoadPixelShader("Copy.frag");

            _postProcessShaders.Add(contentLoader.LoadPixelShader("SSAO"));
        }

        public void Render(IEnumerable<ViewEntity> entities, float time, ITransformation camera)
        {
            foreach (var shader in _postProcessShaders)
            {
                if (shader is null) return;
                shader.Uniform("iGlobalTime", time);
            }

            UpdateInstancing(entities);

            _renderInstanceGroup.UpdateGeometry(_transforms);

            _deferred.Draw(_renderState, camera, _instanceCounts);
            
            _directShadowMap.Draw(_renderState, _instanceCounts, _deferred.Depth, Vector3.Normalize(new Vector3(1f,2f,0f)), camera);

            //if (_postProcessShaders.Count > 0)
            //{
            //    _postProcessingSurfaces[0].Activate();
            //}

            //DrawTexture(_deferred.Color, _copyShaderProgram, time);

            //if (_postProcessShaders.Count > 0)
            //{
            //    _postProcessingSurfaces[0].Deactivate();
            //    ApplyPostProcessing(time);
            //}
        }

        public void Resize(int width, int height)
        {
            _postProcessingSurfaces.Clear();

            _renderInstanceGroup.UpdateResolution(width, height);

            foreach (var shader in _postProcessShaders)
            {
                _postProcessingSurfaces.Add(new FBO(Texture2dGL.Create(width, height)));
                shader.Uniform("iResolution", new Vector2(width, height));
            }

            _resolution = new Vector2(width, height);
        }
        
        private void DrawTexture(ITexture2D texture, IShaderProgram shader, float time)
        {
            texture.Activate();

            shader.Activate(); //activate post processing shader
            GL.DrawArrays(PrimitiveType.Quads, 0, 4); //draw quad
            shader.Deactivate();

            texture.Deactivate();
        }

        private void DrawTextures(Dictionary<string, ITexture2D> namedTextures, IShaderProgram shader, float time, Vector2 resolution)
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
            namedTextures.Add("normal", _deferred.Normals);
            namedTextures.Add("depth", _deferred.Depth);

            for (int i = 0; i < _postProcessShaders.Count - 1; i++)
            {
                namedTextures["color"] = _postProcessingSurfaces[i].Texture;

                _postProcessingSurfaces[i + 1].Activate();

                DrawTextures(namedTextures, _postProcessShaders[i], time, new Vector2(_postProcessingSurfaces[i + 1].Texture.Width, _postProcessingSurfaces[i + 1].Texture.Height));

                _postProcessingSurfaces[i + 1].Deactivate();
            }

            namedTextures["color"] = _postProcessingSurfaces[_postProcessShaders.Count - 1].Texture;
            DrawTextures(namedTextures, _postProcessShaders[_postProcessShaders.Count - 1], time, _resolution);
        }

        private void UpdateInstancing(IEnumerable<ViewEntity> entities)
        {
            _transforms.Clear();
            _instanceCounts.Clear();

            foreach (var type in _meshes.Keys)
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
    }
}
