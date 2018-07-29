using System;
using System.Collections.Generic;
using System.Numerics;
using GraphicsWar.Model.Movement;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class MainModel
    {
        public List<Entity> Entities = new List<Entity>();
        private readonly Orbit _orbit1;
        private readonly Orbit _orbit2;

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit1 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), Vector3.Zero);
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit2 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0, (float)Math.PI, 0));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, 10f, 0f), Vector3.Zero, 3f));
            //Entities.Add(new Entity(Enums.EntityType.Plane, Vector3.UnitY * 0.05f, new Vector3(0, 0.5f * (float)Math.PI, 0), new Vector3(201, 1, 201)));
            Entities.Add(new Entity(Enums.EntityType.TessellationPlane, Vector3.UnitY * -0.1f, Vector3.Zero, new Vector3(1000, 1, 1000)));
            Entities.Add(new Entity(Enums.EntityType.TessellationPlane, new Vector3(0, 1, 500), Vector3.UnitX * (float)Math.PI / 2, new Vector3(1000, 1, 10)));
            Entities.Add(new Entity(Enums.EntityType.TessellationPlane, new Vector3(0, 1, -500), Vector3.UnitX * -(float)Math.PI / 2, new Vector3(1000, 1, 10)));
            Entities.Add(new Entity(Enums.EntityType.TessellationPlane, new Vector3(-500, 1, 0), Vector3.UnitZ * -(float)Math.PI / 2, new Vector3(10, 1, 1000)));
            Entities.Add(new Entity(Enums.EntityType.TessellationPlane, new Vector3(500, 1, 0), Vector3.UnitZ * (float)Math.PI / 2, new Vector3(10, 1, 1000)));
        }

        public void Update(float deltaTime)
        {
            _orbit1.Update(deltaTime);
            Entities[0].AdditionalTransformation = _orbit1.Transformation;
            _orbit2.Update(deltaTime);
            Entities[1].AdditionalTransformation = _orbit2.Transformation;
        }
    }
}
