using System;
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
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(1f, 0, 0), new Vector3(0)));
            Entities[0].Scale(0.28f);
            Entities[0].Rotate(new Vector3(0f, 0f, (float)Math.PI / 2));
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(-1f, 0, 0), new Vector3(0)));
            Entities[1].Scale(0.28f);
            Entities[1].Rotate(new Vector3(0f, 0f, (float)Math.PI / 2));
        }

        public void Update(float deltaTime)
        {
            Entities[0].Rotate(new Vector3(-0.5f * deltaTime, 0f, 0f));
            Entities[1].Rotate(new Vector3(-0.5f * deltaTime, 0f, 0f));
        }
    }
}
