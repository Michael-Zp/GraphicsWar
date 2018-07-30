using GraphicsWar.View.Rendering.Management;
using OpenTK.Graphics.OpenGL4;
using Zenseless.Geometry;
using Zenseless.HLGL;
using Zenseless.OpenGL;

namespace GraphicsWar.View.Rendering.Instances
{
    public class Tesselation : IUpdateResolution
    {
        public ITexture2D Color => _outputSurface.Textures[0];
        public ITexture2D Normal => _outputSurface.Textures[1];
        public ITexture2D Depth => _outputSurface.Textures[2];
        public ITexture2D Position => _outputSurface.Textures[3];


        private IShaderProgram _tesselationProgram;
        private IRenderSurface _outputSurface;
        private ITexture2D _displacementMap;

        private static readonly DrawBuffersEnum[] _drawBuffers = new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 };

        public Tesselation(IContentLoader contentLoader)
        {
            _tesselationProgram = contentLoader.Load<IShaderProgram>("TerrainTessellation.*");
            _displacementMap = contentLoader.Load<ITexture2D>("h4.jpg");
        }


        public void Draw(IRenderState renderState, ITransformation camera)
        {
            _outputSurface.Activate();

            var oldBackFaceCullingState = renderState.Get<BackFaceCulling>();
            var oldDepthTestState = renderState.Get<DepthTest>();

            renderState.Set(new DepthTest(true));
            renderState.Set(new BackFaceCulling(true));

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _tesselationProgram.Activate();
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); //Does not work with our FBOs. I don´t know why. Turn off for now or only test without FBO.
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);


            int instanceSqrt = 100;
            _tesselationProgram.Uniform("camera", camera);
            _tesselationProgram.Uniform(nameof(instanceSqrt), instanceSqrt);
            _displacementMap.Activate();

            GL.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearBuffer(ClearBuffer.Color, 2, new float[] { 1000 });
            GL.DrawBuffers(_drawBuffers.Length, _drawBuffers);

            GL.DrawArraysInstanced(PrimitiveType.Patches, 0, 4, instanceSqrt * instanceSqrt);

            _displacementMap.Deactivate();
            _tesselationProgram.Deactivate();

            renderState.Set(oldDepthTestState);
            renderState.Set(oldBackFaceCullingState);

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
    }
}
