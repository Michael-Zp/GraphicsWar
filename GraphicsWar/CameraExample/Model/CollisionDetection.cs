using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace GraphicsWar.Model
{
    public static class CollisionDetection
    {
        private static OctreeNode _octreeRoot;

        private static readonly int _levels = 7;

        public static void InitializeCollisionDetectionForFrame(List<Entity> entities, float size, Vector3 center)
        {

            _octreeRoot = new OctreeNode(center.X, center.Y, center.Z, size, size / 2, size / 4, 0);

            Stopwatch sw = new Stopwatch();

            sw.Start();

            IEnumerable<CollisionSphereEntity> collisionEntities =
                from entity in entities
                where entity is CollisionSphereEntity
                select entity as CollisionSphereEntity;

            sw.Stop();

            float time1 = sw.ElapsedMilliseconds;
            float ticks1 = sw.ElapsedTicks;

            sw.Restart();

            InitializeOctreeRoot(_octreeRoot, _levels); //Move into static part

            float time2 = sw.ElapsedMilliseconds;
            float ticks2 = sw.ElapsedTicks;


            sw.Restart();

            //Should be faster but I really dont know how I could do that.
            ResetOctree(_octreeRoot, _levels);

            float time3 = sw.ElapsedMilliseconds;
            float ticks3 = sw.ElapsedTicks;


            sw.Restart();

            foreach (var entity in collisionEntities)
            {
                InsertIntoOctree(_octreeRoot, entity);
            }

            float time4 = sw.ElapsedMilliseconds;
            float ticks4 = sw.ElapsedTicks;

            Console.WriteLine("");
        }

        
        private static void InsertIntoOctree(OctreeNode currentNode, CollisionSphereEntity entity)
        {
            //As you might see in this method there is much code that look rather similar
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


            //Get axis alinged distance
            float distX = currentNode.CenterX - entity.PosX;
            float distY = currentNode.CenterY - entity.PosY;
            float distZ = currentNode.CenterZ - entity.PosZ;
            
            //Slice parent into left/right, top/bottom and front/back sections
            float distToCenterLeftX = (currentNode.CenterX - currentNode.QuaterSize) - entity.PosX;
            float distToCenterRightX = (currentNode.CenterX + currentNode.QuaterSize) - entity.PosX;
            float distToCenterBottomY = (currentNode.CenterY - currentNode.QuaterSize) - entity.PosY;
            float distToCenterTopY = (currentNode.CenterY + currentNode.QuaterSize) - entity.PosY;
            float distToCenterBackZ = (currentNode.CenterZ - currentNode.QuaterSize) - entity.PosZ;
            float distToCenterFrontZ = (currentNode.CenterZ + currentNode.QuaterSize) - entity.PosZ;

            float distXSquare = distX * distX;
            float distYSquare = distY * distY;
            float distZSquare = distZ * distZ;


            //Generate nearest points to left/right... sections
            float lengthLeftToCenter = (float)Math.Sqrt(distToCenterLeftX * distToCenterLeftX + distYSquare + distZSquare);
            float lengthRightToCenter = (float)Math.Sqrt(distToCenterRightX * distToCenterRightX + distYSquare + distZSquare);
            float lengthBottomToCenter = (float)Math.Sqrt(distXSquare + distToCenterBottomY * distToCenterBottomY + distZSquare);
            float lengthTopToCenter = (float)Math.Sqrt(distXSquare + distToCenterTopY * distToCenterTopY + distZSquare);
            float lengthBackToCenter = (float)Math.Sqrt(distXSquare + distYSquare + distToCenterBackZ * distToCenterBackZ);
            float lengthFrontToCenter = (float)Math.Sqrt(distXSquare + distYSquare + distToCenterFrontZ * distToCenterFrontZ);


            float nearestPointXLeft = distX / lengthLeftToCenter * entity.CollisionSphereRadius;
            float nearestPointYLeft = distY / lengthLeftToCenter * entity.CollisionSphereRadius;
            float nearestPointZLeft = distZ / lengthLeftToCenter * entity.CollisionSphereRadius;

            float nearestPointXRight = distX / lengthRightToCenter * entity.CollisionSphereRadius;
            float nearestPointYRight = distY / lengthRightToCenter * entity.CollisionSphereRadius;
            float nearestPointZRight = distZ / lengthRightToCenter * entity.CollisionSphereRadius;

            float nearestPointXBottom = distX / lengthBottomToCenter * entity.CollisionSphereRadius;
            float nearestPointYBottom = distY / lengthBottomToCenter * entity.CollisionSphereRadius;
            float nearestPointZBottom = distZ / lengthBottomToCenter * entity.CollisionSphereRadius;

            float nearestPointXTop = distX / lengthTopToCenter * entity.CollisionSphereRadius;
            float nearestPointYTop = distY / lengthTopToCenter * entity.CollisionSphereRadius;
            float nearestPointZTop = distZ / lengthTopToCenter * entity.CollisionSphereRadius;

            float nearestPointXBack = distX / lengthBackToCenter * entity.CollisionSphereRadius;
            float nearestPointYBack = distY / lengthBackToCenter * entity.CollisionSphereRadius;
            float nearestPointZBack = distZ / lengthBackToCenter * entity.CollisionSphereRadius;

            float nearestPointXFront = distX / lengthFrontToCenter * entity.CollisionSphereRadius;
            float nearestPointYFront = distY / lengthFrontToCenter * entity.CollisionSphereRadius;
            float nearestPointZFront = distZ / lengthFrontToCenter * entity.CollisionSphereRadius;


            //Check if points are in the sections
            bool xIncludeLeft = nearestPointXLeft > currentNode.MinCoordsX && nearestPointXLeft < currentNode.CenterX;
            bool yIncludeLeft = nearestPointYLeft > currentNode.MinCoordsY && nearestPointYLeft < currentNode.MaxCoordsY;
            bool zIncludeLeft = nearestPointZLeft > currentNode.MinCoordsZ && nearestPointZLeft < currentNode.MaxCoordsZ;

            bool xIncludeRight = nearestPointXRight > currentNode.CenterX && nearestPointXRight < currentNode.MaxCoordsX;
            bool yIncludeRight = nearestPointYRight > currentNode.MinCoordsY && nearestPointYRight < currentNode.MaxCoordsY;
            bool zIncludeRight = nearestPointZRight > currentNode.MinCoordsZ && nearestPointZRight < currentNode.MaxCoordsZ;

            bool xIncludeBottom = nearestPointXBottom > currentNode.MinCoordsX && nearestPointXBottom < currentNode.MaxCoordsX;
            bool yIncludeBottom = nearestPointYBottom > currentNode.MinCoordsY && nearestPointYBottom < currentNode.CenterY;
            bool zIncludeBottom = nearestPointZBottom > currentNode.MinCoordsZ && nearestPointZBottom < currentNode.MaxCoordsZ;

            bool xIncludeTop = nearestPointXTop > currentNode.MinCoordsX && nearestPointXTop < currentNode.MaxCoordsX;
            bool yIncludeTop = nearestPointYTop > currentNode.CenterY && nearestPointYTop < currentNode.MaxCoordsY;
            bool zIncludeTop = nearestPointZTop > currentNode.MinCoordsZ && nearestPointZTop < currentNode.MaxCoordsZ;

            bool xIncludeBack = nearestPointXBack > currentNode.MinCoordsX && nearestPointXBack < currentNode.MaxCoordsZ;
            bool yIncludeBack = nearestPointYBack > currentNode.MinCoordsY && nearestPointYBack < currentNode.MaxCoordsY;
            bool zIncludeBack = nearestPointZBack > currentNode.MinCoordsZ && nearestPointZBack < currentNode.CenterZ;

            bool xIncludeFront = nearestPointXFront > currentNode.MinCoordsX && nearestPointXFront < currentNode.MaxCoordsZ;
            bool yIncludeFront = nearestPointYFront > currentNode.MinCoordsY && nearestPointYFront < currentNode.MaxCoordsY;
            bool zIncludeFront = nearestPointZFront > currentNode.MinCoordsZ && nearestPointZFront < currentNode.CenterZ;


            //check collisions with sections
            bool collidesWithLeft = xIncludeLeft && yIncludeLeft && zIncludeLeft;
            bool collidesWithRight = xIncludeRight && yIncludeRight && zIncludeLeft;
            bool collidesWithBottom = xIncludeBottom && yIncludeBottom && zIncludeBottom;
            bool collidesWithTop = xIncludeTop && yIncludeTop && zIncludeTop;
            bool collidesWithBack = xIncludeBack && yIncludeBack && zIncludeBack;
            bool collidesWithFront = xIncludeFront && yIncludeFront && zIncludeFront;


            //Child arrangement is binary
            //OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0);                 //---
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1); //--+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2); //-+-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3); //-++
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4); //+--
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5); //+-+
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6); //++-
            //currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7); //+++

            OctreeNode currentChild = currentNode.firstChild;

            //left = 0 right = 1; bottom = 0 top = 1; back = 0 front = 1
            //Conditions are binary counted up like the child arrangement
            //000; 001; 010; 011...
            //==
            //leftBotBack; leftBotFront; leftTopBack; leftTopFront...
            if (collidesWithLeft && collidesWithBottom && collidesWithBack)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithLeft && collidesWithBottom && collidesWithFront)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithBack)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithLeft && collidesWithTop && collidesWithFront)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithBack)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithRight && collidesWithBottom && collidesWithFront)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithBack)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;

            if (collidesWithRight && collidesWithTop && collidesWithFront)
            {
                currentChild.collisionSphereEntities.Add(entity);
                collisionCount++;
            }
            currentChild = currentChild.nextSibling;



            if (collisionCount == 8)
            {
                currentNode.collisionSphereEntities.Add(entity);
                return;
            }

            currentChild = currentNode.firstChild;
            int lastIdx = 0;
            foreach (var idx in collisions)
            {
                for (int i = 0; i < idx - lastIdx; i++)
                {
                    currentChild = currentChild.nextSibling;
                }

                InsertIntoOctree(currentChild, entity);
                lastIdx = idx;
            }
        }


        //private static void InsertIntoOctree(OctreeNode currentNode, CollisionSphereEntity entity)
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

        private static void InitializeOctreeRoot(OctreeNode current, int level)
        {
            if (level == 0)
            {
                return;
            }

            current.AddChilds();

            OctreeNode currentChild = current.firstChild;

            while (currentChild != null)
            {
                InitializeOctreeRoot(currentChild, level - 1);
                currentChild = currentChild.nextSibling;
            }
        }

        private static void ResetOctree(OctreeNode current, int level)
        {
            if (level == 0)
            {
                return;
            }

            current.Reset();

            int lowerLevel = level - 1;

            OctreeNode currentChild = current.firstChild;

            while (currentChild != null)
            {
                ResetOctree(currentChild, lowerLevel);
                currentChild = currentChild.nextSibling;
            }
        }
    }
}
