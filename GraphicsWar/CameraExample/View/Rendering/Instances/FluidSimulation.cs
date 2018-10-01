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
    public class FluidSimulation : IUpdateResolution, IUpdateTransforms
    {
        public ITexture2D Color => _outputSurface.Textures[0];
        public ITexture2D Normal => _outputSurface.Textures[1];
        public ITexture2D Depth => _outputSurface.Textures[2];
        public ITexture2D Position => _outputSurface.Textures[3];
        public ITexture2D InitialDepth => _initialDepthSurface.Texture;
        public ITexture2D BlurredDepth => _blurDepth2Surface.Texture;
        public ITexture2D Thickness => _thicknessSurface.Texture;
      
        private readonly VAO _initialDepthVAO;
        private readonly IShaderProgram _initialDepthFluidSimulationProgram;
        private IRenderSurface _initialDepthSurface;

        private readonly IShaderProgram _blurDepthPass1;
        private IRenderSurface _blurDepth1Surface;
        private readonly IShaderProgram _blurDepthPass2;
        private IRenderSurface _blurDepth2Surface;

        private readonly VAO _thicknessVAO;
        private readonly IShaderProgram _thicknessFluidSimulationProgram;
        private IRenderSurface _thicknessSurface;
        
        private readonly VAO _convergeVAO;
        private readonly IShaderProgram _convergeFluidSimulationProgram;
        private IRenderSurface _outputSurface;


        private static readonly DrawBuffersEnum[] _drawBuffers = new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 };

        public FluidSimulation(IContentLoader contentLoader, Dictionary<Enums.EntityType, DefaultMesh> meshes)
        {
            _initialDepthFluidSimulationProgram = contentLoader.Load<IShaderProgram>("initialDepthOfFluid.*");
            _initialDepthVAO = VAOLoader.FromMesh(meshes[Enums.EntityType.FluidSphere], _initialDepthFluidSimulationProgram);

            _blurDepthPass1 = contentLoader.LoadPixelShader("fluidBlurPass1.glsl");
            _blurDepthPass2 = contentLoader.LoadPixelShader("fluidBlurPass2.glsl");

            _thicknessFluidSimulationProgram = contentLoader.Load<IShaderProgram>("thicknessOfFluid.*");
            _thicknessVAO = VAOLoader.FromMesh(meshes[Enums.EntityType.FluidSphere], _thicknessFluidSimulationProgram);

            _convergeFluidSimulationProgram = contentLoader.Load<IShaderProgram>("convergeFluid.*");
            _convergeVAO = VAOLoader.FromMesh(meshes[Enums.EntityType.FluidSphere], _convergeFluidSimulationProgram);
        }


        public void Draw(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts)
        {
            DrawInitialDepth(renderState, camera, instanceCounts);

            BlurInitialDepth();

            DrawThickness(renderState, camera, instanceCounts);

            DrawConverge(renderState, camera, instanceCounts);
        }

        private void DrawInitialDepth(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts)
        {
            _initialDepthSurface.Activate();

            var oldBackFaceCullingState = renderState.Get<BackFaceCulling>();
            var oldDepthTestState = renderState.Get<DepthTest>();

            renderState.Set(new DepthTest(true));
            renderState.Set(new BackFaceCulling(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _initialDepthFluidSimulationProgram.Activate();

            _initialDepthFluidSimulationProgram.Uniform("camera", camera);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(_drawBuffers.Length, _drawBuffers);

            _initialDepthVAO.Draw(instanceCounts[Enums.EntityType.FluidSphere]);

            _initialDepthFluidSimulationProgram.Deactivate();

            renderState.Set(oldDepthTestState);
            renderState.Set(oldBackFaceCullingState);

            _initialDepthSurface.Deactivate();
        }
        
        private void BlurInitialDepth()
        {
            DrawPass(InitialDepth, _blurDepth1Surface, _blurDepthPass1);
            DrawPass(_blurDepth1Surface.Texture, _blurDepth2Surface, _blurDepthPass2);
        }


        private void DrawPass(ITexture2D inputTexture, IRenderSurface surface, IShaderProgram program)
        {
            surface.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Activate();

            inputTexture.Activate();

            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            inputTexture.Deactivate();

            program.Deactivate();

            surface.Deactivate();
        }

        private void DrawThickness(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts)
        {
            _thicknessSurface.Activate();

            var oldBackFaceCullingState = renderState.Get<BackFaceCulling>();
            var oldDepthTestState = renderState.Get<DepthTest>();
            var oldBlendState = renderState.Get<BlendState>();

            renderState.Set(new DepthTest(false));
            renderState.Set(new BackFaceCulling(true));
            renderState.Set(new BlendState(BlendOperator.Add, BlendParameter.SourceAlpha, BlendParameter.DestinationAlpha));
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _thicknessFluidSimulationProgram.Activate();

            _thicknessFluidSimulationProgram.Uniform("camera", camera);
            Matrix4x4.Invert(camera.Matrix, out var invert);
            _thicknessFluidSimulationProgram.Uniform("camPos", invert.Translation / invert.M44);

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(_drawBuffers.Length, _drawBuffers);

            _thicknessVAO.Draw(instanceCounts[Enums.EntityType.FluidSphere]);

            _thicknessFluidSimulationProgram.Deactivate();

            renderState.Set(oldBlendState);
            renderState.Set(oldDepthTestState);
            renderState.Set(oldBackFaceCullingState);

            _thicknessSurface.Deactivate();
        }


        private void DrawConverge(IRenderState renderState, ITransformation camera, Dictionary<Enums.EntityType, int> instanceCounts)
        {
            //_thicknessSurface.Activate();

            //var oldBackFaceCullingState = renderState.Get<BackFaceCulling>();
            //var oldDepthTestState = renderState.Get<DepthTest>();

            //renderState.Set(new DepthTest(true));
            //renderState.Set(new BackFaceCulling(true));

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //_thicknessFluidSimulationProgram.Activate();

            //_thicknessFluidSimulationProgram.Uniform("camera", camera);

            //GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            //GL.DrawBuffers(_drawBuffers.Length, _drawBuffers);

            //_thicknessVAO.Draw(instanceCounts[Enums.EntityType.FluidSphere]);

            //_thicknessFluidSimulationProgram.Deactivate();

            //renderState.Set(oldDepthTestState);
            //renderState.Set(oldBackFaceCullingState);

            //_thicknessSurface.Deactivate();
        }


        public void UpdateResolution(int width, int height)
        {
            ((FBOwithDepth)_initialDepthSurface)?.Dispose();
            _initialDepthSurface = new FBOwithDepth(Texture2dGL.Create(width, height, 1, true));

            ((FBOwithDepth)_blurDepth1Surface)?.Dispose();
            _blurDepth1Surface = new FBOwithDepth(Texture2dGL.Create(width, height, 1, true));

            ((FBOwithDepth)_blurDepth2Surface)?.Dispose();
            _blurDepth2Surface = new FBOwithDepth(Texture2dGL.Create(width, height, 1, true));

            ((FBOwithDepth)_thicknessSurface)?.Dispose();
            _thicknessSurface = new FBOwithDepth(Texture2dGL.Create(width, height, 4, true));

            ((FBOwithDepth)_outputSurface)?.Dispose();
            _outputSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 1, true));
            _outputSurface.Attach(Texture2dGL.Create(width, height, 3, true));
        }

        public void UpdateTransforms(Dictionary<Enums.EntityType, Matrix4x4[]> transforms)
        {
            _initialDepthVAO.SetAttribute(_initialDepthFluidSimulationProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[Enums.EntityType.FluidSphere], true);
            _thicknessVAO.SetAttribute(_thicknessFluidSimulationProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[Enums.EntityType.FluidSphere], true);
            _convergeVAO.SetAttribute(_convergeFluidSimulationProgram.GetResourceLocation(ShaderResourceType.Attribute, "transform"), transforms[Enums.EntityType.FluidSphere], true);
        }
    }
}
