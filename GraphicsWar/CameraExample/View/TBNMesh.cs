using System;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Geometry;
using System.Linq;

namespace GraphicsWar.View
{
    public class TBNMesh : DefaultMesh
    {

        /// <summary>
        /// The TBN matrix name
        /// </summary>
        public static readonly string TangentName = "tangent";

        /// <summary>
        /// Tangent vector.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public List<Vector3> Tangent => tangent;

        private List<Vector3> tangent;

        /// <summary>
        /// BitangentName
        /// </summary>
        public static readonly string BitangentName = "bitangent";

        /// <summary>
        /// Bitangent vector.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public List<Vector3> Bitangent => bitangent;

        private List<Vector3> bitangent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMesh"/> class.
        /// </summary>

        public TBNMesh(DefaultMesh mesh) : base()
        {
            Position.AddRange(mesh.Position);
            Normal.AddRange(mesh.Normal);
            TexCoord.AddRange(mesh.TexCoord);
            IDs.AddRange(mesh.IDs);

            tangent = AddAttribute<Vector3>(TangentName);
            bitangent = AddAttribute<Vector3>(BitangentName);
            CalcTangentsAndBitangents();
        }

        /// <summary>
        /// The matricies can only be calculated if the mesh has all positions, indices and uv coordinates initialized.
        /// Thus call this after initializing the mesh.
        /// </summary>
        private void CalcTangentsAndBitangents()
        {
            float maxId = IDs.Max();

            if (maxId > TexCoord.Count)
            {
                throw new ArgumentException("Not all TexCoords are set, tangents and bitangents can not be calculated.");
            }

            //Calculate TBN for every triangle in the mesh
            List<Vector3> tangentPerTriangle = new List<Vector3>();
            List<Vector3> bitangentPerTriangle = new List<Vector3>();

            for (int i = 0; i < IDs.Count; i += 3)
            {
                int id0 = (int)IDs[i + 0];
                int id1 = (int)IDs[i + 1];
                int id2 = (int)IDs[i + 2];

                Vector3 p0 = Position[id0];
                Vector3 p1 = Position[id1];
                Vector3 p2 = Position[id2];

                Vector3 q1 = p1 - p0;
                Vector3 q2 = p2 - p0;

                float u0 = TexCoord[id0].X;
                float u1 = TexCoord[id1].X;
                float u2 = TexCoord[id2].X;

                float v0 = TexCoord[id0].Y;
                float v1 = TexCoord[id1].Y;
                float v2 = TexCoord[id2].Y;

                Vector2 s = new Vector2(u1 - u0, u2 - u0);
                Vector2 t = new Vector2(v1 - v0, v2 - v0);

                var leftSide = Mat2x2(t.Y, -t.X, -s.Y, s.X);
                var rightSide = VecToMat(q1, q2);
                var det = (1.0f / (s.X * t.Y - s.Y * t.X));

                Matrix4x4 tb = Matrix4x4.Multiply(Matrix4x4.Multiply(leftSide, det), rightSide);

                Vector3 tan = new Vector3(tb.M11, tb.M12, tb.M13);
                Vector3 bitan = new Vector3(tb.M21, tb.M22, tb.M23);

                tangentPerTriangle.Add(Vector3.Normalize(tan));
                bitangentPerTriangle.Add(Vector3.Normalize(bitan));
            }

            //Average TBN on every vertex
            for (int i = 0; i < Position.Count; i++)
            {
                float countOfPointMatches = 0;
                Vector3 resultingTangent = Vector3.Zero;
                Vector3 resultingBitangent = Vector3.Zero;

                for (int k = 0; k < IDs.Count; k += 3)
                {
                    int id0 = (int)IDs[k + 0];
                    int id1 = (int)IDs[k + 1];
                    int id2 = (int)IDs[k + 2];

                    if (id0 == i || id1 == i || id2 == i)
                    {
                        countOfPointMatches++;
                        resultingTangent += tangentPerTriangle[(k - k % 3) / 3];
                        resultingBitangent += bitangentPerTriangle[(k - k % 3) / 3];
                    }
                }

                if (countOfPointMatches != 0)
                {
                    resultingTangent = resultingTangent / countOfPointMatches;
                    resultingBitangent = resultingBitangent / countOfPointMatches;
                }

                Tangent.Add(resultingTangent);
                Bitangent.Add(resultingBitangent);
            }
        }

        private Matrix4x4 Mat2x2(float m11, float m12, float m21, float m22)
        {
            return new Matrix4x4(m11, m12, 0, 0, m21, m22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private Matrix4x4 VecToMat(Vector3 a, Vector3 b)
        {
            return new Matrix4x4(a.X, a.Y, a.Z, 0, b.X, b.Y, b.Z, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
