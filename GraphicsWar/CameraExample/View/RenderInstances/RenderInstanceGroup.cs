using GraphicsWar.Shared;
using System.Collections.Generic;
using System.Numerics;

namespace GraphicsWar.View.RenderInstances
{
    public class RenderInstanceGroup
    {
        public List<RenderInstanceBase> RenderInstances = new List<RenderInstanceBase>();

        public void UpdateGeometry(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            foreach(var instance in RenderInstances)
            {
                var geom = instance as IUpdateGeometry;

                geom?.UpdateAttributes(transforms);
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
    }
}
