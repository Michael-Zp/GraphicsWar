using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    public class Deferred : IRenderInstance, IUpdateTransforms, IUpdateResolution
    {
        private readonly IShaderProgram _shaderWithGeometryNormals;
        private readonly IShaderProgram _shaderWithNormalMap;
        private IRenderSurface _deferredSurface;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();

        public ITexture2D Color
        {
            get
            {
                return _deferredSurface.Textures[0];
            }
        }

        public ITexture2D Normals
        {
            get
            {
                return _deferredSurface.Textures[1];
            }
        }

        public ITexture2D Depth
        {
            get
            {
                return _deferredSurface.Textures[2];
            }
        }
        public ITexture2D Position
        {
            get
            {
                return _deferredSurface.Textures[3];
            }
        }

        public Deferred(IContentLoader contentLoader, Dictionary<Enums.EntityType, Mesh> meshes, Dictionary<Enums.EntityType, ITexture2D> normalMaps)
        {
            _shaderWithGeometryNormals = contentLoader.Load<IShaderProgram>("deferred.*");
            _shaderWithNormalMap = contentLoader.Load<IShaderProgram>("deferredNormalMap.*");

            foreach (var meshContainer in meshes)
            {
                if(normalMaps.ContainsKey(meshContainer.Key))
                {
                    _geometries.Add(meshContainer.Key, VAOLoader.FromMesh(meshContainer.Value, _shaderWithNormalMap));
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

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts, Dictionary<Enums.EntityType, ITexture2D> normalMaps)
        {
            _deferredSurface.Activate();
            renderState.Set(new DepthTest(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(4, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 });

            SetUniforms(_shaderWithGeometryNormals, camera);
            SetUniforms(_shaderWithNormalMap, camera);

            //TODO: Can be accelerated with sorting the normal map and not normal map useage beforhand
            foreach (var type in _geometries.Keys)
            {

                //_shaderWithGeometryNormals.Activate();

                //_geometries[type].Draw(instanceCounts[type]);

                //_shaderWithGeometryNormals.Deactivate();

                if (normalMaps.ContainsKey(type))
                {
                    _shaderWithNormalMap.Activate();
                    normalMaps[type].Activate();

                    _geometries[type].Draw(instanceCounts[type]);

                    normalMaps[type].Deactivate();
                    _shaderWithNormalMap.Deactivate();
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

        public void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_shaderWithGeometryNormals.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type].ToArray(), true);
            }
        }
    }
}
