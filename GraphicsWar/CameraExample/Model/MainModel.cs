using System;
using System.Collections.Generic;
using System.Numerics;
using BulletSharp;
using GraphicsWar.ExtensionMethods;
using GraphicsWar.Model.Movement;
using GraphicsWar.Shared;

namespace GraphicsWar.Model
{
    public class MainModel
    {
        public List<Entity> Entities = new List<Entity>();
        private readonly List<int> _sphereIndices = new List<int>();
        private readonly List<RigidBody> _sphereBodies = new List<RigidBody>();
        private readonly Orbit _orbit1;
        private readonly Orbit _orbit2;
        //private readonly DiscreteDynamicsWorldMultiThreaded _world;
        private readonly DiscreteDynamicsWorld _world;

        public MainModel()
        {
            Entities.Add(new Entity(Enums.EntityType.Nvidia, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit1 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), Vector3.Zero);
            Entities.Add(new Entity(Enums.EntityType.Radeon, new Vector3(5, 15, 0), new Vector3((float)Math.PI, (float)Math.PI / 2, (float)Math.PI / 30), 1f));
            _orbit2 = new Orbit(new Vector3(2, 0, 0), new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0), new Vector3(0, (float)Math.PI, 0));
            Entities.Add(new Entity(Enums.EntityType.Sphere, new Vector3(0f, 10f, 0f), Vector3.Zero, 3f));
            for (int i = 0; i < 6; i++)
            {
                Entities.Add(new TriangleEntity(new Vector3(5, 15, 0), Vector3.Zero, 0.4f, i * 2));
            }


            int maxNumThreads = 4;
            Threads.TaskScheduler = Threads.GetPplTaskScheduler();
            Threads.TaskScheduler.NumThreads = maxNumThreads;

            CollisionConfiguration CollisionConf = null;
            using (var collisionConfigurationInfo = new DefaultCollisionConstructionInfo
            {
                DefaultMaxPersistentManifoldPoolSize = 80000,
                DefaultMaxCollisionAlgorithmPoolSize = 80000
            })
            {
                CollisionConf = new DefaultCollisionConfiguration(collisionConfigurationInfo);
            };

            //var Dispatcher = new CollisionDispatcherMultiThreaded(CollisionConf);
            //var broad = new DbvtBroadphase();
            //var constraintSolver = new ConstraintSolverPoolMultiThreaded(maxNumThreads);
            //_world = new DiscreteDynamicsWorldMultiThreaded(Dispatcher, broad, constraintSolver, null, CollisionConf);
            //_world.SolverInfo.SolverMode = SolverModes.Simd | SolverModes.UseWarmStarting;
            var config = new DefaultCollisionConfiguration();
            _world = new DiscreteDynamicsWorld(new CollisionDispatcher(config), new AxisSweep3(new BulletSharp.Math.Vector3(-1000), new BulletSharp.Math.Vector3(1000)), null, config);

            _world.Gravity = new BulletSharp.Math.Vector3(0, -9.81f, 0);

            Random rand = new Random(DateTime.Now.Millisecond + DateTime.UtcNow.Second);
            Func<Random, float, float> getRandom = (locRand, range) => { return ((float)locRand.NextDouble() * 2 - 1) * range; };

            var size = 0.3f;
            var sphereShape = new SphereShape(size);
            BulletSharp.Math.Vector3 baseSpherePos = new BulletSharp.Math.Vector3(.1f, 80, 0);
            for (int i = 0; i < 2000; i++)
            {
                BulletSharp.Math.Vector3 randVec = new BulletSharp.Math.Vector3(getRandom(rand, 2), getRandom(rand, 20), getRandom(rand, 2));
                randVec += baseSpherePos;

                Entities.Add(new Entity(Enums.EntityType.FluidSphere, randVec.ToNumericsVector(), Vector3.Zero, size));
                _sphereIndices.Add(Entities.Count - 1);

                var rbInfoSphere = new RigidBodyConstructionInfo(1.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(randVec)), sphereShape);
                _sphereBodies.Add(new RigidBody(rbInfoSphere));

                _world.AddRigidBody(_sphereBodies[i]);
            }

            var bigSphereShape = new SphereShape(3f);
            var rbInfoBigSphere = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(0, 10, 0)), bigSphereShape);
            var bigSphereBody = new RigidBody(rbInfoBigSphere);

            var groundShape = new Box2DShape(20, 5f, 20);
            var rbInfoGround = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(0, 30, 0)), groundShape);
            var groundBody = new RigidBody(rbInfoGround);

            var groundShape2 = new Box2DShape(5, 1000, 1000);
            var rbInfoGround2 = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(12, 30, 0)), groundShape2);
            var groundBody2 = new RigidBody(rbInfoGround2);

            var groundShape3 = new Box2DShape(5, 1000, 1000);
            var rbInfoGround3 = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(-12, 30, 0)), groundShape3);
            var groundBody3 = new RigidBody(rbInfoGround3);

            var groundShape4 = new Box2DShape(1000, 1000, 5);
            var rbInfoGround4 = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(0, 30, 12)), groundShape4);
            var groundBody4 = new RigidBody(rbInfoGround4);

            var groundShape5 = new Box2DShape(1000, 1000, 5);
            var rbInfoGround5 = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(BulletSharp.Math.Matrix.Translation(0, 30, -12)), groundShape5);
            var groundBody5 = new RigidBody(rbInfoGround5);

            _world.AddRigidBody(bigSphereBody);
            _world.AddRigidBody(groundBody);
            _world.AddRigidBody(groundBody2);
            _world.AddRigidBody(groundBody3);
            _world.AddRigidBody(groundBody4);
            _world.AddRigidBody(groundBody5);
        }

        public void Update(float deltaTime)
        {
            Random rand = new Random(DateTime.Now.Millisecond + DateTime.UtcNow.Second);
            Func<Random, float, float> getRandom = (locRand, range) => { return ((float)locRand.NextDouble() * 2 - 1) * range; };
            
            _world.StepSimulation(deltaTime);
            for (var i = 0; i < _sphereIndices.Count; i++)
            {
                Entities[_sphereIndices[i]].Position = _sphereBodies[i].WorldTransform.Origin.ToNumericsVector();
                //_sphereBodies[i].LinearVelocity += new BulletSharp.Math.Vector3(getRandom(rand, 0.5f), getRandom(rand, 0.1f), getRandom(rand, 0.5f));
            }


            _orbit1.Update(deltaTime);
            Entities[0].AdditionalTransformation = _orbit1.Transformation;
            _orbit2.Update(deltaTime);
            Entities[1].AdditionalTransformation = _orbit2.Transformation;
            Entities[2].Rotate(new Vector3(0, -deltaTime, 0));
            foreach (var entity in Entities)
            {
                if (entity is TriangleEntity triangleEntity)
                {
                    triangleEntity.Update(deltaTime, _orbit1.Transformation);
                }
            }
        }
    }
}
