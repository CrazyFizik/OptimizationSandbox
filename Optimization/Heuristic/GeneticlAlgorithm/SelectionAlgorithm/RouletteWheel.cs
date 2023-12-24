using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class RouletteWheel : BaseSelection, ISelection
    {
        public double _beta = 2;

        [Obsolete("Вынести экпоненциальное скалирование в отдельный класс")]
        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            // new population, initially empty
            IChromosome[] newPopulation = new IChromosome[size];

            //exponetial scalling
            double max = 0;
            for (int i = 0; i < population.Length; i++)
            {
                if (population[i]._Fitness > max) max = population[i]._Fitness;
            }

            double[] probability = new double[population.Length];
            double s = 0;
            double sum = 0;
            int k = 0;
            for (int i = 0; i < population.Length; i++)
            {
                // cumulative normalized fitness
                s = Math.Exp(-population[i]._Fitness * _beta / max);
                //if (double.IsInfinity(s) || double.IsNaN(s)) s = _beta;
                //s = population[i]._Fitness;                
                sum += s;
                probability[k++] = sum;
            }

            for (int i = 0; i < probability.Length; i++)
            {
                probability[i] /= sum;
            }

            //double cumsum = 0;
            //for (int i = 0; i < probability.Length; i++)
            //{
            //    cumsum += probability[i];
            //    probability[i] = cumsum;
            //}

            // select chromosomes from old population to the new population
            int[] popadanija = new int[population.Length];
            for (int j = 0; j < size; j++)
            {
                // get wheel value
                double wheelValue = _random.NextDouble();
                // find the chromosome for the wheel value
                for (int i = 0; i < population.Length; i++)
                {
                    if (wheelValue <= probability[i])
                    {
                            // add the chromosome to the new population
                            newPopulation[j] = population[i].Clone();
                            popadanija[i] += 1;
                            break;
                    }
                }
            }

            return newPopulation;
        }
    }
}
