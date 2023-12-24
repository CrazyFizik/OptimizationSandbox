using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Chromosome
{
    public class ChromosomeDoubleMatrix : IChromosome
    {
        public Function _function;
        public IProblem _obctiveFunction;

        static Random _random = new Random();

        private eCrossoverType _crossoverType = eCrossoverType.Intermediate;
        private eMutationType _mutationType = eMutationType.Muhleblein;

        private double _fitness;
        public double[][] _chromosome;

        private int _rows;
        private int _columns;

        private int _generationsMax = 1;
        private int _generationCount = 0;
        private int _mutationCount = 0;
        private int _crossoverCount = 0;

        private double[] _min;
        private double[] _max;

        //mutation parameters
        private double _mutationRate = 0.01;//0.01;      //for uniform multiple mutation
        private double _mutationBalancer = 0.5;         //
        private double _crossoverBalancer = 0.5;        //

        public double this[int i, int j]
        {
            get
            {
                return _chromosome[i][j];
            }
            set
            {
                _chromosome[i][j] = value;
            }
        }

        #region Interface properties implement

        public eCrossoverType _CrossoverType
        {
            get
            {
                return _crossoverType;
            }

            set
            {
                _crossoverType = value;
            }
        }

        public eMutationType _MutationType
        {
            get
            {
                return _mutationType;
            }

            set
            {
                _mutationType = value;
            }
        }

        public int _Length
        {
            get
            {
                return _rows;
            }
        }

        public double _Fitness
        {
            get
            {
                return _fitness;
            }
        }

        public int _Generation
        {
            get
            {
                return _generationCount;
            }

            set
            {
                _generationCount = value;
            }
        }

        public int _GenerationMax
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

        public Random _Random
        {
            get
            {
                return _random;
            }
        }

        #endregion

        #region COnstructors

        public ChromosomeDoubleMatrix(IProblem objective)
        {
            _obctiveFunction = objective;
            _function = objective._ObjectiveFunction;

            _max = objective._UB;
            _min = objective._LB;
            _rows = objective._Length;
            _generationsMax = 100 * _rows; // !!!!!!

            _chromosome = new double[_rows][];
            for (int i = 0; i < _rows; i++)
            {
                _chromosome[i] = new double[_columns];
            }

            Generate();
        }

        public ChromosomeDoubleMatrix(ChromosomeDoubleMatrix source) :
            this(source._obctiveFunction)
        {
            source._chromosome.CopyTo(this._chromosome, 0);
            this._fitness = source._fitness;

            this._function = source._function;
            this._obctiveFunction = source._obctiveFunction;

            this._mutationType = source._mutationType;
            this._crossoverType = source._crossoverType;

            this._generationCount = source._generationCount;
            this._generationsMax = source._generationsMax;
            this._crossoverCount = source._crossoverCount;
            this._mutationCount = source._mutationCount;

            this._mutationBalancer = source._mutationBalancer;
            this._crossoverBalancer = source._crossoverBalancer;
            this._mutationRate = source._mutationRate;
        }

        public void Generate()
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _chromosome[i][j] = _random.NextDouble() * (_max[j] - _min[j]) + _min[j];
                }
            }
        }

        #endregion

        #region Mutation

        /// <summary>
        /// This mutation operator replaces the genome with either lower or upper bound randomly. 
        /// This can be used for integer and float genes.
        /// <remarks>Michalewicz, 1992</remarks>
        /// </summary>
        void MutationBoundary()
        {
            throw new Exception();
        }

        /// <summary>
        /// Uniform mutatiom (UM)
        /// </summary>
        /// <remarks>This operator replaces the value of the chosen gene with a uniform random value 
        /// selected between the user-specified upper and lower bounds for that gene. 
        /// This mutation operator can only be used for integer and float genes.
        /// 
        /// Michalewicz, 1992</remarks>
        void MutationFixedUniform()
        {
            int index = _random.Next(0, _columns);

            for (int i = 0; i < _rows; i++)
            {
                _chromosome[i][index] =
                    _random.NextDouble()
                    * (_max[index] - _min[index])
                    + _min[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void MutationUniform()
        {
            ////set numbers of genes, which will mutated
            //int genNum = (int)Math.Ceiling(_mutationRate * _size);

            ////set real probabylity
            //double probability = (double)genNum / _size;


            for (int j = 0; j < _columns; j++)
            {
                if (_random.NextDouble() < _mutationRate)
                {
                    for (int i = 0; i < _rows; i++)
                    {
                        _chromosome[i][j] =
                        _random.NextDouble()
                        * (_max[j] - _min[j])
                        + _min[j];
                    }
                }
            }
        }

        /// <summary>
        /// None-uniform mutation (NUM).
        /// One of the commonly used mutation self-daptation operators in real coded GAs.
        /// </summary>
        /// <remarks> The probability that amount of mutation will go to 0 with the next generation is increased by using non-uniform mutation operator. 
        /// It keeps the population from stagnating in the early stages of the evolution. 
        /// It tunes solution in later stages of evolution. 
        /// This mutation operator can only be used for integer and float genes.
        /// 
        /// Michalewicz, 1992</remarks>
        void MutationNonUniform()
        {
            int t = _generationCount; // current generation, need acces to population later
            int T = _generationsMax;

            double delta = 0;
            double r = 0;
            double y = 0;
            double tau = 0;
            double index = 0;
            double b = 5;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_random.NextDouble() < _mutationRate)
                    {
                        //select y
                        if (_random.NextDouble() < 0.5)
                        {
                            y = _max[j] - _chromosome[i][j];
                        }
                        else
                        {
                            y = -(_chromosome[i][j] - _min[j]);
                        }

                        // generate exponentas
                        r = _random.NextDouble();
                        tau = 1.0 - ((double)t / T);
                        index = Math.Pow(tau, b);

                        // generate delta
                        delta = y * (1.0 - Math.Pow(r, index));

                        //apply
                        _chromosome[i][j] =
                            _chromosome[i][j] + delta;

                        //Check limits
                        if (_chromosome[i][j] > _max[j])
                        {
                            _chromosome[i][j] = _max[j];
                        }

                        if (_chromosome[i][j] < _min[j])
                        {
                            _chromosome[i][j] = _min[j];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Power Mutation (PM)
        /// </summary>
        /// <remarks> In Deep and Thakur (2007b) </remarks>
        void MutationPower()
        {
            throw new Exception();
        }

        /// <summary>
        /// This operator adds a unit Gaussian distributed random value to the chosen gene. 
        /// If it falls outside of the user-specified lower or upper bounds for that gene, 
        /// the new gene value is clipped. 
        /// This mutation operator can only be used for integer and float genes.
        /// </summary>
        void MutationGaussian()
        {
            for (int i = 0; i < _chromosome.Length; i++)
            {
                for (int j = 0; j < _chromosome[i].Length; j++)
                {
                    if (_random.NextDouble() < _mutationRate)
                    {
                        //Box-Muller Transformation
                        double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
                        double u2 = 1.0 - _random.NextDouble();
                        double randStdNormal =
                            Math.Sqrt(-2.0 * Math.Log(u1))
                            * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)      
                        double stddev = _mutationBalancer;
                        double mean = 0;
                        double randomn = randStdNormal * stddev + mean;
                        double delta = 0.1 * (_max[j] - _min[j]) * randomn;

                        //Add gaussian
                        _chromosome[i][j] += delta;

                        //Check limits
                        if (_chromosome[i][j] > _max[j])
                        {
                            _chromosome[i][j] = _max[j];
                        }

                        if (_chromosome[i][j] < _min[j])
                        {
                            _chromosome[i][j] = _min[j];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This operator adds a random number taken from a Gaussian distribution 
        /// with mean equal to the original value of each decision variable characterizing the entry parent vector
        /// </summary>
        void MutationShrink()
        {
            throw new Exception();
        }

        /// <summary>
        /// 
        /// </summary>
        void MutationHeuristic()
        {
            throw new Exception();
        }

        /// <summary>
        /// The most effective stohoastic mutation operator.
        /// </summary>
        /// <remarks>
        /// Muhlenbein et al., 1993</remarks>
        void MutationMuhlenbein() // 
        {          
            double delta = 0;           // muttaion delta
            const int K = 15;           // fraction precision
            double p = 1.0 / (K + 1);   // fraction probability

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    double range = 0.1 * (_max[j] - _min[j]);
                    if (_random.NextDouble() < _mutationRate)
                    {
                        // set delta
                        for (int k = 0; k < K; k++)
                        {
                            if (_random.NextDouble() < p)
                            {
                                delta =
                                    delta
                                    + Math.Pow(2, -k);
                            }
                        }

                        // set sign
                        if (_random.NextDouble() < 0.5)
                        {
                            delta = -delta;
                        }

                        //aply mutation
                        _chromosome[i][j] =
                            this._chromosome[i][j]
                            + delta;

                        //Check limits
                        if (_chromosome[i][j] > _max[j])
                        {
                            _chromosome[i][j] = _max[j];
                        }

                        if (_chromosome[i][j] < _min[j])
                        {
                            _chromosome[i][j] = _min[j];
                        }
                    }
                }
            }
        }
        #endregion

        #region Crossover

        /// <summary>
        /// Classic binary crossover operator
        /// </summary>
        /// <remarks>
        /// 1. Selects vector entries numbered less than or equal to n from the first parent.
        /// 2. Selects vector entries numbered greater than n from the second parent.
        /// 3. Concatenates these entries to form a child vector
        /// 
        /// Holland, 1975; Goldberg, 1989</remarks>
        /// <param name="pair"></param>
        void CrossoverSinglePoint(ChromosomeDoubleMatrix pair)
        {
            int cross = _random.Next(0, _rows - 1); // crossover point. 0 means 'between 0 and 1'.

            for (int i = 0; i < _chromosome.Length; i++)
            {
                for (int j = 0; j <= cross; j++)
                {
                    this._chromosome[i][j] = pair._chromosome[i][j];
                }
            } // for each gene
        }

        /// <summary>
        /// Generate random number beetween c1 and c2
        /// 
        /// </summary>
        /// <remarks>Radcliffe, 1991</remarks>
        /// <param name="pair"></param>
        void CrossoverFlat(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_random.NextDouble() < 0.5)
                    {
                        this._chromosome[i][j] =
                            (pair._chromosome[i][j] - this._chromosome[i][j]) * _random.NextDouble()
                            + this._chromosome[i][j];
                    }
                }
            } //for each gene
        }

        /// <summary>
        /// Discreate crossover/ Scaterred crossover: 
        /// swap genes between parents by random mask 
        /// </summary>
        /// <remarks>
        /// The default crossover function for problems without linear constraints, 
        /// creates a random binary vector and selects the genes where the vector is a 1 from the first parent, 
        /// and the genes where the vector is a 0 from the second parent, and combines the genes to form the child.
        /// 
        /// Michalewicz, 1992</remarks>
        /// <param name="pair"></param>
        void CrossoverUniform(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    {
                        if (_random.NextDouble() < 0.5)
                        {
                            this._chromosome[i][j] = pair._chromosome[i][j];
                        }
                    }
                }
            } //for each gene
        }

        /// <summary>
        /// Simple arithmetic crossover method (AMXO) 
        /// 
        /// Michalewicz, 1992
        /// </summary>
        /// <param name="pair"></param>
        void CrossoverArithmeticUniform(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < this._chromosome.Length; i++)
            {
                for (int j = 0; j < this._chromosome[i].Length; j++)
                {
                    this._chromosome[i][j] =
                        pair._chromosome[i][j]
                        + _crossoverBalancer * (this._chromosome[i][j] - pair._chromosome[i][j]);
                }
            } // for each gene
        }

        /// <summary>
        /// Arithmetic with random crossover balancer (NAMXO) 
        /// 
        /// </summary>
        /// <remarks>Michalewicz, 1992</remarks>
        /// <param name="pair"></param>
        void CrossoverArithmeticNonUniform(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < this._chromosome.Length; i++)
            {
                for (int j = 0; j < this._chromosome[i].Length; j++)
                {
                    this._chromosome[i][j] =
                        pair._chromosome[i][j]
                        + _crossoverBalancer
                        * _random.NextDouble()
                        * (this._chromosome[i][j] - pair._chromosome[i][j]);
                }
            } // for each gene
        }

        /// <summary>
        /// Heuristic crossover (HX) has been applied to solve nonlinear constrained optimization problems
        /// </summary>
        /// <remarks> 
        /// Returns a child that lies on the line containing the two parents, 
        /// a small distance away from the parent with the better fitness value in the direction away from the parent with the worse fitness value. 
        /// If parent1 and parent2 are the parents, and parent1 has the better fitness value, the function returns the child:
        /// child = parent2 + random[0,1] * (parent1 - parent2);
        /// 
        /// Wright, 1990 </remarks>
        /// <param name="pair"></param>
        void CrossoverHeuristic(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < _rows; i++)
            {
                double[] c1;
                double[] c2;

                // slect c1 - best parent
                if (this._fitness < pair._fitness)
                {
                    c1 = this._chromosome[i];
                    c2 = pair._chromosome[i];
                }
                else
                {
                    c1 = pair._chromosome[i];
                    c2 = this._chromosome[i];
                }

                //double r = _random.NextDouble();
                for (int j = 0; j < _columns; j++)
                {
                    this._chromosome[i][j] =
                        c1[i]
                        + _random.NextDouble() * (c1[i] - c2[i]);
                } // for each gene
            }
        }

        /// <summary>
        /// The most effective crossover operator.
        /// </summary>
        /// <remarks>
        /// The default crossover function when there are linear constraints, 
        /// The function creates the child from parent1 and parent2 using the following formula:
        /// child = parent1 + rand * Ratio * ( parent2 - parent1)
        /// 
        /// Optimal Ratio from -0.25 to 1.25.
        /// 
        /// Muhlenbein et al., 1993</remarks>
        /// <param name="pair"></param>
        void CrossoverExtendedIntermediate(ChromosomeDoubleMatrix pair)
        {
            double b = (_crossoverBalancer / 2.0) + 1.0;
            double a = -(_crossoverBalancer / 2.0);
            double alpha = 0.0;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    alpha = //best from -0.25 to 1.25
                        (b - a) * _random.NextDouble()
                        + a;

                    this._chromosome[i][j] =
                        this._chromosome[i][j]
                        + alpha * (pair._chromosome[i][j] - this._chromosome[i][j]);

                    //Check limits
                    if (_chromosome[i][j] > _max[j])
                    {
                        _chromosome[i][j] = _max[j];
                    }

                    if (_chromosome[i][j] < _min[j])
                    {
                        _chromosome[i][j] = _min[j];
                    }
                }
            } //for each gene
        }

        /// <summary>
        /// Scalar version of Intermediate operator.
        /// </summary>
        /// <remarks>The random number:
        /// rand * Ratio
        /// set for the whole chromosome.
        /// 
        /// Muhlenbein et al., 1993</remarks>
        /// <param name="pair">Wife</param>
        void CrossoverExtendedLine(ChromosomeDoubleMatrix pair)
        {
            double b = _crossoverBalancer / 2.0 + 1.0;
            double a = -_crossoverBalancer / 2.0;
            double alpha = //best from -0.25 to 1.25
                    (b - a) * _random.NextDouble()
                    + a;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    this._chromosome[i][j] =
                        this._chromosome[i][j]
                        + alpha * (pair._chromosome[i][j] - this._chromosome[i][j]);
                }
            } //for each gene
        }

        /// <summary>
        /// Very effective stohoastic crossover operator.
        /// </summary>
        /// <remarks>
        /// If corssover balncer equal 0.25 this is method similary to Flat Crossover.
        /// Best value of crossover balancer is 0.5
        /// 
        /// Eshelman et al., 1993
        /// </remarks>
        /// <param name="pair"></param>
        void CrossoverBLX_alpha(ChromosomeDoubleMatrix pair)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    double cmax = Math.Max(this._chromosome[i][j], pair._chromosome[i][j]);
                    double cmin = Math.Min(this._chromosome[i][j], pair._chromosome[i][j]);
                    double I = cmax - cmin;

                    double b = cmax + I * _crossoverBalancer; // best is 0.5
                    double a = cmin - I * _crossoverBalancer; // best is 0.5

                    if (b > _max[j]) b = _max[j];
                    if (a < _min[j]) a = _min[j];

                    double r = 0.0;

                    r = _random.NextDouble() * (b - a) + a;
                    this._chromosome[i][j] = r;
                }
            } //for each gene
        }

        #endregion

        #region Interfae method implementation

        public void Mutation()
        {
            _mutationCount++;

            switch (_mutationType)
            {
                case eMutationType.Gaussian:
                    MutationGaussian();
                    break;

                case eMutationType.FixedUniform:
                    MutationFixedUniform();
                    break;

                case eMutationType.Uniform:
                    MutationUniform();
                    break;

                case eMutationType.Heuristic:
                    MutationHeuristic();
                    break;

                case eMutationType.Muhleblein:
                    MutationMuhlenbein();
                    break;

                case eMutationType.NonUniform:
                    MutationNonUniform();
                    break;
            }// select mutation

        }

        public void Crossover(IChromosome pair)
        {
            ChromosomeDoubleMatrix parent = (ChromosomeDoubleMatrix)pair;
            _crossoverCount++;

            switch (_crossoverType)
            {
                case eCrossoverType.OnePoint:
                    CrossoverSinglePoint(parent);
                    break;

                case eCrossoverType.Flat:
                    CrossoverFlat(parent);
                    break;

                case eCrossoverType.Uniform:
                    CrossoverUniform(parent);
                    break;

                case eCrossoverType.ArithmeticUniform:
                    CrossoverArithmeticUniform(parent);
                    break;

                case eCrossoverType.ArithmeticNonUniform:
                    CrossoverArithmeticNonUniform(parent);
                    break;

                case eCrossoverType.Heuristic:
                    CrossoverHeuristic(parent);
                    break;

                case eCrossoverType.Intermediate:
                    CrossoverExtendedIntermediate(parent);
                    break;

                case eCrossoverType.Line:
                    CrossoverExtendedLine(parent);
                    break;

                case eCrossoverType.BLX_alpha:
                    CrossoverBLX_alpha(parent);
                    break;
            } //select mutation
        }

        public IChromosome Clone()
        {
            return new ChromosomeDoubleMatrix(this);
        }

        public IChromosome CreateNew()
        {
            ChromosomeDoubleMatrix c = new ChromosomeDoubleMatrix(_obctiveFunction);
            c._MutationType = this._mutationType;
            c._crossoverType = this._crossoverType;
            c._mutationRate = this._mutationRate;
            c._mutationBalancer = this._mutationBalancer;
            c._crossoverBalancer = this._crossoverBalancer;
            c._generationsMax = this._GenerationMax;
            return c;
        }

        public double Distance(IChromosome pair)
        {
            ChromosomeDoubleMatrix other = (ChromosomeDoubleMatrix)pair;
            double result = 0;
            double l = 0;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    l = (this._chromosome[i][j] - other._chromosome[i][j]) * (this._chromosome[i][j] - other._chromosome[i][j]);
                    result = result + l;
                }
            }
            result = Math.Sqrt(result);
            return result;
        }

        public void Evaluate()
        {
            //_fitness = _function(_chromosome);
        }

        public int CompareTo(IChromosome other)
        {
            if (this._fitness < other._Fitness)
            {
                return -1;
            }
            else if (this._fitness > other._Fitness)
            {
                return 1;
            }
            else return 0;
        }
        #endregion

        public override string ToString()
        {
            string s = "";

            // genetic operators information
            s += "g: " + _generationCount
                + " m: " + _mutationCount
                + " c: " + _crossoverCount
                + " |";

            //genotype
            for (int i = 0; i < _chromosome.Length; ++i)
            {
                for (int j = 0; j < _chromosome[i].Length; j++)
                {
                    s += _chromosome[i][j].ToString("F2") + " ";
                }
            }

            // fitness
            if (this._fitness == double.MaxValue)
            {
                s += "| fitness = maxValue";
            }
            else
            {
                s += "| fitness = " + this._fitness.ToString("F4");
            }


            return s;
        }

        public void Mutation(IChromosome attraction)
        {
            throw new NotImplementedException();
        }
    }
}
