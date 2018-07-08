using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;

namespace GraphicsWar.View.Rendering.Management
{
    public interface IUpdateTransforms : IRenderInstance
    {
        void UpdateTransforms(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms);
    }
}
