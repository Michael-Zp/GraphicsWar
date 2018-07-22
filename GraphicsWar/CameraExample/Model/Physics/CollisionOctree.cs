using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GraphicsWar.Model.Physics
{
    public class CollisionOctree
    {
        private OctreeNode _octreeRoot;

        private Random _random;

        private int _levels;

        public void InitializeNewOctree(int levels, Vector3 center, float size)
        {
            _levels = levels;

            _random = new Random(DateTime.Now.Millisecond);

            _octreeRoot = new OctreeNode(center.X, center.Y, center.Z, size, size / 2, size / 4, 0, null);
            InitializeOctreeRoot(_octreeRoot, _levels);
        }

        public void InsertIntoOctree(List<Entity> entities)
        {
            IEnumerable<CollisionSphereEntity> collisionSpheres =
                from entity in entities
                where entity is CollisionSphereEntity
                select entity as CollisionSphereEntity;

            foreach (var entity in collisionSpheres)
            {
                if (entity.CollisionSphereRadius >= _octreeRoot.Size)
                {
                    _octreeRoot.collisionSphereEntities.Add(entity);
                }
                else
                {
                    InsertIntoOctree(_octreeRoot, entity);
                }
            }

            IEnumerable<CollisionCubeEntity> collisionCubes =
                            from entity in entities
                            where entity is CollisionCubeEntity
                            select entity as CollisionCubeEntity;

            foreach (var entity in collisionCubes)
            {
                bool xBigger = entity.SizeX >= _octreeRoot.Size;
                bool yBigger = entity.SizeY >= _octreeRoot.Size;
                bool zBigger = entity.SizeZ >= _octreeRoot.Size;

                if (xBigger || yBigger || zBigger)
                {
                    _octreeRoot.collisionCubeEntities.Add(entity);
                }
                else
                {
                    InsertIntoOctree(_octreeRoot, entity);
                }
            }
        }

        public void CheckCollisionsInTree() => CheckCollisionsInTree(_octreeRoot);

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

        private void CheckCollisionsInNode(OctreeNode baseNode)
        {
            int baseSphereCount = baseNode.collisionSphereEntities.Count;
            int baseCubeCount = baseNode.collisionCubeEntities.Count;

            int currentSphereIndex = 0;
            int currentCubeIndex = 0;

            OctreeNode currentNode = baseNode;


            for (int bs = 0, bc = 0; bs < baseSphereCount || bc < baseCubeCount; bs++, bc++)
            {
                CollisionSphereEntity baseSphere = null;
                if (bs < baseSphereCount)
                {
                    baseSphere = currentNode.collisionSphereEntities[bs];
                }

                CollisionCubeEntity baseCube = null;
                if (bc < baseCubeCount)
                {
                    baseCube = currentNode.collisionCubeEntities[bc];
                }


                currentSphereIndex = bs + 1;
                currentCubeIndex = bc + 1;

                while (currentNode != null)
                {
                    int currentSphereCount = currentNode.collisionSphereEntities.Count;
                    int currentCubeCount = currentNode.collisionCubeEntities.Count;



                    //Check sphere sphere
                    for (; currentSphereIndex < currentSphereCount; currentSphereIndex++)
                    {
                        CollisionSphereEntity currentSphere = currentNode.collisionSphereEntities[currentSphereIndex];
                        float distX = baseSphere.PosX - currentSphere.PosX;
                        float distY = baseSphere.PosY - currentSphere.PosY;
                        float distZ = baseSphere.PosZ - currentSphere.PosZ;

                        float distSquare = (distX * distX + distY * distY + distZ * distZ);

                        float radiusSquare = baseSphere.CollisionSphereRadius + currentSphere.CollisionSphereRadius;
                        radiusSquare *= radiusSquare;


                        if (distSquare < radiusSquare)
                        {
                            float baseToCurrentX = currentSphere.PosX - baseSphere.PosX;
                            float baseToCurrentY = currentSphere.PosY - baseSphere.PosY;
                            float baseToCurrentZ = currentSphere.PosZ - baseSphere.PosZ;

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

                            baseSphere.PosX = centerX + (1 - baseMassRatio) * -baseToCurrentX * combinedRadius * 1.01f;
                            baseSphere.PosY = centerY + (1 - baseMassRatio) * -baseToCurrentY * combinedRadius * 1.01f;
                            baseSphere.PosZ = centerZ + (1 - baseMassRatio) * -baseToCurrentZ * combinedRadius * 1.01f;

                            currentSphere.PosX = centerX + (1 - currentMassRatio) * baseToCurrentX * combinedRadius * 1.01f;
                            currentSphere.PosY = centerY + (1 - currentMassRatio) * baseToCurrentY * combinedRadius * 1.01f;
                            currentSphere.PosZ = centerZ + (1 - currentMassRatio) * baseToCurrentZ * combinedRadius * 1.01f;
                            

                            if (baseSphere.MoveableByForce)
                            {
                                if (currentSphere.MoveableByForce)
                                {

                                    float baseNormalX = baseSphere.PosX - currentSphere.PosX;
                                    float baseNormalY = baseSphere.PosY - currentSphere.PosY;
                                    float baseNormalZ = baseSphere.PosZ - currentSphere.PosZ;

                                    float lengthNorm = baseNormalX * baseNormalX + baseNormalY * baseNormalY + baseNormalZ * baseNormalZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u;
                                    u.tmp = 0;
                                    u.f = lengthNorm;
                                    u.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u.tmp >>= 1; /* Divide by 2. */
                                    u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthNorm = u.f;

                                    baseNormalX = baseNormalX / lengthNorm;
                                    baseNormalY = baseNormalY / lengthNorm;
                                    baseNormalZ = baseNormalZ / lengthNorm;


                                    float lengthVelBase = baseSphere.VelocityX * baseSphere.VelocityX + baseSphere.VelocityY * baseSphere.VelocityY + baseSphere.VelocityZ * baseSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u2;
                                    u2.tmp = 0;
                                    u2.f = lengthVelBase;
                                    u2.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u2.tmp >>= 1; /* Divide by 2. */
                                    u2.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelBase = u2.f;

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

                                    baseSphere.VelocityX = xBaseReflect * baseForce;
                                    baseSphere.VelocityY = yBaseReflect * baseForce;
                                    baseSphere.VelocityZ = zBaseReflect * baseForce;

                                    baseSphere.VelocityX += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    baseSphere.VelocityY += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    baseSphere.VelocityZ += ((float)_random.NextDouble() * 2 - 1) * 0.03f;


                                    //-------
                                    //CurrentSphereVel
                                    //-------

                                    float currentNormalX = -baseNormalX;
                                    float currentNormalY = -baseNormalY;
                                    float currentNormalZ = -baseNormalZ;

                                    lengthNorm = currentNormalX * currentNormalX + currentNormalY * currentNormalY + currentNormalZ * currentNormalZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u3;
                                    u3.tmp = 0;
                                    u3.f = lengthNorm;
                                    u3.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u3.tmp >>= 1; /* Divide by 2. */
                                    u3.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthNorm = u3.f;

                                    currentNormalX = currentNormalX / lengthNorm;
                                    currentNormalY = currentNormalY / lengthNorm;
                                    currentNormalZ = currentNormalZ / lengthNorm;


                                    float lengthVelCurrent = currentSphere.VelocityX * currentSphere.VelocityX + currentSphere.VelocityY * currentSphere.VelocityY + currentSphere.VelocityZ * currentSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u4;
                                    u4.tmp = 0;
                                    u4.f = lengthVelCurrent;
                                    u4.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u4.tmp >>= 1; /* Divide by 2. */
                                    u4.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelCurrent = u4.f;

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

                                    currentSphere.VelocityX = xCurrentReflect * currentForce;
                                    currentSphere.VelocityY = yCurrentReflect * currentForce;
                                    currentSphere.VelocityZ = zCurrentReflect * currentForce;

                                    currentSphere.VelocityX += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    currentSphere.VelocityY += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
                                    currentSphere.VelocityZ += ((float)_random.NextDouble() * 2 - 1) * 0.03f;
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
                        CollisionCubeEntity currentCube = currentNode.collisionCubeEntities[cc];


                        //Thanks iq
                        //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
                        //Resembels: Math.Max(Math.Abs(entity.PosZ - currentNode.CenterZ) - currentNode.Size, 0.0f);
                        float distX = baseSphere.PosX - currentCube.PosX;
                        distX = distX > 0 ? distX : -distX;
                        distX -= currentCube.SizeX / 2;
                        distX = distX > 0 ? distX : 0;

                        float distY = baseSphere.PosY - currentCube.PosY;
                        distY = distY > 0 ? distY : -distY;
                        distY -= currentCube.SizeY / 2;
                        distY = distY > 0 ? distY : 0;

                        float distZ = baseSphere.PosZ - currentCube.PosZ;
                        distZ = distZ > 0 ? distZ : -distZ;
                        distZ -= currentCube.SizeZ / 2;
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

                                    baseNormalX /= lengthNorm;
                                    baseNormalY /= lengthNorm;
                                    baseNormalZ /= lengthNorm;
                                    
                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion d;
                                    d.tmp = 0;
                                    d.f = distanceSquared;
                                    d.tmp -= 1 << 23; /* Subtract 2^m. */
                                    d.tmp >>= 1; /* Divide by 2. */
                                    d.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    float distance = d.f;

                                    float returnDist = baseSphere.CollisionSphereRadius / 2.0f - distance;

                                    baseSphere.PosX += baseNormalX * (returnDist * 1f);
                                    baseSphere.PosY += baseNormalY * (returnDist * 1f);
                                    baseSphere.PosZ += baseNormalZ * (returnDist * 1f);


                                    //Reflect

                                    float lengthVelBase = baseSphere.VelocityX * baseSphere.VelocityX + baseSphere.VelocityY * baseSphere.VelocityY + baseSphere.VelocityZ * baseSphere.VelocityZ;

                                    //Fast square root
                                    //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
                                    FloatIntUnion u2;
                                    u2.tmp = 0;
                                    u2.f = lengthVelBase;
                                    u2.tmp -= 1 << 23; /* Subtract 2^m. */
                                    u2.tmp >>= 1; /* Divide by 2. */
                                    u2.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */

                                    lengthVelBase = u2.f;


                                    float baseVelocityX = baseSphere.VelocityX / lengthVelBase;
                                    float baseVelocityY = baseSphere.VelocityY / lengthVelBase;
                                    float baseVelocityZ = baseSphere.VelocityZ / lengthVelBase;
                                    

                                    float dotProduct = baseNormalX * baseVelocityX + baseNormalY * baseVelocityY + baseNormalZ * baseVelocityZ;


                                    //I - 2.0 * dot(N, I) * N.
                                    //https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/reflect.xhtml
                                    float xBaseReflect = baseVelocityX - 2.0f * dotProduct * baseNormalX;
                                    float yBaseReflect = baseVelocityY - 2.0f * dotProduct * baseNormalY;
                                    float zBaseReflect = baseVelocityZ - 2.0f * dotProduct * baseNormalZ;

                                    baseSphere.VelocityX = xBaseReflect;
                                    baseSphere.VelocityY = yBaseReflect;
                                    baseSphere.VelocityZ = zBaseReflect;
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
                    }


                    //Check cube cube
                    for (; currentCubeIndex < currentCubeCount && baseCube != null; currentCubeIndex++)
                    {
                        CollisionCubeEntity currentCube = currentNode.collisionCubeEntities[currentCubeIndex];

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
                currentNode.collisionSphereEntities.Add(entity);
                return;
            }

            List<int> collisions = new List<int>();
            int collisionCount = 0;


            //Thanks iq
            //http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
            //Resembels: Math.Max(Math.Abs(entity.PosZ - currentNode.CenterZ) - currentNode.Size, 0.0f);
            float distX = entity.PosX - currentNode.CenterX;
            distX = distX > 0 ? distX : -distX;
            distX -= currentNode.HalfSize;
            distX = distX > 0 ? distX : 0;

            float distY = entity.PosY - currentNode.CenterY;
            distY = distY > 0 ? distY : -distY;
            distY -= currentNode.HalfSize;
            distY = distY > 0 ? distY : 0;

            float distZ = entity.PosZ - currentNode.CenterZ;
            distZ = distZ > 0 ? distZ : -distZ;
            distZ -= currentNode.HalfSize;
            distZ = distZ > 0 ? distZ : 0;


            float distLeftX = entity.PosX - (currentNode.CenterX - currentNode.QuaterSize);
            distLeftX = distLeftX > 0 ? distLeftX : -distLeftX;
            distLeftX -= currentNode.QuaterSize;
            distLeftX = distLeftX > 0 ? distLeftX : 0;

            float distRightX = entity.PosX - (currentNode.CenterX + currentNode.QuaterSize);
            distRightX = distRightX > 0 ? distRightX : -distRightX;
            distRightX -= currentNode.QuaterSize;
            distRightX = distRightX > 0 ? distRightX : 0;

            float distBottomY = entity.PosY - (currentNode.CenterY - currentNode.QuaterSize);
            distBottomY = distBottomY > 0 ? distBottomY : -distBottomY;
            distBottomY -= currentNode.QuaterSize;
            distBottomY = distBottomY > 0 ? distBottomY : 0;

            float distTopY = entity.PosY - (currentNode.CenterY + currentNode.QuaterSize);
            distTopY = distTopY > 0 ? distTopY : -distTopY;
            distTopY -= currentNode.QuaterSize;
            distTopY = distTopY > 0 ? distTopY : 0;

            float distBackZ = entity.PosZ - (currentNode.CenterZ - currentNode.QuaterSize);
            distBackZ = distBackZ > 0 ? distBackZ : -distBackZ;
            distBackZ -= currentNode.QuaterSize;
            distBackZ = distBackZ > 0 ? distBackZ : 0;

            float distFrontZ = entity.PosZ - (currentNode.CenterZ + currentNode.QuaterSize);
            distFrontZ = distFrontZ > 0 ? distFrontZ : -distFrontZ;
            distFrontZ -= currentNode.QuaterSize;
            distFrontZ = distFrontZ > 0 ? distFrontZ : 0;

            float distXSquare = distX * distX;
            float distYSquare = distY * distY;
            float distZSquare = distZ * distZ;

            //Generate nearest points to left/right... sections
            float lengthLeft = distLeftX * distLeftX + distYSquare + distZSquare;
            float lengthRight = distRightX * distRightX + distYSquare + distZSquare;
            float lengthBottom = distXSquare + distBottomY * distBottomY + distZSquare;
            float lengthTop = distXSquare + distTopY * distTopY + distZSquare;
            float lengthBack = distXSquare + distYSquare + distBackZ * distBackZ;
            float lengthFront = distXSquare + distYSquare + distFrontZ * distFrontZ;

            float radiusSquared = entity.CollisionSphereRadius * entity.CollisionSphereRadius;

            //check collisions with sections
            bool collidesWithLeft = lengthLeft <= radiusSquared;
            bool collidesWithRight = lengthRight <= radiusSquared;
            bool collidesWithBottom = lengthBottom <= radiusSquared;
            bool collidesWithTop = lengthTop <= radiusSquared;
            bool collidesWithBack = lengthBack <= radiusSquared;
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

            OctreeNode currentChild = currentNode.FirstChild;

            //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
            //Conditions are binary counted up like the child arrangement
            //000; 001; 010; 011...
            //==
            //leftBotBack; leftBotFront; leftTopBack; leftTopFront...
            if (collidesWithLeft && collidesWithBottom && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(0);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithBottom && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(1);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(2);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(3);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(4);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(5);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(6);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(7);
            }
            currentChild = currentChild.NextSibling;



            if (collisionCount == 8)
            {
                currentNode.collisionSphereEntities.Add(entity);
                return;
            }

            currentChild = currentNode.FirstChild;
            int lastIdx = 0;
            foreach (var idx in collisions)
            {
                for (int i = 0; i < idx - lastIdx; i++)
                {
                    currentChild = currentChild.NextSibling;
                }

                InsertIntoOctree(currentChild, entity);
                lastIdx = idx;
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
                currentNode.collisionCubeEntities.Add(entity);
                return;
            }

            List<int> collisions = new List<int>();
            int collisionCount = 0;

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

            OctreeNode currentChild = currentNode.FirstChild;

            //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
            //Conditions are binary counted up like the child arrangement
            //000; 001; 010; 011...
            //==
            //leftBotBack; leftBotFront; leftTopBack; leftTopFront...
            if (collidesWithLeft && collidesWithBottom && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(0);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithBottom && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(1);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(2);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(3);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(4);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(5);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithBack)
            {
                collisionCount++;
                collisions.Add(6);
            }
            currentChild = currentChild.NextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithFront)
            {
                collisionCount++;
                collisions.Add(7);
            }
            currentChild = currentChild.NextSibling;



            if (collisionCount == 8)
            {
                currentNode.collisionCubeEntities.Add(entity);
                return;
            }

            currentChild = currentNode.FirstChild;
            int lastIdx = 0;
            foreach (var idx in collisions)
            {
                for (int i = 0; i < idx - lastIdx; i++)
                {
                    currentChild = currentChild.NextSibling;
                }

                InsertIntoOctree(currentChild, entity);
                lastIdx = idx;
            }
        }

        private void ResetOctree(OctreeNode current)
        {
            if (current == null)
            {
                return;
            }


            current.Reset();

            OctreeNode currentChild = current.FirstChild;


            while (currentChild != null)
            {
                ResetOctree(currentChild);
                currentChild = currentChild.NextSibling;
            }
        }


        // Not so old insert, but still bad

        //private void InsertIntoOctree(OctreeNode currentNode, CollisionSphereEntity entity)
        //{
        //    //As you might see in this method there is much code that looks rather similar
        //    //Thank c# for not having macros or use inline methods too often
        //    //Because of one function call that would cut down the method drastically the algorithm works 1.5 times slower
        //    //Thus there is a lot of similar but not equal code

        //    //HOW IT WORKS:
        //    //Slice the parent node into left/right, top/bottom and front/back sections
        //    //Check for each of these sections a cube to sphere intersection
        //    //Based on the collisions there can be determined in which subcubes there is an intersection


        //    //Last level of octree
        //    if (currentNode.ChildsAreInitialized == false)
        //    {
        //        currentNode.collisionSphereEntities.Add(entity);
        //        return;
        //    }

        //    List<int> collisions = new List<int>();
        //    int collisionCount = 0;


        //    //Get axis alinged distance
        //    float distX = currentNode.CenterX - entity.PosX;
        //    float distY = currentNode.CenterY - entity.PosY;
        //    float distZ = currentNode.CenterZ - entity.PosZ;

        //    //Slice parent into left/right, top/bottom and front/back sections
        //    float distToCenterLeftX = (currentNode.CenterX - currentNode.QuaterSize) - entity.PosX;
        //    float distToCenterRightX = (currentNode.CenterX + currentNode.QuaterSize) - entity.PosX;
        //    float distToCenterBottomY = (currentNode.CenterY - currentNode.QuaterSize) - entity.PosY;
        //    float distToCenterTopY = (currentNode.CenterY + currentNode.QuaterSize) - entity.PosY;
        //    float distToCenterBackZ = (currentNode.CenterZ - currentNode.QuaterSize) - entity.PosZ;
        //    float distToCenterFrontZ = (currentNode.CenterZ + currentNode.QuaterSize) - entity.PosZ;

        //    float distXSquare = distX * distX;
        //    float distYSquare = distY * distY;
        //    float distZSquare = distZ * distZ;


        //    //Generate nearest points to left/right... sections
        //    float lengthLeftToCenter = (float)Math.Sqrt(distToCenterLeftX * distToCenterLeftX + distYSquare + distZSquare);
        //    float lengthRightToCenter = (float)Math.Sqrt(distToCenterRightX * distToCenterRightX + distYSquare + distZSquare);
        //    float lengthBottomToCenter = (float)Math.Sqrt(distXSquare + distToCenterBottomY * distToCenterBottomY + distZSquare);
        //    float lengthTopToCenter = (float)Math.Sqrt(distXSquare + distToCenterTopY * distToCenterTopY + distZSquare);
        //    float lengthBackToCenter = (float)Math.Sqrt(distXSquare + distYSquare + distToCenterBackZ * distToCenterBackZ);
        //    float lengthFrontToCenter = (float)Math.Sqrt(distXSquare + distYSquare + distToCenterFrontZ * distToCenterFrontZ);



        //    float minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthLeftToCenter ? entity.CollisionSphereRadius : lengthLeftToCenter;
        //    float nearestPointXLeft = distX / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYLeft = distY / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZLeft = distZ / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

        //    minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthRightToCenter ? entity.CollisionSphereRadius : lengthRightToCenter;
        //    float nearestPointXRight = distX / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYRight = distY / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZRight = distZ / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

        //    minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthBottomToCenter ? entity.CollisionSphereRadius : lengthBottomToCenter;
        //    float nearestPointXBottom = distX / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYBottom = distY / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZBottom = distZ / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

        //    minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthTopToCenter ? entity.CollisionSphereRadius : lengthTopToCenter;
        //    float nearestPointXTop = distX / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYTop = distY / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZTop = distZ / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

        //    minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthBackToCenter ? entity.CollisionSphereRadius : lengthBackToCenter;
        //    float nearestPointXBack = distX / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYBack = distY / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZBack = distZ / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

        //    minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthFrontToCenter ? entity.CollisionSphereRadius : lengthFrontToCenter;
        //    float nearestPointXFront = distX / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
        //    float nearestPointYFront = distY / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
        //    float nearestPointZFront = distZ / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;


        //    //Check if points are in the sections
        //    bool xIncludeLeft = nearestPointXLeft >= currentNode.MinCoordsX && nearestPointXLeft <= currentNode.CenterX;
        //    bool yIncludeLeft = nearestPointYLeft >= currentNode.MinCoordsY && nearestPointYLeft <= currentNode.MaxCoordsY;
        //    bool zIncludeLeft = nearestPointZLeft >= currentNode.MinCoordsZ && nearestPointZLeft <= currentNode.MaxCoordsZ;

        //    bool xIncludeRight = nearestPointXRight >= currentNode.CenterX && nearestPointXRight <= currentNode.MaxCoordsX;
        //    bool yIncludeRight = nearestPointYRight >= currentNode.MinCoordsY && nearestPointYRight <= currentNode.MaxCoordsY;
        //    bool zIncludeRight = nearestPointZRight >= currentNode.MinCoordsZ && nearestPointZRight <= currentNode.MaxCoordsZ;

        //    bool xIncludeBottom = nearestPointXBottom >= currentNode.MinCoordsX && nearestPointXBottom <= currentNode.MaxCoordsX;
        //    bool yIncludeBottom = nearestPointYBottom >= currentNode.MinCoordsY && nearestPointYBottom <= currentNode.CenterY;
        //    bool zIncludeBottom = nearestPointZBottom >= currentNode.MinCoordsZ && nearestPointZBottom <= currentNode.MaxCoordsZ;

        //    bool xIncludeTop = nearestPointXTop >= currentNode.MinCoordsX && nearestPointXTop <= currentNode.MaxCoordsX;
        //    bool yIncludeTop = nearestPointYTop >= currentNode.CenterY && nearestPointYTop <= currentNode.MaxCoordsY;
        //    bool zIncludeTop = nearestPointZTop >= currentNode.MinCoordsZ && nearestPointZTop <= currentNode.MaxCoordsZ;

        //    bool xIncludeBack = nearestPointXBack >= currentNode.MinCoordsX && nearestPointXBack <= currentNode.MaxCoordsX;
        //    bool yIncludeBack = nearestPointYBack >= currentNode.MinCoordsY && nearestPointYBack <= currentNode.MaxCoordsY;
        //    bool zIncludeBack = nearestPointZBack >= currentNode.MinCoordsZ && nearestPointZBack <= currentNode.CenterZ;

        //    bool xIncludeFront = nearestPointXFront >= currentNode.MinCoordsX && nearestPointXFront <= currentNode.MaxCoordsX;
        //    bool yIncludeFront = nearestPointYFront >= currentNode.MinCoordsY && nearestPointYFront <= currentNode.MaxCoordsY;
        //    bool zIncludeFront = nearestPointZFront >= currentNode.CenterZ && nearestPointZFront <= currentNode.MaxCoordsZ;


        //    //check collisions with sections
        //    bool collidesWithLeft = xIncludeLeft && yIncludeLeft && zIncludeLeft;
        //    bool collidesWithRight = xIncludeRight && yIncludeRight && zIncludeLeft;
        //    bool collidesWithBottom = xIncludeBottom && yIncludeBottom && zIncludeBottom;
        //    bool collidesWithTop = xIncludeTop && yIncludeTop && zIncludeTop;
        //    bool collidesWithBack = xIncludeBack && yIncludeBack && zIncludeBack;
        //    bool collidesWithFront = xIncludeFront && yIncludeFront && zIncludeFront;


        //    //Child arrangement is binary
        //    //OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0);                 //---
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1); //--+
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2); //-+-
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3); //-++
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4); //+--
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5); //+-+
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6); //++-
        //    //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7); //+++

        //    OctreeNode currentChild = currentNode.FirstChild;

        //    //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
        //    //Conditions are binary counted up like the child arrangement
        //    //000; 001; 010; 011...
        //    //==
        //    //leftBotBack; leftBotFront; leftTopBack; leftTopFront...
        //    if (collidesWithLeft && collidesWithBottom && collidesWithBack)
        //    {
        //        collisionCount++;
        //        collisions.Add(0);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithLeft && collidesWithBottom && collidesWithFront)
        //    {
        //        collisionCount++;
        //        collisions.Add(1);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithLeft && collidesWithTop && collidesWithBack)
        //    {
        //        collisionCount++;
        //        collisions.Add(2);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithLeft && collidesWithTop && collidesWithFront)
        //    {
        //        collisionCount++;
        //        collisions.Add(3);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithRight && collidesWithBottom && collidesWithBack)
        //    {
        //        collisionCount++;
        //        collisions.Add(4);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithRight && collidesWithBottom && collidesWithFront)
        //    {
        //        collisionCount++;
        //        collisions.Add(5);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithRight && collidesWithTop && collidesWithBack)
        //    {
        //        collisionCount++;
        //        collisions.Add(6);
        //    }
        //    currentChild = currentChild.NextSibling;

        //    if (collidesWithRight && collidesWithTop && collidesWithFront)
        //    {
        //        collisionCount++;
        //        collisions.Add(7);
        //    }
        //    currentChild = currentChild.NextSibling;



        //    if (collisionCount == 8)
        //    {
        //        currentNode.collisionSphereEntities.Add(entity);
        //        return;
        //    }

        //    currentChild = currentNode.FirstChild;
        //    int lastIdx = 0;
        //    foreach (var idx in collisions)
        //    {
        //        for (int i = 0; i < idx - lastIdx; i++)
        //        {
        //            currentChild = currentChild.NextSibling;
        //        }

        //        InsertIntoOctree(currentChild, entity);
        //        lastIdx = idx;
        //    }
        //}




        //Old insert

        //private void InsertIntoOctree(OctreeNode currentNode, CollisionSphereEntity entity)
        //{
        //    //Last level of octree
        //    if (currentNode.ChildsAreInitialized == false)
        //    {
        //        currentNode.collisionSphereEntities.Add(entity);
        //        return;
        //    }

        //    float minDistTillCollisionSquared = (entity.CollisionSphereRadius + currentNode.firstChild.QuaterSize * 2);
        //    minDistTillCollisionSquared *= minDistTillCollisionSquared;


        //    //Simple collision detection. Octree node simplified to sphere with radius HalfSize
        //    List<int> collisions = new List<int>();

        //    float entityPosX = entity.PosX;
        //    float entityPosY = entity.PosY;
        //    float entityPosZ = entity.PosZ;

        //    float collisionCount = 0;

        //    OctreeNode currentChild = currentNode.firstChild;

        //    while(currentChild != null)
        //    {
        //        float xDist = currentChild.CenterX - entityPosX;
        //        float yDist = currentChild.CenterY - entityPosY;
        //        float zDist = currentChild.CenterZ - entityPosZ;
        //        if (minDistTillCollisionSquared > (xDist * xDist + yDist * yDist + zDist * zDist))
        //        {
        //            collisions.Add(currentChild.Index);
        //            collisionCount++;
        //        }

        //        currentChild = currentChild.nextSibling;
        //    }


        //    //Collides with every child so add it to the current node and end insert
        //    if (collisionCount == 8)
        //    {
        //        currentNode.collisionSphereEntities.Add(entity);
        //        return;
        //    }

        //    List<int> hardCollisions = new List<int>();
        //    var collIndexEnumerator = collisions.GetEnumerator();
        //    bool hasNext = collIndexEnumerator.MoveNext();

        //    Vector3 entityPos = new Vector3(entity.PosX, entityPosY, entity.PosZ);

        //    currentChild = currentNode.firstChild;

        //    while (currentChild != null)
        //    {
        //        if (hasNext)
        //        {
        //            if (currentChild.Index == collIndexEnumerator.Current)
        //            {
        //                hasNext = collIndexEnumerator.MoveNext();
        //                continue;
        //            }
        //        }

        //        float distX = currentChild.CenterX - entity.PosX;
        //        float distY = currentChild.CenterY - entity.PosY;
        //        float distZ = currentChild.CenterZ - entity.PosZ;
        //        float length = (float)Math.Sqrt(distX * distX + distY * distY + distZ * distZ);

        //        float nearestPointX = distX / length * entity.CollisionSphereRadius;
        //        float nearestPointY = distY / length * entity.CollisionSphereRadius;
        //        float nearestPointZ = distZ / length * entity.CollisionSphereRadius;

        //        bool xInclude = nearestPointX > currentChild.MinCoordsX && nearestPointX < currentChild.MaxCoordsX;
        //        bool yInclude = nearestPointY > currentChild.MinCoordsY && nearestPointY < currentChild.MaxCoordsY;
        //        bool zInclude = nearestPointZ > currentChild.MinCoordsZ && nearestPointZ < currentChild.MaxCoordsZ;

        //        if (xInclude && yInclude && zInclude)
        //        {
        //            hardCollisions.Add(currentChild.Index);
        //            collisionCount++;
        //        }

        //        currentChild = currentChild.nextSibling;
        //    }

        //    if (collisionCount == 8)
        //    {
        //        currentNode.collisionSphereEntities.Add(entity);
        //        return;
        //    }

        //    currentChild = currentNode.firstChild;
        //    int lastIdx = 0;

        //    foreach (var idx in collisions)
        //    {
        //        for(int i = 0; i < idx - lastIdx; i++)
        //        {
        //            currentChild = currentChild.nextSibling;
        //        }
        //        InsertIntoOctree(currentChild, entity);
        //        lastIdx = idx;
        //    }

        //    currentChild = currentNode.firstChild;
        //    lastIdx = 0;

        //    foreach (var idx in collisions)
        //    {
        //        for (int i = 0; i < idx - lastIdx; i++)
        //        {
        //            currentChild = currentChild.nextSibling;
        //        }
        //        InsertIntoOctree(currentChild, entity);
        //        lastIdx = idx;
        //    }
        //}
    }
}
