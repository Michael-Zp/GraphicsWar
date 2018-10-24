using System;
using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Model.Movement;
using GraphicsWar.Model.Triangles;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class MainModel
    {
        public List<Entity> Entities = new List<Entity>();
        public List<Entity> EntitiesToDelete = new List<Entity>();
        private List<int> _baseTriangleIndices = new List<int>();
        private readonly Orbit _orbit1;
        private readonly Orbit _orbit2;

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit1 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), Vector3.Zero);
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit2 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0, (float)Math.PI, 0));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, 10f, 0f), Vector3.Zero, 3f));
            for (int i = 0; i < 6; i++)
            {
                TriangleEntity triangle = new TriangleEntity(Enums.EntityType.NvidiaParticle, new Vector3(5, 15, 0), Vector3.Zero, 0.4f, new ArchMovement(i * 2));
                triangle.TriangleDied += (position) => TriangleDied(position, triangle.Type);
                Entities.Add(triangle);
                _baseTriangleIndices.Add(Entities.Count - 1);
            }
            for (int i = 0; i < 6; i++)
            {
                TriangleEntity triangle = new TriangleEntity(Enums.EntityType.RadeonParticle, new Vector3(5, 15, 0), Vector3.Zero, 0.4f, new ArchMovement(i * 2));
                triangle.TriangleDied += (position) => TriangleDied(position, triangle.Type);
                Entities.Add(triangle);
                _baseTriangleIndices.Add(Entities.Count - 1);
            }
        }

        public void Update(float deltaTime)
        {
            _orbit1.Update(deltaTime);
            Entities[0].AdditionalTransformation = _orbit1.Transformation;
            _orbit2.Update(deltaTime);
            Entities[1].AdditionalTransformation = _orbit2.Transformation;
            Entities[2].Rotate(new Vector3(0, -deltaTime, 0));
            for(int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is TriangleEntity triangleEntity)
                {
                    if(_baseTriangleIndices.Contains(i))
                    {
                        switch (triangleEntity.Type)
                        {
                            case Enums.EntityType.NvidiaParticle:
                                triangleEntity.Update(deltaTime, _orbit1.Transformation);
                                break;

                            case Enums.EntityType.RadeonParticle:
                                triangleEntity.Update(deltaTime, _orbit2.Transformation);
                                break;

                            default:
                                Console.WriteLine("No origin for triangle found.");
                                break;
                        }
                    }
                    else
                    {
                        triangleEntity.Update(deltaTime, Matrix4x4.Identity);
                    }
                }
            }

            foreach (Entity entity in EntitiesToDelete)
            {
                Entities.Remove(entity);
            }
            EntitiesToDelete.Clear();
        }

        private void TriangleDied(Vector3 position, Enums.EntityType triangleType)
        {
            for(int i = 0; i < 10; i++)
            {
                TriangleEntity triangle = new TriangleEntity(triangleType, position, Vector3.Zero, 0.3f, new BoomMovement());
                triangle.TriangleDied += (p) => EntitiesToDelete.Add(triangle);
                Entities.Add(triangle);
            }
        }
    }
}
