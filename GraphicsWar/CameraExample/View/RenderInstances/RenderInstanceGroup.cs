using GraphicsWar.Shared;
using System.Collections.Generic;
using System.Numerics;

namespace GraphicsWar.View.RenderInstances
{
    public class RenderInstanceGroup
    {
        private List<IRenderInstance> RenderInstances = new List<IRenderInstance>();

        public void UpdateGeometry(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach(var instance in RenderInstances)
            {
                var geom = instance as IUpdateTransforms;

                geom?.UpdateTransforms(transforms);
            }
        }

        public void UpdateResolution(int width, int height)
        {
            foreach (var instance in RenderInstances)
            {
                var reso = instance as IUpdateResolution;

                reso?.UpdateResolution(width, height);
            }
        }

        public T AddShader<T>(IRenderInstance shader) where T : IRenderInstance
        {
            RenderInstances.Add(shader);
            return (T)shader;
        }
    }
}
