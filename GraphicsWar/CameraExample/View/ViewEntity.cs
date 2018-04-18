using GraphicsWar.Shared;
using System.Numerics;

namespace GraphicsWar.View
{
    public struct ViewEntity
    {
        public Enums.EntityType Type { get; private set; }
        public Matrix4x4 Transform { get; private set; }

        public ViewEntity(Enums.EntityType type, Matrix4x4 transformation)
        {
            Type = type;
            Transform = transformation;
        }
    }
}
