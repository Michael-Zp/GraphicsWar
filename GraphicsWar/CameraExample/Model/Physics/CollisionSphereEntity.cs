using System.Numerics;
using GraphicsWar.Shared;

namespace GraphicsWar.Model.Physics
{
    public class CollisionSphereEntity : Entity, IPhysicsObject
    {
        public float CollisionSphereRadius;

        public float Mass { get; set; }

        //Accessing the transformation matrix is expensive, thus do it like this
        public float PosX {
            get {
                return _position.X;
            }
            set {
                _position.X = value;
            }
        }
        public float PosY {
            get {
                return _position.Y;
            }
            set {
                _position.Y = value;
            }
        }
        public float PosZ {
            get {
                return _position.Z;
            }
            set {
                _position.Z = value;
            }
        }

        public float VelocityX { get; set; } = 0;
        public float VelocityY { get; set; } = 0;
        public float VelocityZ { get; set; } = 0;
        public bool UseGravity { get; set; } = true;
        public bool MoveableByForce { get; set; } = true;

        public CollisionSphereEntity(Enums.EntityType type, Vector3 position, Vector3 rotation, float collisionSphereRadius, float mass, bool useGravity, bool moveableByForce) : base(type, position, rotation)
        {
            CollisionSphereRadius = collisionSphereRadius;
            Mass = mass;
            UseGravity = useGravity;
            MoveableByForce = moveableByForce;
        }
    }
}
