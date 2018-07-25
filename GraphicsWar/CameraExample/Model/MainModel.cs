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
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(1f, 0, 0), new Vector3(0f, 0f, (float)Math.PI / 2), 0.28f));
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(-1f, 0, 0), new Vector3(0f, 0f, (float)Math.PI / 2), 0.28f));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, 1f, -1f), Vector3.Zero, 1f));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, -1f, -1f), Vector3.Zero, 1f));
        }

        public void Update(float deltaTime)
        {
            Entities[0].Rotate(new Vector3(-0.5f * deltaTime, 0f, 0f));
            Entities[1].Rotate(new Vector3(-0.5f * deltaTime, 0f, 0f));
        }
    }
}
