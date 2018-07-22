using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.Model.Physics
{
    public class CollisionCubeEntity : Entity, IPhysicsObject
    {
        public float SizeX;
        public float SizeY;
        public float SizeZ;

        public float MinX;
        public float MinY;
        public float MinZ;

        public float MaxX;
        public float MaxY;
        public float MaxZ;

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
        public bool UseGravity { get; set; } = false;
        public bool MoveableByForce { get; set; } = false;

        public CollisionCubeEntity(Enums.EntityType type, Vector3 position, Vector3 rotation, Vector3 size, float mass, bool useGravity, bool moveableByForce) : base(type, position, rotation)
        {
            SizeX = size.X;
            SizeY = size.Y;
            SizeZ = size.Z;

            float halfX = SizeX / 2;
            float halfY = SizeY / 2;
            float halfZ = SizeZ / 2;

            MinX = position.X - halfX;
            MinY = position.Y - halfY;
            MinZ = position.Z - halfZ;
            
            MaxX = position.X + halfX;
            MaxY = position.Y + halfY;
            MaxZ = position.Z + halfZ;

            Mass = mass;
            UseGravity = useGravity;
            MoveableByForce = moveableByForce;
        }


    }
}
