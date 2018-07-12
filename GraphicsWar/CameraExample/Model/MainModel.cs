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
            Entities.Add(new Entity(Enums.EntityType.Type1, new Vector3(2.5f, 0, 0), new Vector3(0)));
            //Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(-2.5f, 0, 0), new Vector3(0)));
            //Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(0, 0, 0), new Vector3(0)));
            Entities.Add(new Entity(Enums.EntityType.Type3, new Vector3(0, 0, 0), new Vector3(0, 0, 0)));
            Entities.Add(new Entity(Enums.EntityType.Type4, new Vector3(-3, 0, 0), new Vector3(0, 0, 0)));


            List<Entity> collisionSpheres = new List<Entity>();
            
            for(int i = 0; i < 30000; i++)
            {
                float rand1 = (i * 50 - 25) % 25;
                float rand2 = (i * 50 - 25) % 25;
                float rand3 = (i * 50 - 25) % 25;
                float rand4 = ((i * 10) % 10) + 1;

                collisionSpheres.Add(new CollisionSphereEntity(Enums.EntityType.Type1, new Vector3(rand1, rand2, rand3), new Vector3(0), rand4));
            }
            
            CollisionDetection.InitializeCollisionDetectionForFrame(collisionSpheres, 100, new Vector3(0));
        }

        public void Update(float deltaTime)
        {
            //Entities[0].Rotate(new Vector3(2f * deltaTime));
            //Entities[1].Rotate(new Vector3(2f * deltaTime));
            //Entities[2].Rotate(new Vector3(-2f * deltaTime));
            //Entities[1].Rotate(new Vector3(1, 0, 0) * deltaTime);
        }
    }
}
