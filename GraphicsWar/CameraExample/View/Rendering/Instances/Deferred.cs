using System.Collections.Generic;
using System.Linq;
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
        private readonly IShaderProgram _shaderWithGeometryNormals;
        private readonly IShaderProgram _shaderWithNormalMap;
        private readonly IShaderProgram _shaderParalax;
        private IRenderSurface _deferredSurface;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();

        public ITexture2D Color => _deferredSurface.Textures[0];

        public ITexture2D Normals => _deferredSurface.Textures[1];

        public ITexture2D Depth => _deferredSurface.Textures[2];

        public ITexture2D Position => _deferredSurface.Textures[3];

        public Deferred(IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes, ICollection<Enums.EntityType> normalMapped, ICollection<Enums.EntityType> heightMapped)
        {
            _shaderWithGeometryNormals = contentLoader.Load<IShaderProgram>("deferred.*");
            _shaderWithNormalMap = contentLoader.Load<IShaderProgram>("deferredNormalMap.*");
            _shaderParalax = contentLoader.Load<IShaderProgram>("deferredNormalMapParalax.*");

            foreach (var meshContainer in meshes)
            {
                if (normalMapped.Contains(meshContainer.Key))
                {
                    VAO geometry;
                    if (heightMapped.Contains(meshContainer.Key))
                    {
                        geometry = VAOLoader.FromMesh(meshContainer.Value, _shaderParalax);

                        if (meshContainer.Value is TBNMesh mesh)
                        {
                            var loc = _shaderParalax.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.TangentName);
                            geometry.SetAttribute(loc, mesh.Tangent.ToArray(), VertexAttribPointerType.Float, 3);

                            loc = _shaderParalax.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.BitangentName);
                            geometry.SetAttribute(loc, mesh.Bitangent.ToArray(), VertexAttribPointerType.Float, 3);
                        }
                    }
                    else
                    {
                        geometry = VAOLoader.FromMesh(meshContainer.Value, _shaderWithNormalMap);

                        if (meshContainer.Value is TBNMesh mesh)
                        {
                            var loc = _shaderWithNormalMap.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.TangentName);
                            geometry.SetAttribute(loc, mesh.Tangent.ToArray(), VertexAttribPointerType.Float, 3);

                            loc = _shaderWithNormalMap.GetResourceLocation(ShaderResourceType.Attribute, TBNMesh.BitangentName);
                            geometry.SetAttribute(loc, mesh.Bitangent.ToArray(), VertexAttribPointerType.Float, 3);
                        }
                    }

                    _geometries.Add(meshContainer.Key, geometry);
                }
                else
                {
                    _geometries.Add(meshContainer.Key, VAOLoader.FromMesh(meshContainer.Value, _shaderWithGeometryNormals));
                }
            }
        }

        public void UpdateResolution(int width, int height)
        {
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));

            _shaderWithGeometryNormals.Uniform("iResolution", new Vector2(width, height));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            int loc = _shaderWithGeometryNormals.GetResourceLocation(ShaderResourceType.Attribute, "transform");
            
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(loc, transforms[type], true);
            }
        }

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps)
        {
            _deferredSurface.Activate();
            renderState.Set(new DepthTest(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(4, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 });

            SetUniforms(_shaderWithGeometryNormals, camera);
            SetUniforms(_shaderWithNormalMap, camera);
            SetUniforms(_shaderParalax, camera);

            //TODO: Can be accelerated with sorting the normal map and not normal map useage beforhand
            foreach (var type in _geometries.Keys)
            {
                if (normalMaps.ContainsKey(type))
                {
                    if (heightMaps.ContainsKey(type))
                    {
                        _shaderParalax.Activate();
                        _shaderParalax.ActivateOneOfMultipleTextures("normalMap", 0, normalMaps[type]);
                        _shaderParalax.ActivateOneOfMultipleTextures("heightMap", 1, heightMaps[type]);

                        _geometries[type].Draw(instanceCounts[type]);

                        _shaderParalax.DeativateOneOfMultipleTextures(1, heightMaps[type]);
                        _shaderParalax.DeativateOneOfMultipleTextures(0, normalMaps[type]);
                        _shaderParalax.Deactivate();
                    }
                    else
                    {
                        _shaderWithNormalMap.Activate();
                        normalMaps[type].Activate();

                        _geometries[type].Draw(instanceCounts[type]);

                        normalMaps[type].Deactivate();
                        _shaderWithNormalMap.Deactivate();
                    }
                }
                else
                {
                    _shaderWithGeometryNormals.Activate();

                    _geometries[type].Draw(instanceCounts[type]);

                    _shaderWithGeometryNormals.Deactivate();
                }
            }

            renderState.Set(new DepthTest(false));
            _deferredSurface.Deactivate();
        }

        private void SetUniforms(IShaderProgram shader, ITransformation camera)
        {
            shader.Activate();
            shader.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            shader.Uniform("camPos", invert.Translation / invert.M44);
            shader.Deactivate();
        }
    }
}
