using System;
using System.Numerics;

namespace GraphicsWar.Model.Triangles
{
    public interface ITriangleMovement
    {
        Matrix4x4 CalculateMovement(float deltaTime);

        Vector3 Rotation { get; }
        float Scale { get; }

        event Action MovementFinished;
    }
}
