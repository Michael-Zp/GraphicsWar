using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Management;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    class EnvironmentMap : IUpdateResolution, IUpdateTransforms
    {
        private readonly ITexture2D[] _textures = new ITexture2D[6];
        private readonly ITransformation[] _cameras = new ITransformation[6];
        private readonly Orbit[] _orbits = new Orbit[6];

        private Deferred _deferred;
        private Lighting _lighting;

        private IRenderSurface _outputSurface;

        public EnvironmentMap(int size, IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes, ICollection<Enums.EntityType> normalMapped, ICollection<Enums.EntityType> heightMapped)
        {
            Vector2[] rotations = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(90,0),
                new Vector2(180,0),
                new Vector2(270,0),
                new Vector2(0,90),
                new Vector2(0,-90)
            };

            for (int i = 0; i < 6; i++)
            {
                _orbits[i] = new Orbit(0.0f, rotations[i].X, rotations[i].Y);
                _cameras[i] = new Camera<Orbit, Perspective>(_orbits[i], new Perspective());
            }

            _deferred = new Deferred(contentLoader, meshes, normalMapped, heightMapped);
            _deferred.UpdateResolution(size, size);
            _lighting = new Lighting(contentLoader);
            _lighting.UpdateResolution(size, size);
        }

        public void CreateMap(Vector3 position, IRenderState state, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<LightSource> lightSources, Vector3 ambientColor)
        {
            for (int i = 0; i < 6; i++)
            {
                _orbits[i].Target = position;
                _deferred.Draw(state, _cameras[i], instanceCounts, normalMaps, heightMaps);
                _lighting.Draw(_cameras[i], _deferred.Color, _deferred.Normals, _deferred.Position, Texture2dGL.Create(1,1), lightSources, ambientColor);
            }
        }

        public void UpdateResolution(int width, int height)
        {

        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            _deferred.UpdateTransforms(transforms);
        }
    }
}
