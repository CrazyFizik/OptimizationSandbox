using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Scalling
{
    public class LinearScalling : IScalling
    {
        private static double _max;
        private static double _min;
        private static double _mean;
        private static double _sum;
        private static double _c = 1.2;

        static public double[] GetScaling(IChromosome[] population)
        {
            double[] fitnesses = new double[population.Length];
            for (int i = 0; i < fitnesses.Length; i++)
            {
                fitnesses[i] = population[i]._Fitness;
            }         
            return Scaling(fitnesses);
        }

        static void Prescale(double[] finesses)
        {
            _max = finesses.Max();
            _min = finesses.Min();
            _sum = 0;
            for (int i = 0; i < finesses.Length; i++)
            {
                _sum += finesses[i];
                _mean += finesses[i] / finesses.Length;
            }
        }

        static double[] Scaling(double[] finesses)
        {
            double[] result = new double[finesses.Length];
            double delta = 0;
            double a = 0;
            double b = 0;

            Prescale(finesses);

            if (_min > (_c * _mean - _max)/(_c - 1.0)) //if nonnegative min
            {
                delta = _max - _mean;
                a = (_c - 1.0) * _mean / delta;
                b = _mean * (_max - _c * _mean) / delta;
            }
            else
            {
                delta = _mean - _min;
                a = _mean / delta;
                b = -_min * _mean / delta;
            }

            //if (delta < 0.00001 && delta > -0.00001)
            //{ /* if converged */
            //    a = 1.0;
            //    b = 0.0;
            //}

            for (int i = 0; i < finesses.Length; i++)
            {
                result[i] = a * finesses[i] + b;
            }
            return result;
        }
    }
}
