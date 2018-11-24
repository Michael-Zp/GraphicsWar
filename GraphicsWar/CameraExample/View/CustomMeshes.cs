using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Zenseless.Geometry;
using GraphicsWar.ExtensionMethods;

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


        private struct OrthogonalLine
        {
            public Vector2 Normal;
            public Vector2 Origin;
            public Vector3 Crossprodukt;

            public OrthogonalLine(Vector2 normal, Vector2 point)
            {
                Vector2 pointOnLine = point + new Vector2(normal.Y, -normal.X);
                Normal = normal;
                Origin = point;
                Crossprodukt = Vector3.Cross(new Vector3(Origin.X, Origin.Y, 1.0f), new Vector3(pointOnLine.X, pointOnLine.Y, 1.0f));
            }
        }

        private struct ValidIntersection
        {
            public Vector2 Point;
            public float Angle;

            public ValidIntersection(Vector2 point, float angle)
            {
                Point = point;
                Angle = angle;
            }
        }



        private static VoronoiMesh VoronoiMeshTower(Vector2 center, List<Vector2> neighbors, float height)
        {
            VoronoiMesh mesh = new VoronoiMesh(new DefaultMesh());

            List<OrthogonalLine> orthogonals = new List<OrthogonalLine>();

            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector2 centerToNeighbor = neighbors[i] - center;
                Vector2 centerBetweenCenters = center + centerToNeighbor / 2;

                orthogonals.Add(new OrthogonalLine(Vector2.Normalize(centerToNeighbor), centerBetweenCenters));
            }

            List<Vector2> intersections = new List<Vector2>();

            for (int i = 0; i < orthogonals.Count; i++)
            {
                for (int k = (i + 1); k < orthogonals.Count; k++)
                {
                    Vector3 crossP12 = Vector3.Cross(orthogonals[i].Crossprodukt, orthogonals[k].Crossprodukt);

                    if (crossP12.Z != 0)
                    {
                        Vector2 intersectPoint = new Vector2(crossP12.X, crossP12.Y) / crossP12.Z;
                        intersections.Add(intersectPoint);
                        //Vector2 distanceVector = intersectPoint - center;
                        //intersections.Add(new Intersection(intersectPoint, Vector2.Dot(distanceVector, distanceVector)));
                    }
                }
            }

            List<ValidIntersection> validIntersections = new List<ValidIntersection>();

            foreach (var intersection in intersections)
            {
                bool inside = true;


                foreach (var orthogonal in orthogonals)
                {
                    Vector2 originToIntersection = Vector2.Normalize(intersection - orthogonal.Origin);
                    inside = Vector2.Dot(orthogonal.Normal, originToIntersection) <= 1e-6f;

                    if (!inside)
                        break;
                }

                if (inside)
                {
                    Vector2 centerToIntersection = intersection - center;
                    validIntersections.Add(new ValidIntersection(intersection, (float)(Math.Atan2(1, 0) - Math.Atan2(centerToIntersection.Y, centerToIntersection.X))));
                }
            }

            validIntersections = validIntersections.OrderBy(validIntersection => validIntersection.Angle)
                                                   .Distinct()
                                                   .ToList();
            uint id = 0;

            for (int i = 0; i < validIntersections.Count; i++)
            {
                mesh.Position.Add(new Vector3(validIntersections[i].Point.X, height, validIntersections[i].Point.Y));
                mesh.Normal.Add(Vector3.UnitY);
                if (i > 1)
                {
                    if(i == 2)
                    {
                        mesh.Plateaus.Add(new List<int[]>());
                    }

                    mesh.IDs.Add(0);
                    mesh.IDs.Add(id - 1);
                    mesh.IDs.Add(id);

                    int count = mesh.IDs.Count;
                    mesh.Plateaus[mesh.Plateaus.Count - 1].Add(new int[] { (int)mesh.IDs[count - 3], (int)mesh.IDs[count - 2], (int)mesh.IDs[count - 1] });
                }
                id++;
            }

            for (int i = 0; i < validIntersections.Count; i++)
            {
                int nextI = (i + 1) % validIntersections.Count;

                Vector3[] points = new Vector3[]
                {
                    new Vector3(validIntersections[i].Point.X, height, validIntersections[i].Point.Y),
                    new Vector3(validIntersections[i].Point.X, 0, validIntersections[i].Point.Y),
                    new Vector3(validIntersections[nextI].Point.X, 0, validIntersections[nextI].Point.Y),
                    new Vector3(validIntersections[nextI].Point.X, height, validIntersections[nextI].Point.Y)
                };

                Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);

                mesh.Position.Add(points[0]);
                mesh.Normal.Add(normal);
                mesh.Position.Add(points[1]);
                mesh.Normal.Add(normal);
                mesh.Position.Add(points[2]);
                mesh.Normal.Add(normal);
                mesh.Position.Add(points[3]);
                mesh.Normal.Add(normal);

                mesh.IDs.Add(id);
                mesh.IDs.Add(id + 1);
                mesh.IDs.Add(id + 2);

                mesh.IDs.Add(id);
                mesh.IDs.Add(id + 2);
                mesh.IDs.Add(id + 3);

                id += 4;
            }

            return mesh;
        }


        public static VoronoiMesh VoronoiMesh(int sizeX, int sizeY)
        {
            Random rand = new Random(345546);
            VoronoiMesh mesh = new VoronoiMesh(new DefaultMesh());

            float RandFloat() => (float)rand.NextDouble();

            float[,] heights = new float[sizeX + 2, sizeY + 2];
            Vector2[,] centers = new Vector2[sizeX + 2, sizeY + 2];

            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    heights[x, y] = RandFloat();
                    centers[x, y] = new Vector2(RandFloat() + x - ((sizeX + 2) / 2), RandFloat() + y - ((sizeY + 2) / 2));
                }
            }

            for (int x = 1; x < sizeX + 1; x++)
            {
                for (int y = 1; y < sizeY + 1; y++)
                {
                    List<Vector2> neighbors = new List<Vector2>()
                    {
                        centers[x-1,y-1],
                        centers[x-1,y],
                        centers[x-1,y+1],
                        centers[x,y-1],
                        centers[x,y+1],
                        centers[x+1,y-1],
                        centers[x+1,y],
                        centers[x+1,y+1]
                    };
                    mesh.Add(VoronoiMeshTower(centers[x, y], neighbors, heights[x, y]));
                }
            }

            DefaultMesh plane = Meshes.CreatePlane(sizeX, sizeY, 1, 1);
            plane.TexCoord.Clear();
            mesh.Add(plane);

            mesh.GenerateRandomPositionsOnPlateau();

            return mesh;
        }
    }
}
