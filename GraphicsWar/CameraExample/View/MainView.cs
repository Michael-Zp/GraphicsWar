using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.ExtensionMethods;
using Zenseless.Geometry;
using Zenseless.HLGL;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Instances;
using GraphicsWar.View.Rendering.Management;

namespace GraphicsWar.View
{
    public class MainView
    {
        private readonly IRenderState _renderState;

        private readonly Dictionary<Enums.EntityType, ITexture2D> _textures = new Dictionary<Enums.EntityType, ITexture2D>();
        private readonly Dictionary<Enums.EntityType, ITexture2D> _normalMaps = new Dictionary<Enums.EntityType, ITexture2D>();
        private readonly Dictionary<Enums.EntityType, ITexture2D> _heightMaps = new Dictionary<Enums.EntityType, ITexture2D>();
        private readonly Dictionary<Enums.EntityType, DefaultMesh> _meshes = new Dictionary<Enums.EntityType, DefaultMesh>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private readonly RenderInstanceGroup _renderInstanceGroup = new RenderInstanceGroup();
        private readonly Deferred _deferred;
        private readonly DirectionalShadowMapping _directShadowMap;
        private readonly SSAOWithBlur _ssaoWithBlur;
        private readonly Lighting _lighting;
        private readonly EnvironmentMap _environmentMap;

        private readonly List<LightSource> _lights = new List<LightSource>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            //_renderState.Set(new BackFaceCulling(true));

            _meshes.Add(Enums.EntityType.Type1, Meshes.CreateSphere(subdivision: 0));
            _meshes.Add(Enums.EntityType.Type2, Meshes.CreateCornellBox());
            _meshes.Add(Enums.EntityType.Type3, new TBNMesh(Meshes.CreatePlane(2, 2, 10, 10)));
            _meshes.Add(Enums.EntityType.Type4, new TBNMesh(Meshes.CreatePlane(2, 2, 10, 10)));
            _meshes.Add(Enums.EntityType.Nvidia, contentLoader.Load<DefaultMesh>("Nvidia.obj"));
            _meshes.Add(Enums.EntityType.Radeon, contentLoader.Load<DefaultMesh>("Radeon.obj"));

            _textures.Add(Enums.EntityType.Nvidia, contentLoader.Load<ITexture2D>("Nvidia.png"));
            _textures.Add(Enums.EntityType.Radeon, contentLoader.Load<ITexture2D>("Radeon.png"));

            _normalMaps.Add(Enums.EntityType.Type3, contentLoader.Load<ITexture2D>("n3.png"));
            _normalMaps.Add(Enums.EntityType.Type4, contentLoader.Load<ITexture2D>("n3.png"));
            _heightMaps.Add(Enums.EntityType.Type4, contentLoader.Load<ITexture2D>("h3.jpg"));

            _deferred = _renderInstanceGroup.AddShader<Deferred>(new Deferred(contentLoader, _meshes));
            _directShadowMap = _renderInstanceGroup.AddShader<DirectionalShadowMapping>(new DirectionalShadowMapping(contentLoader, _meshes));
            _ssaoWithBlur = _renderInstanceGroup.AddShader<SSAOWithBlur>(new SSAOWithBlur(contentLoader, 15));
            _lighting = _renderInstanceGroup.AddShader<Lighting>(new Lighting(contentLoader));
            _environmentMap = _renderInstanceGroup.AddShader<EnvironmentMap>(new EnvironmentMap(1024, contentLoader, _meshes));

            _lights.Add(new LightSource(Vector3.Zero, new Vector3(0f, -1f, 0f), Vector3.One, 1));
        }

        public void Render(IEnumerable<ViewEntity> entities, float time, ITransformation camera)
        {
            UpdateInstancing(entities);

            _renderInstanceGroup.UpdateGeometry(_transforms);

            _deferred.Draw(_renderState, camera, _instanceCounts, _textures, _normalMaps, _heightMaps);

            _directShadowMap.Draw(_renderState, camera, _instanceCounts, _deferred.Depth, _lights[0].Direction);

            _lighting.Draw(camera, _deferred.Color, _deferred.Normals, _deferred.Position, _directShadowMap.Output, _lights, new Vector3(0.2f, 0.2f, 0.2f));

            _ssaoWithBlur.Draw(_deferred.Depth, _lighting.Output);

            _environmentMap.CreateMap(Vector3.UnitY * 0.1f, _renderState, _instanceCounts, _textures, _normalMaps, _heightMaps, _lights, new Vector3(0.2f, 0.2f, 0.2f), camera);

            TextureDrawer.Draw(_deferred.Color);
            //TextureDrawer.Draw(_lighting.Output);

            //_environmentMap.DrawCubeMap(camera);
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
