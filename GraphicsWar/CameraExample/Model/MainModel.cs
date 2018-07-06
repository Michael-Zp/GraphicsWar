using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class MainModel
    {
        public List<Entity> Entities = new List<Entity>();

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Type1, new Vector3(2.5f, 0, 0), new Vector3(0)));
            Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(-2.5f, 0, 0), new Vector3(0)));
            Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(0, 0, 0), new Vector3(0)));
            Entities.Add(new Entity(Enums.EntityType.Type3, new Vector3(0, -2, 2), new Vector3(0)));
        }

        public void Update(float deltaTime)
        {
            //Entities[0].Rotate(new Vector3(2f * deltaTime));
            //Entities[1].Rotate(new Vector3(2f * deltaTime));
            //Entities[2].Rotate(new Vector3(-2f * deltaTime));
        }
    }
}
