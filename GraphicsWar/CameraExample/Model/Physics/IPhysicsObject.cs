namespace GraphicsWar.Model.Physics
{
    public interface IPhysicsObject
    {
        float Mass { get; set; }

        bool UseGravity { get; set; }

        bool MoveableByForce { get; set; }

        float VelocityX { get; set; }
        float VelocityY { get; set; }
        float VelocityZ { get; set; }

        float PosX { get; set; }
        float PosY { get; set; }
        float PosZ { get; set; }
    }
}
