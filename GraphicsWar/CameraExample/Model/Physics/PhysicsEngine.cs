using System.Collections.Generic;
using System.Linq;

namespace GraphicsWar.Model.Physics
{
    public static class PhysicsEngine
    {
        public static float Gravity = 9.81f;

        public static void ApplyPhysics(List<Entity> entities, float deltaTime)
        {
            IEnumerable<IPhysicsObject> collisionEntities =
                from entity in entities
                where entity is IPhysicsObject
                select entity as IPhysicsObject;

            foreach (var entity in collisionEntities)
            {
                if(entity.UseGravity)
                {
                    entity.VelocityY -= Gravity * deltaTime;
                }

                entity.PosX += entity.VelocityX * deltaTime;
                entity.PosY += entity.VelocityY * deltaTime;
                entity.PosZ += entity.VelocityZ * deltaTime;
            }
        }
    }
}
