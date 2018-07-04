﻿using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using GraphicsWar.View.RenderInstances;
using GraphicsWar.Shared;
using System.Drawing;

namespace GraphicsWar.View
{
    public class MainView
    {
        private readonly IRenderState _renderState;

        private readonly Dictionary<Enums.EntityType, Mesh> _meshes = new Dictionary<Enums.EntityType, Mesh>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private RenderInstanceGroup _renderInstanceGroup = new RenderInstanceGroup();
        private Deferred _deferred;
        private DirectionalShadowMapping _directShadowMap;
        private OnePassPostProcessShader _copy;
        private OnePassPostProcessShader _ssao;
        private TwoPassPostProcessShader _bloom;
        private TwoPassPostProcessShader _blur;
        private SSAOWithBlur _ssaoWithBlur;
        private DeferredLighting _deferredLighting;

        private List<LightSource> _lights = new List<LightSource>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new FaceCullingModeState(FaceCullingMode.BACK_SIDE));

            _meshes.Add(Enums.EntityType.Type1, Meshes.CreateSphere(subdivision: 5));
            _meshes.Add(Enums.EntityType.Type2, Meshes.CreateSphere(subdivision: 5));
            //_meshes.Add(Enums.EntityType.Type2, Meshes.CreateCornellBox());

            _deferred = _renderInstanceGroup.AddShader<Deferred>(new Deferred(contentLoader, _meshes));
            _directShadowMap = _renderInstanceGroup.AddShader<DirectionalShadowMapping>(new DirectionalShadowMapping(contentLoader, _meshes));
            _copy = _renderInstanceGroup.AddShader<OnePassPostProcessShader>(new OnePassPostProcessShader(contentLoader.LoadPixelShader("Copy.frag")));
            _ssao = _renderInstanceGroup.AddShader<OnePassPostProcessShader>(new OnePassPostProcessShader(contentLoader.LoadPixelShader("SSAO.glsl")));
            _blur = _renderInstanceGroup.AddShader<TwoPassPostProcessShader>(new TwoPassPostProcessShader(contentLoader.LoadPixelShader("BlurGausPass1"), contentLoader.LoadPixelShader("BlurGausPass2")));
            _bloom = _renderInstanceGroup.AddShader<TwoPassPostProcessShader>(new TwoPassPostProcessShader(contentLoader.LoadPixelShader("BloomGausPass1"), contentLoader.LoadPixelShader("BloomGausPass2")));
            _ssaoWithBlur = _renderInstanceGroup.AddShader<SSAOWithBlur>(new SSAOWithBlur(contentLoader, 15));
            _deferredLighting = _renderInstanceGroup.AddShader<DeferredLighting>(new DeferredLighting(contentLoader.LoadPixelShader("deferredLighting.glsl")));

            _lights.Add(new LightSource(Vector3.Zero, new Vector3(0f, -1f, 0f), Vector3.One, 1));
        }

        public void Render(IEnumerable<ViewEntity> entities, float time, ITransformation camera)
        {
            UpdateInstancing(entities);
            
            _renderInstanceGroup.UpdateGeometry(_transforms);

            _deferred.Draw(_renderState, camera, _instanceCounts);
            
            _directShadowMap.Draw(_renderState, _instanceCounts, _deferred.Depth, _lights[0].Direction, camera);

            _ssao.Draw(_deferred.Depth);

            _ssaoWithBlur.Draw(_deferred.Depth);
            
            _deferredLighting.Draw(camera, _deferred.Color, _deferred.Normals, _deferred.Position, _directShadowMap.ShadowSurface, _lights, new Vector3(0.2f, 0.2f, 0.2f));

            TextureDebugger.Draw(_deferredLighting.Output);
        }

        public void Resize(int width, int height)
        {
            _renderInstanceGroup.UpdateResolution(width, height);
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
