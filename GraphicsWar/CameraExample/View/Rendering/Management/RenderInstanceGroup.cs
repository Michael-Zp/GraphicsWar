using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.Shared;

namespace GraphicsWar.View.Rendering.Management
{
    public class RenderInstanceGroup
    {
        private readonly List<IRenderInstance> _renderInstances = new List<IRenderInstance>();

        public void UpdateGeometry(Dictionary<Enums.EntityType, List<Matrix4x4>> transforms)
        {
            Dictionary<Enums.EntityType, Matrix4x4[]> arrTrans = new Dictionary<Enums.EntityType, Matrix4x4[]>();
            
            foreach(var type in transforms.Keys)
            {
                arrTrans.Add(type, transforms[type].ToArray());
            }

            foreach(var instance in _renderInstances)
            {
                if(instance is IUpdateTransforms geom)
                {
                    geom.UpdateTransforms(arrTrans);
                }
            }
        }

        public void UpdateResolution(int width, int height)
        {
            foreach (var instance in _renderInstances)
            {
                if(instance is IUpdateResolution reso)
                {
                    reso.UpdateResolution(width, height);
                }
            }
        }

        public T AddShader<T>(IRenderInstance shader) where T : IRenderInstance
        {
            _renderInstances.Add(shader);
            return (T)shader;
        }
    }
}
