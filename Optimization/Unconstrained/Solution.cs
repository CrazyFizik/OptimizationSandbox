using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Unconstrained
{
    public class Solution : ISolution
    {
        double _fval;
        double[] _x;

        public double _Fval
        {
            get
            {
                return _fval;
            }
        }

        public double[] _X
        {
            get
            {
                return _x;
            }
        }

        public Solution(double[] x, double fval)
        {
            _x = x;
            _fval = fval;
        }
    }
}
