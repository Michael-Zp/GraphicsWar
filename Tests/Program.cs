using System;
using System.Numerics;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            float m1 = 0;
            float b1 = 0;

            float m2 = 1;
            float b2 = 4;

            float xIntersect = (b1 + b2) / (m1 - m2);
            float yIntersect = m1 * xIntersect + b1;

            Console.WriteLine("Intersect at: [" + xIntersect + ", " + yIntersect + "]");


            Console.ReadKey();


            Vector2 targetCenter = new Vector2(-1, 1);
            Vector2 center = new Vector2(0);

            Vector2 vector = targetCenter - center;

            //The center of the current gridPos is the origin of the coord system
            Vector2 centerBetweenCenters = vector / 2;

            //Turn 90 deg
            Vector2 rotatedVector = new Vector2(vector.Y, -vector.X);

            //float isZero = step(abs(rotatedVector.x), 1e-3);
            //rotatedVector.x = isZero * 1e-3 * (rotatedVector.x / abs(rotatedVector.x)) + (1 - isZero) * rotatedVector.x;

            float m = rotatedVector.Y / rotatedVector.X;

            //y at point x is given and m is known. b unknown.
            //y = m * x + b
            //y - m * x = b
            float b = centerBetweenCenters.Y - m * centerBetweenCenters.X;

            Console.WriteLine("y = " + m + " * x + " + b);


            Console.ReadKey();
        }
    }
}
