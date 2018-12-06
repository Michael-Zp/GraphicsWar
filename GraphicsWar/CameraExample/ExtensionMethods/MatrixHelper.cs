using System.Numerics;

namespace GraphicsWar.ExtensionMethods
{
    public static class MatrixHelper
    {
        public static Matrix4x4 CreateRotation(Vector3 rotation)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.Identity;

            rotationMatrix *= Matrix4x4.CreateRotationX(rotation.X);
            rotationMatrix *= Matrix4x4.CreateRotationY(rotation.Y);
            rotationMatrix *= Matrix4x4.CreateRotationZ(rotation.Z);

            return rotationMatrix;
        }
    }
}
