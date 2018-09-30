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
        private readonly ShadowBlur _blurredShadowMap;
        private readonly SSAOWithBlur _ssaoWithBlur;
        private readonly EnvironmentMap _environmentMap;
        private readonly Add _addEnvMap;
        private readonly Lighting _lighting;
        private readonly AddWithDepthTest _addProjectileColor;
        private readonly SphereCut _sphereCut;
        private readonly Skybox _skybox;
        private readonly Add _addSkybox;

        private readonly List<LightSource> _lights = new List<LightSource>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            _renderState = renderState;
            _renderState.Set(new BackFaceCulling(true));

            _meshes.Add(Enums.EntityType.Sphere, new TBNMesh(contentLoader.Load<DefaultMesh>("sphereBlocky.obj")));
            _meshes.Add(Enums.EntityType.FluidSphere, Meshes.CreateSphere(1, 1));
            _meshes.Add(Enums.EntityType.Nvidia, contentLoader.Load<DefaultMesh>("Nvidia.obj"));
            _meshes.Add(Enums.EntityType.Radeon, contentLoader.Load<DefaultMesh>("Radeon.obj"));
            _meshes.Add(Enums.EntityType.Triangle, CustomMeshes.CreateTriangle());

            _normalMaps.Add(Enums.EntityType.Sphere, contentLoader.Load<ITexture2D>("sphereBlockyNormals.png"));

            _disableBackFaceCulling.Add(Enums.EntityType.Nvidia);
            _disableBackFaceCulling.Add(Enums.EntityType.Radeon);

            _textures.Add(Enums.EntityType.Nvidia, contentLoader.Load<ITexture2D>("Nvidia.png"));
            _textures.Add(Enums.EntityType.Radeon, contentLoader.Load<ITexture2D>("Radeon.png"));

            _deferred = _renderInstanceGroup.AddShader<Deferred>(new Deferred(contentLoader, _meshes));
            _directShadowMap = _renderInstanceGroup.AddShader<DirectionalShadowMapping>(new DirectionalShadowMapping(contentLoader, _meshes));
            _blurredShadowMap = _renderInstanceGroup.AddShader<ShadowBlur>(new ShadowBlur(contentLoader, 5));
            _ssaoWithBlur = _renderInstanceGroup.AddShader<SSAOWithBlur>(new SSAOWithBlur(contentLoader, 15));
            _environmentMap = _renderInstanceGroup.AddShader<EnvironmentMap>(new EnvironmentMap(1024, contentLoader, _meshes));
            _addEnvMap = _renderInstanceGroup.AddShader<Add>(new Add(contentLoader));
            _lighting = _renderInstanceGroup.AddShader<Lighting>(new Lighting(contentLoader));
            _addProjectileColor = _renderInstanceGroup.AddShader<AddWithDepthTest>(new AddWithDepthTest(contentLoader));
            _sphereCut = _renderInstanceGroup.AddShader<SphereCut>(new SphereCut(contentLoader, 100));
            _skybox = _renderInstanceGroup.AddShader<Skybox>(new Skybox(contentLoader, 100, "violentdays"));
            _addSkybox = _renderInstanceGroup.AddShader<Add>(new Add(contentLoader));


            _lights.Add(new LightSource(Vector3.Zero, Vector3.Normalize(new Vector3(-1f, -1f, 0.6f)), Vector3.One));

        }

        public void Render(List<ViewEntity> entities, float time, ITransformation camera)
        {
            UpdateInstancing(entities);

            var arrTrans = new Dictionary<Enums.EntityType, Matrix4x4[]>();

            foreach (var transform in _transforms)
            {
                arrTrans.Add(transform.Key, transform.Value.ToArray());
            }

            _renderInstanceGroup.UpdateGeometry(arrTrans);

            _deferred.Draw(_renderState, camera, _instanceCounts, _textures, _normalMaps, _heightMaps, _disableBackFaceCulling, time);

            _directShadowMap.Draw(_renderState, camera, _instanceCounts, _deferred.Depth, _lights[0].Direction, _disableBackFaceCulling, _deferred.Position, _deferred.Normal);
            _blurredShadowMap.Draw(_directShadowMap.Output);

            _environmentMap.CreateMap(entities[2], _renderState, 0, arrTrans, _instanceCounts, _textures, _normalMaps, _heightMaps, _disableBackFaceCulling, _lights, new Vector3(0.1f), camera, time);
            _environmentMap.Draw(_renderState, _deferred.Depth);
            _addEnvMap.Draw(_deferred.Color, _environmentMap.Output, 0.5f);

            _lighting.Draw(camera, _addEnvMap.Output, _deferred.Normal, _deferred.Position, _blurredShadowMap.Output, _lights, new Vector3(0.1f));

            _addProjectileColor.Draw(_deferred.ProjectileDepth, _deferred.Depth, _deferred.ProjectileColor, _lighting.Output, _deferred.ProjectileColor, _lighting.Output, _deferred.ProjectileColor, _lighting.Output);

            _sphereCut.Draw(camera, _addProjectileColor.Color, _deferred.Depth);

            _skybox.Draw(camera);
            _addSkybox.Draw(_skybox.Output, _sphereCut.Output);

            _ssaoWithBlur.Draw(_deferred.Depth, _addSkybox.Output);

            TextureDrawer.Draw(_ssaoWithBlur.Output);
            //TextureDrawer.Draw(_deferred.Normal);
            //TextureDrawer.Draw(_addWithDepthTest.Depth);
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
