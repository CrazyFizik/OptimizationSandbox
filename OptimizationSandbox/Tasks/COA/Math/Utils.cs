using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public static class Utils
    {
        public static bool InCircle(double x, double y, double r)
        {
            double R = x * x + y * y;

            if (R <= (r * r))
                return true;
            else
                return false;
        }

        public static bool InCircle(Vector2 point, double r)
        {
            return InCircle(point.X, point.Y, r);
        }

        public static bool InPolygon(Vector2[] poly, Vector2 p)
        {
            Vector2 p1, p2;


            bool inside = false;


            if (poly.Length < 3)
            {
                return inside;
            }


            var oldPoint = new Vector2(
                poly[poly.Length - 1].X,
                poly[poly.Length - 1].Y);


            for (int i = 0; i < poly.Length; i++)
            {
                var newPoint = new Vector2(poly[i].X, poly[i].Y);


                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;

                    p2 = newPoint;
                }

                else
                {
                    p1 = newPoint;

                    p2 = oldPoint;
                }


                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }


                oldPoint = newPoint;
            }


            return inside;
        }
    }
}
