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
        private readonly IShaderProgram _deferredProgram;
        private IRenderSurface _deferredSurface;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();

        public ITexture2D Color => _deferredSurface.Textures[0];

        public ITexture2D Normals => _deferredSurface.Textures[1];

        public ITexture2D Depth => _deferredSurface.Textures[2];

        public ITexture2D Position => _deferredSurface.Textures[3];

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
        }

        public void UpdateResolution(int width, int height)
        {
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3, true));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_deferredProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type].ToArray(), true);
            }
        }

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> textures, Dictionary<Enums.EntityType, ITexture2D> normalMaps, Dictionary<Enums.EntityType, ITexture2D> heightMaps, List<Enums.EntityType> disableBackFaceCulling)
        {
            _deferredSurface.Activate();
            renderState.Set(new DepthTest(true));

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
                if (normalMaps.ContainsKey(type))
                {
                    _deferredProgram.ActivateTexture("normalMap", 1, normalMaps[type]);

                    if (heightMaps.ContainsKey(type))
                    {
                        _deferredProgram.ActivateTexture("heightMap", 2, heightMaps[type]);
                        _deferredProgram.Uniform("normalMapped", 0f);
                        _deferredProgram.Uniform("paralexMapped", 1f);
                    }
                    else
                    {
                        _deferredProgram.Uniform("normalMapped", 1f);
                        _deferredProgram.Uniform("paralexMapped", 0f);
                    }
                }
                else
                {
                    _deferredProgram.Uniform("normalMapped", 0f);
                    _deferredProgram.Uniform("paralexMapped", 0f);
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

                if (disableBackFaceCulling.Contains(type))
                {
                    renderState.Set(new BackFaceCulling(false));
                }
                else
                {
                    renderState.Set(new BackFaceCulling(true));
                }

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
            }

            renderState.Set(new DepthTest(false));
            renderState.Set(new BackFaceCulling(true));
            _deferredSurface.Deactivate();
        }
    }
}
