using OpenTK.Graphics.OpenGL4;
using Zenseless.HLGL;

namespace GraphicsWar.ExtensionMethods
{
    public static class ShaderProgramExtensionMethods
    {
        public static void ActivateOneOfMultipleTextures(this IShaderProgram shader, string name, int id, ITexture2D texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + id);
            texture.Activate();
            shader.Uniform(name, id);
        }

        public static void DeativateOneOfMultipleTextures(this IShaderProgram shader, int id, ITexture2D texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + id);
            texture.Deactivate(); //Don´t use GL.BindTexture(..., 0). The texture target is not known this way and thus is not useable universally.
        }
    }
}
