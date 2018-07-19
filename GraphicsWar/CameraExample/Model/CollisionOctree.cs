using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GraphicsWar.Model
{
    public class CollisionOctree
    {
        private OctreeNode _octreeRoot;

        private int _levels;

        public void InitializeNewOctree(int levels, Vector3 center, float size)
        {
            _levels = levels;

            _octreeRoot = new OctreeNode(center.X, center.Y, center.Z, size, size / 2, size / 4, 0, null);
            InitializeOctreeRoot(_octreeRoot, _levels);
        }

        public void InsertIntoOctree(List<Entity> entities)
        {
            IEnumerable<CollisionSphereEntity> collisionEntities =
                from entity in entities
                where entity is CollisionSphereEntity
                select entity as CollisionSphereEntity;

            foreach (var entity in collisionEntities)
            {
                InsertIntoOctree(_octreeRoot, entity);
            }
        }

        public void CheckCollsiionsInTree() => CheckCollisionsInTree(_octreeRoot);

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
        
        private void CheckCollisionsInNode(OctreeNode entityNode)
        {
            while(entityNode != null)
            {
                int entityCount = entityNode.collisionSphereEntities.Count;

                for (int i = 0; i < entityCount; i++)
                {
                    CollisionSphereEntity currEntity = entityNode.collisionSphereEntities[i];
                    for (int k = i + 1; k < entityCount; k++)
                    {
                        CollisionSphereEntity otherEntity = entityNode.collisionSphereEntities[k];
                        float distX = currEntity.PosX - otherEntity.PosX;
                        float distY = currEntity.PosY - otherEntity.PosY;
                        float distZ = currEntity.PosZ - otherEntity.PosZ;


                        float distSquare = (distX * distX + distY * distY + distZ * distZ);


                        float radiusSquare = entityNode.collisionSphereEntities[i].CollisionSphereRadius + entityNode.collisionSphereEntities[k].CollisionSphereRadius;
                        radiusSquare *= radiusSquare;


                        if (distSquare < radiusSquare)
                        {
                            //Do collision code
                        }
                    }
                }

                entityNode = entityNode.Parent;
            }
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



            float minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthLeftToCenter ? entity.CollisionSphereRadius : lengthLeftToCenter;
            float nearestPointXLeft = distX / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYLeft = distY / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZLeft = distZ / lengthLeftToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

            minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthRightToCenter ? entity.CollisionSphereRadius : lengthRightToCenter;
            float nearestPointXRight = distX / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYRight = distY / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZRight = distZ / lengthRightToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

            minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthBottomToCenter ? entity.CollisionSphereRadius : lengthBottomToCenter;
            float nearestPointXBottom = distX / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYBottom = distY / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZBottom = distZ / lengthBottomToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

            minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthTopToCenter ? entity.CollisionSphereRadius : lengthTopToCenter;
            float nearestPointXTop = distX / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYTop = distY / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZTop = distZ / lengthTopToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

            minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthBackToCenter ? entity.CollisionSphereRadius : lengthBackToCenter;
            float nearestPointXBack = distX / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYBack = distY / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZBack = distZ / lengthBackToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;

            minimumOfSphereRadiusAndLengthToCenter = entity.CollisionSphereRadius < lengthFrontToCenter ? entity.CollisionSphereRadius : lengthFrontToCenter;
            float nearestPointXFront = distX / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosX;
            float nearestPointYFront = distY / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosY;
            float nearestPointZFront = distZ / lengthFrontToCenter * minimumOfSphereRadiusAndLengthToCenter + entity.PosZ;


            //Check if points are in the sections
            bool xIncludeLeft = nearestPointXLeft >= currentNode.MinCoordsX && nearestPointXLeft <= currentNode.CenterX;
            bool yIncludeLeft = nearestPointYLeft >= currentNode.MinCoordsY && nearestPointYLeft <= currentNode.MaxCoordsY;
            bool zIncludeLeft = nearestPointZLeft >= currentNode.MinCoordsZ && nearestPointZLeft <= currentNode.MaxCoordsZ;

            bool xIncludeRight = nearestPointXRight >= currentNode.CenterX && nearestPointXRight <= currentNode.MaxCoordsX;
            bool yIncludeRight = nearestPointYRight >= currentNode.MinCoordsY && nearestPointYRight <= currentNode.MaxCoordsY;
            bool zIncludeRight = nearestPointZRight >= currentNode.MinCoordsZ && nearestPointZRight <= currentNode.MaxCoordsZ;

            bool xIncludeBottom = nearestPointXBottom >= currentNode.MinCoordsX && nearestPointXBottom <= currentNode.MaxCoordsX;
            bool yIncludeBottom = nearestPointYBottom >= currentNode.MinCoordsY && nearestPointYBottom <= currentNode.CenterY;
            bool zIncludeBottom = nearestPointZBottom >= currentNode.MinCoordsZ && nearestPointZBottom <= currentNode.MaxCoordsZ;

            bool xIncludeTop = nearestPointXTop >= currentNode.MinCoordsX && nearestPointXTop <= currentNode.MaxCoordsX;
            bool yIncludeTop = nearestPointYTop >= currentNode.CenterY && nearestPointYTop <= currentNode.MaxCoordsY;
            bool zIncludeTop = nearestPointZTop >= currentNode.MinCoordsZ && nearestPointZTop <= currentNode.MaxCoordsZ;

            bool xIncludeBack = nearestPointXBack >= currentNode.MinCoordsX && nearestPointXBack <= currentNode.MaxCoordsZ;
            bool yIncludeBack = nearestPointYBack >= currentNode.MinCoordsY && nearestPointYBack <= currentNode.MaxCoordsY;
            bool zIncludeBack = nearestPointZBack >= currentNode.MinCoordsZ && nearestPointZBack <= currentNode.CenterZ;

            bool xIncludeFront = nearestPointXFront >= currentNode.MinCoordsX && nearestPointXFront <= currentNode.MaxCoordsZ;
            bool yIncludeFront = nearestPointYFront >= currentNode.MinCoordsY && nearestPointYFront <= currentNode.MaxCoordsY;
            bool zIncludeFront = nearestPointZFront >= currentNode.CenterZ && nearestPointZFront <= currentNode.MaxCoordsZ;


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
