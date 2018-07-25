using GraphicsWar.Model.Physics;
using System.Collections.Generic;
using System.Threading;

namespace GraphicsWar.Model
{
    public class OctreeNode
    {
        public Mutex NodeMutex = new Mutex();
        public OctreeNode FirstChild = null;
        public OctreeNode NextSibling = null;
        public OctreeNode Parent = null;

        
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

            OctreeNode currentChild = new OctreeNode(xMin, yMin, zMin, childSize, childHalfSize, childQuaterSize, 0, this); //---
            FirstChild = currentChild;

            currentChild = currentChild.NextSibling = new OctreeNode(xMin, yMin, zMax, childSize, childHalfSize, childQuaterSize, 1, this); //--+
            currentChild = currentChild.NextSibling = new OctreeNode(xMin, yMax, zMin, childSize, childHalfSize, childQuaterSize, 2, this); //-+-
            currentChild = currentChild.NextSibling = new OctreeNode(xMin, yMax, zMax, childSize, childHalfSize, childQuaterSize, 3, this); //-++
            currentChild = currentChild.NextSibling = new OctreeNode(xMax, yMin, zMin, childSize, childHalfSize, childQuaterSize, 4, this); //+--
            currentChild = currentChild.NextSibling = new OctreeNode(xMax, yMin, zMax, childSize, childHalfSize, childQuaterSize, 5, this); //+-+
            currentChild = currentChild.NextSibling = new OctreeNode(xMax, yMax, zMin, childSize, childHalfSize, childQuaterSize, 6, this); //++-
            currentChild = currentChild.NextSibling = new OctreeNode(xMax, yMax, zMax, childSize, childHalfSize, childQuaterSize, 7, this); //+++

            ChildsAreInitialized = true;
        }

        public Mutex CollisionSphereEntitiesMutex = new Mutex();
        public List<CollisionSphereEntity> CollisionSphereEntities = new List<CollisionSphereEntity>();
        public Mutex CollisionCubeEntitiesMutex = new Mutex();
        public List<CollisionCubeEntity> CollisionCubeEntities = new List<CollisionCubeEntity>();
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
            CollisionSphereEntities.Clear();
            CollisionCubeEntities.Clear();
        }

        public OctreeNode(float centerX, float centerY, float centerZ, float size, float halfSize, float quaterSize, int index, OctreeNode parent)
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

            Parent = parent;
        }
    }
}
