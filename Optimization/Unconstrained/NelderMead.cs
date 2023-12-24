using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization.Interfaces;

namespace Optimization.Unconstrained
{
    public class NelderMead : ISolver
    {
        static Random _random = new Random(0);

        public IProblem _objective;
        public Function _function;

        int _iteration = 0;
        public int _maxIterations = 1024; // The number of iterations that were performed

        int _stall = 0;
        public int _stallIterLimit = 100;

        double _tolFun = 1e-6;

        double[] _x0; // Start point
        double _fval0;

        List<double> _convergence = new List<double>();     // The convergence curve
        List<double[]> _xTrajectory = new List<double[]>(); // Trajectory of solutions positions
        List<double[]> _pTrajectory = new List<double[]>(); // Trajectory of search direction

        public int _amoebaSize;  // number of solutions
        public int _dim;         // vector-solution size, also problem dimension
        public Solution[] _solutions;  // potential solutions (vector + value)

        public double alpha;  // reflection
        public double beta;   // contraction
        public double gamma;  // expansion

        public List<double> _ConvergenceCurve
        {
            get
            {
                return _convergence;
            }
        }

        public ISolver _Hybrid
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

        public int _MaxStall
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

        public IProblem _Problem
        {
            get
            {
                return _objective;
            }

            set
            {
                _objective = value;
            }
        }

        public ISolution _Solution
        {
            get
            {
                return _solutions[0];
            }
        }

        public List<ISolution> _Solutions
        {
            get
            {
                return _solutions.ToList<ISolution>();
            }
        }

        public List<double[]> _Trajectory
        {
            get
            {
                return _xTrajectory;
            }
        }

        public NelderMead(IProblem objective)
        {
            this._objective = objective;
            this._function = _objective._ObjectiveFunction;
            this._x0 = _objective._X0;
            
            this._dim = objective._Length;
            this._amoebaSize = _dim + 1;

            this._stallIterLimit = _dim * _dim;
            this._maxIterations += this._stallIterLimit;

            this.alpha = 1.0;  // hard-coded values from theory
            this.beta = 0.5;
            this.gamma = 2.0;

            this.alpha = 2.0;  // hard-coded values from theory
            this.beta = 0.25;
            this.gamma = 2.5;

            this._solutions = new Solution[_amoebaSize];
        }

        void Init()
        {
            if (_x0 == null)
            {
                _x0 = new double[_dim];
                for (int j = 0; j < _dim; j++)
                {
                    _x0[j] = _random.NextDouble() * (_objective._UB[j] - _objective._LB[j]) + _objective._LB[j];
                }
                double fval = _function(_x0);
            }

            _solutions[0] = new Solution(_x0, _function(_x0));
            for (int i = 1; i < _solutions.Length; ++i)
            {
                double[] x = new double[_dim];
                for (int j = 0; j < _dim; j++)
                {
                    x[j] = _x0[j];
                }
                x[i-1] = _x0[i-1] + 0.1 * (_objective._UB[i-1] - _objective._LB[i-1]);
                double fval = _function(x);
                _solutions[i] = new Solution(x, fval);  // the Solution ctor calls the objective function to compute value
            }

            Array.Sort(_solutions);
        }

        public ISolution Solve()
        {
            Init();
            _iteration = 0;  // loop counter

            while (_iteration < _maxIterations)
            {
                ++_iteration;

                if (_iteration % 10 == 0)
                {
                    Console.WriteLine("At t = " + _iteration + " curr best solution = " + this._solutions[0]._Fval);
                }

                Solution centroid = Centroid();  // compute centroid
                Solution reflected = Reflected(centroid);  // compute reflected

                if (reflected._Fval < _solutions[0]._Fval)  // reflected is better than the curr best
                {
                    Solution expanded = Expanded(reflected, centroid);  // can we do even better??
                    if (expanded._Fval < _solutions[0]._Fval)  // winner! expanded is better than curr best
                    {
                        ReplaceWorst(expanded);  // replace curr worst solution with expanded
                    }
                    else
                    {
                        ReplaceWorst(reflected);  // it was worth a try . . . 
                    }
                    continue;
                }

                if (IsWorseThanAllButWorst(reflected) == true)  // reflected is worse (larger value) than all solution vectors (except possibly the worst one)
                {
                    if (reflected._Fval <= _solutions[_amoebaSize - 1]._Fval)  // reflected is better (smaller) than the curr worst (last index) vector
                    {
                        ReplaceWorst(reflected);
                    }

                    Solution contracted = Contracted(centroid);  // compute a point 'inside' the amoeba

                    if (contracted._Fval > _solutions[_amoebaSize - 1]._Fval)  // contracted is worse (larger value) than curr worst (last index) solution vector
                    {
                        Shrink();
                    }
                    else
                    {
                        ReplaceWorst(contracted);
                    }

                    continue;
                }

                ReplaceWorst(reflected);

                double[] x = _solutions[0]._X;
                double fval = _solutions[0]._Fval;

                if (Math.Abs(_fval0 - fval) <= _tolFun)
                {
                    _stall++;
                }
                else
                {
                    _stall = 0;
                    _fval0 = fval;
                    _x0 = x;
                }

                if (_stall > _stallIterLimit) break;

            }  // solve loop

            return _solutions[0];  // best solution is always at [0]
        }

