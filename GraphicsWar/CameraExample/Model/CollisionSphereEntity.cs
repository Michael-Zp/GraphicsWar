using System.Numerics;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class CollisionSphereEntity : Entity
    {
        public float CollisionSphereRadius;

        private Matrix4x4 _transformation;

        public new Matrix4x4 Transformation {
            get {
                return _transformation;
            }
            set {
                _transformation = value;

                PosX = _transformation.M41;
                PosY = _transformation.M42;
                PosZ = _transformation.M43;
            }
        }

        //Accessing the transformation matrix is expensive, thus do it like this
        public float PosX;
        public float PosY;
        public float PosZ;

        public CollisionSphereEntity(Enums.EntityType type, Vector3 position, Vector3 rotation, float collisionSphereRadius) : base(type, position, rotation)
        {
            CollisionSphereRadius = collisionSphereRadius;
            Transformation = base.Transformation;
        }
    }
}
