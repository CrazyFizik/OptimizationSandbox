using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm
{
    public class Reward : BaseSelection, ISelection
    {
        public double SelectionPressure
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IChromosome[] ApplySelection(IChromosome[] population, int size)
        {
            throw new NotImplementedException();
        }
    }
}
