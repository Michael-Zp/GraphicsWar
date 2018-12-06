using System.Numerics;
using GraphicsWar.ExtensionMethods;

namespace GraphicsWar.Model.Movement
{
    class Orbit
    {
        public Matrix4x4 Transformation
        {
            get
            {
                Matrix4x4 returnMat = Matrix4x4.CreateTranslation(Radius) * MatrixHelper.CreateRotation(_rotation) *
                                      Matrix4x4.CreateTranslation(Center);
                if (_selfRotated)
                {
                    returnMat = MatrixHelper.CreateRotation(-_rotation) * returnMat;
                }

                return returnMat;
            }
        }

        public Vector3 Center { private get; set; }
        public Vector3 Radius { private get; set; }
        public Vector3 Speed { private get; set; }

        private Vector3 _rotation;

        private readonly bool _selfRotated;

        public Orbit(Vector3 radius, Vector3 speed, Vector3 center, Vector3 initialRotation, bool selfRotated = false)
        {
            _rotation = initialRotation;

            Radius = radius;
            Speed = speed;
            Center = center;
            _selfRotated = selfRotated;
        }

        public void Update(float deltaTime)
        {
            _rotation += Speed * deltaTime;
        }
    }
}
