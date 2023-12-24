using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OptimizationSandbox.Tasks.COA
{
    public class Cavity
    {
        public int _number;
        public Vector2 _position;
        public float _radius;

        public Color _color;

        public Cavity()
        {

        }

        public Cavity(Color color, float radius, int number)
        {
            _color = color;
            _radius = radius;
            _number = number;
        }
    }
}
