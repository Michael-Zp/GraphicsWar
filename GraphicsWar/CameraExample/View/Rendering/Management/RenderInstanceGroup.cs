using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;

namespace GraphicsWar.View.Rendering.Management
{
    public class RenderInstanceGroup
    {
        private readonly List<IRenderInstance> _renderInstances = new List<IRenderInstance>();

        public void UpdateGeometry(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach(var instance in _renderInstances)
            {
                var geom = instance as IUpdateTransforms;

                geom?.UpdateTransforms(transforms);
            }
        }

        public void UpdateResolution(int width, int height)
        {
            foreach (var instance in _renderInstances)
            {
                var reso = instance as IUpdateResolution;

                reso?.UpdateResolution(width, height);
            }
        }

        public T AddShader<T>(IRenderInstance shader) where T : IRenderInstance
        {
            _renderInstances.Add(shader);
            return (T)shader;
        }
    }
}
