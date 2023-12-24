using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public class Coupling
    {
        public Cavity _A;
        public Cavity _B;
        public float _maxLength;
        public float _minLength;

        public Coupling()
        {

        }

        //public Coupling(int A, int B)
        //{
        //    _A = A;
        //    _B = B;
        //}

        //public Coupling(int A, int B, float max, float min) : this(A,B)
        //{
        //    _maxLength = max;
        //    _minLength = min;
        //}

        public Coupling(Cavity A, Cavity B, float min, float max) 
        {
            _A = A;
            _B = B;
            _maxLength = max;
            _minLength = min;
        }

        public double GetMax()
        {
            double max = _A._radius + _B._radius + _maxLength;
            return max;
        }

        public double GetMin()
        {
            double min = _A._radius + _B._radius + _minLength;
            return min;
        }

        public double GetConflictLength()
        {
            Vector2 vector = _A._position - _B._position;
            double l = vector.Length();

            double max = GetMax();
            double min = GetMin();

            double conflict = 0;
            if (l > max)
            {
                conflict = l - max;
            }
            else if (l < min)
            {
                conflict = min - l;
            }

            return conflict;
        }
    }
}
