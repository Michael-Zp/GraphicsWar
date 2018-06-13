using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.RenderInstances
{
    class Deferred : IRenderInstance
    {
        private readonly IShaderProgram _shaderProgram;
        private IRenderSurface _deferredSurface;

        private readonly Dictionary<Enums.EntityType, VAO> _geometries = new Dictionary<Enums.EntityType, VAO>();
        private readonly Dictionary<Enums.EntityType, int> _instanceCounts = new Dictionary<Enums.EntityType, int>();
        private readonly Dictionary<Enums.EntityType, List<Matrix4x4>> _transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

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

        public Deferred(IContentLoader contentLoader)
        {
            _shaderProgram = contentLoader.Load<IShaderProgram>("deferred.*");

            //var mesh = contentLoader.Load<DefaultMesh>("suzanne");
            var mesh = Meshes.CreateSphere();
            mesh = Meshes.CreateCornellBox();

            _geometries.Add(Enums.EntityType.Type1, VAOLoader.FromMesh(mesh, _shaderProgram));
            _geometries.Add(Enums.EntityType.Type2, VAOLoader.FromMesh(mesh, _shaderProgram));
        }

        public void UpdateResolution(int width, int height)
        {
            _deferredSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 3));
            _deferredSurface.Attach(Texture2dGL.Create(width, height, 1, true));

            _shaderProgram.Uniform("iResolution", new Vector2(width, height));
        }

        public void Draw(IRenderState renderState, ITransformation camera)
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
                _geometries[type].Draw(_instanceCounts[type]);
            }
            _shaderProgram.Deactivate();

            renderState.Set(new DepthTest(false));
            _deferredSurface.Deactivate();
        }

        public void UpdateInstancing(IEnumerable<ViewEntity> entities)
        {
            _transforms.Clear();
            _instanceCounts.Clear();

            foreach (var type in _geometries.Keys)
            {
                _instanceCounts.Add(type, 0);
                _transforms.Add(type, new List<Matrix4x4>());
            }

            foreach (var entity in entities)
            {
                _instanceCounts[entity.Type]++;
                _transforms[entity.Type].Add(entity.Transform);
            }

            UpdateAttributes();
        }

        private void UpdateAttributes()
        {
            foreach (var type in _geometries.Keys)
            {
                _geometries[type].SetAttribute(_shaderProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), _transforms[type].ToArray(), true);
            }
        }
    }
}
