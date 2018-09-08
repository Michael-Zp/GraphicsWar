﻿using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class ProjectileGeneration : IUpdateResolution, IUpdateTransforms
    {
        public ITexture2D Color => _outputSurface.Textures[0];
        public ITexture2D Normal => _outputSurface.Textures[1];
        public ITexture2D Depth => _outputSurface.Textures[2];
        public ITexture2D Position => _outputSurface.Textures[3];

        private IShaderProgram _projectileGenerationProgram;
        private IRenderSurface _outputSurface;

        private VAO _trianglesGeometry;

        private static readonly DrawBuffersEnum[] DrawBuffers = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 };

        public ProjectileGeneration(IContentLoader contentLoader, DefaultMesh triangleMesh)
        {
            _projectileGenerationProgram = contentLoader.Load<IShaderProgram>(new [] { "ProjectileGeneration.vert", "deferred.frag" } );
            _trianglesGeometry = VAOLoader.FromMesh(triangleMesh, _projectileGenerationProgram);
        }
        

        public void Draw(IRenderState renderState, ITransformation camera, int trianglesCount, float time)
        {
            _outputSurface.Activate();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _projectileGenerationProgram.Activate();

            Matrix4x4.Invert(camera.Matrix, out var invert);
            _projectileGenerationProgram.Uniform("camPos", invert.Translation / invert.M44);
            _projectileGenerationProgram.Uniform("camera", camera);
            _projectileGenerationProgram.Uniform("time", time);
            _projectileGenerationProgram.Uniform("normalMapping", 0f);
            _projectileGenerationProgram.Uniform("paralaxMapping", 0f);
            _projectileGenerationProgram.Uniform("materialColor", System.Drawing.Color.ForestGreen);
            _projectileGenerationProgram.Uniform("textured", 0f);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(DrawBuffers.Length, DrawBuffers);
            
            _trianglesGeometry.Draw(trianglesCount);

            _projectileGenerationProgram.Deactivate();
            
            _outputSurface.Deactivate();
        }


        public void UpdateResolution(int width, int height)
        {
            ((FBOwithDepth)_outputSurface)?.Dispose();
            _outputSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            _trianglesGeometry.SetAttribute(_projectileGenerationProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[Enums.EntityType.Triangle], true);
        }
    }
}
