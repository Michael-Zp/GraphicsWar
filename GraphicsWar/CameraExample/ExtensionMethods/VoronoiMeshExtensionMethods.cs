using GraphicsWar.View;
using System.Collections.Generic;
using Zenseless.Geometry;

namespace GraphicsWar.ExtensionMethods
{
    public static class VoronoiMeshExtensionMethods
    {
        public static void Add(this VoronoiMesh a, VoronoiMesh b)
        {
            ((DefaultMesh)a).Add(b);

            var count = a.Position.Count;

            for(int i = 0; i < b.Plateaus.Count; i++)
            {
                List<int[]> newPlateau = new List<int[]>();
                foreach(var plateauIds in b.Plateaus[i])
                {
                    int[] temp = new int[plateauIds.Length];
                    for(int u = 0; u < plateauIds.Length; u++)
                    {
                        temp[u] = plateauIds[u] + count;
                    }
                    newPlateau.Add(temp);
                }

                a.Plateaus.Add(newPlateau);
            }
        }
    }
}
