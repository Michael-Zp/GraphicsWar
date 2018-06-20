using GraphicsWar.Shared;
using System.Collections.Generic;
using System.Numerics;

namespace GraphicsWar.View.RenderInstances
{
    public interface IUpdateTransforms
    {
        void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms);
    }
}
