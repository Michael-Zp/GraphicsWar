using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.Model.Triangles
{
    public class TriangleEntity : Entity
    {
        public delegate void TriangleDiedHandler(Vector3 position);
        public event TriangleDiedHandler TriangleDied;

        private ITriangleMovement _movement;


        public TriangleEntity(Enums.EntityType triangleType, Vector3 position, Vector3 rotation, float scale, ITriangleMovement movement) : this(triangleType, position, rotation, new Vector3(scale), movement) { }

        public TriangleEntity(Enums.EntityType triangleType, Vector3 position, Vector3 rotation, Vector3 scale, ITriangleMovement movement) : base(triangleType,
            position, rotation, scale)
        {
            _movement = movement;
            _movement.MovementFinished += () => TriangleDied(Transformation.Translation);
        }

        public void Update(float deltaTime, Matrix4x4 parentTransformation)
        {
            Rotate(deltaTime * 3 * _movement.Rotation);
            ScaleFactor = _movement.Scale;
            AdditionalTransformation = _movement.CalculateMovement(deltaTime) * parentTransformation;
        }




    }
}
