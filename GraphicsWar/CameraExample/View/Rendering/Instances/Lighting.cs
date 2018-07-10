using System;
using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    struct LightInShader
    {
        public Vector3 Position;
        public float Align1;
        public Vector3 Direction;
        public float Align2;
        public Vector3 Color;
        public float Intensity;
    }

    public class Lighting : IUpdateResolution
    {
        public ITexture2D Output => _renderSurface.Texture;

        private readonly IShaderProgram _shader;
        private IRenderSurface _renderSurface;
        private readonly int _lightArraySizeInShader = 8;
        
        public Lighting(IContentLoader contentLoader)
        {
            _shader = contentLoader.LoadPixelShader("lighting.glsl");
        }

        public void Draw(ITransformation camera, ITexture2D materialColor, ITexture2D normals, ITexture2D position, ITexture2D shadowSurface, List<LightSource> lightSources, Vector3 ambientColor)
        {
            if (lightSources.Count > _lightArraySizeInShader)
            {
                throw new ArgumentException("A maximum of " + _lightArraySizeInShader + " light sources is possible. See shader 'deferredLighting.glsl' for details.");
            }

            _renderSurface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Activate();

            _shader.Uniform("ambientColor", ambientColor);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _shader.Uniform("camPos", invert.Translation / invert.M44);

            _shader.ActivateOneOfMultipleTextures("materialColor", 0, materialColor);
            _shader.ActivateOneOfMultipleTextures("normals", 1, normals);
            _shader.ActivateOneOfMultipleTextures("position", 2, position);
            _shader.ActivateOneOfMultipleTextures("shadowSurface", 3, shadowSurface);

            var bufferObject = LightSourcesToBufferObject(lightSources);
            var loc = _shader.GetResourceLocation(ShaderResourceType.RWBuffer, "Lights");
            bufferObject.ActivateBind(loc);
            

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            _shader.DeativateOneOfMultipleTextures(3, shadowSurface);
            _shader.DeativateOneOfMultipleTextures(2, position);
            _shader.DeativateOneOfMultipleTextures(1, normals);
            _shader.DeativateOneOfMultipleTextures(0, materialColor);

            _shader.Deactivate();

            _renderSurface.Deactivate();
        }

        private BufferObject LightSourcesToBufferObject(List<LightSource> lightSources)
        {
            LightInShader[] lightInShader = new LightInShader[_lightArraySizeInShader];

            for (int i = 0; i < lightSources.Count; i++)
            {
                lightInShader[i].Position = lightSources[i].Position;
                lightInShader[i].Direction = lightSources[i].Direction;
                lightInShader[i].Color = lightSources[i].Color;
                lightInShader[i].Intensity = lightSources[i].Intensity;
            }

            for (int i = lightSources.Count; i < _lightArraySizeInShader; i++)
            {
                lightInShader[i].Position = LightSource.DefaultLightSource.Position;
                lightInShader[i].Direction = LightSource.DefaultLightSource.Direction;
                lightInShader[i].Color = LightSource.DefaultLightSource.Color;
                lightInShader[i].Intensity = LightSource.DefaultLightSource.Intensity;
            }

            BufferObject bufferObject = new BufferObject(BufferTarget.ShaderStorageBuffer);
            bufferObject.Set(lightInShader, BufferUsageHint.StaticCopy);

            return bufferObject;
        }

        public void UpdateResolution(int width, int height)
        {
            _renderSurface = new FBO(Texture2dGL.Create(width, height));

            _shader.Uniform("iResolution", new Vector2(width, height));
        }
    }
}
