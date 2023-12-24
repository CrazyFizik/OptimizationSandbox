namespace Optimization.TestFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Optimization.Interfaces;

    public class Himmelblau : IProblem, IModel
    {
        int _length = 2;
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
                return _x0;
            }

            set
            {
                _x0 = value;
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

        public Himmelblau()
        {
            _function = new Function(Evaluate);
            //_function = Evaluate;
        }


        public void Save(string name)
        {

        }

        public double Evaluate(double[] x)
        {
            _x0 = x;
            double f = 0;
            double a = (x[0] * x[0] + x[1] - 11);
            double b = (x[0] + x[1] * x[1] - 7);

            f = a * a + b * b;

            return f / _norm;
        }

        public override string ToString()
        {
            string s = "";
            s += "=======================\n";
            s += "Goal is to minimize Himmelblau's function\n";
            s += "Function has known solutions of 0.0 are:\n";
            s += " x0 = 3.0, x1 = 2.0\n";
            s += " x0 = -2.80, x1 = 3.13\n";
            s += " x0 = -3.78, x1 = -3.28\n";
            s += " x0 = 3.58, x1 = -1.84\n";
            s += "Min = " + _min.ToString() + " Max = " + _max.ToString();
            s += "\n=======================\n";
            return s;
        }

        public double[] Evaluate()
        {
            throw new NotImplementedException();
        }

        double[] IModel.Evaluate(double[] arg)
        {
            throw new NotImplementedException();
        }
    }
}
