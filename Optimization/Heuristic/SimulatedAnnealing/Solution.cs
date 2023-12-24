using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm;
using Optimization.Interfaces;

namespace Optimization.Heuristic.SimulatedAnnealing
{
    public class Solution : IComparable<Solution>, ISolution
    {
        public static Random _random = new Random();

        SimulatedAnnealing  _solver;
        IProblem            _problem;
        Function            _function;

        public double _temperature = 1;

        public int _valnum;
        public double[] _x0;
        public double[] _x;
        public double[] _p;

        public double[] _position;
        public double _cost;
        public double[] _objectivies;

        public double[] _max;
        public double[] _min;

        //List<double> _convergence = new List<double>();     // The convergence curve
        //List<double[]> _xTrajectory = new List<double[]>(); // Trajectory of solutions positions
        //List<double[]> _pTrajectory = new List<double[]>(); // Trajectory of search direction

        #region ISolution implemetn

        public double[] _X
        {
            get
            {
                return _position;
            }
        }

        public double _Fval
        {
            get
            {
                return _cost;
            }
        }

        #endregion

        #region Constructots
        public Solution(SimulatedAnnealing sa)
        {
            _solver = sa;
            _problem = _solver._Problem;
            _function = _problem._ObjectiveFunction;
            _max = _problem._UB;
            _min = _problem._LB;

            _valnum = _problem._Length;
            _position = new double[_valnum];
            _x = new double[_valnum];
            _x0 = new double[_valnum];
            _p = new double[_valnum];

            for (int i = 0; i < _valnum; i++)
            {
                _position[i] = _random.NextDouble() * (_max[i] - _min[i]) + _min[i];
            }
            _position.CopyTo(_x, 0);
            _position.CopyTo(_x0, 0);

            Evaluate();
        }

        public Solution(Solution source)
        {
            _solver = source._solver;
            _problem = source._problem;
            _function = source._function;

            _max = source._max;
            _min = source._min;

            _valnum = source._valnum;
            _position = new double[_valnum];
            _x = new double[_valnum];
            _x0 = new double[_valnum];
            _p = new double[_valnum];

            source._position.CopyTo(_position, 0);
            source._x.CopyTo(_x, 0);
            source._x0.CopyTo(_x0, 0);

            //_xTrajectory = source._xTrajectory;
            //_pTrajectory = source._pTrajectory;
            //_convergence = source._convergence;

            _temperature = source._temperature;
            _cost = source._cost;
        }

        public Solution Clone()
        {
            return new Solution(this);
        }
        #endregion

        public void Evaluate()
        {
            //_x.CopyTo(_x0, 0);
            //_position.CopyTo(_x, 0);
            //for (int i = 0; i < _valnum; i++)
            //{
            //    _p[i] = _x[i] - _x0[i];
            //}
                        
            _cost = _function(_position);
            _solver._funcount++;
            //_convergence.Add(_cost);
            //_xTrajectory.Add(_x);
            //_pTrajectory.Add(_p);
        }

        public void Evaluate(double[] args)
        {
            args.CopyTo(_position, 0);
            Evaluate();
        }

        /// <summary>
        /// Generates a point based on the current point and the current temperature using Uniform distribution.        /// 
        /// </summary>
        /// <param name="t">Temperature</param>
        public void Annealing(double t = 1)
        {
            int n = _position.Length;
            double[] randn = new double[n];
            double[] x = new double[n];
            double delta = 0;
            double norm = 0;

            for (int i = 0; i < n; i++)
            {
                randn[i] = _random.NextDouble() * (_max[i] - _min[i]) + _min[i];
                norm += randn[i] * randn[i];
            }
            norm = Math.Sqrt(norm);
            if (norm <= 0) norm = _valnum;

            for (int i = 0; i < _position.Length; i++)
            {
                delta = (_max[i] - _min[i]) * randn[i] / norm;
                x[i] = _position[i] + delta * t;
            }
            _position = SahonorBounds(_position, x, _min, _max);
        }

        /// <summary>
        /// Generates a point based on the current point and the current temperature using Student's t distribution.        /// 
        /// </summary>
        /// <param name="t">Temperature</param>
        public void AnnealingFast(double t = 1)
        {
            int n = _position.Length;
            double[] randn = new double[n];
            double[] x = new double[n];
            double delta = 0;
            double norm = 0;

            for (int i = 0; i < n; i++)
            {
                randn[i] = NextGaussianMarsaglia(_random); // 140 ms
                norm += randn[i] * randn[i];
            }
            norm = Math.Sqrt(norm);
            if (norm <= 0) norm = _valnum;

            for (int i = 0; i < _position.Length; i++)
            {
                delta = (_max[i] - _min[i]) * randn[i] / norm;
                x[i] = _position[i] + delta * t;
            }
            _position = SahonorBounds(_position, x, _min, _max); // 10ms
        }

        /// <summary>
        /// Generates a point based on the current point and the current temperature using multivariate normal distribution
        /// </summary>
        /// <param name="t">Температура</param>
        public void AnnealingBoltzman(double t = 1)
        {
            int n = _position.Length;
            double[] randn = new double[n];
            double[] x = new double[n];
            double delta = 0;
            double norm = 0;

            for (int i = 0; i < n; i++)
            {
                randn[i] = NextGaussianMarsaglia(_random);
                norm += randn[i] * randn[i];
            }
            norm = Math.Sqrt(norm);
            if (norm <= 0) norm = _valnum;

            for (int i = 0; i < _position.Length; i++)
            {
                delta = (_max[i] - _min[i]) * randn[i] / norm;
                x[i] = _position[i] + delta * Math.Sqrt(t);
            }
            _position = SahonorBounds(_position, x, _min, _max);
        }

