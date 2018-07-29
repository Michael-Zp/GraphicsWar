using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class Tesselation : IUpdateResolution
    {
        public ITexture2D Output => _outputSurface.Texture;


        private IShaderProgram _tesselationProgram;
        private IRenderSurface _outputSurface;
        private ITexture2D _displacementMap;


        public Tesselation(IContentLoader contentLoader)
        {
            _tesselationProgram = contentLoader.Load<IShaderProgram>("TerrainTessellation.*");
            _displacementMap = contentLoader.Load<ITexture2D>("h4.jpg");
        }


        public void Draw(IRenderState renderState, ITransformation camera)
        {
            //_outputSurface.Activate();
            var oldBackFaceCullingState = renderState.Get<BackFaceCulling>();
            var oldDepthTestState = renderState.Get<DepthTest>();

            renderState.Set(new DepthTest(true));
            renderState.Set(new BackFaceCulling(false));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _tesselationProgram.Activate();
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); //Does not work with our FBOs. I don´t know why. Turn off for now or only test without FBO.
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);


            int instanceSqrt = 100;
            _tesselationProgram.Uniform("camera", camera);
            _tesselationProgram.Uniform(nameof(instanceSqrt), instanceSqrt);
            _displacementMap.Activate();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 4, instanceSqrt * instanceSqrt);

            _displacementMap.Deactivate();
            _tesselationProgram.Deactivate();

            renderState.Set(oldDepthTestState);
            renderState.Set(oldBackFaceCullingState);
            //_outputSurface.Deactivate();
        }


        public void UpdateResolution(int width, int height)
        {
            ((FBOwithDepth)_outputSurface)?.Dispose();
            _outputSurface = new FBOwithDepth(Texture2dGL.Create(width, height));
        }
    }
}
