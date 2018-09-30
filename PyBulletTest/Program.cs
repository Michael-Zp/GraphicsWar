using BulletSharp;
using BulletSharp.Math;
using System;
using System.Diagnostics;

namespace PyBulletTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int numThreads = 8;
            
            Threads.TaskScheduler = Threads.GetPplTaskScheduler();
            Threads.TaskScheduler.NumThreads = numThreads;

            CollisionConfiguration CollisionConf = null;
            using (var collisionConfigurationInfo = new DefaultCollisionConstructionInfo
            {
                DefaultMaxPersistentManifoldPoolSize = 80000,
                DefaultMaxCollisionAlgorithmPoolSize = 80000
            })
            {
                CollisionConf = new DefaultCollisionConfiguration(collisionConfigurationInfo);
            };


            var Dispatcher = new CollisionDispatcherMultiThreaded(CollisionConf);
            var broad = new DbvtBroadphase();
            var constraintSolver = new ConstraintSolverPoolMultiThreaded(numThreads);
            var world = new DiscreteDynamicsWorldMultiThreaded(Dispatcher, broad, constraintSolver, null, CollisionConf);
            world.SolverInfo.SolverMode = SolverModes.Simd | SolverModes.UseWarmStarting;
            var sphereA = new SphereShape(0.1f);

            RigidBody sphereBody = null;
            Random rand = new Random(DateTime.Now.Millisecond + DateTime.UtcNow.Second);
            Func<Random, float, float> getRandom = (locRand, range) => { return ((float)locRand.NextDouble() * 2 - 1) * range; };
            Vector3 baseVec = new Vector3(0, 1, 0);
            for (int i = 0; i < 20000; i++)
            {
                BulletSharp.Math.Vector3 randVec = new BulletSharp.Math.Vector3(getRandom(rand, 500), getRandom(rand, 1), getRandom(rand, 500));
                var rbInfo = new RigidBodyConstructionInfo(1.0f, new DefaultMotionState(Matrix.Translation(baseVec + randVec)), sphereA);
                sphereBody = new RigidBody(rbInfo);
                world.AddRigidBody(sphereBody);
            }

            var groundShape = new Box2DShape(100, 0.1f, 100);
            var rbInfoGround = new RigidBodyConstructionInfo(0.0f, new DefaultMotionState(Matrix.Translation(0, -1, 0)), groundShape);
            var groundBody = new RigidBody(rbInfoGround);

            world.Gravity = new Vector3(0, -9.81f, 0);
            world.AddRigidBody(groundBody);

            Stopwatch sw = new Stopwatch();
            float averageTime = 0;
            while (true)
            {
                sw.Restart();
                world.StepSimulation(0.1f);
                averageTime += sw.ElapsedMilliseconds * 0.1f;
                averageTime /= 1.1f;
                
                //System.Console.WriteLine(sphereBody.WorldTransform.Origin);
                System.Console.WriteLine(averageTime);
            }
        }
    }
}
