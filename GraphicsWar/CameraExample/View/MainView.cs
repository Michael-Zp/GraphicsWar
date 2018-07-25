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
        private readonly List<Enums.EntityType> _disableBackFaceCulling = new List<Enums.EntityType>();
        private readonly Dictionary<Enums.EntityType, DefaultMesh> _meshes = new Dictionary<Enums.EntityType, DefaultMesh>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        private readonly RenderInstanceGroup _renderInstanceGroup = new RenderInstanceGroup();
        private readonly Deferred _deferred;
        private readonly DirectionalShadowMapping _directShadowMap;
        private readonly SSAOWithBlur _ssaoWithBlur;
        private readonly Lighting _lighting;
        private readonly EnvironmentMap _environmentMap;
        private readonly Add _add;
        private readonly Add _add2;

        private readonly List<LightSource> _lights = new List<LightSource>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new BackFaceCulling(true));

            _meshes.Add(Enums.EntityType.Sphere, Meshes.CreateSphere(subdivision: 5));
            _meshes.Add(Enums.EntityType.CornellBox, Meshes.CreateCornellBox());
            _meshes.Add(Enums.EntityType.Type3, new TBNMesh(Meshes.CreatePlane(2, 2, 10, 10)));
            _meshes.Add(Enums.EntityType.Type4, new TBNMesh(Meshes.CreatePlane(2, 2, 10, 10)));
            _meshes.Add(Enums.EntityType.Nvidia, contentLoader.Load<DefaultMesh>("Nvidia.obj"));
            _meshes.Add(Enums.EntityType.Radeon, contentLoader.Load<DefaultMesh>("Radeon.obj"));

            _disableBackFaceCulling.Add(Enums.EntityType.Nvidia);
            _disableBackFaceCulling.Add(Enums.EntityType.Radeon);

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
            _add = _renderInstanceGroup.AddShader<Add>(new Add(contentLoader));
            _add2 = _renderInstanceGroup.AddShader<Add>(new Add(contentLoader));

            _lights.Add(new LightSource(Vector3.Zero, new Vector3(0f, -1f, 0f), Vector3.One, 1));
        }

        public void Render(List<ViewEntity> entities, float time, ITransformation camera)
        {
            UpdateInstancing(entities);

            _renderInstanceGroup.UpdateGeometry(_transforms);

            _deferred.Draw(_renderState, camera, _instanceCounts, _textures, _normalMaps, _heightMaps, _disableBackFaceCulling);

            _directShadowMap.Draw(_renderState, camera, _instanceCounts, _deferred.Depth, _lights[0].Direction);

            _lighting.Draw(camera, _deferred.Color, _deferred.Normals, _deferred.Position, _directShadowMap.Output, _lights, new Vector3(0.2f, 0.2f, 0.2f));

            _ssaoWithBlur.Draw(_deferred.Depth, _lighting.Output);

            Vector3 mapPos = new Vector3(entities[2].Transform.M41, entities[2].Transform.M42,
                entities[2].Transform.M43);
            _environmentMap.CreateMap(mapPos, _renderState, _instanceCounts, _textures, _normalMaps, _heightMaps, _lights, new Vector3(0.2f, 0.2f, 0.2f), camera);

            _environmentMap.Draw(entities[2].Transform, _renderState, _meshes[entities[2].Type], camera, _deferred.Depth);

            _add.Draw(_deferred.Color, _environmentMap.Output, 0.9f);


            Vector3 mapPos2 = new Vector3(entities[3].Transform.M41, entities[3].Transform.M42,
                entities[3].Transform.M43);
            _environmentMap.CreateMap(mapPos2, _renderState, _instanceCounts, _textures, _normalMaps, _heightMaps, _lights, new Vector3(0.2f, 0.2f, 0.2f), camera);

            _environmentMap.Draw(entities[3].Transform, _renderState, _meshes[entities[3].Type], camera, _deferred.Depth);

            _add2.Draw(_add.Output, _environmentMap.Output, 0.7f);
            
            //TextureDrawer.Draw(_deferred.Depth);
            //TextureDrawer.Draw(_environmentMap.Output);
            TextureDrawer.Draw(_add2.Output);

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
