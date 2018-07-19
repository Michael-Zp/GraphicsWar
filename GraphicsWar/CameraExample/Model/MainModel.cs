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


            int sphereCount = 60000;
            float treeSize = 100;

            List<Entity> collisionSpheres = new List<Entity>();
            CollisionOctree collisionOctree = new CollisionOctree();
            collisionOctree.InitializeNewOctree(5, new Vector3(0, 0, 0), treeSize);


            for (int i = 0; i < sphereCount; i++)
            {
                float radius = (float)Math.Pow((treeSize * treeSize * treeSize) / sphereCount, 1.0f / 3.0f);

                float rowColumnDepthSize = 100.0f / radius;

                int row = (int)(i % rowColumnDepthSize);
                int column = (int)(((float)Math.Floor((radius * i) / treeSize) % rowColumnDepthSize));
                int depth = (int)(((float)Math.Floor((radius * i) / (treeSize * treeSize)) % rowColumnDepthSize));

                float xPos = -(treeSize / 2.0f) + radius / 2.0f + radius * row;
                float yPos = -(treeSize / 2.0f) + radius / 2.0f + radius * column;
                float zPos = -(treeSize / 2.0f) + radius / 2.0f + radius * depth;
                
                collisionSpheres.Add(new CollisionSphereEntity(Enums.EntityType.Type1, new Vector3(xPos, yPos, zPos), new Vector3(0), radius / 8.0f, i));
            }

            collisionOctree.InsertIntoOctree(collisionSpheres);
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
