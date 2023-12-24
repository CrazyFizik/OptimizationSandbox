using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Unconstrained
{
    public class GradienDescent
    {
        static Random _random = new Random(0);

        public IProblem _objective;
        public Function _function;

        int _iteration = 0;
        int _maxIterations = 1024;

        public int _Iteration
        {
            get
            {
                return _iteration;
            }
        }

        public int _IterationsMax
        {
            get
            {
                return _maxIterations;
            }
            set
            {
                _maxIterations = value;
            }
        }

        public double _h = .01;
        public double _stepSize = 0.000005;
        public double _tolerance = 1e-9;

        double[] _x0;
        List<double> _learningRate = new List<double>();

        public GradienDescent(IProblem objective)
        {
            _objective = objective;
            _function = objective._ObjectiveFunction;

            _x0 = objective._X0;

            if (_x0 == null)
            {
                _x0 = new double[objective._Length];
                for (int i = 0; i < objective._Length; i++)
                {
                    _x0[i] = _random.NextDouble() * (objective._UB[i] - objective._LB[i]) + objective._LB[i];
                }
            }
        }

        public double[] Solve()
        {
            double[] x = _x0;
            double[] newX;
            double[] grad;
            double f0 = _function(x);
            double f = f0;
            _learningRate.Add(f);
            double mu = _stepSize;
            _maxIterations = x.Length + 1;
            for(int i = _iteration; i < _maxIterations; i++)
            {
                _iteration++;
                grad = Gradient(_function, x, _h);
                newX = GetNewArg(x, mu, grad);

                f = _function(newX);

                if (f < f0)
                {
                    //mu *= 2;
                }
                else
                {
                    mu /= 2;
                }

                f0 = f;

                Console.WriteLine(_iteration.ToString() + " | " + f.ToString());
                _learningRate.Add(f);
                x = newX;
                if (f*f < _tolerance)
                    break;
            }

            return x;
        }

        public double[] Gradient(Function f, double[] arg, double h)
        {
            double[] x = new double[arg.Length];
            arg.CopyTo(x, 0);
            double[] result = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                double tempX = x[i];
                x[i] = tempX + h;
                double f2 = f(x);
                x[i] = tempX - h;
                double f0 = f(x);

                result[i] = (f2 - f0) / (2 * h);
            }
            return result;
        }

        public double[] GetNewArg(double[] arg, double mu, double[] direction)
        {
            double[] result = new double[arg.Length];
            for (int i = 0; i < arg.Length; i++)
            {
                result[i] = arg[i] - mu * direction[i];
            }
            return result;
        }
        
    }
}
