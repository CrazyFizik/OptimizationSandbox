using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public class Output
    {
        public double _result
        {
            get
            {
                double r = 0;
                r =
                    _w[0] * _crossCouplingConflict
                    + _w[1] * _mainCouplingConflict
                    + _w[2] * _conflictedArea
                    + _w[3] * (_freeArea - _coveredArea);

                return r;
            }
        }

        double[] _w = new double[4];

        public double _mainCouplingConflict;
        public double _crossCouplingConflict;
        public double _conflictedArea;
        public double _coveredArea;
        public double _freeArea;

        public Output()
        {
            for (int i = 0; i < _w.Length; i++)
            {
                _w[i] = 1;
            }
            Reset();
        }

        public void Reset()
        {
            _mainCouplingConflict = 0;
            _crossCouplingConflict = 0;
            _conflictedArea = 0;
            _coveredArea = 0;
            _freeArea = 0;
        }

    }
}
