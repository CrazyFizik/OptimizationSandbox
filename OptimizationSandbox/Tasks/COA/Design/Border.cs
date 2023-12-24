using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public class Border
    {
        public Vertexes _vertexes = new Vertexes();

        public Border()
        {

        }

        public Border(Vertexes vertexes)
        {
            _vertexes = vertexes;
        }

        public Border(Vector2[] vertexes)
        {
            foreach(Vector2 point in vertexes)
            {
                _vertexes.Add(point);
            }
        }
    }
}
