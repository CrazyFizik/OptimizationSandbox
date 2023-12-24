using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm;
using Optimization.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Population
{
    public class Individual : IChromosome
    {
        IChromosome[] _chromosomes;

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

        public Individual()
        {

        }

        public Individual(Individual seed)
        {

        }

        public IChromosome Clone()
        {
            return new Individual(this);
        }

        public int CompareTo(IChromosome other)
        {
            if (this._Fitness < other._Fitness)
            {
                return -1;
            }
            else if (this._Fitness > other._Fitness)
            {
                return 1;
            }
            else return 0;
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

        }

        public void Mutation()
        {
            foreach(IChromosome x in _chromosomes)
            {
                x.Mutation();
            }
        }

        public void Mutation(IChromosome attraction)
        {
            throw new NotImplementedException();
        }
    }
}
