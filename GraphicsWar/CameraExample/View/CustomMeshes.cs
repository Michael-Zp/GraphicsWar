using System;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;

namespace GraphicsWar.View
{
    public static class CustomMeshes
    {
        public static DefaultMesh CreateTriangle()
        {
            var mesh = new DefaultMesh();
            mesh.Position.Add(new Vector3(0.0f, 1.0f, 0.0f));
            mesh.IDs.Add(0);
            mesh.Normal.Add(-Vector3.UnitZ);
            mesh.Position.Add(Vector3.Normalize(new Vector3(1.0f, -1.0f, 0.0f)));
            mesh.IDs.Add(1);
            mesh.Normal.Add(-Vector3.UnitZ);
            mesh.Position.Add(Vector3.Normalize(new Vector3(-1.0f, -1.0f, 0.0f)));
            mesh.IDs.Add(2);
            mesh.Normal.Add(-Vector3.UnitZ);

            mesh.Position.Add(new Vector3(0.0f, 1.0f, 0.0f));
            mesh.IDs.Add(3);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Position.Add(Vector3.Normalize(new Vector3(-1.0f, -1.0f, 0.0f)));
            mesh.IDs.Add(4);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Position.Add(Vector3.Normalize(new Vector3(1.0f, -1.0f, 0.0f)));
            mesh.IDs.Add(5);
            mesh.Normal.Add(Vector3.UnitZ);

            return mesh;
        }
        public static DefaultMesh CreateIcosaeder()
        {
            var mesh = new DefaultMesh();
            mesh.Position.Add(new Vector3(-0.382f, 0.618f, 0.0f));
            mesh.Position.Add(new Vector3(0.382f, 0.618f, 0.0f));
            mesh.Position.Add(new Vector3(-0.382f, -0.618f, 0.0f));
            mesh.Position.Add(new Vector3(0.382f, -0.618f, 0.0f));

            mesh.Position.Add(new Vector3(-0.618f, 0.0f, 0.382f));
            mesh.Position.Add(new Vector3(0.618f, 0.0f, 0.382f));
            mesh.Position.Add(new Vector3(-0.618f, 0.0f, -0.382f));
            mesh.Position.Add(new Vector3(0.618f, 0.0f, -0.382f));

            mesh.Position.Add(new Vector3(0.0f, -0.382f, 0.618f));
            mesh.Position.Add(new Vector3(0.0f, 0.382f, 0.618f));
            mesh.Position.Add(new Vector3(0.0f, -0.382f, -0.618f));
            mesh.Position.Add(new Vector3(0.0f, 0.382f, -0.618f));

            mesh.IDs.Add(0);
            mesh.IDs.Add(9);
            mesh.IDs.Add(1);

            mesh.IDs.Add(0);
            mesh.IDs.Add(1);
            mesh.IDs.Add(11);

            mesh.IDs.Add(8);
            mesh.IDs.Add(2);
            mesh.IDs.Add(3);

            mesh.IDs.Add(3);
            mesh.IDs.Add(2);
            mesh.IDs.Add(10);

            mesh.IDs.Add(1);
            mesh.IDs.Add(5);
            mesh.IDs.Add(7);

            mesh.IDs.Add(0);
            mesh.IDs.Add(6);
            mesh.IDs.Add(4);

            mesh.IDs.Add(6);
            mesh.IDs.Add(2);
            mesh.IDs.Add(4);

            mesh.IDs.Add(7);
            mesh.IDs.Add(5);
            mesh.IDs.Add(3);

            mesh.IDs.Add(1);
            mesh.IDs.Add(9);
            mesh.IDs.Add(5);

            mesh.IDs.Add(9);
            mesh.IDs.Add(0);
            mesh.IDs.Add(4);

            mesh.IDs.Add(0);
            mesh.IDs.Add(11);
            mesh.IDs.Add(6);

            mesh.IDs.Add(1);
            mesh.IDs.Add(7);
            mesh.IDs.Add(11);

            mesh.IDs.Add(8);
            mesh.IDs.Add(3);
            mesh.IDs.Add(5);

            mesh.IDs.Add(4);
            mesh.IDs.Add(2);
            mesh.IDs.Add(8);

            mesh.IDs.Add(7);
            mesh.IDs.Add(3);
            mesh.IDs.Add(10);

            mesh.IDs.Add(10);
            mesh.IDs.Add(2);
            mesh.IDs.Add(6);

            mesh.IDs.Add(5);
            mesh.IDs.Add(9);
            mesh.IDs.Add(8);

            mesh.IDs.Add(4);
            mesh.IDs.Add(8);
            mesh.IDs.Add(9);

            mesh.IDs.Add(7);
            mesh.IDs.Add(10);
            mesh.IDs.Add(11);

            mesh.IDs.Add(6);
            mesh.IDs.Add(11);
            mesh.IDs.Add(10);

            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);
            mesh.Normal.Add(Vector3.UnitZ);


            return mesh;
        }




        private static DefaultMesh VoronoiMeshTower(Vector2 center, IEnumerable<Vector2> neighbors, float height)
        {
            DefaultMesh mesh = new DefaultMesh();



            return mesh;
        }
        public static DefaultMesh VoronoiMesh(int sizeX, int sizeZ)
        {
            Random rand = new Random(0);
            DefaultMesh mesh = new DefaultMesh();

            float RandFloat() => (float)rand.NextDouble();

            float[,] heights = new float[sizeX + 2, sizeZ + 2];
            Vector2[,] centers = new Vector2[sizeX + 2, sizeZ + 2];

            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int z = 0; z < sizeZ + 2; z++)
                {
                    heights[x, z] = RandFloat();
                    centers[x, z] = new Vector2(RandFloat(), RandFloat());
                }
            }

            for (int x = 1; x < sizeX + 1; x++)
            {
                for (int z = 1; z < sizeZ + 1; z++)
                {
                    List<Vector2> neighbors = new List<Vector2>()
                    {
                        centers[x-1,z-1],
                        centers[x-1,z],
                        centers[x-1,z+1],
                        centers[x,z-1],
                        centers[x,z+1],
                        centers[x+1,z-1],
                        centers[x+1,z],
                        centers[x+1,z+1]
                    };
                    mesh.Add(VoronoiMeshTower(centers[x, z], neighbors, heights[x, z]));
                }
            }

            

            mesh.Add(VoronoiMeshTower(0, 0))


            return mesh;
        }
    }
}
