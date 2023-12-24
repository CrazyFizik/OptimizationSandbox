using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class Elite : BaseSelection, ISelection
    {
        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            Array.Sort(population);
            IChromosome[] newPopulation = new IChromosome[size];
            newPopulation[0] = population[0].Clone();
            int index = 0;
            for (int j = 1; j < size; j++)
            {
                for (++index; index < population.Length; index++)
                {
                    //if (newPopulation[j - 1].Distance(population[index]) > (1.0 / population[index]._Length))
                    {
                        newPopulation[j] = population[index].Clone();
                        break;
                    }
                }                
            }

            return newPopulation;
        }
    }
}
