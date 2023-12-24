using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Heuristic.GeneticlAlgorithm.Interfaces
{
    public interface ISelection
    {
        /// <summary>
        /// Return selected population
        /// </summary>
        /// <param name="chromosomes">Selected population</param>
        /// <param name="size">Size of selected population</param>
        /// <returns> parents </returns>
        IChromosome[] ApplySelection(IChromosome[] population, int size);
    }
}
