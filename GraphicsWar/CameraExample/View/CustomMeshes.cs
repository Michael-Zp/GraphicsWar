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
    }
}