        /// <summary>
        /// Поменять местами два элемента вектора
        /// </summary>
        public void Swap(double mu)
        {
            if (_random.NextDouble() <= mu)
            {
                double temp;
                int N;

                N = _position.Length;

                int i1 = _random.Next(0, N);
                int i2 = _random.Next(0, N);

                temp = _position[i1];
                _position[i1] = _position[i2];
                _position[i2] = temp;
            }
        }

        /// <summary>
        /// Поменять местами пару векторов - четного и следующего за ним
        /// </summary>
        public void SwapPair(double mu)
        {
            if (_random.NextDouble() <= mu)
            {
                double temp;
                int N;

                N = (_position.Length / 2) - 1;

                int i1 = 2 * _random.Next(0, N);
                int i2 = 2 * _random.Next(0, N);

                temp = _position[i1];
                _position[i1] = _position[i2];
                _position[i2] = temp;


                temp = _position[i1 + 1];
                _position[i1 + 1] = _position[i2 + 1];
                _position[i2 + 1] = temp;
            }
        }

        #region Norma Distribution

        [Obsolete("Вывести в отдельный статичный класс")]
        /// <summary>
        ///  Box-Muller transform
        ///  https://en.wikipedia.org/wiki/Box–Muller_transform 
        /// </summary>
        double NextGaussianBoxMuller(Random random, double stddev = 1, double mean = 0)
        {
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();

            double randStdNormal =
                Math.Sqrt(-2.0 * Math.Log(u1)) *
                Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)   

            double randomn = randStdNormal * stddev + mean;

            return randomn;
        }

        [Obsolete("Вывести в отдельный статичный класс")]
        /// <summary>
        ///  Marsaglia polar method with random number strong in (-1, 1) interval (ie  sigma = 0.33(3)
        ///  https://en.wikipedia.org/wiki/Marsaglia_polar_method 
        /// </summary>
        double NextGaussianMarsaglia(Random random, double stddev = 1, double mean = 0)
        {
            double u, v, rSquared;
            do
            {
                u = 2.0 * random.NextDouble() - 1.0;
                v = 2.0 * random.NextDouble() - 1.0;
                rSquared = u * u + v * v;
            }
            while (rSquared >= 1.0 || rSquared == 0);

            double polar = Math.Sqrt(-2.0 * Math.Log(rSquared) / rSquared);
            double randomn = u * polar * stddev + mean;

            return randomn;
        }

        #endregion

        #region Clamp

        [Obsolete("Вывести в отдельный статичный класс")]
        /// <summary>
        /// Циклическая обрезка
        /// </summary>
        /// <param name="x"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        double CyclicClamp(double x, double min, double max)
        {
            if (x > max)
            {
                x = (max - x) + min;
            }

            if (x < min)
            {
                x = max - (min - x);
            }

            return x;
        }

        [Obsolete("Вывести в отдельный статичный класс")]
        /// <summary>
        /// Обрезка отражнием
        /// </summary>
        /// <param name="x"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        double ReflectionClamp(double x, double min, double max)
        {
            if (x > max)
            {
                x = max + (max - x);
            }

            if (x < min)
            {
                x = min + (min - x);
            }

            if (x > max || x < min)
            {
                x = ReflectionClamp(x, min, max);
            }

            return x;
        }

        /// <summary>
        ///SAHONORBOUNDS ensures that the points that SIMULANNEAL move forward with are always feasible.
        ///It does so by checking to see if the given point is outside of the bounds, and then if it is, 
        ///creating a point called which is on the bound that was being violated 
        ///and then generating a new point on the line between the previous point x0 and x.
        ///It is assumed that optimValues.x is within bounds.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x"></param>
        /// <param name="lb"></param>
        /// <param name="ub"></param>
        /// <returns></returns>
        double[] SahonorBounds(double[] x0, double[] x, double[] lb, double[] ub)
        {
            double alpha;
            int n = x0.Length;

            bool feasible = true;
            for (int i = 0; i < n; i++)
            {
                if (x[i] > ub[i] || x[i] < lb[i])
                {
                    feasible = false;
                    break;
                }
            }

            if (!feasible)
            {
                for (int i = 0; i < n; i++)
                {
                    alpha = _random.NextDouble();
                    if (x[i] > ub[i]) x[i] = ub[i];
                    if (x[i] < lb[i]) x[i] = lb[i];

                    x[i] = alpha * x[i] + (1 - alpha) * x0[i];
                }
            }

            return x;
        }

        #endregion

        public override string ToString()
        {
            string s = "";
            s += _temperature.ToString("F4")
                + " | " + _cost.ToString("F4");

            s += "[ ";
            for (int i = 0; i < _position.Length; i++)
            {
                s += _position[i].ToString("F4") + " ";
                if (i > 4)
                {
                    s += "... ";
                    break;
                }
            }
            s += "]";
            return s;
        }

        public int CompareTo(Solution other)
        {
            if (this._cost < other._cost)
            {
                return -1;
            }
            else if (this._cost > other._cost)
            {
                return 1;
            }
            else return 0;
        }
    }
}
