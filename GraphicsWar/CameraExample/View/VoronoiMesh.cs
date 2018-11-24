using System;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;

namespace GraphicsWar.View
{
    public class VoronoiMesh : DefaultMesh
    {
        /// <summary>
        /// Tangent vector.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public List<List<int[]>> Plateaus { get; } = new List<List<int[]>>();
        
        
        public Dictionary<Shared.Enums.EntityType, List<Matrix4x4>> Crystals {
            get {
                if(_crystals == null)
                {
                    GenerateRandomPositionsOnPlateau();
                }

                return _crystals;
            }
        }

        private Dictionary<Shared.Enums.EntityType, List<Matrix4x4>> _crystals = null;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMesh"/> class.
        /// </summary>

        public VoronoiMesh(DefaultMesh mesh)
        {
            Position.AddRange(mesh.Position);
            Normal.AddRange(mesh.Normal);
            TexCoord.AddRange(mesh.TexCoord);
            IDs.AddRange(mesh.IDs);
        }

        public void GenerateRandomPositionsOnPlateau()
        {
            if(_crystals == null)
            {
                _crystals = new Dictionary<Shared.Enums.EntityType, List<Matrix4x4>>();

                Random random = new Random();

                _crystals.Add(Shared.Enums.EntityType.Crystal1, new List<Matrix4x4>());
                _crystals.Add(Shared.Enums.EntityType.Crystal2, new List<Matrix4x4>());


                foreach (var plateauIds in Plateaus)
                {
                    float height = Position[plateauIds[0][0]].Y;
                    int trianglesCount = plateauIds.Count / 3;

                    int crystalCount = random.Next(0, 2);


                    for (int i = 0; i < crystalCount; i++)
                    {
                        int selectedTriangle = random.Next(0, trianglesCount);
                        Vector2 barycentricCoords = new Vector2((float)random.NextDouble(), (float)random.NextDouble());

                        Vector2 point0 = new Vector2(Position[plateauIds[selectedTriangle][0]].X, Position[plateauIds[selectedTriangle][0]].Z);
                        Vector2 point1 = new Vector2(Position[plateauIds[selectedTriangle][1]].X, Position[plateauIds[selectedTriangle][1]].Z);
                        Vector2 point2 = new Vector2(Position[plateauIds[selectedTriangle][2]].X, Position[plateauIds[selectedTriangle][2]].Z);

                        Vector2 vecA = point1 - point0;
                        Vector2 vecB = point2 - point0;

                        Vector2 point2D = point0;// + vecA * barycentricCoords.X + vecB * barycentricCoords.Y;

                        Shared.Enums.EntityType type = random.Next(0, 2) == 0 ? Shared.Enums.EntityType.Crystal1 : Shared.Enums.EntityType.Crystal2;

                        //_crystals[type].Add(Matrix4x4.CreateTranslation( new Vector3(point2D.X, height, point2D.Y)));
                        _crystals[type].Add(Matrix4x4.CreateTranslation(Position[plateauIds[0][0]]));
                    }
                }
            }
        }
    }
}
