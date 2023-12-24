using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Heuristic.GeneticlAlgorithm.Interfaces
{
    public interface IChromosome : IComparable<IChromosome>, IGeneticOperatorsParameters
    {
        #region Properties
              

        Random _Random
        {
            get;
        }

        double _Fitness
        {
            get;
        }

        int _Length
        {
            get;
        }

        int _Generation
        {
            get;
            set;
        }

        int _GenerationMax
        {
            get;
            set;
        }

        double _MutationRate
        {
            get;
            set;
        }

        
        #endregion

        #region Methods

        /// <summary>
        /// Apply Mutation operator to this chromosome
        /// </summary>
        void Mutation();

        void Mutation(IChromosome attraction);

        /// <summary>
        /// Apply Crossover operator to this chromosome
        /// </summary>
        /// <param name="pair">partner</param>
        void Crossover(IChromosome pair);

        /// <summary>
        /// Clone operation
        /// </summary>
        /// <returns>Copy of this chromosome</returns>
        IChromosome Clone();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IChromosome CreateNew();

        /// <summary>
        /// Get Hamming distance
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        double Distance(IChromosome pair);

        /// <summary>
        /// Evaluate fitness fnction
        /// </summary>
        void Evaluate();

        #endregion
    }
}
