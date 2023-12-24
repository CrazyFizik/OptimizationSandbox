using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Heuristic.GeneticlAlgorithm.Interfaces
{
    public interface IGeneticOperatorsParameters
    {
        /// <summary>
        /// Crossover type
        /// </summary>
        eCrossoverType _CrossoverType
        {
            get;
            set;
        }

        /// <summary>
        /// Crossover type
        /// </summary>
        eMutationType _MutationType
        {
            get;
            set;
        }
    }
}
