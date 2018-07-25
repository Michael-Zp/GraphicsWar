﻿using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.Model
{
    public class Entity
    {
        public Enums.EntityType Type { get; }
        public Matrix4x4 Transformation => Matrix4x4.CreateScale(_scale) * CalculateRotation(_rotation) * Matrix4x4.CreateTranslation(_position);

        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale = Vector3.One;

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

        public void Scale(float scale) => Scale(new Vector3(scale, scale, scale));
        public void Scale(Vector3 scale)
        {
            _scale *= scale;
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
