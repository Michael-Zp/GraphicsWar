using System;
using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Model.Physics;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class MainModel
    {
        public List<Entity> Entities = new List<Entity>();

        private CollisionOctree _collisionOctree;

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Type1, new Vector3(2.5f, 0, 0), new Vector3(0)));
            //Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(-2.5f, 0, 0), new Vector3(0)));
            //Entities.Add(new Entity(Enums.EntityType.Type2, new Vector3(0, 0, 0), new Vector3(0)));
            Entities.Add(new Entity(Enums.EntityType.Type3, new Vector3(0, 0, 0), new Vector3(0, 0, 0)));
            Entities.Add(new Entity(Enums.EntityType.Type4, new Vector3(-3, 0, 0), new Vector3(0, 0, 0)));


            int sphereCount = 6000;
            float treeSize = 100;

            List<Entity> collisionSpheres = new List<Entity>();
            _collisionOctree = new CollisionOctree();
            _collisionOctree.InitializeNewOctree(5, new Vector3(0, 0, 0), treeSize);

            int row = 0, column = 0, depth = 0;

            for (int i = 0; i < sphereCount; i++)
            {
                float radius = (float)Math.Pow((treeSize * treeSize * treeSize) / sphereCount, 1.0f / 3.0f);

                float rowColumnDepthSize = 100.0f / radius;

                float xPos = -(treeSize / 2.0f) + radius / 2.0f + radius * row;
                float yPos = -(treeSize / 2.0f) + radius / 2.0f + radius * column;
                float zPos = -(treeSize / 2.0f) + radius / 2.0f + radius * depth;

                
                //float radius = 0.5f;
                //float xPos = 5;
                //float yPos = 0;
                //float zPos = -i;

                collisionSpheres.Add(new CollisionSphereEntity(Enums.EntityType.Type5, new Vector3(xPos, yPos, zPos), new Vector3(0), 0.3f, 1, true, true));

                row++;
                if (row > rowColumnDepthSize)
                {
                    row = 0;
                    column++;
                    if (column > rowColumnDepthSize)
                    {
                        column = 0;
                        depth++;
                    }
                }
            }

            Entities.AddRange(collisionSpheres);

            Entities.Add(new CollisionCubeEntity(Enums.EntityType.Type6, new Vector3(0, -20, 0), new Vector3(0), new Vector3(1000, 10, 1000), 10000, false, false));
            
            _collisionOctree.InsertIntoOctree(Entities);
        }

        public void Update(float deltaTime)
        {
            //Entities[0].Rotate(new Vector3(2f * deltaTime));
            //Entities[1].Rotate(new Vector3(2f * deltaTime));
            //Entities[2].Rotate(new Vector3(-2f * deltaTime));
            //Entities[1].Rotate(new Vector3(1, 0, 0) * deltaTime);

            _collisionOctree.ResetOctree();
            _collisionOctree.InsertIntoOctree(Entities);
            _collisionOctree.CheckCollisionsInTree();

            PhysicsEngine.ApplyPhysics(Entities, deltaTime);
        }
    }
}
