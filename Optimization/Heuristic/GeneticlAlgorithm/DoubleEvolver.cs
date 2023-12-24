using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm;
using Optimization.Interfaces;


namespace Optimization.Heuristic.GeneticlAlgorithm
{
    public class DoubleEvolver
    {
        public Function _function;
        public IProblem _obctiveFunction;

        public IChromosome _ancestor;
        public Population.Population _population;
        public Options _options;

        bool isInit = false;

        delegate bool Step();
        static Step _step;

        public DoubleEvolver(
            IProblem objective, //?????
            IChromosome ancestor,
            Options options)
        {
            this._ancestor = ancestor;
            this._options = options;

            this._obctiveFunction = objective;
            this._function = objective._ObjectiveFunction;
        }

        public void Init()
        {
            _ancestor._MutationType = _options._mutationType;
            _ancestor._CrossoverType = _options._crossoverType;

            _population = new Population.Population(_ancestor, _options._size);
            _population._SelectionType = _options._selectionType;
            _population._Genertaions = _options._maxLoop;
            _population._MutationRate = _options._mutationRate;
            _population._EliteRate = _options._eliteRate;
            _population._XoverRate = _options._crossoverRate;

            _population.Init();

            if (_options._strategyType == eStrategyType.Simple)
            {
                //_population._MutationRate = 1.0 / _options._size;
                _step = _population.StepSimple;
                
                //eCrossoverType.BLX_alpha;
                //eMutationType.Muhleblein || eMutationType.Nonuniform;
            }

            if (_options._strategyType == eStrategyType.Separate)
            {
                _population._MutationRate = 1;
                _step = _population.StepSeparate;
                
                //eCrossoverType.Intermediate;
                //eMutationType.NonUniform;
            }

            if (_options._strategyType == eStrategyType.Canonical)
            {
                //_population._MutationRate = 1.0 / _options._size;
                _step = _population.StepCanonical;
            }

            isInit = true;
        }

        public double[] Solve()
        {
            if (!isInit) Init();

            for (;;)
            {
                if (_step())
                    break;
            }

            return (_population._BestChromosome as Chromosome.ChromosomeDouble)._chromosome;
        }
    }

    public struct Options
    {
        public eStrategyType _strategyType;
        public eCrossoverType _crossoverType;
        public eMutationType _mutationType;
        public eSelectionType _selectionType;

        public int _size;
        public int _maxLoop;

        public double _eliteRate;
        public double _crossoverRate;
        public double _mutationRate;
    }
}
