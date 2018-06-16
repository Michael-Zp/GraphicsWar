using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    public class Deferred : IRenderInstance, IUpdateGeometry, IUpdateResolution
    {
        private readonly IShaderProgram _shaderProgram;
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

        public Deferred(IContentLoader contentLoader, Dictionary<Enums.EntityType, Mesh> meshes)
        {
            _shaderProgram = contentLoader.Load<IShaderProgram>("deferred.*");

            foreach (var meshContainer in meshes)
            {
                _geometries.Add(meshContainer.Key, VAOLoader.FromMesh(meshContainer.Value, _shaderProgram));
            }
        }

        public void UpdateResolution(int width, int height)
        {
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));

            _shaderProgram.Uniform("iResolution", new Vector2(width, height));
        }

        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts)
        {
            _deferredSurface.Activate();
            renderState.Set(new DepthTest(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            _shaderProgram.Activate();
            _shaderProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _shaderProgram.Uniform("camPos", invert.Translation / invert.M44);
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].Draw(instanceCounts[type]);
            }
            _shaderProgram.Deactivate();

            renderState.Set(new DepthTest(false));
            _deferredSurface.Deactivate();
        }

        public void UpdateAttributes(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_shaderProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type].ToArray(), true);
            }
        }
    }
}
