using GraphicsWar.Shared;
using System.Numerics;
using GraphicsWar.ExtensionMethods;

namespace GraphicsWar.Model
{
    public class Entity
    {
        public Enums.EntityType Type { get; }

        public Matrix4x4 AdditionalTransformation { private get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Transformation => Matrix4x4.CreateScale(ScalingFactor) * MatrixHelper.CreateRotation(Rotation) * Matrix4x4.CreateTranslation(Position) * AdditionalTransformation;

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 ScalingFactor;


        public Entity(Enums.EntityType type, Vector3 position, Vector3 rotation, float scale) : this(type, position,
            rotation, new Vector3(scale))
        { }

        public Entity(Enums.EntityType type, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
            ScalingFactor = scale;
        }

        public void Rotate(Vector3 rotation)
        {
            Rotation += rotation;
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }
        
        public void Scale(float scale) => Scale(new Vector3(scale));
        public void Scale(Vector3 scale)
        {
            ScalingFactor *= scale;
        }
    }
}
