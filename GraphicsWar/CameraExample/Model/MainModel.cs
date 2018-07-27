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
        private Orbit _orbit1;
        private Orbit _orbit2;

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(2, 0, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 0.28f));
            _orbit1 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), Vector3.Zero);
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(2, 0, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 0.28f));
            _orbit2 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector3(0, (float)Math.PI, 0));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, 1f, -1f), Vector3.Zero, 1f));
        }

        public void Update(float deltaTime)
        {
            _orbit1.Update(deltaTime*10);
            Entities[0].AdditionalTransformation = _orbit1.Transformation;
            _orbit2.Update(deltaTime*10);
            Entities[1].AdditionalTransformation = _orbit2.Transformation;
        }
    }
}
