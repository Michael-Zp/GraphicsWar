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


    }
}
