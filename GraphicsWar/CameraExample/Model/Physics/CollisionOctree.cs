using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace GraphicsWar.Model.Physics
{
    public class CollisionOctree
    {
        private OctreeNode _octreeRoot;

        private Random _random;

        private int _levels;

        private readonly int _threadCount = 8;


        //INIT
        public void InitializeNewOctree(int levels, Vector3 center, float size)
        {
            _levels = levels;

            _random = new Random(DateTime.Now.Millisecond);

            _octreeRoot = new OctreeNode(center.X, center.Y, center.Z, size, size / 2, size / 4, 0, null);
            InitializeOctreeRoot(_octreeRoot, _levels);
        }


        //INSERT
        public void InsertIntoOctree(List<Entity> entities)
        {
            int entitiesPerThread = (int)Math.Floor((double)entities.Count / _threadCount);

            ManualResetEvent[] doneInserts = new ManualResetEvent[_threadCount];

            for (int i = 0; i < _threadCount - 1; i++)
            {
                doneInserts[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(InsertCallback, new InsertParameters(i * entitiesPerThread, (i + 1) * entitiesPerThread, entities, doneInserts[i]));
            }

            doneInserts[doneInserts.Length - 1] = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(InsertCallback, new InsertParameters((_threadCount - 1) * entitiesPerThread, entities.Count, entities, doneInserts[doneInserts.Length - 1]));

            for (int i = 0; i < doneInserts.Length; i++)
            {
                doneInserts[i].WaitOne();
            }
        }

        private struct InsertParameters
        {
            public readonly int Start;
            public readonly int End;
            public readonly List<Entity> List;
            public ManualResetEvent doneEvent;

            public InsertParameters(int start, int end, List<Entity> list, ManualResetEvent doneEvent)
            {
                Start = start;
                End = end;
                List = list;
                this.doneEvent = doneEvent;
            }
        }

        private void InsertCallback(Object insertParam)
        {
            InsertParameters param = (InsertParameters)insertParam;

            for (int k = param.Start; k < param.End; k++)
            {
                if (param.List[k] is CollisionSphereEntity sphere)
                {
                    if (sphere.CollisionSphereRadius >= _octreeRoot.Size)
                    {
                        _octreeRoot.CollisionSphereEntitiesMutex.WaitOne();
                        _octreeRoot.CollisionSphereEntities.Add(sphere);
                        _octreeRoot.CollisionSphereEntitiesMutex.ReleaseMutex();
                    }
                    else
                    {
                        InsertIntoOctree(_octreeRoot, sphere);
                    }
                }
                else if (param.List[k] is CollisionCubeEntity cube)
                {
                    bool xBigger = cube.SizeX >= _octreeRoot.Size;
                    bool yBigger = cube.SizeY >= _octreeRoot.Size;
                    bool zBigger = cube.SizeZ >= _octreeRoot.Size;

                    if (xBigger || yBigger || zBigger)
                    {
                        _octreeRoot.CollisionCubeEntitiesMutex.WaitOne();
                        _octreeRoot.CollisionCubeEntities.Add(cube);
                        _octreeRoot.CollisionCubeEntitiesMutex.ReleaseMutex();
                    }
                    else
                    {
                        InsertIntoOctree(_octreeRoot, cube);
                    }
                }
            }

            param.doneEvent.Set();
        }

        //CHECK
        public void CheckCollisions() => CheckCollisions(_octreeRoot);

        //RESET
        public void ResetOctree() => ResetOctree(_octreeRoot);


        //Private

        private void InitializeOctreeRoot(OctreeNode current, int level)
        {
            if (level == 0)
            {
                return;
            }

            current.AddChilds();

            OctreeNode currentChild = current.FirstChild;

            while (currentChild != null)
            {
                InitializeOctreeRoot(currentChild, level - 1);
                currentChild = currentChild.NextSibling;
            }
        }


        private void CheckCollisionsInTree(OctreeNode current)
        {
            if (current == null)
            {
                return;
            }

            CheckCollisionsInNode(current);

            OctreeNode currentChild = current.FirstChild;

            while (currentChild != null)
            {
                CheckCollisionsInTree(currentChild);
                currentChild = currentChild.NextSibling;
            }
        }
        
        private void CheckCollisionsCallback(Object checkCollisionsParameters)
        {
            CheckCollisionsParameters param = (CheckCollisionsParameters)checkCollisionsParameters;

            CheckCollisionsInTree(param.Node);

            param.DoneHandle.Set();
        }

        private struct CheckCollisionsParameters
        {
            public OctreeNode Node;
            public ManualResetEvent DoneHandle;

            public CheckCollisionsParameters(OctreeNode node, ManualResetEvent doneHandle)
            {
                Node = node;
                DoneHandle = doneHandle;
            }
        }


        private void CheckCollisions(OctreeNode current)
        {
            ManualResetEvent[] doneChecks = new ManualResetEvent[_threadCount];

            CheckCollisionsInNode(current);

            OctreeNode currentChild = current.FirstChild;

            for (int i = 0; i < _threadCount; i++)
            {
                doneChecks[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(CheckCollisionsCallback, new CheckCollisionsParameters(currentChild, doneChecks[i]));
                currentChild = currentChild.NextSibling;
            }

            for (int i = 0; i < doneChecks.Length; i++)
            {
                doneChecks[i].WaitOne();
            }
        }

        private void CheckCollisionsInNode(OctreeNode baseNode)
        {
            int baseSphereCount = baseNode.CollisionSphereEntities.Count;
            int baseCubeCount = baseNode.CollisionCubeEntities.Count;

            int currentSphereIndex = 0;
            int currentCubeIndex = 0;

            OctreeNode currentNode = baseNode;


            for (int bs = 0, bc = 0; bs < baseSphereCount || bc < baseCubeCount; bs++, bc++)
            {
                CollisionSphereEntity baseSphere = null;
                if (bs < baseSphereCount)
                {
                    baseSphere = currentNode.CollisionSphereEntities[bs];
                }

                CollisionCubeEntity baseCube = null;
                if (bc < baseCubeCount)
                {
                    baseCube = currentNode.CollisionCubeEntities[bc];
                }


                currentSphereIndex = bs + 1;
                currentCubeIndex = bc + 1;

                while (currentNode != null)
                {
                    int currentSphereCount = currentNode.CollisionSphereEntities.Count;
                    int currentCubeCount = currentNode.CollisionCubeEntities.Count;



                    //Check sphere sphere
                    for (; currentSphereIndex < currentSphereCount && baseSphere != null; currentSphereIndex++)
                    {
                        CollisionSphereEntity currentSphere = currentNode.CollisionSphereEntities[currentSphereIndex];
                        float distX = baseSphere.PosX - currentSphere.PosX;
                        float distY = baseSphere.PosY - currentSphere.PosY;
                        float distZ = baseSphere.PosZ - currentSphere.PosZ;

                        float distSquare = (distX * distX + distY * distY + distZ * distZ);

                        float radiusSquare = baseSphere.CollisionSphereRadius + currentSphere.CollisionSphereRadius;
                        radiusSquare *= radiusSquare;


                        if (distSquare < radiusSquare)
                        {
                            float baseToCurrentX = -distX;
                            float baseToCurrentY = -distY;
                            float baseToCurrentZ = -distZ;

                            //Fast square root
                            //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                            FloatIntUnion b;
                            b.tmp = 0;
                            b.f = distSquare;
                            b.tmp -= 1 << 23; /* Subtract 2^m. */
                            b.tmp >>= 1; /* Divide by 2. */
                            b.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                            float lengthBaseToCurrent = b.f;

                            baseToCurrentX = baseToCurrentX / lengthBaseToCurrent;
                            baseToCurrentY = baseToCurrentY / lengthBaseToCurrent;
                            baseToCurrentZ = baseToCurrentZ / lengthBaseToCurrent;

                            float combinedMass = baseSphere.Mass + currentSphere.Mass;
                            float baseMassRatio = baseSphere.Mass / combinedMass;
                            float currentMassRatio = currentSphere.Mass / combinedMass;

                            float centerX = (currentSphere.PosX + baseSphere.PosX) / 2.0f;
                            float centerY = (currentSphere.PosY + baseSphere.PosY) / 2.0f;
                            float centerZ = (currentSphere.PosZ + baseSphere.PosZ) / 2.0f;

                            float combinedRadius = baseSphere.CollisionSphereRadius + currentSphere.CollisionSphereRadius;

                            baseSphere.Mutex.WaitOne();
                            baseSphere.PosX = centerX + (1 - baseMassRatio) * -baseToCurrentX * combinedRadius * 1.01f;
                            baseSphere.PosY = centerY + (1 - baseMassRatio) * -baseToCurrentY * combinedRadius * 1.01f;
                            baseSphere.PosZ = centerZ + (1 - baseMassRatio) * -baseToCurrentZ * combinedRadius * 1.01f;
                            baseSphere.Mutex.ReleaseMutex();

                            currentSphere.Mutex.WaitOne();
                            currentSphere.PosX = centerX + (1 - currentMassRatio) * baseToCurrentX * combinedRadius * 1.01f;
                            currentSphere.PosY = centerY + (1 - currentMassRatio) * baseToCurrentY * combinedRadius * 1.01f;
                            currentSphere.PosZ = centerZ + (1 - currentMassRatio) * baseToCurrentZ * combinedRadius * 1.01f;
                            currentSphere.Mutex.ReleaseMutex();


                            if (baseSphere.MoveableByForce)
                            {
                                if (currentSphere.MoveableByForce)
                                {
                                    float baseNormalX = distX;
                                    float baseNormalY = distY;
                                    float baseNormalZ = distZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u;
                                    u.tmp = 0;
                                    u.f = distSquare;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    float lengthNorm = u.f;

                                    baseNormalX = baseNormalX / lengthNorm;
                                    baseNormalY = baseNormalY / lengthNorm;
                                    baseNormalZ = baseNormalZ / lengthNorm;


                                    float lengthVelBase = baseSphere.VelocityX * baseSphere.VelocityX + baseSphere.VelocityY * baseSphere.VelocityY + baseSphere.VelocityZ * baseSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    u.tmp = 0;
                                    u.f = lengthVelBase;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelBase = u.f;

                                    float baseVelocityX = baseSphere.VelocityX / lengthVelBase;
                                    float baseVelocityY = baseSphere.VelocityY / lengthVelBase;
                                    float baseVelocityZ = baseSphere.VelocityZ / lengthVelBase;

                                    float dotProduct = baseNormalX * baseVelocityX + baseNormalY * baseVelocityY + baseNormalZ * baseVelocityZ;

                                    //I - 2.0 * dot(N, I) * N.
                                    //https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/reflect.xhtml
                                    float xBaseReflect = baseVelocityX - 2.0f * dotProduct * baseNormalX;
                                    float yBaseReflect = baseVelocityY - 2.0f * dotProduct * baseNormalY;
                                    float zBaseReflect = baseVelocityZ - 2.0f * dotProduct * baseNormalZ;

                                    float baseForce = baseMassRatio * lengthVelBase;

                                    baseSphere.Mutex.WaitOne();
                                    baseSphere.VelocityX = xBaseReflect * baseForce;
                                    baseSphere.VelocityY = yBaseReflect * baseForce;
                                    baseSphere.VelocityZ = zBaseReflect * baseForce;
                                    
                                    baseSphere.VelocityX += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    baseSphere.VelocityY += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    baseSphere.VelocityZ += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    baseSphere.Mutex.ReleaseMutex();


                                    //-------
                                    //CurrentSphereVel
                                    //-------

                                    float currentNormalX = -baseNormalX;
                                    float currentNormalY = -baseNormalY;
                                    float currentNormalZ = -baseNormalZ;


                                    float lengthVelCurrent = currentSphere.VelocityX * currentSphere.VelocityX + currentSphere.VelocityY * currentSphere.VelocityY + currentSphere.VelocityZ * currentSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    u.tmp = 0;
                                    u.f = lengthVelCurrent;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelCurrent = u.f;

                                    float currentVelocityX = currentSphere.VelocityX / lengthVelCurrent;
                                    float currentVelocityY = currentSphere.VelocityY / lengthVelCurrent;
                                    float currentVelocityZ = currentSphere.VelocityZ / lengthVelCurrent;

                                    dotProduct = currentNormalX * currentVelocityX + currentNormalY * currentVelocityY + currentNormalZ * currentVelocityZ;

                                    //I - 2.0 * dot(N, I) * N.
                                    //https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/reflect.xhtml
                                    float xCurrentReflect = currentVelocityX - 2.0f * dotProduct * currentNormalX;
                                    float yCurrentReflect = currentVelocityY - 2.0f * dotProduct * currentNormalY;
                                    float zCurrentReflect = currentVelocityZ - 2.0f * dotProduct * currentNormalZ;

                                    float currentForce = currentMassRatio * lengthVelCurrent;

                                    currentSphere.Mutex.WaitOne();
                                    currentSphere.VelocityX = xCurrentReflect * currentForce;
                                    currentSphere.VelocityY = yCurrentReflect * currentForce;
                                    currentSphere.VelocityZ = zCurrentReflect * currentForce;

                                    currentSphere.VelocityX += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    currentSphere.VelocityY += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    currentSphere.VelocityZ += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    currentSphere.Mutex.ReleaseMutex();
                                }
                                else
                                {
                                    //No need
                                }
                            }
                            else
                            {
                                if (currentSphere.MoveableByForce)
                                {
                                    //No need
                                }
                                else
                                {
                                    //No need
                                }
                            }
                        }
                    }

                    //Check sphere cube
                    for (int cc = 0; cc < currentCubeCount && baseSphere != null; cc++)
                    {
                        CollisionCubeEntity currentCube = currentNode.CollisionCubeEntities[cc];


                        //Thanks iq
                        //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
                        //Resembels: Math.Max(Math.Abs(entity.PosZ - currentNode.CenterZ) - currentNode.Size, 0.0f);
                        float distX = baseSphere.PosX - currentCube.PosX;
                        distX = (distX > 0 ? distX : -distX) - (currentCube.SizeX / 2);
                        distX = distX > 0 ? distX : 0;

                        float distY = baseSphere.PosY - currentCube.PosY;
                        distY = (distY > 0 ? distY : -distY) - (currentCube.SizeY / 2);
                        distY = distY > 0 ? distY : 0;

                        float distZ = baseSphere.PosZ - currentCube.PosZ;
                        distZ = (distZ > 0 ? distZ : -distZ) - (currentCube.SizeZ / 2);
                        distZ = distZ > 0 ? distZ : 0;


                        float distanceSquared = distX * distX + distY * distY + distZ * distZ;

                        float radiusSquared = baseSphere.CollisionSphereRadius * baseSphere.CollisionSphereRadius;

                        if (radiusSquared > distanceSquared)
                        {
                            if (baseSphere.MoveableByForce)
                            {
                                if (currentCube.MoveableByForce)
                                {
                                    //Force interaction
                                }
                                else
                                {
                                    float baseNormalX = 0;
                                    float baseNormalY = 0;
                                    float baseNormalZ = 0;

                                    float lengthNorm = 0;

                                    if (baseSphere.PosX > currentCube.MaxX)
                                    {
                                        baseNormalX = 1;
                                        lengthNorm += 1;
                                    }
                                    else if (baseSphere.PosX < currentCube.MinX)
                                    {
                                        baseNormalX = -1;
                                        lengthNorm += 1;
                                    }

                                    if (baseSphere.PosY > currentCube.MaxY)
                                    {
                                        baseNormalY = 1;
                                        lengthNorm += 1;
                                    }
                                    else if (baseSphere.PosY < currentCube.MinY)
                                    {
                                        baseNormalY = -1;
                                        lengthNorm += 1;
                                    }

                                    if (baseSphere.PosZ > currentCube.MaxZ)
                                    {
                                        baseNormalZ = 1;
                                        lengthNorm += 1;
                                    }
                                    else if (baseSphere.PosZ < currentCube.MinZ)
                                    {
                                        baseNormalZ = -1;
                                        lengthNorm += 1;
                                    }

                                    if (lengthNorm == 0)
                                    {
                                        lengthNorm = 1;
                                        baseNormalX = 0;
                                        baseNormalY = 1;
                                        baseNormalZ = 0;
                                    }

                                    if(lengthNorm == 2)
                                    {
                                        lengthNorm = 1.4142135623730950488016887242097f; //Sqrt(2)
                                    }
                                    else if(lengthNorm == 3)
                                    {
                                        lengthNorm = 1.7320508075688772935274463415059f; //Sqrt(3)
                                    }

                                    baseNormalX /= lengthNorm;
                                    baseNormalY /= lengthNorm;
                                    baseNormalZ /= lengthNorm;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u;
                                    u.tmp = 0;
                                    u.f = distanceSquared;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    float distance = u.f;

                                    float returnDist = baseSphere.CollisionSphereRadius - distance;

                                    baseSphere.Mutex.WaitOne();
                                    baseSphere.PosX += baseNormalX * returnDist;
                                    baseSphere.PosY += baseNormalY * returnDist;
                                    baseSphere.PosZ += baseNormalZ * returnDist;
                                    baseSphere.Mutex.ReleaseMutex();


                                    //Reflect

                                    float lengthVelBase = baseSphere.VelocityX * baseSphere.VelocityX + baseSphere.VelocityY * baseSphere.VelocityY + baseSphere.VelocityZ * baseSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    u.tmp = 0;
                                    u.f = lengthVelBase;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelBase = u.f;


                                    float baseVelocityX = baseSphere.VelocityX / lengthVelBase;
                                    float baseVelocityY = baseSphere.VelocityY / lengthVelBase;
                                    float baseVelocityZ = baseSphere.VelocityZ / lengthVelBase;


                                    float dotProduct = baseNormalX * baseVelocityX + baseNormalY * baseVelocityY + baseNormalZ * baseVelocityZ;


                                    //I - 2.0 * dot(N, I) * N.
                                    //https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/reflect.xhtml
                                    float xBaseReflect = baseVelocityX - 2.0f * dotProduct * baseNormalX;
                                    float yBaseReflect = baseVelocityY - 2.0f * dotProduct * baseNormalY;
                                    float zBaseReflect = baseVelocityZ - 2.0f * dotProduct * baseNormalZ;

                                    baseSphere.Mutex.WaitOne();
                                    baseSphere.VelocityX *= xBaseReflect;
                                    baseSphere.VelocityY *= yBaseReflect;
                                    baseSphere.VelocityZ *= zBaseReflect;
                                    baseSphere.Mutex.ReleaseMutex();
                                }
                            }
                            else
                            {
                                if (currentCube.MoveableByForce)
                                {
                                    //No fking idea
                                }
                                else
                                {
                                    //No fking idea
                                }
                            }
                        }

                        //currentCube.Mutex.ReleaseMutex();
                    }
                    

                    //Check cube cube
                    for (; currentCubeIndex < currentCubeCount && baseCube != null; currentCubeIndex++)
                    {
                        CollisionCubeEntity currentCube = currentNode.CollisionCubeEntities[currentCubeIndex];

                        bool xInclude = currentCube.MinX >= baseCube.MinX && currentCube.MinX <= baseCube.MaxX ||
                            currentCube.MaxX <= baseCube.MaxX && currentCube.MaxX >= baseCube.MinX;
                        bool yInclude = currentCube.MinY >= baseCube.MinY && currentCube.MinY <= baseCube.MaxY ||
                            currentCube.MaxY <= baseCube.MaxY && currentCube.MaxY >= baseCube.MinY;
                        bool zInclude = currentCube.MinZ >= baseCube.MinZ && currentCube.MinZ <= baseCube.MaxZ ||
                            currentCube.MaxZ <= baseCube.MaxZ && currentCube.MaxZ >= baseCube.MinZ;

                        if (xInclude && yInclude && zInclude)
                        {
                            //Well I don´t need that right now
                        }
                        
                    }

                    currentSphereIndex = 0;
                    currentCubeIndex = 0;

                    currentNode = currentNode.Parent;
                }

                currentNode = baseNode;
            }
        }

        private static float Sqrt(float z)
        {
            //Fast square root
            //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; /* Subtract 2^m. */
            u.tmp >>= 1; /* Divide by 2. */
            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
            return u.f;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }



        private void InsertIntoOctree(OctreeNode currentNode, CollisionSphereEntity entity)
        {
            //As you might see in this method there is much code that looks rather similar
            //Thank c# for not having macros or use inline methods too often
            //Because of one function call that would cut down the method drastically the algorithm works 1.5 times slower
            //Thus there is a lot of similar but not equal code

            //HOW IT WORKS:
            //Slice the parent node into left/right, top/bottom and front/back sections
            //Check for each of these sections a cube to sphere intersection
            //Based on the collisions there can be determined in which subcubes there is an intersection


            //Last level of octree
            if (currentNode.ChildsAreInitialized == false)
            {
                currentNode.CollisionSphereEntitiesMutex.WaitOne();
                currentNode.CollisionSphereEntities.Add(entity);
                currentNode.CollisionSphereEntitiesMutex.ReleaseMutex();
                return;
            }


            //Thanks iq
            //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
            //Resembels: Math.Max(Math.Abs(entity.PosZ - currentNode.CenterZ) - currentNode.Size, 0.0f);
            float xPosWithCenter = entity.PosX - currentNode.CenterX;
            float yPosWithCenter = entity.PosY - currentNode.CenterY;
            float zPosWithCenter = entity.PosZ - currentNode.CenterZ;


            float distX = xPosWithCenter;
            distX = (distX > 0 ? distX : -distX) - currentNode.HalfSize;
            distX = distX > 0 ? distX : 0;

            float distY = yPosWithCenter;
            distY = (distY > 0 ? distY : -distY) - currentNode.HalfSize;
            distY = distY > 0 ? distY : 0;

            float distZ = xPosWithCenter;
            distZ = (distZ > 0 ? distZ : -distZ) - currentNode.HalfSize;
            distZ = distZ > 0 ? distZ : 0;


            float distLeftX = xPosWithCenter + currentNode.QuaterSize;
            distLeftX = (distLeftX > 0 ? distLeftX : -distLeftX) - currentNode.QuaterSize;
            distLeftX = distLeftX > 0 ? distLeftX : 0;

            float distBottomY = yPosWithCenter + currentNode.QuaterSize;
            distBottomY = (distBottomY > 0 ? distBottomY : -distBottomY) - currentNode.QuaterSize;
            distBottomY = distBottomY > 0 ? distBottomY : 0;

            float distBackZ = zPosWithCenter + currentNode.QuaterSize;
            distBackZ = (distBackZ > 0 ? distBackZ : -distBackZ) - currentNode.QuaterSize;
            distBackZ = distBackZ > 0 ? distBackZ : 0;


            float distXSquare = distX * distX;
            float distYSquare = distY * distY;
            float distZSquare = distZ * distZ;

            //Generate nearest points to left/right... sections
            float lengthLeft = distLeftX * distLeftX + distYSquare + distZSquare;
            float lengthBottom = distXSquare + distBottomY * distBottomY + distZSquare;
            float lengthBack = distXSquare + distYSquare + distBackZ * distBackZ;

            float radiusSquared = entity.CollisionSphereRadius * entity.CollisionSphereRadius;

            //check collisions with sections
            bool collidesWithLeft = lengthLeft <= radiusSquared;
            bool collidesWithRight = false;
            if (collidesWithLeft == false)
            {
                //Entity is allways smaller than the currentNodeCube. Thus it has to collide either with right or left. If it has not collided with left it collides with right.
                collidesWithRight = true;
            }
            else
            {
                float distRightX = xPosWithCenter - currentNode.QuaterSize;
                distRightX = (distRightX > 0 ? distRightX : -distRightX) - currentNode.QuaterSize;
                distRightX = distRightX > 0 ? distRightX : 0;

                float lengthRight = distRightX * distRightX + distYSquare + distZSquare;

                collidesWithRight = lengthRight <= radiusSquared;
            }

            bool collidesWithBottom = lengthBottom <= radiusSquared;
            bool collidesWithTop = false;
            if (collidesWithBottom == false)
            {
                collidesWithTop = true;
            }
            else
            {
                float distTopY = yPosWithCenter - currentNode.QuaterSize;
                distTopY = (distTopY > 0 ? distTopY : -distTopY) - currentNode.QuaterSize;
                distTopY = distTopY > 0 ? distTopY : 0;

                float lengthTop = distXSquare + distTopY * distTopY + distZSquare;

                collidesWithTop = lengthTop <= radiusSquared;
            }

            bool collidesWithBack = lengthBack <= radiusSquared;

            //Somehow the acceleration does not work with the z direction. Something is fishy
            float distFrontZ = zPosWithCenter - currentNode.QuaterSize;
            distFrontZ = (distFrontZ > 0 ? distFrontZ : -distFrontZ) - currentNode.QuaterSize;
            distFrontZ = distFrontZ > 0 ? distFrontZ : 0;

            float lengthFront = distXSquare + distYSquare + distFrontZ * distFrontZ;

            bool collidesWithFront = lengthFront <= radiusSquared;


            //Child arrangement is binary
            //OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0);                 //---
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1); //--+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2); //-+-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3); //-++
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4); //+--
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5); //+-+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6); //++-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7); //+++

            if (collidesWithLeft && collidesWithRight && collidesWithBottom && collidesWithTop && collidesWithBack && collidesWithFront)
            {
                currentNode.CollisionSphereEntitiesMutex.WaitOne();
                currentNode.CollisionSphereEntities.Add(entity);
                currentNode.CollisionSphereEntitiesMutex.ReleaseMutex();
                return;
            }
            else
            {
                //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
                //Conditions are binary counted up like the child arrangement
                //000; 001; 010; 011...
                //==
                //leftBotBack; leftBotFront; leftTopBack; leftTopFront...

                OctreeNode currentChild = currentNode.FirstChild;

                if (collidesWithLeft && collidesWithBottom && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithBottom && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithTop && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithTop && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithBottom && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithBottom && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithTop && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithTop && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
            }
        }




        private void InsertIntoOctree(OctreeNode currentNode, CollisionCubeEntity entity)
        {
            //HOW IT WORKS:
            //Slice the parent node into left/right, top/bottom and front/back sections
            //Check for each of these sections a cube to sphere intersection
            //Based on the collisions there can be determined in which subcubes there is an intersection

            //Last level of octree
            if (currentNode.ChildsAreInitialized == false)
            {
                currentNode.CollisionCubeEntitiesMutex.WaitOne();
                currentNode.CollisionCubeEntities.Add(entity);
                currentNode.CollisionCubeEntitiesMutex.ReleaseMutex();
                return;
            }

            bool defaultXInclude = entity.MinX >= currentNode.MinCoordsX && entity.MinX <= currentNode.MaxCoordsX ||
                entity.MaxX <= currentNode.MaxCoordsX && entity.MaxX >= currentNode.MinCoordsX;
            bool defaultYInclude = entity.MinY >= currentNode.MinCoordsY && entity.MinY <= currentNode.MaxCoordsY ||
                entity.MaxY <= currentNode.MaxCoordsY && entity.MaxY >= currentNode.MinCoordsY;
            bool defaultZInclude = entity.MinZ >= currentNode.MinCoordsZ && entity.MinZ <= currentNode.MaxCoordsZ ||
                entity.MaxZ <= currentNode.MaxCoordsZ && entity.MaxZ >= currentNode.MinCoordsZ;

            //Check if points are in the sections
            bool xIncludeLeft = entity.MinX >= currentNode.MinCoordsX && entity.MinX <= currentNode.CenterX ||
                entity.MaxX <= currentNode.CenterX && entity.MaxX >= currentNode.MinCoordsX;

            bool xIncludeRight = entity.MinX >= currentNode.CenterX && entity.MinX <= currentNode.MaxCoordsX ||
                entity.MaxX <= currentNode.MaxCoordsX && entity.MaxX >= currentNode.CenterX;

            bool yIncludeBottom = entity.MinY >= currentNode.MinCoordsY && entity.MinY <= currentNode.CenterY ||
                entity.MaxY <= currentNode.CenterY && entity.MaxY >= currentNode.MinCoordsY;

            bool yIncludeTop = entity.MinY >= currentNode.CenterY && entity.MinY <= currentNode.MaxCoordsY ||
                entity.MaxY <= currentNode.MaxCoordsY && entity.MaxY >= currentNode.CenterY;

            bool zIncludeBack = entity.MinZ >= currentNode.MinCoordsZ && entity.MinZ <= currentNode.CenterZ ||
                entity.MaxZ <= currentNode.CenterZ && entity.MaxZ >= currentNode.MinCoordsZ;

            bool zIncludeFront = entity.MinZ >= currentNode.CenterZ && entity.MinZ <= currentNode.MaxCoordsZ ||
                entity.MaxZ <= currentNode.MaxCoordsZ && entity.MaxZ >= currentNode.CenterZ;



            //check collisions with sections
            bool collidesWithLeft = xIncludeLeft && defaultYInclude && defaultZInclude;
            bool collidesWithRight = xIncludeRight && defaultYInclude && defaultZInclude;
            bool collidesWithBottom = defaultXInclude && yIncludeBottom && defaultZInclude;
            bool collidesWithTop = defaultXInclude && yIncludeTop && defaultZInclude;
            bool collidesWithBack = defaultXInclude && defaultYInclude && zIncludeBack;
            bool collidesWithFront = defaultXInclude && defaultYInclude && zIncludeFront;


            //Child arrangement is binary
            //OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0);                 //---
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1); //--+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2); //-+-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3); //-++
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4); //+--
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5); //+-+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6); //++-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7); //+++

            if (collidesWithLeft && collidesWithRight && collidesWithBottom && collidesWithTop && collidesWithBack && collidesWithFront)
            {
                currentNode.CollisionCubeEntitiesMutex.WaitOne();
                currentNode.CollisionCubeEntities.Add(entity);
                currentNode.CollisionCubeEntitiesMutex.ReleaseMutex();
                return;
            }
            else
            {
                //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
                //Conditions are binary counted up like the child arrangement
                //000; 001; 010; 011...
                //==
                //leftBotBack; leftBotFront; leftTopBack; leftTopFront...

                OctreeNode currentChild = currentNode.FirstChild;

                if (collidesWithLeft && collidesWithBottom && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithBottom && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithTop && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithLeft && collidesWithTop && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithBottom && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithBottom && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithTop && collidesWithBack)
                {
                    InsertIntoOctree(currentChild, entity);
                }
                currentChild = currentChild.NextSibling;

                if (collidesWithRight && collidesWithTop && collidesWithFront)
                {
                    InsertIntoOctree(currentChild, entity);
                }
            }
        }

        private void ResetNode(OctreeNode current)
        {
            if (current == null)
            {
                return;
            }

            current.Reset();

            OctreeNode currentChild = current.FirstChild;


            while (currentChild != null)
            {
                ResetNode(currentChild);
                currentChild = currentChild.NextSibling;
            }
        }

        private void ResetCallback(Object resetParams)
        {
            ResetParamters param = (ResetParamters)resetParams;

            ResetNode(param.Node);

            param.DoneHandle.Set();
        }

        private struct ResetParamters
        {
            public OctreeNode Node;
            public ManualResetEvent DoneHandle;

            public ResetParamters(OctreeNode node, ManualResetEvent doneHandle)
            {
                Node = node;
                DoneHandle = doneHandle;
            }
        }


        private void ResetOctree(OctreeNode current)
        {
            ManualResetEvent[] doneResets = new ManualResetEvent[_threadCount];

            current.Reset();

            OctreeNode currentChild = current.FirstChild;

            for (int i = 0; i < _threadCount; i++)
            {
                doneResets[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(ResetCallback, new ResetParamters(currentChild, doneResets[i]));
                currentChild = currentChild.NextSibling;
            }

            for (int i = 0; i < doneResets.Length; i++)
            {
                doneResets[i].WaitOne();
            }
        }
    }
}
