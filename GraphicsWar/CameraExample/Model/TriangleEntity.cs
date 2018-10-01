using System;
using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.Model
{
    public class TriangleEntity : Entity
    {
        private static readonly Random Rand;
        private const float StartOffset = 1;
        private const float Distance = 13;


        private float _translation;
        private Vector3 _rotation;
        private float _aimOffset;
        private float _archHeight;


        static TriangleEntity()
        {
            Rand = new Random();
        }

        public TriangleEntity(Enums.EntityType triangleType, Vector3 position, Vector3 rotation, float scale, float offset) : this(triangleType, position, rotation, new Vector3(scale), offset) { }

        public TriangleEntity(Enums.EntityType triangleType, Vector3 position, Vector3 rotation, Vector3 scale, float offset) : base(triangleType,
            position, rotation, scale)
        {
            _translation = -StartOffset - offset;
            InitShot();
        }

        public void Update(float deltaTime, Matrix4x4 shipTransformation)
        {
            Rotate(deltaTime * 3 * _rotation);
            _translation -= deltaTime * 3;
            if (_translation < -Distance - StartOffset)
            {
                _translation = -StartOffset;
                InitShot();
            }

            float arch = 1 - (float)Math.Pow((double)((_translation + StartOffset) / (Distance / 2 + 0.5f)) + 1, 2);

            AdditionalTransformation = Matrix4x4.CreateTranslation(new Vector3(_translation, arch * _archHeight, 0)) * Matrix4x4.CreateRotationY(_aimOffset) * shipTransformation;

            ;
        }




        private void InitShot()
        {
            _rotation = new Vector3((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1);
            _aimOffset = (float)Rand.NextDouble() * 0.6f - 0.3f;
            _archHeight = 1 + 2 * (float)Rand.NextDouble();
        }
    }
}
