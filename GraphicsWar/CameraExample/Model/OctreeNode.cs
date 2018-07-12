using System.Collections.Generic;

namespace GraphicsWar.Model
{
    public class OctreeNode
    {
        public OctreeNode firstChild = null;
        public OctreeNode nextSibling = null;

        
        public void AddChilds()
        {
            float xMin = CenterX - QuaterSize;
            float xMax = CenterX + QuaterSize;

            float yMin = CenterY - QuaterSize;
            float yMax = CenterY + QuaterSize;

            float zMin = CenterZ - QuaterSize;
            float zMax = CenterZ + QuaterSize;

            float childSize = HalfSize;
            float childHalfSize = childSize / 2;
            float childQuaterSize = childSize / 4;

            OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0); //---
            firstChild = currentChild;

            currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1); //--+
            currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2); //-+-
            currentChild = currentChild.nextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3); //-++
            currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4); //+--
            currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5); //+-+
            currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6); //++-
            currentChild = currentChild.nextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7); //+++

            ChildsAreInitialized = true;
        }

        public List<CollisionSphereEntity> collisionSphereEntities = new List<CollisionSphereEntity>();
        public float CenterX;
        public float CenterY;
        public float CenterZ;
        public float Size;
        public float HalfSize;
        public float QuaterSize;
        public int Index;
        public bool ChildsAreInitialized = false;

        public float MinCoordsX;
        public float MinCoordsY;
        public float MinCoordsZ;

        public float MaxCoordsX;
        public float MaxCoordsY;
        public float MaxCoordsZ;

        public void Reset()
        {
            collisionSphereEntities.Clear();
        }

        public OctreeNode(float centerX, float centerY, float centerZ, float size, float halfSize, float quaterSize, int index)
        {
            CenterX = centerX;
            CenterY = centerY;
            CenterZ = centerZ;
            Size = size;
            Index = index;

            HalfSize = halfSize;
            QuaterSize = quaterSize;

            MinCoordsX = centerX - halfSize;
            MinCoordsY = centerY - halfSize;
            MinCoordsZ = centerZ - halfSize;
            
            MaxCoordsX = centerX + halfSize;
            MaxCoordsY = centerY + halfSize;
            MaxCoordsZ = centerZ + halfSize;
        }
    }
}
