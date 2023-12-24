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
    [Obsolete("Отрефакторить класс")]
    public class Population : IPopulation
    {
        bool isInit = false;
        private eSelectionType _selectionType = eSelectionType.Tournament;

        IChromosome[] _population;

        //
        Elite _eliteSelector;
        ISelection _selector;

        // preallocated intermediate populations
        IChromosome[] _elites;
        IChromosome[] _parents;
        IChromosome[] _xoverOffsprings;
        IChromosome[] _mutationOffsprings;
        List<IChromosome> _spices = new List<IChromosome>();

        // population parameters
        int _size = 50;
        private double _eliteRate = 0.05;
        private double _crossoverRate = 0.8;
        private double _mutationRate = 0.1;

        private int _eliteCount = 1;
        private int _xoverCount = 8;
        private int _mutantsCount = 1;

        private int _xover = 0;

        public int _ElitesNumber
        {
            get
            {
                return _eliteCount;
            }
        }

        public int _XoverNumner
        {
            get
            {
                return _xoverCount;
            }
        }

        public int _MutantsNumber
        {
            get
            {
                return _mutantsCount;
            }
        }

        // fitness parameters
        private double _fitnessMax = 0;
        private double _fitnessSum = 0;
        private double _fitnessAvg = 0;
        private double _fitnessBest = 0;

        public List<double> _learning = new List<double>();
        private IChromosome _bestChromosome = null;

        //solver parameters
        int _generation = 0;
        int _generationsMax = 200;
        int _stall = 0;

        #region IPopulation implementation

        public int _Size
        {
            get
            {
                return _size;
            }
        }

        public int _Genertaions
        {
            get
            {
                return _generationsMax;
            }

            set
            {
                _generationsMax = value;
            }
        }

        public double _EliteRate
        {
            get
            {
                return _eliteRate;
            }

            set
            {
                _eliteRate = value;
            }
        }

        public double _XoverRate
        {
            get
            {
                return _crossoverRate;
            }

            set
            {
                _crossoverRate = value;
            }
        }

        public double _MutationRate
        {
            get
            {
                return _mutationRate;
            }

            set
            {
                _mutationRate = value;
            }
        }

        public IChromosome _BestChromosome
        {
            get
            {
                return _bestChromosome;
            }
        }

        public IChromosome[] _Elites
        {
            get
            {
                return _elites;
            }
        }

        public double _FitnessBest
        {
            get
            {
                return _fitnessBest;
            }
        }

        public double _FitnessMax
        {
            get
            {
                return _fitnessMax;
            }
        }

        public double _FitnessAvg
        {
            get
            {
                return _fitnessAvg;
            }
        }

        public double _FitnessSum
        {
            get
            {
                return _fitnessSum;
            }
        }

        public eSelectionType _SelectionType
        {
            get
            {
                return _selectionType;
            }

            set
            {
                _selectionType = value;
            }
        }

        public IChromosome this[int index]
        {
            get
            {
                return _population[index];
            }
            set
            {
                _population[index] = value;
            }
        }
        #endregion

        #region Constructors

        public Population(
            IChromosome ancestor,
            int size)
        {
            this._size = size;
            _bestChromosome = ancestor;
        }

        public void Init()
        {
            _population = new IChromosome[_size];

            _eliteCount = (int)Math.Ceiling(_eliteRate * _size);
            _xoverCount = (int)(_crossoverRate * (_size - _eliteCount));
            _mutantsCount = _size - _eliteCount - _xoverCount;

            _elites = new IChromosome[_eliteCount];
            _xoverOffsprings = new IChromosome[_xoverCount];
            _mutationOffsprings = new IChromosome[_mutantsCount];

            _eliteSelector = new Elite();
            _selector = new Elite();

            //Selection 
            if (_selectionType == eSelectionType.Tournament)
            {
                _selector = new BinaryTournament();
            }
            if (_selectionType == eSelectionType.RouletteWheel)
            {
                _selector = new RouletteWheel();
            }
            if (_selectionType == eSelectionType.RouletteWheelRank)
            {
                _selector = new RouletteWheelRank();
            }
            if (_selectionType == eSelectionType.StohoasticUniversalSampling)
            {
                _selector = new StochasticUniversalSampling();
            }
            if (_selectionType == eSelectionType.LinearRankSelection)
            {
                _selector = new Elite();
            }

            Generate();

            isInit = true;
        }

        public void Init(IChromosome[] initial)
        {
            for (int i = 1; i < _size; i++)
            {
                initial[i].Evaluate();
                _population[i] = initial[i];
            }

            Array.Sort(_population);
            _bestChromosome = _population[0].Clone();
        }

        void Generate()
        {
            // add ancestor to the population
            _bestChromosome._MutationRate = _mutationRate;
            _bestChromosome.Evaluate();
            _fitnessBest = _bestChromosome._Fitness;
            _population[0] = _bestChromosome.Clone();
            // add more chromosomes to the population
            for (int i = 1; i < _size; i++)
            {
                // create new chromosome
                IChromosome c = _bestChromosome.CreateNew();
                // calculate it's fitness
                c.Evaluate();
                // add it to population
                _population[i] = c;
            }
        }

        #endregion

        #region Loop

        public IChromosome SolveLoop(int maxLoop)
        {
            _generationsMax = maxLoop;
            return SolveLoop();
        }

        [Obsolete("Вывести Solver Main Loop в класс алгоритма")]
        public IChromosome SolveLoop()
        {
            if (!isInit) Init();

            //Main loop
            while (_generation < _generationsMax)
            {
                //Selection
                _elites = _eliteSelector.ApplySelection(_population, _eliteCount);
                _parents = _selector.ApplySelection(_population, _xoverCount * 2 + _mutantsCount);

                //Crossover
                int index = 0;
                for (int i = 0; i < _xoverCount * 2; i += 2)
                {
                    IChromosome parent1 = _parents[i].Clone();
                    IChromosome parent2 = _parents[i + 1].Clone();

                    if (parent1._Fitness == parent2._Fitness) //check ananism
                    {
                        parent2.Mutation();
                    }

                    parent1.Crossover(parent2);
                    parent1.Evaluate();
                    _xoverOffsprings[index] = parent1;
                    index++;
                }

                //Mutation
                for (int i = 0; i < _mutantsCount; i++)
                {
                    IChromosome parent = _parents[i + _xoverCount * 2].Clone();
                    parent.Mutation();
                    parent.Evaluate();
                    _mutationOffsprings[i] = parent;
                }

                //Create new population
                _fitnessSum = 0;
                for (int i = 0; i < _size; ++i)
                {
                    if (i < _eliteCount) //add elites
                    {
                        _population[i] = _elites[i];
                    }
                    else if (i < _eliteCount + _xoverCount)
                    {
                        _population[i] = _xoverOffsprings[i - _eliteCount];
                    }
                    else if (i < _eliteCount + _xoverCount + _mutantsCount) //add xover child
                    {
                        _population[i] = _mutationOffsprings[i - _eliteCount - _xoverCount];
                    }
                    else //generate new
                    {
                        _population[i] = _bestChromosome.Clone();
                        _population[i].Evaluate();
                    }

                    //Update fitness function
                    _fitnessSum += _population[i]._Fitness;
                    if (_population[i]._Fitness < _fitnessBest)
                    {
                        _fitnessBest = _population[i]._Fitness;

                        IChromosome best = _population[i].Clone();
                        _bestChromosome = best;
                        _stall = 0;
                    }
                    else if (_population[i]._Fitness > _fitnessMax)
                    {
                        _fitnessMax = _population[i]._Fitness;
                    }
                } // for each generation

                _fitnessAvg = _fitnessSum / _size;
                _generation++;

                _learning.Add(_fitnessBest);
                _stall++;
            }
            return _bestChromosome;
        }

        #endregion

        #region Step

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool StepSeparate()
        {
            if (!isInit) Init();


            //Selection
            _elites = _eliteSelector.ApplySelection(_population, _eliteCount);
            _parents = _selector.ApplySelection(_population, _xoverCount * 2 + _mutantsCount);

            //Crossover
            int index = 0;
            for (int i = 0; i < _xoverCount * 2; i += 2)
            {
                IChromosome parent1 = _parents[i].Clone();
                IChromosome parent2 = _parents[i + 1].Clone();

                if (parent1._Fitness == parent2._Fitness) //check ananism
                {
                    for (int j = 0; j < _xoverCount; j++) 
                    {
                        if ((parent1.Distance(parent2) != 0))
                        {
                            parent2 = _parents[Chromosome.BaseChromosome._random.Next(0, _xoverCount)].Clone();
                            break;
                        }
                    }
                }

                parent1.Crossover(parent2);
                parent1.Evaluate();
                _xoverOffsprings[index] = parent1;
                index++;
            }

            //Mutation
            for (int i = 0; i < _mutantsCount; i++)
            {
                IChromosome parent = _parents[i + _xoverCount * 2].Clone();
                parent._MutationRate = _MutationRate;
                parent.Mutation();
                parent.Evaluate();
                _mutationOffsprings[i] = parent;
            }

            //Create new population
            IChromosome best = _population[0];
            _fitnessSum = 0;
            _fitnessMax = best._Fitness;
            for (int i = 0; i < _size; ++i)
            {
                if (i < _eliteCount) //add elites
                {
                    _population[i] = _elites[i];
                }
                else if (i < _eliteCount + _xoverCount)
                {
                    _population[i] = _xoverOffsprings[i - _eliteCount];
                }
                else if (i < _eliteCount + _xoverCount + _mutantsCount) //add xover child
                {
                    _population[i] = _mutationOffsprings[i - _eliteCount - _xoverCount];
                }
                else //generate new
                {
                    _population[i] = _bestChromosome.Clone();
                    _population[i].Evaluate();
                }

                //Set generation
                _population[i]._Generation = _generation;
                _population[i]._GenerationMax = _generationsMax;
                //Update fitness function
                _fitnessSum += _population[i]._Fitness;
                if (_population[i]._Fitness < best._Fitness)
                {
                    best = _population[i];
                }
                else if (_population[i]._Fitness > _fitnessMax)
                {
                    _fitnessMax = _population[i]._Fitness;
                }
            } // for each generation

            _fitnessAvg = _fitnessSum / _size;
            _generation++;

            if (best._Fitness < _fitnessBest)
            {
                _bestChromosome = best.Clone();
                _fitnessBest = best._Fitness;
                _stall = 0;

                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best._Fitness.ToString();
                Console.WriteLine(s);
            }

            _learning.Add(_fitnessBest);
            _stall++;
            
            return _generation > _generationsMax;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool StepSimple()
        {
            if (!isInit) Init();

            //Intermediate offsprings population
            List<IChromosome> _offsprings = new List<IChromosome>();

            ////Elites conservation
            _elites = _eliteSelector.ApplySelection(_population, _eliteCount);            
            for (int i = 0; i < _eliteCount; i++)
            {
                _offsprings.Add(_elites[i].Clone());
            }

            //Selection parent B for mating
            _xoverCount = 2 * (int)Math.Floor((double)(_population.Length) / 2);
            _xoverCount -= _eliteCount;
            _parents = _selector.ApplySelection(_population, _xoverCount);

            //Crossover
            for (int i = 0; i < _xoverCount; i += 1)
            {
                IChromosome parent = _population[i + _eliteCount].Clone();
                if (Chromosome.BaseChromosome._random.NextDouble() < _XoverRate)
                {
                    IChromosome mate = _parents[i].Clone();
                    if (parent.Distance(mate) < (1.0 / parent._Length))
                    {
                        mate = _parents[Chromosome.BaseChromosome._random.Next(0, _xoverCount)].Clone();
                    }
                    IChromosome children1 = parent.Clone();
                    children1.Crossover(mate);
                    _offsprings.Add(children1);  
                    _xover++;
                }
                else
                {
                    _offsprings.Add(parent);
                }
            }

            //Mutation
            int N = _offsprings.Count;
            for (int i = _eliteCount; i < N; i++)
            {
                IChromosome offspring = _offsprings[i].Clone();
                offspring.Mutation();
                offspring.Evaluate();
                _offsprings[i] = offspring;
            }

            //sorting
            _offsprings.Sort();

            //Replace
            _fitnessSum = 0;
            for (int i = 0; i < _size; ++i)
            {
                _population[i] = _offsprings[i];
                _population[i]._Generation = _generation;
                _population[i]._GenerationMax = _generationsMax;
                //Update fitness function
                _fitnessSum += _population[i]._Fitness;
            } // for each generation
            
            //Update fitness
            _fitnessMax = _population[_size - 1]._Fitness;
            _fitnessAvg = _fitnessSum / _size;
            double best = _population[0]._Fitness;
            if (best < _fitnessBest)
            {
                IChromosome bestChromosome = _population[0].Clone();
                _bestChromosome = bestChromosome;
                _fitnessBest = bestChromosome._Fitness;
                _stall = 0;

                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString();
                Console.WriteLine(s);
            }

            _generation++;
            _stall++;
            _learning.Add(_fitnessBest);

            bool exit =
                (_generation > _generationsMax);
                //|| (_fitnessBest < 1);
                //|| (_fitnessBest == _fitnessAvg);

            if (exit)
            {
                string s = 
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString();
                Console.WriteLine(s);
            }

            return exit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool StepCanonical()
        {
            if (!isInit) Init();

            //Intermediate offsprings population
            List<IChromosome> _offsprings = new List<IChromosome>();

            //Crossover
            for (int i = 0; i < _xoverCount; i += 2)
            {
                IChromosome parent1 = _population[i].Clone();
                IChromosome parent2 = _population[i + 1].Clone();
                if (Chromosome.BaseChromosome._random.NextDouble() < _XoverRate)
                {
                    IChromosome children1 = parent1.Clone();
                    IChromosome children2 = parent2.Clone();

                    children1.Crossover(parent2);
                    children2.Crossover(parent1);

                    _offsprings.Add(children1);
                    _offsprings.Add(children2);

                    _xover++;
                }
            }

            //Mutation
            int N = _offsprings.Count;
            for (int i = 0; i < N; i++)
            {
                IChromosome offspring = _offsprings[i].Clone();
                offspring.Mutation();
                offspring.Evaluate();
                _offsprings[i] = offspring;
            }

            //Add parents
            for (int i = _eliteCount; i < _size; i++)
            {
                _offsprings.Add(_population[i].Clone());
            }    
            
            //Add elites
            if (_eliteCount > 0)
            {
                _elites = _eliteSelector.ApplySelection(_population, _eliteCount);
                for (int i = 0; i < _eliteCount; i++)
                {
                    _population[i] = _elites[i].Clone();
                }
            }

            //Sorting
            //_offsprings.Sort();

            //Replace
            IChromosome[] _nextGeneration = _selector.ApplySelection(_offsprings.ToArray<IChromosome>(), _size - _eliteCount);
            _fitnessSum = 0;
            for (int i = 0; i < _size - _eliteCount; ++i)
            {
                _population[i+_eliteCount] = _nextGeneration[i].Clone();
                _population[i+_eliteCount]._Generation = _generation;
                _population[i + _eliteCount]._GenerationMax = _generationsMax;
                //Update fitness function
                _fitnessSum += _population[i+_eliteCount]._Fitness;
            } // for each generation

            //Update fitness
            _fitnessMax = _population[_size - 1]._Fitness;
            _fitnessAvg = _fitnessSum / _size;
            double best = _population[0]._Fitness;
            IChromosome bestChromosome = _population[0].Clone();
            for (int i = 1; i < _size; i++)
            {
                if (_population[i]._Fitness < best)
                {
                    best = _population[i]._Fitness;
                    bestChromosome = _population[i].Clone();
                }
                if (_population[i]._Fitness > _fitnessMax) _fitnessMax = _population[i]._Fitness;
            }
            if (best < _fitnessBest)
            {                
                _bestChromosome = bestChromosome;
                _fitnessBest = bestChromosome._Fitness;
                _stall = 0;

                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString();
                Console.WriteLine(s);
            }

            _generation++;
            _stall++;
            _learning.Add(_fitnessBest);

            bool exit =
                (_generation > _generationsMax);
            //|| (_fitnessBest < 1);
            //|| (_fitnessBest == _fitnessAvg);

            if (exit)
            {
                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString();
                Console.WriteLine(s);
            }

            return exit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool StepSpice()
        {
            if (!isInit) Init();

            //Intermediate offsprings population
            List<IChromosome> _offsprings = new List<IChromosome>();

            //select seeds
            List<List<int>> S = new List<List<int>>();
            _spices.Clear();
            double r = (double)_bestChromosome._Length / (Math.Pow(4, (1.0 / _bestChromosome._Length)));
            r = r / 4;
            r = 1.6;
            //r = 100;
            Array.Sort(_population);
            for (int i = 0; i < _population.Length; i++)
            {
                IChromosome x = _population[i];
                bool found = false;
                if (_spices.Count > 0)
                {
                    for (int j = 0; j < _spices.Count; j++)
                    {
                        double d = x.Distance(_spices[j]);
                        if (d < r)
                        {
                            found = true;
                            S[j].Add(i);
                            break;
                        }
                    }
                }
                if (!found)
                {
                    _spices.Add(x.Clone());
                    S.Add(new List<int>());
                }
            }

            //Elites conservation
            //_elites = _eliteSelector.ApplySelection(_population, _eliteCount);
            //for (int i = 0; i < _eliteCount; i++)
            //{
            //    _offsprings.Add(_elites[i].Clone());
            //}
            _eliteCount = 0;

            //Selection parent B for mating
            _xoverCount = 2 * (int)Math.Floor((double)_population.Length / 2);
            _parents = _selector.ApplySelection(_population, _xoverCount);

            //Crossover
            //for (int i = 0; i < _xoverCount; i += 2)
            //{
            //    IChromosome parent1 = _population[i].Clone();
            //    IChromosome parent2 = _parents[i + 1].Clone();
            //    if (_random.NextDouble() < _XoverRate)
            //    {
            //        IChromosome children1 = parent1.Clone();
            //        IChromosome children2 = parent2.Clone();

            //        children1.Crossover(parent2);
            //        children2.Crossover(parent1);

            //        _offsprings.Add(children1);
            //        _offsprings.Add(children2);

            //        _xover++;
            //    }
            //    else
            //    {
            //        _offsprings.Add(parent1);
            //        _offsprings.Add(parent2);
            //    }
            //}

            for (int i = _eliteCount; i < _xoverCount; i += 1)
            {
                IChromosome parent1 = _population[i].Clone();
                if (Chromosome.BaseChromosome._random.NextDouble() < _XoverRate)
                {
                    IChromosome parent2 = _parents[i - _eliteCount].Clone();
                    IChromosome children1 = parent1.Clone();

                    children1.Crossover(parent2);
                    _offsprings.Add(children1);

                    _xover++;
                }
                else
                {
                    _offsprings.Add(parent1);
                }
            }

            int N = _offsprings.Count;
            for (int i = _eliteCount; i < N; i++)
            {
                IChromosome parent = _offsprings[i].Clone();
                parent.Mutation();
                parent.Evaluate();
                if (parent._Fitness < _fitnessBest)
                {
                    //TO DO
                }
                _offsprings[i] = parent;
            }

            //Add parents to new population
            //_offsprings.AddRange(_parents.ToArray<IChromosome>());

            //Conservation
            List<int> market = new List<int>();
            for (int i = 0; i < _spices.Count; i++)
            {
                if (S[i].Count > 0)
                {
                    int worst = S[i][0];
                    for (int j = 0; j < S[i].Count; j++)
                    {
                        int index = S[i][j];
                        if (_offsprings[index]._Fitness > _offsprings[worst]._Fitness)
                        {
                            worst = index;
                        }
                    }

                    if (_offsprings[worst]._Fitness > _spices[i]._Fitness)
                    {
                        _offsprings[worst] = _spices[i].Clone();
                        market.Add(worst);
                    }
                }
                else
                {
                    for (int j = _offsprings.Count - 1; j > 0; j--)
                    {
                        bool have = false;
                        for (int k = 0; k < market.Count; k++)
                        {
                            if (market[k] == j)
                            {
                                have = true;
                                break;
                            }
                        }

                        if (!have)
                        {
                            _offsprings[j] = _spices[i].Clone();
                            market.Add(j);
                            break;
                        }
                    }
                }
            }

            //sorting
            _offsprings.Sort();

            //Replace
            _fitnessSum = 0;
            for (int i = 0; i < _size; ++i)
            {
                _population[i] = _offsprings[i];
                _population[i]._Generation = _generation;

                //Update fitness function
                _fitnessSum += _population[i]._Fitness;
            } // for each generation

            //Update fitness
            _fitnessMax = _population[_size - 1]._Fitness;
            _fitnessAvg = _fitnessSum / _size;
            double best = _population[0]._Fitness;
            if (best < _fitnessBest)
            {
                IChromosome bestChromosome = _population[0].Clone();
                _bestChromosome = bestChromosome;
                _fitnessBest = bestChromosome._Fitness;
                _stall = 0;

                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString()
                    + " | Seeds = " + _spices.Count.ToString();
                Console.WriteLine(s);
            }

            _generation++;
            _stall++;
            _learning.Add(_fitnessBest);

            bool exit =
                (_generation > _generationsMax)
                || (_fitnessBest < 1);
            //|| (_fitnessBest == _fitnessAvg);

            if (exit)
            {
                string s =
                    "Generation = " + _generation.ToString()
                    + " | Avg = " + _fitnessAvg.ToString()
                    + " | Max = " + _fitnessMax.ToString()
                    + " | Best = " + best.ToString();
                Console.WriteLine(s);
            }

            return exit;
        }

        #endregion

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < this._population.Length; ++i)
            {
                s += i + ": " + this._population[i].ToString() + Environment.NewLine;
            }
            return s;
        }
    }
}
