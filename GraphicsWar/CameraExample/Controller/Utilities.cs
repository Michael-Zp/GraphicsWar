using GraphicsWar.Model;
using GraphicsWar.View;
using System.Collections.Generic;

namespace GraphicsWar.Controller
{
    public static class Utilities
    {
        public static IEnumerable<ViewEntity> ToViewEntities(this IEnumerable<Entity> entities)
        {
            List<ViewEntity> viewEntities = new List<ViewEntity>();
            foreach (var entity in entities)
            {
                viewEntities.Add(new ViewEntity(entity.Type, entity.Transformation));
            }
            return viewEntities;
        }
    }
}
