using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.ParticleSwarmOptimization
{
    /// <summary>
    /// 
    /// v = W*v + y1*u1.*(p-x) + y2*u2.*(g-x)
    /// x = x + v
    /// 
    /// Problem Input:
    /// objective   - Objective function
    /// nvars       - Number of variables
    /// LB          - lower bounds
    /// UB          - Upper bounds
    /// rng         - Random generator
    /// solver      - Solver "PSO"
    /// options     - Options
    /// 
    /// 
    /// Algorithm Output:
    /// x           - Solution, returned as a real vector that minimizes the objective function subject to any bound constraints
    /// fval        - Objective value, returned as the real scalar fun(x)
    /// exitflag    - Algorithm stopping condition
    /// iterations  - Number of solver iterations
    /// funccount   - Number of objective function evaluations
    /// rng
    /// message
    /// 
    /// Options:
    /// SwarmSize
    /// InertiaRange        - W = [0.1,1.1]
    /// SelfAdjustment      - c1 = 1.49
    /// SocialAdjustment    - c2 = 1.49
    /// InitialSwarm
    /// InitialSwarmSpan    - 2000
    /// HybridFcn
    /// MaxIter             - 200*nvars
    /// MaxTime
    /// StallIterLimit      - 20
    /// StallTimeLimit      - Inf
    /// TolFun              - 1e-6
    /// 
    /// </summary>
    public class Swarm
    {
        Function _function;
        public IProblem _obctiveFunction;

        static Random _ran; // Solve() has random features

        public Particle[] _particles;
        public double[] _bestSwarmPos;
        public double _bestSwarmCost;
        int _dim;
        public double _minX;
        public double _maxX;

        int _iteration = 0;

        double _previousBest;
        int _bestIteration = 0;
        bool convergence = false;

        public double _w = 0.529; // inertia
        public double _c1 = 1.49445; // particle / cogntive
        public double _c2 = 1.49445; // swarm / social
        public double _death = 0.005; // prob of particle death

        public Swarm(IProblem function, int numParticles, double minX, double maxX)
        {
            _ran = new Random(0);

            _obctiveFunction = function;
            _function = function._ObjectiveFunction;
            _dim = function._Length;

            _minX = minX;
            _maxX = maxX;

            _bestSwarmCost = double.MaxValue;
            _bestSwarmPos = new double[_dim];
            _particles = new Particle[numParticles];
            for (int i = 0; i < _particles.Length; ++i)
            {
                _particles[i] = new Particle(function, minX, maxX);
                if (_particles[i]._cost < _bestSwarmCost)
                {
                    _bestSwarmCost = _particles[i]._cost;
                    Array.Copy(_particles[i]._position, _bestSwarmPos, _dim);
                }
            }
        }

        public void Solve(int maxLoop)
        {
            while (_iteration < maxLoop)
            {
                ++_iteration;

                for (int j = 0; j < _particles.Length; ++j) // each particle
                {
                    // death?
                    double p = _ran.NextDouble();
                    if (p < _death)
                    {
                        _particles[j] = new Particle(_obctiveFunction, _minX, _maxX);
                    }


                    for (int k = 0; k < _dim; ++k) // update velocity. each x position component
                    {
                        double r1 = _ran.NextDouble();
                        double r2 = _ran.NextDouble();

                        _particles[j]._velocity[k] = (_w * _particles[j]._velocity[k])
                            + (_c1 * r1 * (_particles[j]._bestPartPos[k] - _particles[j]._position[k]))
                            + (_c2 * r2 * (_bestSwarmPos[k] - _particles[j]._position[k]));

                        //Clamp
                        //if (_particles[j]._velocity[k] < _minX)
                        //{
                        //    _particles[j]._velocity[k] = _minX;
                        //}
                        //else if (_particles[j]._velocity[k] > _maxX)
                        //{
                        //    _particles[j]._velocity[k] = _maxX;
                        //}

                    }

                    for (int k = 0; k < _dim; ++k) // update position
                    {
                        _particles[j]._position[k] += _particles[j]._velocity[k];

                        //Clamp
                        if (_particles[j]._position[k] < _minX)
                        {
                            _particles[j]._position[k] = _minX;
                        }
                        else if (_particles[j]._position[k] > _maxX)
                        {
                            _particles[j]._position[k] = _maxX;
                        }
                    }

                    // update cost
                    _particles[j]._cost = _function(_particles[j]._position);

                    // check if new best cost
                    if (_particles[j]._cost < _particles[j]._bestPartCost)
                    {
                        _particles[j]._bestPartCost = _particles[j]._cost;
                        Array.Copy(_particles[j]._position, _particles[j]._bestPartPos, _dim);
                    }

                    if (_particles[j]._cost < _bestSwarmCost)
                    {
                        _previousBest = _bestSwarmCost;
                        _bestSwarmCost = _particles[j]._cost;
                        Array.Copy(_particles[j]._position, _bestSwarmPos, _dim);
                    }
                }//particle

                if (Math.Abs(_previousBest - _bestSwarmCost) < 1e-6 && !convergence && _previousBest != 0)
                {
                    _bestIteration = _iteration;
                    convergence = true;
                }

            }//iteration
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _particles.Length; ++i)
                s += "[" + i + "] " + _particles[i].ToString() + "\n";
            s += "Best Swarm Pos [ ";
            for (int i = 0; i < _bestSwarmPos.Length; ++i)
                s += _bestSwarmPos[i].ToString("F2") + " ";
            s += "] ";
            s += "Best Swarm Cost = " + _bestSwarmCost.ToString("F3") + " Iterations " + _bestIteration;
            s += "\n";
            return s;
        }
    } // Swarm
}
