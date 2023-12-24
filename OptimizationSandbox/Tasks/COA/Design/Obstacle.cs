using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OptimizationSandbox.Tasks.COA
{
    public class Obstacle
    {
        public Vector2 _position;
        public Color _color;

        public Obstacle()
        {

        }

        public Obstacle(Color color, Vector2 position)
        {
            _color = color;
            _position = position;
        }
    }

    public class Screw : Obstacle
    {
        public float _radius;

        public Screw()
        {

        }

        public Screw(Color color, Vector2 position, float radius) :
            base(color, position)
        {
            _radius = radius;
        }
    }

    public class Polygon : Obstacle
    {
        public Vertexes _vertexes;

        public Polygon()
        {

        }

        public Polygon(Color color, Vector2 position, Vertexes vertexes) : 
            base(color, position)
        {
            _vertexes = vertexes;
        }
    }
}
