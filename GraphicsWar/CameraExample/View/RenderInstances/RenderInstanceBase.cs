namespace GraphicsWar.View.RenderInstances
{
    public abstract class RenderInstanceBase
    {
        public RenderInstanceBase(RenderInstanceGroup group)
        {
            group.RenderInstances.Add(this);
        }
    }
}
