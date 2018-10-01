using System;
using System.Numerics;

namespace GraphicsWar.Model.Triangles
{
    public class BoomMovement : ITriangleMovement
    {
        private static readonly Random Rand;

        public Vector3 Rotation { get; private set; }

        public event Action MovementFinished;

        static BoomMovement()
        {
            Rand = new Random();
        }

        public BoomMovement()
        {
            Rotation = new Vector3((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1);
        }

        public Matrix4x4 CalculateMovement(float deltaTime)
        {
            return Matrix4x4.Identity;
        }
    }
}
