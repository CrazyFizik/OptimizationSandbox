using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public class Design
    {
        public Channel[] _channels;
        public Obstacle[] _obstacles;
        public Border _border;

        public Design()
        {

        }

        public Design(Channel[] channels, Obstacle[] obstacles, Border border)
        {
            _channels = channels;
            _obstacles = obstacles;
            _border = border;
        }
    }
}
