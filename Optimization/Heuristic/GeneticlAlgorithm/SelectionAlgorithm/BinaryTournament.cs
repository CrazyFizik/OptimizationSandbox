using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class BinaryTournament : BaseSelection, ISelection
    {
        int _tournamentSize = 2;

        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            IChromosome[] result = new IChromosome[size];
            for (int i = 0; i < size; i++)
            {
                int index = _random.Next(0, population.Length);
                IChromosome winner = population[index];
                for (int j = 1; j < _tournamentSize; j++)
                {
                    index = _random.Next(0, population.Length);
                    IChromosome opponent = population[index];
                    if (opponent._Fitness < winner._Fitness)
                    {
                        winner = opponent;
                    }
                }
                result[i] = winner;
            }
            return result;
        }
    }
}
