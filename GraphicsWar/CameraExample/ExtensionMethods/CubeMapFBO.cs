using OpenTK.Graphics.OpenGL4;
using Zenseless.OpenGL;

namespace GraphicsWar.ExtensionMethods
{
    using Zenseless.HLGL;
    using System;


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FBO" />
    public class CubeMapFBO : IDisposable
    {

        private readonly int _size = 0;
        private uint _fboHandle = 0;
        private uint _lastFbo = 0;
        private static uint _currentFrameBufferHandle = 0;
        private readonly RenderBuffer _depth;

        /// <summary>
        /// Initializes a new instance of the <see cref="FBO"/> class.
        /// </summary>
        /// <param name="texture">The texture to draw to. The FBO will try to dispose the texture when the FBO is disposed.</param>
        /// <param name="size"></param>
        /// <exception cref="FBOException">
        /// Given texture is null or texture dimensions do not match primary texture
        /// </exception>
        public CubeMapFBO(int size)
        {
            _size = size;

            // Create an FBO object
            GL.GenFramebuffers(1, out _fboHandle);

            Texture = new Texture(TextureTarget.TextureCubeMap);

            //Initialize empty textures in texture
            Texture.Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba32f, size, size, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            }
            Texture.Deactivate();

            //Bind texture to framebuffer
            Activate();
            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.TextureCubeMapPositiveX + i, Texture.ID, 0);
            }
            string status = GetStatusMessage();
            Deactivate();

            if (!string.IsNullOrEmpty(status))
            {
                throw new FBOException(status);
            }

            //Attach depth
            Activate();
            _depth = new RenderBuffer(RenderbufferStorage.DepthComponent32, _size, _size);
            _depth.Attach(FramebufferAttachment.DepthAttachment);
            status = GetStatusMessage();
            Deactivate();

            if (!string.IsNullOrEmpty(status))
            {
                throw new FBOException(status);
            }
            

            int linMin = (int)TextureMinFilter.LinearMipmapLinear;
            int linMag = (int)TextureMagFilter.Linear;
            int clampToEdge = (int)TextureWrapMode.ClampToEdge;
            Texture.Activate();
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, ref linMin);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, ref linMag);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, ref clampToEdge);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, ref clampToEdge);
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, ref clampToEdge);
            Texture.Deactivate();
        }

        /// <summary>
        /// Gets the texture.
        /// </summary>
        /// <value>
        /// The texture.
        /// </value>
        public ITexture Texture { get; }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        public void Activate()
        {
            OpenTK.Graphics.OpenGL.GL.PushAttrib(OpenTK.Graphics.OpenGL.AttribMask.ViewportBit);
            _lastFbo = _currentFrameBufferHandle;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboHandle);
            GL.Viewport(0, 0, _size, _size);
            _currentFrameBufferHandle = _fboHandle;
        }

        /// <summary>
        /// Deactivates this instance.
        /// </summary>
        public void Deactivate()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _lastFbo);
            OpenTK.Graphics.OpenGL.GL.PopAttrib();
            _currentFrameBufferHandle = _lastFbo;
        }


        private static string GetStatusMessage()
        {
            switch (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer))
            {
                case FramebufferErrorCode.FramebufferComplete: return string.Empty;
                case FramebufferErrorCode.FramebufferIncompleteAttachment: return "One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for _depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.";
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachment: return "There are no attachments.";
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt: return "Attachments are of different _size. All attachments must have the same width and height.";
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt: return "The color attachments have different format. All color attachments must have the same format.";
                case FramebufferErrorCode.FramebufferIncompleteDrawBuffer: return "An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.";
                case FramebufferErrorCode.FramebufferIncompleteReadBuffer: return "The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.";
                case FramebufferErrorCode.FramebufferUnsupported: return "This particular FBO configuration is not supported by the implementation.";
                default: return "Status unknown. (yes, this is really bad.)";
            }
        }

        void IDisposable.Dispose()
        {
            Texture.Dispose();
            GL.DeleteFramebuffers(1, ref _fboHandle);
            _depth.Dispose();
        }

    }
}