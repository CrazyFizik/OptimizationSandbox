using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class StochasticUniversalSampling : BaseSelection, ISelection
    {
        double _eps = 1e-8;
        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            Array.Sort(population);
            // Calculate total fitness of population
            double f = 0;
            foreach (IChromosome x in population)
            {
                f += x._Fitness;
            }

            double[] fitness = new double[population.Length];//= Scalling.LinearScalling.GetScaling(population);
            for (int i = 0; i < fitness.Length; i++)
            {
                if (Math.Abs(population[i]._Fitness) > _eps)
                {
                    fitness[i] = f / population[i]._Fitness;
                }
                else
                {
                    fitness[i] = 1;
                }
            }

            //f = 0;
            double p = 0;
            for (int i = 0; i < fitness.Length; i++)
            {
                //f += fitness[i] / size;
                p += fitness[i] / size;
            }

            // Calculate distance between the pointers
            //p = f / size;

            // Pick random number between 0 and p
            double start = _random.NextDouble() * p;

            IChromosome[] result = new IChromosome[size];
            int index = 0;
            double sum = fitness[index];
            for (int i = 0; i < size; i++)
            {
                // Determine pointer to a segment in the population
                double pointer = start + i * p;
                if (sum >= pointer)
                {
                    result[i] = population[index].Clone();
                }
                else
                {
                    for (++index; index < population.Length; index++)
                    {
                        sum += fitness[index];
                        if (sum >= pointer)
                        {
                            result[i] = population[index].Clone();
                            break;
                        }
                    }
                }
            }

            int[] indexes = new int[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                indexes[i] = i;
            }

            for (int i = 0; i < indexes.Length; ++i)
            {
                int r = _random.Next(i, indexes.Length);
                int tmp = indexes[r];
                indexes[r] = indexes[i];
                indexes[i] = tmp;
            }

            IChromosome[] temp = new IChromosome[result.Length];
            result.CopyTo(temp, 0);

            for (int i = 0; i < temp.Length; i++)
            {
                result[i] = temp[indexes[i]];
            }

            return result;
        }
    }
}
