using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsWar.View.RenderInstances
{
    public abstract class RenderInstaceBase
    {
        public RenderInstaceBase(RenderInstanceGroup group)
        {
            group.RenderInstances.Add(this);
        }
    }
}
