using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Naive
{
    public class NaivePopulation
    {
        public Function _function;
        public IProblem _obctiveFunction;

        static Random _random; // Solve() has random features

        //algorithm parameneters
        public eSelectionType _selectionType = eSelectionType.Tournament;
        public eCrossoverType _crossoverType = eCrossoverType.Uniform;

        //population parameters
        public Individual[] _population;
        int _size = 50;
        double _crossoverRate = 0.8; //part for corssover
        double _mutationRate = 0.01; // mutation chance   
        double _precision = 0.005;//     
        double _eliteRate = 0.05; //part of elite without changing to next generation
        double _beta = 2; //

        //fitness parameters
        private double _fitnessMax = 0;
        private double _fitnessSum = 0;
        private double _fitnessAvg = 0;
        private double _fitnessBest = 0;
        private double[] _bestChromosome = null;

        public List<double> _learning = new List<double>();

        int _stall = 0;
        int _maxStall = 50;

        public NaivePopulation(IProblem objective, int size)
        {
            _obctiveFunction = objective;
            _function = _obctiveFunction._ObjectiveFunction;

            _random = new Random(0);

            Init();
        }

        public NaivePopulation(IProblem objective, int size, double mutationRate, double precision, double crossoverRate, double eliteRate)
        {
            _obctiveFunction = objective;
            _function = _obctiveFunction._ObjectiveFunction;

            _random = new Random(0);

            _size = size; if (_size < 4) _size = 4;
            _mutationRate = mutationRate;
            _precision = precision;
            _crossoverRate = crossoverRate;
            _eliteRate = eliteRate;

            Init();
        }

        public void Init()
        {
            _population = new Individual[_size];
            for (int i = 0; i < _size; ++i)
            {
                _population[i] = new Individual(
                    _obctiveFunction,
                    _obctiveFunction._LB,
                    _obctiveFunction._UB,
                    _mutationRate,
                    _precision);

                _population[i].Evaluate();
            }
            Array.Sort(_population);
        }

        public double[] Solve(int maxGeneration)
        {
            _fitnessBest = this._population[0]._fitness;
            _bestChromosome = new double[_population[0]._chromosome.Length];
            _population[0]._chromosome.CopyTo(_bestChromosome, 0);

            int eliteCount = (int)(_size * _eliteRate);
            if (eliteCount < 1) eliteCount = 1;

            int crossoverCount = (int)((_size - eliteCount) * _crossoverRate);
            if (crossoverCount < 2) crossoverCount = 2;

            int mutantCount = _size - eliteCount - crossoverCount;

            int generation = 0;
            while (generation < maxGeneration)
            {
                Individual[] population = new Individual[_population.Length];
                _population.CopyTo(population, 0);

                //Reproduce
                Individual[] elite = new Individual[eliteCount];
                Individual[] parents = new Individual[2 * crossoverCount + mutantCount];
                Individual[] children = new Individual[crossoverCount];

                //Elite selection
                elite = EliteSelection(population, eliteCount);

                //Selection 
                if (_selectionType == eSelectionType.Tournament)
                {
                    parents = TournamentSelection(population, 2 * crossoverCount + mutantCount);
                }
                if (_selectionType == eSelectionType.RouletteWheel)
                {
                    parents = RouletteWheelSelectionExponential(population, 2 * crossoverCount + mutantCount);
                }

                //Crossover
                if (_crossoverType == eCrossoverType.Uniform)
                {
                    children = CrossoverScaterred(parents, crossoverCount);
                }
                if (_crossoverType == eCrossoverType.OnePoint)
                {
                    children = CrossoverSinglePoit(parents, crossoverCount);
                }
                if (_crossoverType == eCrossoverType.ArithmeticUniform)
                {
                    children = CrossoverHeusitic(parents, crossoverCount);
                }


                //Update
                for (int i = 0; i < _size; ++i)
                {
                    if (i < eliteCount)
                    {
                        _population[i] = elite[i];
                    }
                    else if (i < eliteCount + crossoverCount)
                    {
                        _population[i] = children[i - eliteCount];
                    }
                    else
                    {
                        _population[i] = parents[i - eliteCount];
                        _population[i].MutateGaussian();//.MutateUniform();//.MutateGaussian();//.MutateUniformAdder();//.MutateUniform();//.MutateGaussian();
                    }

                    //Update fitness function
                    _population[i].Evaluate();
                    if (_population[i]._fitness < _fitnessBest)
                    {
                        _fitnessBest = _population[i]._fitness;
                        _population[i]._chromosome.CopyTo(_bestChromosome, 0);
                        _stall = 0;
                    }
                }

                //sorting population
                Array.Sort(_population);
                _learning.Add(_fitnessBest);
                ++generation;

                _stall++;
                if (_stall > _maxStall)
                    break;
            }
            return _bestChromosome;
        }
        public Individual[] EliteSelection(Individual[] population, int eliteCount)
        {
            //Array.Sort(population);
            Individual[] newPopulation = new Individual[eliteCount];

            for (int j = 0; j < eliteCount; j++)
            {
                newPopulation[j] = population[j];
            }

            return newPopulation;
        }

        [Obsolete("Создать спцальный класс")]
        public Individual[] TournamentSelection(Individual[] population, int crossoverCount, int tournamentSize = 4)
        {
            Individual[] result = new Individual[crossoverCount];
            for (int i = 0; i < crossoverCount; i++)
            {
                int index = _random.Next(0, population.Length);
                Individual winner = population[index];
                for (int j = 1; j < tournamentSize; j++)
                {
                    index = _random.Next(0, population.Length);
                    Individual opponent = population[index];
                    if (opponent._fitness < winner._fitness)
                    {
                        winner = opponent;
                    }
                }
                result[i] = winner;
            }
            return result;
        }

        [Obsolete("Создать специальный класс")]
        public Individual[] RouletteWheelSelectionExponential(Individual[] population, int crossoverCount)
        {
            // new population, initially empty
            Individual[] newPopulation = new Individual[crossoverCount];

            double max = 0;
            for (int i = 0; i < population.Length; i++)
            {
                if (population[i]._fitness > max)
                    max = population[i]._fitness;
            }
            if (max < 1 / _beta) max = 1 / _beta;

            double[] probability = new double[population.Length];
            double s = 0;
            double sum = 0;
            int k = 0;
            foreach (Individual individual in population)
            {
                // cumulative normalized fitness
                s = Math.Exp(-individual._fitness * _beta / max);
                probability[k++] = s;
                sum += s;
            }

            for (int i = 0; i < probability.Length; i++)
            {
                probability[i] /= sum;
            }

            double cumsum = 0;
            for (int i = 0; i < probability.Length; i++)
            {
                cumsum += probability[i];
                probability[i] = cumsum;
            }

            // select chromosomes from old population to the new population
            int[] popadanija = new int[population.Length];
            for (int j = 0; j < crossoverCount; j++)
            {
                // get wheel value
                double wheelValue = _random.NextDouble();
                // find the chromosome for the wheel value
                for (int i = 0; i < population.Length; i++)
                {
                    if (wheelValue <= probability[i])
                    {
                        // add the chromosome to the new population
                        newPopulation[j] = population[i];
                        popadanija[i] += 1;
                        break;
                    }
                }
            }

            return newPopulation;
        }

        [Obsolete("Создать специальный класс")]
        public Individual[] RouletteWheelSelection(Individual[] population, int crossoverCount)
        {
            // new population, initially empty
            Individual[] newPopulation = new Individual[crossoverCount];

            // calculate summary fitness of current population
            double sum = 0;
            foreach (Individual individual in population)
            {
                sum += individual._fitness;
            }

            // create wheel ranges
            double[] probability = new double[population.Length];
            double s = 0;
            int k = 0;
            foreach (Individual individual in population)
            {
                // cumulative normalized fitness
                s += (individual._fitness / sum);
                probability[k++] = s;
            }

            // select chromosomes from old population to the new population
            int[] popadanija = new int[population.Length];
            for (int j = 0; j < crossoverCount; j++)
            {
                // get wheel value
                double wheelValue = _random.NextDouble();
                // find the chromosome for the wheel value
                for (int i = 0; i < population.Length; i++)
                {
                    if (wheelValue <= probability[i])
                    {
                        // add the chromosome to the new population
                        newPopulation[j] = population[i];
                        popadanija[i] += 1;
                        break;
                    }
                }
            }

            return newPopulation;
        }

        [Obsolete("Создать специальный класс")]
        public Individual[] CrossoverScaterred(Individual[] parents, int count)
        {
            Individual[] result = new Individual[count];
            int numGenes = parents[0]._chromosome.Length;
            int index = 0;
            for (int i = 0; i < count * 2; i += 2)
            {
                Individual parent1 = parents[i];
                Individual parent2 = parents[i + 1];

                Individual child = new Individual(
                    _obctiveFunction,
                    _obctiveFunction._LB,
                    _obctiveFunction._UB,
                    _mutationRate,
                    _precision); // random chromosome

                for (int k = 0; k < numGenes; k++)
                {
                    if (_random.NextDouble() < 0.5)
                    {
                        child._chromosome[k] = parent1._chromosome[k];
                    }
                    else
                    {
                        child._chromosome[k] = parent2._chromosome[k];
                    }
                }
                result[index] = child;
                index++;
            }
            return result;
        }

        [Obsolete("Создать специальный класс")]
        public Individual[] CrossoverSinglePoit(Individual[] parents, int count)
        {
            int cross = 0;
            Individual[] result = new Individual[count];
            int numGenes = parents[0]._chromosome.Length;
            for (int j = 0; j < count * 2; j += 2)
            {
                cross = _random.Next(0, numGenes - 1); // crossover point. 0 means 'between 0 and 1'.

                Individual parent1 = parents[j];
                Individual parent2 = parents[j + 1];
                //Individual parent2 = parents[j + 1];

                Individual child1 = new Individual(
                    _obctiveFunction,
                    _obctiveFunction._LB,
                    _obctiveFunction._UB,
                    _mutationRate,
                    _precision); // random chromosome

                for (int k = 0; k <= cross; ++k)
                {
                    child1._chromosome[k] = parent1._chromosome[k];
                }
                for (int k = cross + 1; k < numGenes; ++k)
                {
                    child1._chromosome[k] = parent2._chromosome[k];
                }
                result[j] = child1;

                //Individual child2 = new Individual(
                //    _obctiveFunction,
                //    _obctiveFunction._Min,
                //    _obctiveFunction._Max,
                //    _mutationRate,
                //    _precision); // random chromosome

                //for (int i = cross + 1; i < numGenes; ++i)
                //{
                //    child2._chromosome[i] = parent1._chromosome[i];
                //}
                //for (int i = 0; i <= cross; ++i)
                //{
                //    child2._chromosome[i] = parent2._chromosome[i];
                //}
                //result[j+1] = child2;
            }

            return result;
        }

        [Obsolete("Создать специальный класс")]
        public Individual[] CrossoverHeusitic(Individual[] parents, int count)
        {
            Individual[] children = new Individual[count];
            double r = .8;
            int index = 0;
            for (int i = 0; i < 2 * count; i += 2)
            {
                Individual parent1 = parents[i];
                Individual parent2 = parents[i + 1];

                Individual child = new Individual(
                    _obctiveFunction,
                    _obctiveFunction._LB,
                    _obctiveFunction._UB,
                    _mutationRate,
                    _precision); // random chromosome

                for (int j = 0; j < child._chromosome.Length; j++)
                {
                    child._chromosome[j] =
                        parent2._chromosome[j]
                        + r * (parent1._chromosome[j] - parent2._chromosome[j]); 
                }
                children[index] = child;
                index++;
            }
            return children;
        }

        private int[] ShuffleIndexes(int N)
        {
            int[] indexes = new int[N];
            for (int i = 0; i < N; i++)
            {
                indexes[i] = i;
            }

            for (int i = 0; i < indexes.Length; ++i)
            {
                int r = _random.Next(i, indexes.Length);
                int tmp = indexes[r];
                indexes[r] = indexes[i];
                indexes[i] = tmp;
            }

            return indexes;
        }

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
