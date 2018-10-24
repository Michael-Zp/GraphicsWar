using System;
using System.Collections.Generic;
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
        public ITexture2D Color => _add.Color;
        public ITexture2D Normal => _outputSurface.Textures[1];
        public ITexture2D Depth => _outputSurface.Textures[2];
        public ITexture2D Position => _outputSurface.Textures[3];
        public ITexture2D IntensityMap => _outputSurface.Textures[4];

        private IShaderProgram _projectileGenerationProgram;
        private IRenderSurface _outputSurface;
        private IRenderSurface _wireSurface;

        private AddWithDepthTest _add;

        private VAO _trianglesGeometry;

        private Enums.EntityType _triangleType;

        private static readonly DrawBuffersEnum[] DrawBuffers = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 };

        public ProjectileGeneration(IContentLoader contentLoader, DefaultMesh triangleMesh, Enums.EntityType triangleType)
        {
            _projectileGenerationProgram = contentLoader.Load<IShaderProgram>(new[] { "ProjectileGeneration.vert", "deferred.frag" });
            _trianglesGeometry = VAOLoader.FromMesh(triangleMesh, _projectileGenerationProgram);
            _triangleType = triangleType;

            _add = new AddWithDepthTest(contentLoader);
        }


        public void Draw(IRenderState renderState, ITransformation camera, int trianglesCount, Vector4 intensity, float time)
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
            _projectileGenerationProgram.Uniform("intensity", intensity);

            switch (_triangleType)
            {
                case Enums.EntityType.NvidiaTriangle:
                    _projectileGenerationProgram.Uniform("materialColor", new Vector4(0, 100, 0, 5));
                    break;

                case Enums.EntityType.RadeonTriangle:
                    _projectileGenerationProgram.Uniform("materialColor", new Vector4(100, 0, 0, 5));
                    break;

                default:
                    Console.WriteLine("No origin for triangle found");
                    _projectileGenerationProgram.Uniform("materialColor", System.Drawing.Color.Blue);
                    break;
            }

            _projectileGenerationProgram.Uniform("textured", 0f);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(DrawBuffers.Length, DrawBuffers);

            _trianglesGeometry.Draw(trianglesCount);



            _projectileGenerationProgram.Deactivate();

            _outputSurface.Deactivate();


            _wireSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _projectileGenerationProgram.Activate();

            _projectileGenerationProgram.Uniform("camPos", invert.Translation / invert.M44);
            _projectileGenerationProgram.Uniform("camera", camera);
            _projectileGenerationProgram.Uniform("time", time);
            _projectileGenerationProgram.Uniform("normalMapping", 0f);
            _projectileGenerationProgram.Uniform("paralaxMapping", 0f);
            _projectileGenerationProgram.Uniform("intensity", intensity);

            switch (_triangleType)
            {
                case Enums.EntityType.NvidiaTriangle:
                    _projectileGenerationProgram.Uniform("materialColor", System.Drawing.Color.DarkGreen);
                    break;

                case Enums.EntityType.RadeonTriangle:
                    _projectileGenerationProgram.Uniform("materialColor", System.Drawing.Color.DarkRed);
                    break;

                default:
                    Console.WriteLine("No origin for triangle found");
                    _projectileGenerationProgram.Uniform("materialColor", System.Drawing.Color.DarkBlue);
                    break;
            }

            _projectileGenerationProgram.Uniform("textured", 0f);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(DrawBuffers.Length, DrawBuffers);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

            _trianglesGeometry.Draw(trianglesCount);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);



            _projectileGenerationProgram.Deactivate();

            _wireSurface.Deactivate();

            _add.Draw(_outputSurface.Textures[2], _wireSurface.Textures[2], _outputSurface.Textures[0], _wireSurface.Textures[0], _outputSurface.Textures[0], _wireSurface.Textures[0], _outputSurface.Textures[0], _wireSurface.Textures[0], _outputSurface.Textures[0], _wireSurface.Textures[0], -0.01f);
        }


        public void UpdateResolution(int width, int height)
        {
            ((FBOwithDepth)_outputSurface)?.Dispose();
            _outputSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 4, true));

            ((FBOwithDepth)_wireSurface)?.Dispose();
            _wireSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _wireSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _wireSurface.Attach(Texture2dGL.Create(width, height, 1, true));

            _add.UpdateResolution(width, height);
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            _trianglesGeometry.SetAttribute(_projectileGenerationProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[_triangleType], true);
        }
    }
}
