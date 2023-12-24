using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.ParticleSwarmOptimization
{
    public class FireFlyOptimization
    {
        //public Func<double, double[]> Function;
        public Function _function;
        public IProblem _obctiveFunction;

        static Random _ran; // Solve() has random features

        public int _dim;
        public double _minX;
        public double _maxX;

        int _MaxIt = 1000;         // Maximum Number of Iterations
        int _iteration = 0;
        int _nPop = 25;            //Number of Fireflies(Swarm Size)

        public FireFly[] _population;
        FireFly _best;

        double _gamma = 1;            // Light Absorption Coefficient
        double _beta0 = 2;            // Attraction Coefficient Base Value
        double _alpha = 0.2;          // Mutation Coefficient
        double _alpha_damp = 0.99;    // Mutation Coefficient Damping Ratio
        double _delta;// = 0.05 * (VarMax - VarMin);     // Uniform Mutation Range
        double _m = 2;

        public double[] _Solution
        {
            get
            {
                return _best._position;
            }
        }

        public double _Cost
        {
            get
            {
                return _best._cost;
            }
        }

        public List<double> _learning = new List<double>();

        public FireFlyOptimization(IProblem function, int numParticles, int maxLoop)
        {
            _ran = new Random();

            _obctiveFunction = function;
            _function = function._ObjectiveFunction;
            _dim = function._Length;

            this._minX = _obctiveFunction._LB[0];
            this._maxX = _obctiveFunction._UB[0];

            _nPop = numParticles;
            _MaxIt = maxLoop;

            _delta = 0.05 * (_maxX - _minX);
            _population = new FireFly[_nPop];
            for (int i = 0; i < _nPop; i++)
            {
                _population[i] = new FireFly(function);
            }
            Array.Sort(_population);
            _best = _population[0].Clone();
        }

        public FireFlyOptimization(IProblem function, double[][] particles, int maxLoop)
        {
            _ran = new Random();

            _obctiveFunction = function;
            _function = function._ObjectiveFunction;
            _dim = function._Length;

            this._minX = _obctiveFunction._LB[0];
            this._maxX = _obctiveFunction._UB[0];

            _nPop = particles.Length;
            _MaxIt = maxLoop;

            _delta = 0.05 * (_maxX - _minX);
            _population = new FireFly[_nPop];
            for (int i = 0; i < _nPop; i++)
            {
                _population[i] = new FireFly(function);
                for (int j = 0; j < function._Length; j++)
                {
                    _population[i]._position[j] = particles[i][j];
                }
                _population[i].Evaluate();
            }
            Array.Sort(_population);
            _best = _population[0].Clone();
        }

        public void Solve()
        {
            double dmax = (_maxX - _minX) * Math.Sqrt(_dim);
            while (_iteration < _MaxIt)
            {
                for (int i = 0; i < _nPop; i++)
                {
                    for (int j = 0; j < _nPop; j++)
                    {
                        //if (_population[i]._cost < _population[j]._cost)
                        {
                            double rij = _population[i].Distance(_population[j]) / dmax;
                            double beta = _beta0 * Math.Exp(-_gamma * Math.Pow(rij, _m));
                            FireFly candidate = _population[i].Clone();
                            for (int k = 0; k < _dim; k++)
                            {
                                candidate._position[k] =
                                    _population[i]._position[k]
                                    + beta * _ran.NextDouble() * (_population[j]._position[k] - _population[i]._position[k])
                                    + _alpha * (2 * _ran.NextDouble() - 1);

                                if (candidate._position[k] > _maxX) candidate._position[k] = _maxX;
                                if (candidate._position[k] < _minX) candidate._position[k] = _minX;
                            }

                            candidate.Evaluate();

                            if (candidate._cost < _population[i]._cost)
                            {
                                _population[i] = candidate.Clone();

                                if (candidate._cost < _best._cost)
                                {
                                    _best = candidate.Clone();
                                    Console.WriteLine(_iteration.ToString() + "|" + _best.ToString());
                                }
                            }
                        }
                    }
                }// each firefly
                Array.Sort(_population);
                _learning.Add(_population[0]._cost);
                _alpha = _alpha * _alpha_damp;
                _iteration++;
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _population.Length; ++i)
                s += "[" + i + "] " + _population[i].ToString() + "\n";
            s += "Best Firefly Pos [ ";
            for (int i = 0; i < _best._position.Length; ++i)
                s += _best._position[i].ToString("F2") + " ";
            s += "] ";
            s += "Best Firefly Cost = " + _Cost.ToString("F3");// + " Iterations " + _bestIteration;
            s += "\n";
            return s;
        }
    }

    public class FireFly : IComparable<FireFly>
    {
        IProblem _objective;
        Function _function;

        static Random _ran = new Random(0);

        public double[] _position;
        public double[] _velocity;
        public double _cost;
        public double[] _bestPartPos;
        public double _bestPartCost;
        int _dim;

        double _minX = 0;
        double _maxX = 1;

        public FireFly(IProblem function)
        {
            _objective = function;
            _function = function._ObjectiveFunction;
            _dim = function._Length;

            _minX = function._LB[0];
            _maxX = function._UB[0];

            _position = new double[_dim];
            _velocity = new double[_dim];
            _bestPartPos = new double[_dim];
            for (int i = 0; i < _dim; ++i)
            {
                _position[i] = (_maxX - _minX) * _ran.NextDouble() + _minX;
                _velocity[i] = (_maxX - _minX) * _ran.NextDouble() + _minX;
            }
            _cost = _function(_position);
            _bestPartCost = _cost;
            Array.Copy(_position, _bestPartPos, _dim);
        }

        private FireFly()
        {

        }

        public FireFly Clone()
        {
            FireFly reslt = new FireFly();
            reslt._objective = this._objective;
            reslt._function = this._function;
            reslt._maxX = this._maxX;
            reslt._minX = this._minX;
            reslt._dim = this._dim;
            reslt._position = new double[this._position.Length];
            this._position.CopyTo(reslt._position, 0);
            reslt._cost = this._cost;

            return reslt;
        }

        public double Evaluate()
        {
            _cost = _function(_position);
            return _cost;
        }

        public double Distance(FireFly firefly)
        {
            double result = 0;
            double l = 0;
            for (int i = 0; i < this._position.Length; i++)
            {
                l = (this._position[i] -firefly._position[i]) * (this._position[i] - firefly._position[i]);
                result = result + l;
            }
            result = Math.Sqrt(result);
            return result;
        }

        public int CompareTo(FireFly other)
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

        public override string ToString()
        {
            string s = "";
            s += "Pos [ ";
            for (int i = 0; i < _position.Length; ++i)
                s += _position[i].ToString("F2") + " ";
            s += "] ";

            s += "Cost = " + _cost.ToString("F3");

            return s;
        }
    }
}
