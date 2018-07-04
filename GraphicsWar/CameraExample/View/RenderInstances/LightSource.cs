using System.Numerics;

namespace GraphicsWar.View.RenderInstances
{
    public class LightSource
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Color;
        public float Intensity;

        public LightSource(Vector3 position, Vector3 direction, Vector3 color, float intensity)
        {
            Position = position;
            Direction = direction;
            Color = color;
            Intensity = intensity;
        }

        public static LightSource DefaultLightSource = new LightSource(Vector3.Zero, Vector3.Zero, Vector3.Zero, 0);
    }
}