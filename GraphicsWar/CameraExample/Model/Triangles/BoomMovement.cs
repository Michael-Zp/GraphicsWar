using System;
using System.Numerics;

namespace GraphicsWar.Model.Triangles
{
    public class BoomMovement : ITriangleMovement
    {
        private static readonly Random Rand;
        private static readonly Vector3 Gravity;

        public Vector3 Rotation { get; private set; }
        public float Scale { get; private set; }
        public event Action MovementFinished;

        private Vector3 _position;
        private Vector3 _movement;
        private readonly float _lifeTime;
        private float _timeAlive;

        static BoomMovement()
        {
            Rand = new Random();
            Gravity = new Vector3(0, -9.81f, 0);
        }

        public BoomMovement()
        {
            _position = Vector3.Zero;
            Rotation = new Vector3((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 1);
            _movement = Vector3.Normalize(new Vector3((float)Rand.NextDouble() * 2 - 1, (float)Rand.NextDouble() * 2 - 0.2f, (float)Rand.NextDouble() * 2 - 1));
            _movement *= ((float)Rand.NextDouble() * 0.75f + 1.0f);
            _lifeTime = (float)Rand.NextDouble() * 0.2f + 0.8f;
        }

        public Matrix4x4 CalculateMovement(float deltaTime)
        {
            _timeAlive += deltaTime;

            Matrix4x4 returnMatrix;

            if (_timeAlive > _lifeTime)
            {
                MovementFinished();
                returnMatrix = Matrix4x4.Identity;
            }
            else
            {
                _movement += Gravity * 0.2f * deltaTime;
                _position += _movement * deltaTime;
                returnMatrix = Matrix4x4.CreateTranslation(_position);

                Scale = 1.0f - (float)Math.Pow(((double)_timeAlive / _lifeTime), 4);
            }

            return returnMatrix;
        }
    }
}
