using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;
using GraphicsWar.ExtensionMethods;

namespace GraphicsWar.View.Rendering.Instances
{
    public class Deferred : IUpdateTransforms, IUpdateResolution
    {
        private readonly IShaderProgram _deferredProgram;
        private IRenderSurface _deferredSurface;

        private readonly ITexture2D _defaultMap;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();

        private readonly ProjectileGeneration _projectilesGenerationNvidia;
        private readonly ProjectileGeneration _projectilesGenerationRadeon;
        private readonly AddWithDepthTest _addProjectilesNvidia;
        private readonly AddWithDepthTest _addProjectilesRadeon;
        private readonly AddWithDepthTest _addProjectilesCombined;
        private readonly Tesselation _tesselation;
        private readonly AddWithDepthTest _addTesselation;

        public ITexture2D Color => _addTesselation.Color;

        public ITexture2D Normal => _addTesselation.Normal;

        public ITexture2D Depth => _addTesselation.Depth;

        public ITexture2D Position => _addTesselation.Position;

        public ITexture2D ProjectileColor => _addProjectilesCombined.Color;

        public ITexture2D ProjectileDepth => _addProjectilesCombined.Depth;

        public Deferred(IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes)
        {
            _deferredProgram = contentLoader.Load<IShaderProgram>("deferred.*");

            foreach (var meshContainer in meshes)
            {
                VAO geometry = VAOLoader.FromMesh(meshContainer.Value, _deferredProgram);

                if (meshContainer.Value is TBNMesh mesh)
                {
                    var loc = _deferredProgram.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.TangentName);
                    geometry.SetAttribute(loc, mesh.Tangent.ToArray(), VertexAttribPointerType.Float, 3);

                    loc = _deferredProgram.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.BitangentName);
                    geometry.SetAttribute(loc, mesh.Bitangent.ToArray(), VertexAttribPointerType.Float, 3);
                }

                _geometries.Add(meshContainer.Key, geometry);
            }

            _defaultMap = contentLoader.Load<ITexture2D>("Nvidia.png");

            _projectilesGenerationNvidia = new ProjectileGeneration(contentLoader, meshes[Enums.EntityType.NvidiaTriangle], Enums.EntityType.NvidiaTriangle);
            _projectilesGenerationRadeon = new ProjectileGeneration(contentLoader, meshes[Enums.EntityType.RadeonTriangle], Enums.EntityType.RadeonTriangle);
            _addProjectilesNvidia = new AddWithDepthTest(contentLoader);
            _addProjectilesRadeon = new AddWithDepthTest(contentLoader);
            _addProjectilesCombined = new AddWithDepthTest(contentLoader);

