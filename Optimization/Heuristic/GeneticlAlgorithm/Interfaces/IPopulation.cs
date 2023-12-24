using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Heuristic.GeneticlAlgorithm.Interfaces
{
    public interface IPopulation
    {
        /// <summary>
        /// Population size
        /// </summary>
        int _Size
        {
            get;
        }

        int _Genertaions
        {
            get;
            set;
        }

        /// <summary>
        /// Elites count
        /// _EliteRate x _Size
        /// deafult is 0.05
        /// </summary>
        double _EliteRate
        {
            get;
            set;
        }

        /// <summary>
        /// Crossover count
        /// (_Size - _ElteRate x _Size) x _XoverRate
        /// deafult is 0.8
        /// </summary>
        double _XoverRate
        {
            get;
            set;
        }

        /// <summary>
        /// Mutants count
        /// [(_Size - _ElteRate x _Size) x _XoverRate] x _MutationRate
        /// deafult is 1 ????
        /// </summary>
        double _MutationRate
        {
            get;
            set;
        }

        /// <summary>
        /// Best chromosome
        /// </summary>
        IChromosome _BestChromosome
        {
            get;
        }

        /// <summary>
        /// Best fitness
        /// </summary>
        double _FitnessBest
        {
            get;
        }

        /// <summary>
        /// Max value of fitness
        /// </summary>
        double _FitnessMax
        {
            get;
        }

        /// <summary>
        /// Average fitness
        /// </summary>
        double _FitnessAvg
        {
            get;
        }

        /// <summary>
        /// Sum of fitness
        /// </summary>
        double _FitnessSum
        {
            get;
        }

        /// <summary>
        /// Get chromosome with specified index.
        /// </summary>
        /// 
        /// <param name="index">Chromosome's index to retrieve.</param>
        /// 
        /// <remarks>Allows to access individuals of the population.</remarks>
        /// 
        IChromosome this[int index]
        {
            get;
        }

        IChromosome[] _Elites
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        eSelectionType _SelectionType
        {
            get;
            set;
        }

        ///// <summary>
        ///// Evaluate fitness fnction
        ///// </summary>
        //void Evaluate();

    }
}
