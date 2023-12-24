using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class RouletteWheelRank : BaseSelection, ISelection
    {
        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            int N = population.Length;

            // new population, initially empty
            List<IChromosome> newPopulation = new List<IChromosome>();

            // sort current population
            //Array.Sort(population);

            // calculate amount of ranges in the wheel
            double ranges = N * (N + 1) / 2;

            // create wheel ranges
            double[] rangeMax = new double[N];
            double s = 0;

            for (int i = 0, n = N; i < N; i++, n--)
            {
                s += ((double)n / ranges);
                rangeMax[i] = s;
            }

            // select chromosomes from old population to the new population
            int[] popadanija = new int[population.Length];
            for (int j = 0; j < size; j++)
            {
                // get wheel value
                double wheelValue = _random.NextDouble();
                // find the chromosome for the wheel value
                for (int i = 0; i < N; i++)
                {
                    if (wheelValue <= rangeMax[i])
                    {
                        // add the chromosome to the new population
                        newPopulation.Add(((IChromosome)population[i]).Clone());
                        popadanija[i] += 1;
                        break;
                    }
                }// for each wheel sector
            }//for each new chromosome in new population
            return newPopulation.ToArray();
        }
    }
}
