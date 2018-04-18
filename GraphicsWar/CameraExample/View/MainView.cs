using GraphicsWar.Shared;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View
{
    public class MainView
    {
        private IShaderProgram shaderProgram;

        private Dictionary<Enums.EntityType, VAO> geometries = new Dictionary<Enums.EntityType, VAO>();
        private Dictionary<Enums.EntityType, int> instanceCounts = new Dictionary<Enums.EntityType, int>();
        private Dictionary<Enums.EntityType, List<Matrix4x4>> transforms = new Dictionary<Enums.EntityType, List<Matrix4x4>>();

        public MainView(IRenderState renderState, IContentLoader contentLoader)
        {
            renderState.Set(BoolState<IDepthState>.Enabled);
            renderState.Set(BoolState<IBackfaceCullingState>.Enabled);

            shaderProgram = contentLoader.Load<IShaderProgram>("shader.*");

            var mesh = contentLoader.Load<DefaultMesh>("suzanne");

            geometries.Add(Enums.EntityType.Type1, VAOLoader.FromMesh(mesh, shaderProgram));
            geometries.Add(Enums.EntityType.Type2, VAOLoader.FromMesh(mesh, shaderProgram));
        }

        public void Render(IEnumerable<ViewEntity> entities, float time, Transformation3D camera)
        {
            if (shaderProgram is null) return;
            
            transforms.Clear();
            instanceCounts.Clear();

            foreach (var type in geometries.Keys)
            {
                instanceCounts.Add(type, 0);
                transforms.Add(type, new List<Matrix4x4>());
            }

            foreach (var entity in entities)
            {
                instanceCounts[entity.Type]++;
                transforms[entity.Type].Add(entity.Transform);
            }

            UpdateAttributes();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderProgram.Activate();
            shaderProgram.Uniform("time", time);
            shaderProgram.Uniform("camera", camera.CalcLocalToWorldColumnMajorMatrix());
            foreach (var type in geometries.Keys)
            {
                geometries[type].Draw(instanceCounts[type]);
            }
            shaderProgram.Deactivate();
        }

        private void UpdateAttributes()
        {
            foreach (var type in geometries.Keys)
            {
                geometries[type].SetAttribute(shaderProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[type].ToArray(), true);
            }
        }
    }
}
