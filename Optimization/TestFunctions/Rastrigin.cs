namespace Optimization.TestFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Optimization.Interfaces;

    public class Rastrigin : IProblem, IModel
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

        public double[] _UB
        {
            get
            {
                int n = _length;
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
                int n = _length;
                double[] result = new double[n];
                for (int i = 0; i < n; i++)
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

        public Rastrigin(int dim)
        {
            _length = dim;
            _function = new Function(Compute);
            //_function = Evaluate;
        }


        public void Save(string name)
        {

        }

        double Compute(double[] x)
        {
            double A = 10;
            _x0 = x;
            _length = x.Length;
            double f = 0;
            for (int i = 0; i < _length; i++)
            {
                f = f
                    + A
                    + x[i]*x[i]
                    - A * Math.Cos(2*Math.PI*x[i]);
            }

            return f / _norm;
        }

        public override string ToString()
        {
            string s = "";
            s += "=======================\n";
            s += "Goal is to minimize Rastrigin's function\n";
            s += "Function has known solution of 0.0 at x0 = 0.0, x1 = 0.0\n";
            s += "Min = " + _min.ToString() + " Max = " + _max.ToString();
            s += "\n=======================\n";
            return s;
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
