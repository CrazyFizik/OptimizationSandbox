using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.ParticleSwarmOptimization
{
    public class Multiswarm
    {
        //public Func<double, double[]> Function;
        public Function _function;
        public IProblem _obctiveFunction;

        static Random _ran; // Solve() has random features

        public Swarm[] _swarms;
        public double[] _bestMultiPos;
        public double _bestMultiCost;

        public int _dim;
        public double _minX;
        public double _maxX;

        int _iteration = 0;

        double _previousBest;
        int _bestIteration = 0;
        bool convergence = false;

        public double _w = 0.545; // inertia
        public double _c1 = 1; // particle / cogntive
        public double _c2 = 2; // swarm / social
        public double _c3 = 0.3645; // multiswarm / global
        public double _death = 0.05; // prob of particle death
        public double _immigrate = 0.05;  // prob of particle immigration       

        public Multiswarm(IProblem function, int numSwarms, int numParticles, double minX, double maxX)
        {
            _ran = new Random(0);

            _obctiveFunction = function;
            _function = function._ObjectiveFunction;
            _dim = function._Length;
                
            _swarms = new Swarm[numSwarms];
            _bestMultiPos = new double[_dim];
            _bestMultiCost = double.MaxValue;

            this._minX = minX;
            this._maxX = maxX;

            for (int i = 0; i < numSwarms; ++i)
            {
                _swarms[i] = new Swarm(_obctiveFunction, numParticles, minX, maxX);
                if (_swarms[i]._bestSwarmCost < _bestMultiCost)
                {
                    _bestMultiCost = _swarms[i]._bestSwarmCost;
                    Array.Copy(_swarms[i]._bestSwarmPos, _bestMultiPos, _dim);
                }
            }
        }

        public void Solve(int maxLoop)
        {
            while (_iteration < maxLoop)
            {
                ++_iteration;
                for (int i = 0; i < _swarms.Length; ++i) // each swarm
                {
                    for (int j = 0; j < _swarms[i]._particles.Length; ++j) // each particle
                    {
                        double p = _ran.NextDouble();
                        if (p < _death)
                        {
                            _swarms[i]._particles[j] = new Particle(_obctiveFunction, _minX, _maxX);
                        }

                        double q = _ran.NextDouble();
                        if (q < _immigrate)
                        {
                            Immigration(i, j); // swap curr particle with a random particle in diff swarm
                        }

                        for (int k = 0; k < _dim; ++k) // update velocity. each x position component
                        {
                            double r1 = _ran.NextDouble();
                            double r2 = _ran.NextDouble();
                            double r3 = _ran.NextDouble();

                            _swarms[i]._particles[j]._velocity[k] = (_w * _swarms[i]._particles[j]._velocity[k])
                                + (_c1 * r1 * (_swarms[i]._particles[j]._bestPartPos[k] - _swarms[i]._particles[j]._position[k]))
                                + (_c2 * r2 * (_swarms[i]._bestSwarmPos[k] - _swarms[i]._particles[j]._position[k]))
                                + (_c3 * r3 * (_bestMultiPos[k] - _swarms[i]._particles[j]._position[k]));

                            //Clamp
                            //if (_swarms[i]._particles[j]._velocity[k] < _minX)
                            //{
                            //    _swarms[i]._particles[j]._velocity[k] = _minX;
                            //}
                            //else if (_swarms[i]._particles[j]._velocity[k] > _maxX)
                            //{
                            //    _swarms[i]._particles[j]._velocity[k] = _maxX;
                            //}

                        }

                        for (int k = 0; k < _dim; ++k) // update position
                        {
                            _swarms[i]._particles[j]._position[k] += _swarms[i]._particles[j]._velocity[k];

                            //CLamp
                            if (_swarms[i]._particles[j]._position[k] < _minX)
                            {
                                _swarms[i]._particles[j]._position[k] = _minX;
                            }
                            else if (_swarms[i]._particles[j]._position[k] > _maxX)
                            {
                                _swarms[i]._particles[j]._position[k] = _maxX;
                            }
                        }

                        // update cost
                        _swarms[i]._particles[j]._cost = _function(_swarms[i]._particles[j]._position);

                        // check if new best cost
                        if (_swarms[i]._particles[j]._cost < _swarms[i]._particles[j]._bestPartCost)
                        {
                            _swarms[i]._particles[j]._bestPartCost = _swarms[i]._particles[j]._cost;
                            Array.Copy(_swarms[i]._particles[j]._position, _swarms[i]._particles[j]._bestPartPos, _dim);
                        }

                        if (_swarms[i]._particles[j]._cost < _swarms[i]._bestSwarmCost)
                        {
                            _swarms[i]._bestSwarmCost = _swarms[i]._particles[j]._cost;
                            Array.Copy(_swarms[i]._particles[j]._position, _swarms[i]._bestSwarmPos, _dim);
                        }

                        if (_swarms[i]._particles[j]._cost < _bestMultiCost)
                        {
                            _previousBest = _bestMultiCost;
                            _bestMultiCost = _swarms[i]._particles[j]._cost;
                            Array.Copy(_swarms[i]._particles[j]._position, _bestMultiPos, _dim);
                        }
                    }//particle
                }//swarm

                if (Math.Abs(_previousBest - _bestMultiCost) < 1e-6 && !convergence && _previousBest!=0)
                {
                    _bestIteration = _iteration;
                    convergence = true;
                }
            }//iteration
        }

        public void Solve2(int maxLoop)
        {
            while (_iteration < maxLoop)
            {
                ++_iteration;
                for (int i = 0; i < _swarms.Length; ++i) // each swarm
                {

                }
            }
        }

        private void Immigration(int i, int j)
        {
            // swap particle j in swarm i, with a random particle in a random swarm
            int otheri = _ran.Next(0, _swarms.Length);
            int otherj = _ran.Next(0, _swarms[0]._particles.Length);
            Particle tmp = _swarms[i]._particles[j];
            _swarms[i]._particles[j] = _swarms[otheri]._particles[otherj];
            _swarms[otheri]._particles[otherj] = tmp;
        }

        public override string ToString()
        {
            string s = "";
            s += "=======================\n";
            for (int i = 0; i < _swarms.Length; ++i)
                s += _swarms[i].ToString() + "\n";
            s += "Best Multiswarm Pos [ ";
            for (int i = 0; i < _bestMultiPos.Length; ++i)
                s += _bestMultiPos[i].ToString("F2") + " ";
            s += "] ";
            s += "Best Multiswarm Cost = " + _bestMultiCost.ToString("F3") + " Iterations " + _bestIteration;
            s += "\n=======================\n";
            return s;
        }

    } // Multiswarm
}
