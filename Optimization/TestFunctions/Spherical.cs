namespace Optimization.TestFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Optimization.Interfaces;

    public class Spherical : IProblem, IModel
    {
        int _length;
        public int _Length
        {
            get
            {
                return _length;
            }
        }

        Function _function;
        public Function _ObjectiveFunction
        {
            get
            {
                return _function;
            }
        }

        double _max = 10.0;
        double _min = -10.0;

        public double[] _UB
        {
            get
            {
                double[] result = new double[_length];
                for (int i = 0; i < _length; i++)
                {
                    result[i] = _max;
                }
                return result;
            }
        }

        public double[] _LB
        {
            get
            {
                double[] result = new double[_length];
                for (int i = 0; i < _length; i++)
                {
                    result[i] = _min;
                }
                return result;
            }
        }

        double[] _x0;
        public double[] _X0
        {
            get
            {
                return _x0;
            }
        }

        public double[] _X
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        double _norm = 1.0;
        public double _Norm
        {
            get
            {
                return _norm;
            }

            set
            {
                _norm = value;
            }
        }

        public double _Penalty
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Spherical(int dim)
        {
            _length = dim;
            _function = new Function(Compute);
            //_function = Evaluate;
        }

        double Compute(double[] x)
        {
            _length = x.Length;
            _x0 = new double[x.Length];
            _x0 = x;
            double f = 0;
            for (int i = 0; i < _length; i++)
            {
                f = f
                    + x[i] * x[i];
            }
            return f / _norm;
        }

        public override string ToString()
        {
            string s = "";
            s += "=======================\n";
            s += "Goal is to minimize f(x0,x1) = x0^2 + x1^2 )\n";
            s += "Known solution is at x0 = 0, x1 = 0\n";
            s += "Min = " + _min.ToString() + " Max = " + _max.ToString();
            s += "\n=======================\n";
            return s;
        }

        public void Save(string name)
        {
            //throw new NotImplementedException();
        }

        public double[] Evaluate()
        {
            throw new NotImplementedException();
        }

        public double[] Evaluate(double[] arg)
        {
            throw new NotImplementedException();
        }
    }
}

