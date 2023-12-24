using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Naive
{
    public class Individual : IComparable<Individual>
    {
        public Function _function;
        public IProblem _obctiveFunction;

        public double _fitness;
        public double[] _chromosome;

        private int _numGenes;
        private double[] _min;
        private double[] _max;
        private double _mutationRate;
        private double _precision = 2 * 0.005;

        private int _generation = 0;

        static Random _random = new Random(0);

        public Individual(IProblem objective, double[] minGene, double[] maxGene, double mutateRate, double precision)
        {
            _obctiveFunction = objective;
            _function = objective._ObjectiveFunction;

            this._numGenes = objective._Length;
            this._min = minGene;
            this._max = maxGene;
            this._mutationRate = mutateRate;
            //this._precision = precision;
            this._chromosome = new double[this._numGenes];
            for (int i = 0; i < this._chromosome.Length; ++i)
            {
                this._chromosome[i] = (maxGene[i] - minGene[i]) * _random.NextDouble() + minGene[i];
            }
        }

        public double Evaluate()
        {
            _fitness = _function(_chromosome);
            return _fitness;
        }

        public void Mutate()
        {
            for (int i = 0; i < _chromosome.Length; ++i)
            {
                double hi = _precision * _max[i];
                double lo = _precision * _min[i];

                if (_random.NextDouble() < _mutationRate)
                {
                    _chromosome[i] +=
                        (hi - lo) * _random.NextDouble()
                        + lo;
                }

                if (_chromosome[i] > _max[i])
                {
                    _chromosome[i] = _max[i];
                }

                if (_chromosome[i] < _min[i])
                {
                    _chromosome[i] = _min[i];
                }
            }
        }

        public void MutateGaussian()
        {
            for (int i = 0; i < _chromosome.Length; i++)
            {
                //Box-Muller Transformation
                double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - _random.NextDouble();
                double randStdNormal =
                    Math.Sqrt(-2.0 * Math.Log(u1))
                    * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)      
                double stddev = 1;
                double mean = 0;
                double randomn = randStdNormal * stddev + mean;

                double sigma = _precision * (1 - _generation/(_numGenes*100));
                _generation++;

                //Add gaussian
                _chromosome[i] +=
                    (_max[i] - _min[i]) * sigma * randomn;

                if (_chromosome[i] > _max[i])
                {
                    _chromosome[i] = _max[i];
                }

                if (_chromosome[i] < _min[i])
                {
                    _chromosome[i] = _min[i];
                }
            }

        }

        public void MutateUniform()
        {
            for (int i = 0; i < _chromosome.Length; i++)
            {
                if (_random.NextDouble()<_mutationRate)
                {
                    _chromosome[i] = 
                        _random.NextDouble()
                        * (_max[i] - _min[i])
                        + _min[i];
                }

                if (_chromosome[i] > _max[i])
                {
                    _chromosome[i] = _max[i];
                }

                if (_chromosome[i] < _min[i])
                {
                    _chromosome[i] = _min[i];
                }
            }
        }

        public void MutateUniformAdder()
        {
            int mutationNumber = (int)Math.Ceiling(_mutationRate * _chromosome.Length);
            double probability = (double)mutationNumber / _chromosome.Length; //faster than Mutate

            for (int i = 0; i < _chromosome.Length; ++i)
            {
                double hi = _precision * _max[i];
                double lo = _precision * _min[i];

                if (_random.NextDouble() < probability)
                {
                    _chromosome[i] +=
                        (hi - lo) * _random.NextDouble()
                        + lo;
                }

                if (_chromosome[i] > _max[i])
                {
                    _chromosome[i] = _max[i];
                }

                if (_chromosome[i] < _min[i])
                {
                    _chromosome[i] = _min[i];
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _chromosome.Length; ++i)
                s += _chromosome[i].ToString("F2") + " ";
            if (this._fitness == double.MaxValue)
                s += "| fitness = maxValue";
            else
                s += "| fitness = " + this._fitness.ToString("F4");
            return s;
        }

        public int CompareTo(Individual other)
        {
            if (this._fitness < other._fitness)
            {
                return -1;
            }
            else if (this._fitness > other._fitness)
            {
                return 1;
            }
            else return 0;
        }
    }
}
