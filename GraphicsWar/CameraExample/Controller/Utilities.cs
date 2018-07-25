using GraphicsWar.Model;
using GraphicsWar.View;
using System.Collections.Generic;
using System.Linq;

namespace GraphicsWar.Controller
{
    public static class Utilities
    {
        public static List<ViewEntity> ToViewEntities(this IEnumerable<Entity> entities)
        {
            return entities.Select(entity => new ViewEntity(entity.Type, entity.Transformation)).ToList();
        }
    }
}
