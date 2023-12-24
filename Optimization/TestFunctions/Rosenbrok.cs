namespace Optimization.TestFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Optimization.Interfaces;
    using Optimization;

    public class Rosenbrok : IProblem, IModel
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

        double _max = 100.0;
        double _min = -100.0;

        double[] _x0;
        public double[] _X0
        {
            get
            {
                return _x0;
            }
        }

        public double[] _UB
        {
            get
            {
                int n = _Length;
                double[] result = new double[n];
                for (int i = 0; i < n; i++)
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
                int n = _Length;
                double[] result = new double[n];
                for (int i = 0; i < n; i++)
                {
                    result[i] = _min;
                }
                return result;
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

        public Rosenbrok(int dim)
        {
            _length = dim;
            _function = new Function(Compute);
            //_function = Evaluate;
        }

        public double Compute2(double x, double y)
        {
            _length = 2;
            double f = 
                (1 - x) * (1 - x) 
                + 100 * (y - x * x) * (y - x * x);
            return f / _norm;
        }

        double Compute(double[] x)
        {
            _length = x.Length;
            _x0 = x;
            double f = 0;
            for (int i = 0; i < _length-1; i++)
            {
                f = f
                    + (1 - x[i]) * (1 - x[i]) 
                    + 100 * (x[i+1] - x[i] * x[i]) * (x[i+1] - x[i] * x[i]);
            }

            return f / _norm;
        }

        public override string ToString()
        {
            string s = "";
            s += "=======================\n";
            s += "Goal is to minimize Rosenbrok function\n";
            s += "Function has known solution of 0.0 at x0 = 1.0, x1 = 1.0\n";
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
