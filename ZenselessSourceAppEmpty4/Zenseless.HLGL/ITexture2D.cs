namespace Zenseless.HLGL
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ITexture" />
    public interface ITexture2D : ITexture
    {
        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        int Height { get; }
        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        int Width { get; }

        //void LoadPixels(IntPtr pixels, int width, int height, byte components = 4, bool floatingPoint = false);
    }
}