namespace GraphicsWar.ExtensionMethods
{
    public static class BulletSharpMathExtensions
    {
        public static System.Numerics.Vector3 ToNumericsVector(this BulletSharp.Math.Vector3 thisVec)
        {
            return new System.Numerics.Vector3(thisVec.X, thisVec.Y, thisVec.Z);
        }
    }
}
