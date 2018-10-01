using System;
using System.Numerics;

namespace GraphicsWar.Model.Triangles
{
    public class ArchMovement : ITriangleMovement
    {
        private static readonly Random Rand;
        private const float StartOffset = 1;
        private const float Distance = 13;


        private float _translation;
        public Vector3 Rotation { get; private set; }
        private float _aimOffset;
        private float _archHeight;

        public event Action MovementFinished;

        static ArchMovement()
        {
            Rand = new Random();
        }

        public ArchMovement(float offset)
        {
            _translation = -StartOffset - offset;
            InitMovement();
        }
        


        private void InitMovement()
        {
            Rotation = new Vector3((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1);
            _aimOffset = (float)Rand.NextDouble() * 0.6f - 0.3f;
            _archHeight = 1 + 2 * (float)Rand.NextDouble();
        }

        public Matrix4x4 CalculateMovement(float deltaTime)
        {
            _translation -= deltaTime * 3;
            if (_translation < -Distance - StartOffset)
            {
                _translation = -StartOffset;
                MovementFinished();
                InitMovement();
            }

            float arch = 1 - (float)Math.Pow((double)((_translation + StartOffset) / (Distance / 2 + 0.5f)) + 1, 2);

            return Matrix4x4.CreateTranslation(new Vector3(_translation, arch * _archHeight, 0)) * Matrix4x4.CreateRotationY(_aimOffset);
        }
    }
}