        public ISolution Solve(double[] args)
        {
            Array.Copy(args, _x0, args.Length);
            _fval0 = _function(_x0);
            return Solve();
        }

        public Solution Centroid()
        {
            // return the centroid of all solution vectors except for the worst (highest index) vector
            double[] c = new double[_dim];
            for (int i = 0; i < _amoebaSize - 1; ++i)
            {
                for (int j = 0; j < _dim; ++j)
                {
                    c[j] += _solutions[i]._X[j];  // accumulate sum of each vector component
                }
            }

            for (int j = 0; j < _dim; ++j)
            {
                c[j] = c[j] / (_amoebaSize - 1);
            }

            double fval = _function(c);
            Solution s = new Solution(c, fval);  // feed vector to ctor which calls objective function to compute value
            return s;
        }

        public Solution Reflected(Solution centroid)
        {
            // the reflected solution extends from the worst (lowest index) solution through the centroid
            double[] r = new double[_dim];
            double[] worst = this._solutions[_amoebaSize - 1]._X;  // convenience only
            for (int j = 0; j < _dim; ++j)
            {
                r[j] = ((1 + alpha) * centroid._X[j]) - (alpha * worst[j]);
            }
            double fval = _function(r);
            Solution s = new Solution(r, fval);
            return s;
        }

        public Solution Expanded(Solution reflected, Solution centroid)
        {
            // expanded extends even more, from centroid, thru reflected
            double[] e = new double[_dim];
            for (int j = 0; j < _dim; ++j)
            {
                e[j] = (gamma * reflected._X[j]) + ((1 - gamma) * centroid._X[j]);
            }
            double fval = _function(e);
            Solution s = new Solution(e, fval);
            return s;
        }

        public Solution Contracted(Solution centroid)
        {
            // contracted extends from worst (lowest index) towards centoid, but not past centroid
            double[] v = new double[_dim];  // didn't want to reuse 'c' from centoid routine
            double[] worst = this._solutions[_amoebaSize - 1]._X;  // convenience only
            for (int j = 0; j < _dim; ++j)
            {
                v[j] = (beta * worst[j]) + ((1 - beta) * centroid._X[j]);
            }
            double fval = _function(v);
            Solution s = new Solution(v, fval);
            return s;
        }

        public void Shrink()
        {
            // move all vectors, except for the best vector (at index 0), halfway to the best vector
            // compute new objective function values and sort result
            for (int i = 1; i < _amoebaSize; ++i)  // note we don't start at [0]
            {
                double[] x = new double[_dim];
                for (int j = 0; j < _dim; ++j)
                {
                    x[j] = (_solutions[i]._X[j] + _solutions[0]._X[j]) / 2.0;

                }
                double fval = _function(_solutions[i]._X);
                _solutions[i] = new Solution(x, fval);
            }
            Array.Sort(_solutions);
        }

        public void ReplaceWorst(Solution newSolution)
        {
            // replace the worst solution (at index size-1) with contents of parameter newSolution's vector
            _solutions[_amoebaSize - 1] = new Solution(newSolution._X, newSolution._Fval);
            Array.Sort(_solutions);
        }

        public bool IsWorseThanAllButWorst(Solution reflected)
        {
            // Solve needs to know if the reflected vector is worse (greater value) than every vector in the amoeba, except for the worst vector (highest index)
            for (int i = 0; i < _amoebaSize - 1; ++i)  // not the highest index (worst)
            {
                if (reflected._Fval <= _solutions[i]._Fval)  // reflected is better (smaller value) than at least one of the non-worst solution vectors
                {
                    return false;
                }
            }
            return true;
        }

    }

}
