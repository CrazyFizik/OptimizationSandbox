using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Chromosome
{
    public class ChromosomePermutation : IChromosome
    {
        public eCrossoverType _CrossoverType
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

        public double _Fitness
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int _Generation
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

        public int _GenerationMax
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

        public int _Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double _MutationRate
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

        public eMutationType _MutationType
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

        public Random _Random
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IChromosome Clone()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IChromosome other)
        {
            throw new NotImplementedException();
        }

        public IChromosome CreateNew()
        {
            throw new NotImplementedException();
        }

        public void Crossover(IChromosome pair)
        {
            throw new NotImplementedException();
        }

        public double Distance(IChromosome pair)
        {
            throw new NotImplementedException();
        }

        public void Evaluate()
        {
            throw new NotImplementedException();
        }

        public void Mutation()
        {
            throw new NotImplementedException();
        }

        public void Mutation(IChromosome attraction)
        {
            throw new NotImplementedException();
        }
    }
}