            _tesselation = new Tesselation(contentLoader);
            _addTesselation = new AddWithDepthTest(contentLoader);
        }

        public void UpdateResolution(int width, int height)
        {
            ((FBO)_deferredSurface)?.Dispose();
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));

            _projectilesGenerationNvidia.UpdateResolution(width, height);
            _projectilesGenerationRadeon.UpdateResolution(width, height);
            _addProjectilesNvidia.UpdateResolution(width, height);
            _addProjectilesRadeon.UpdateResolution(width, height);
            _addProjectilesCombined.UpdateResolution(width, height);

            _tesselation.UpdateResolution(width, height);
            _addTesselation.UpdateResolution(width, height);
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_deferredProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type], true);
            }
            _projectilesGenerationNvidia.UpdateTransforms(transforms);
            _projectilesGenerationRadeon.UpdateTransforms(transforms);
        }

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> textures, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<Enums.EntityType> disableBackFaceCulling, float time)
        {

            _deferredSurface.Activate();
            renderState.Set(new DepthTest(true));
            GL.ClearColor(System.Drawing.Color.FromArgb(0,0,0,0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(4, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 });

            _deferredProgram.Activate();

            _deferredProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _deferredProgram.Uniform("camPos", invert.Translation / invert.M44);

            //TODO: Can be accelerated with sorting the normal map and not normal map useage beforhand
            foreach (var type in _geometries.Keys)
            {
                if (instanceCounts[type] == 0 || type == Enums.EntityType.NvidiaTriangle || type == Enums.EntityType.RadeonTriangle)
                {
                    continue;
                }

                if (normalMaps.ContainsKey(type))
                {
                    _deferredProgram.ActivateTexture("normalMap", 1, normalMaps[type]);

                    if (heightMaps.ContainsKey(type))
                    {
                        _deferredProgram.ActivateTexture("heightMap", 2, heightMaps[type]);
                        _deferredProgram.Uniform("normalMapping", 0f);
                        _deferredProgram.Uniform("paralaxMapping", 1f);
                    }
                    else
                    {
                        _deferredProgram.ActivateTexture("heightMap", 2, _defaultMap);
                        _deferredProgram.Uniform("normalMapping", 1f);
                        _deferredProgram.Uniform("paralaxMapping", 0f);
                    }
                }
                else
                {
                    //_deferredProgram.ActivateTexture("normalMap", 1, _defaultMap);
                    //_deferredProgram.ActivateTexture("heightMap", 2, _defaultMap);
                    _deferredProgram.Uniform("normalMapping", 0f);
                    _deferredProgram.Uniform("paralaxMapping", 0f);
                }

                if (textures.ContainsKey(type))
                {
                    _deferredProgram.ActivateTexture("tex", 0, textures[type]);
                    _deferredProgram.Uniform("textured", 1f);
                }
                else
                {
                    _deferredProgram.Uniform("materialColor", System.Drawing.Color.LightGray);
                    _deferredProgram.Uniform("textured", 0f);
                }

                renderState.Set(disableBackFaceCulling.Contains(type)
                    ? new BackFaceCulling(false)
                    : new BackFaceCulling(true));

                _geometries[type].Draw(instanceCounts[type]);
                
                if (textures.ContainsKey(type))
                {
                    _deferredProgram.DeactivateTexture(0, textures[type]);
                }

                if (normalMaps.ContainsKey(type))
                {
                    if (heightMaps.ContainsKey(type))
                    {
                        _deferredProgram.DeactivateTexture(2, heightMaps[type]);
                    }
                    _deferredProgram.DeactivateTexture(1, normalMaps[type]);
                }
                //else
                //{
                //    _deferredProgram.DeactivateTexture(2, _defaultMap);
                //    _deferredProgram.DeactivateTexture(1, _defaultMap);
                //}
            }

            renderState.Set(new DepthTest(false));
            renderState.Set(new BackFaceCulling(true));
            _deferredSurface.Deactivate();


            _projectilesGenerationNvidia.Draw(renderState, camera, instanceCounts[Enums.EntityType.NvidiaTriangle], time);
            _addProjectilesNvidia.Draw(_deferredSurface.Textures[2], _projectilesGenerationNvidia.Depth, _deferredSurface.Textures[0], _projectilesGenerationNvidia.Color, _deferredSurface.Textures[1], _projectilesGenerationNvidia.Normal, _deferredSurface.Textures[3], _projectilesGenerationNvidia.Position);

            _projectilesGenerationRadeon.Draw(renderState, camera, instanceCounts[Enums.EntityType.RadeonTriangle], time);
            _addProjectilesRadeon.Draw(_addProjectilesNvidia.Depth, _projectilesGenerationRadeon.Depth, _addProjectilesNvidia.Color, _projectilesGenerationRadeon.Color, _addProjectilesNvidia.Normal, _projectilesGenerationRadeon.Normal, _addProjectilesNvidia.Position, _projectilesGenerationRadeon.Position);

            _addProjectilesCombined.Draw(_projectilesGenerationNvidia.Depth, _projectilesGenerationRadeon.Depth, _projectilesGenerationNvidia.Color, _projectilesGenerationRadeon.Color, _projectilesGenerationNvidia.Normal, _projectilesGenerationRadeon.Normal, _projectilesGenerationNvidia.Position, _projectilesGenerationRadeon.Position);


            _tesselation.Draw(renderState, camera);
            _addTesselation.Draw(_addProjectilesRadeon.Depth, _tesselation.Depth, _addProjectilesRadeon.Color, _tesselation.Color, _addProjectilesRadeon.Normal, _tesselation.Normal, _addProjectilesRadeon.Position, _tesselation.Position);
        }
    }
}
