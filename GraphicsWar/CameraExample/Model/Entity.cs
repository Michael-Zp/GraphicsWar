using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.Model
{
    public class Entity
    {
        public Enums.EntityType Type { get; }
        public Matrix4x4 Transformation => CalculateRotation(_rotation) * Matrix4x4.CreateTranslation(_position);

        private Vector3 _position;
        private Vector3 _rotation;

        public Entity(Enums.EntityType type, Vector3 position, Vector3 rotation)
        {
            Type = type;
            _position = position;
            _rotation = rotation;
        }

        public void Rotate(Vector3 rotation)
        {
            _rotation += rotation;
        }

        public void Translate(Vector3 translation)
        {
            _position += translation;
        }

        private Matrix4x4 CalculateRotation(Vector3 rotVec)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.Identity;

            rotationMatrix *= Matrix4x4.CreateRotationX(rotVec.X);
            rotationMatrix *= Matrix4x4.CreateRotationY(rotVec.Y);
            rotationMatrix *= Matrix4x4.CreateRotationZ(rotVec.Z);

            return rotationMatrix;
        }


    }
}
