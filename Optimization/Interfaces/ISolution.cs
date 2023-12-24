using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Interfaces
{
    public interface ISolution
    {
        double[] _X { get; }

        double _Fval { get; }
    }
}
