using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.ParticleSwarmOptimization
{
    public class Particle
    {
        Function _function;

        static Random _ran = new Random(0);

        public double[] _position;
        public double[] _velocity;
        public double _cost;
        public double[] _bestPartPos;
        public double _bestPartCost;
        int _dim;

        public Particle(IProblem function, double minX, double maxX)
        {
            _function = function._ObjectiveFunction;
            _dim = function._Length;

            _position = new double[_dim];
            _velocity = new double[_dim];
            _bestPartPos = new double[_dim];
            for (int i = 0; i < _dim; ++i)
            {
                _position[i] = (maxX - minX) * _ran.NextDouble() + minX;
                _velocity[i] = (maxX - minX) * _ran.NextDouble() + minX;
            }
            _cost = _function(_position);
            _bestPartCost = _cost;
            Array.Copy(_position, _bestPartPos, _dim);
        }

        public override string ToString()
        {
            string s = "";
            s += "Pos [ ";
            for (int i = 0; i < _position.Length; ++i)
                s += _position[i].ToString("F2") + " ";
            s += "] ";
            s += "Vel [ ";
            for (int i = 0; i < _velocity.Length; ++i)
                s += _velocity[i].ToString("F2") + " ";
            s += "] ";
            s += "Cost = " + _cost.ToString("F3");
            s += " Best Pos [ ";
            for (int i = 0; i < _bestPartPos.Length; ++i)
                s += _bestPartPos[i].ToString("F2") + " ";
            s += "] ";
            s += "BestCost = " + _cost.ToString("F3");
            return s;
        }

    } // Particle
}
