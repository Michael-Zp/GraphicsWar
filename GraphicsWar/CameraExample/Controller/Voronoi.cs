using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Zenseless.Geometry;

namespace GraphicsWar.View
{
    public class Voronoi
    {
        public struct VoronoiCrystal
        {
            public readonly Vector3 Position;
            public readonly float ScaleFactor;
            public readonly float RotationFactor;

            public VoronoiCrystal(Vector3 position, float scaleFactor, float rotationFactor)
            {
                Position = position;
                ScaleFactor = scaleFactor;
                RotationFactor = rotationFactor;
            }
        }

        /// <summary>
        /// Tangent vector.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public List<List<int[]>> Plateaus { get; } = new List<List<int[]>>();

        public DefaultMesh Mesh;

        public Dictionary<Shared.Enums.EntityType, List<VoronoiCrystal>> Crystals {
            get {
                if(_crystals == null)
                {
                    GenerateRandomPositionsOnPlateau();
                }

                return _crystals;
            }
        }


        private Dictionary<Shared.Enums.EntityType, List<VoronoiCrystal>> _crystals = null;
        private uint _id = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMesh"/> class.
        /// </summary>

        public Voronoi(int sizeX, int sizeY, Vector3 scale)
        {
            Mesh = new DefaultMesh();
            GenerateVoronoi(sizeX, sizeY, scale);
            GenerateRandomPositionsOnPlateau();
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



        private void GenerateVoronoiTower(Vector2 center, List<Vector2> neighbors, float height)
        {
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

            uint baseId = _id;

            for (int i = 0; i < validIntersections.Count; i++)
            {
                Mesh.Position.Add(new Vector3(validIntersections[i].Point.X, height, validIntersections[i].Point.Y));
                Mesh.Normal.Add(Vector3.UnitY);
                if (i > 1)
                {
                    if (i == 2)
                    {
                        Plateaus.Add(new List<int[]>());
                    }

                    Mesh.IDs.Add(baseId);
                    Mesh.IDs.Add(_id - 1);
                    Mesh.IDs.Add(_id);

                    int count = Mesh.IDs.Count;
                    Plateaus[Plateaus.Count - 1].Add(new int[] { (int)Mesh.IDs[count - 3], (int)Mesh.IDs[count - 2], (int)Mesh.IDs[count - 1] });
                }
                _id++;
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

                Mesh.Position.Add(points[0]);
                Mesh.Normal.Add(normal);
                Mesh.Position.Add(points[1]);
                Mesh.Normal.Add(normal);
                Mesh.Position.Add(points[2]);
                Mesh.Normal.Add(normal);
                Mesh.Position.Add(points[3]);
                Mesh.Normal.Add(normal);

                Mesh.IDs.Add(_id);
                Mesh.IDs.Add(_id + 1);
                Mesh.IDs.Add(_id + 2);

                Mesh.IDs.Add(_id);
                Mesh.IDs.Add(_id + 2);
                Mesh.IDs.Add(_id + 3);

                _id += 4;
            }
        }


        private void GenerateVoronoi(int sizeX, int sizeY, Vector3 scale)
        {
            Random rand = new Random(345546);

            float RandFloat() => (float)rand.NextDouble();

            float[,] heights = new float[sizeX + 2, sizeY + 2];
            Vector2[,] centers = new Vector2[sizeX + 2, sizeY + 2];

            for (int x = 0; x < sizeX + 2; x++)
            {
                for (int y = 0; y < sizeY + 2; y++)
                {
                    heights[x, y] = RandFloat() * scale.Y;
                    centers[x, y] = new Vector2((RandFloat() + x - ((sizeX + 2)) / 2) * scale.X, (RandFloat() + y - ((sizeY + 2) / 2)) * scale.Z);
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

                    GenerateVoronoiTower(centers[x, y], neighbors, heights[x, y]);
                }
            }

            DefaultMesh plane = Meshes.CreatePlane(sizeX * scale.X, sizeY * scale.Z, 1, 1);
            plane.TexCoord.Clear();
            Mesh.Add(plane);
        }

        public void GenerateRandomPositionsOnPlateau()
        {
            if(_crystals == null)
            {
                _crystals = new Dictionary<Shared.Enums.EntityType, List<VoronoiCrystal>>();

                Random random = new Random(236578);

                _crystals.Add(Shared.Enums.EntityType.Crystal1, new List<VoronoiCrystal>());
                _crystals.Add(Shared.Enums.EntityType.Crystal2, new List<VoronoiCrystal>());


                foreach (var plateauIds in Plateaus)
                {
                    float height = Mesh.Position[plateauIds[0][0]].Y;
                    int trianglesCount = plateauIds.Count;

                    int crystalCount = random.Next(0, 11);
                    

                    for (int i = 0; i < crystalCount; i++)
                    {
                        int selectedTriangle = random.Next(0, trianglesCount);
                        Vector3 barycentricCoords = Vector3.Normalize(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));

                        Vector2 point0 = new Vector2(Mesh.Position[plateauIds[selectedTriangle][0]].X, Mesh.Position[plateauIds[selectedTriangle][0]].Z);
                        Vector2 point1 = new Vector2(Mesh.Position[plateauIds[selectedTriangle][1]].X, Mesh.Position[plateauIds[selectedTriangle][1]].Z);
                        Vector2 point2 = new Vector2(Mesh.Position[plateauIds[selectedTriangle][2]].X, Mesh.Position[plateauIds[selectedTriangle][2]].Z);


                        //Has to be Length and not LengthSquared, otherwise is bigger than semiperimeter and Sqrt = NaN because negative
                        float p0Len = (point0 - point1).Length();
                        float p1Len = (point0 - point2).Length();
                        float p2Len = (point1 - point2).Length();

                        //Heron`s formula
                        float semiperimeter = (p0Len + p1Len + p2Len) / 2;
                        float surfaceArea = (float)Math.Sqrt(semiperimeter * (semiperimeter - p0Len) * (semiperimeter - p1Len) * (semiperimeter - p2Len));
                        
                        if(surfaceArea < 8)
                        {
                            continue;
                        }

                        Vector2 half = (point2 - point0) / 2 + point0;
                        Vector2 center = half + (point1 - half) / 3;

                        Vector2 vec0 = point0 - center;
                        Vector2 vec1 = point1 - center;
                        Vector2 vec2 = point2 - center;

                        vec0 *= 0.4f;
                        vec1 *= 0.4f;
                        vec2 *= 0.4f;

                        float x = barycentricCoords.X * vec0.X + barycentricCoords.Y * vec1.X + barycentricCoords.Z * vec2.X;
                        float y = barycentricCoords.X * vec0.Y + barycentricCoords.Y * vec1.Y + barycentricCoords.Z * vec2.Y;


                        Vector2 point2D = new Vector2(x, y) + center;

                        Shared.Enums.EntityType type = random.Next(0, 2) == 0 ? Shared.Enums.EntityType.Crystal1 : Shared.Enums.EntityType.Crystal2;

                        float scaleFactor = (float)(type == Shared.Enums.EntityType.Crystal1 ? random.NextDouble() * 0.4 + 0.8 : random.NextDouble() * 0.05 + 0.2);
                        float rotationFactor = (float)(random.NextDouble() * 2 * Math.PI);

                        _crystals[type].Add(new VoronoiCrystal(new Vector3(point2D.X, height, point2D.Y), scaleFactor, rotationFactor));
                    }
                }
            }
        }
    }
}
