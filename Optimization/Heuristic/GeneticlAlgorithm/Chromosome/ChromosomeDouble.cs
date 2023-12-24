using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;

namespace Optimization.Heuristic.GeneticlAlgorithm.Chromosome
{
    public class ChromosomeDouble : BaseChromosome, IChromosome
    {
        public Function _function;
        public IProblem _obctiveFunction;        

        private eCrossoverType _crossoverType = eCrossoverType.Intermediate;
        private eMutationType _mutationType = eMutationType.Muhleblein;

        private double _fitness;
        public double[] _chromosome;

        private int _size;
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

        public double this[int i]
        {
            get
            {
                return _chromosome[i];
            }
            set
            {
                _chromosome[i] = value;
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
                return _size;
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

        public ChromosomeDouble(IProblem objective)
        {
            _obctiveFunction = objective;
            _function = objective._ObjectiveFunction;

            _max = objective._UB;
            _min = objective._LB;
            _size = objective._Length;
            _generationsMax = 100 * _size; // !!!!!!

            _chromosome = new double[objective._Length];

            Generate();
        }

        public ChromosomeDouble(ChromosomeDouble source) :
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
            for (int i = 0; i < _size; i++)
            {
                _chromosome[i] = _random.NextDouble() * (_max[i] - _min[i]) + _min[i];
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
            int index = _random.Next(0, _size);
            _chromosome[index] =
                _random.NextDouble()
                * (_max[index] - _min[index])
                + _min[index];
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

            for (int i = 0; i < _size; i++)
            {
                if (_random.NextDouble() < _mutationRate)
                {
                    int N = 24;
                    int n = _random.Next(0, N);
                    double r = (double)n / N;

                    _chromosome[i] =
                        r
                        * (_max[i] - _min[i])
                        + _min[i];
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

            for (int i = 0; i < _size; i++)
            {
                if (_random.NextDouble() < _mutationRate)
                {
                    //select y
                    if (_random.NextDouble() < 0.5)
                    {
                        y = _max[i] - _chromosome[i];
                    }
                    else
                    {
                        y = -(_chromosome[i] - _min[i]);
                    }

                    // generate exponentas
                    r = _random.NextDouble();
                    tau = 1.0 - ((double)t / T);
                    index = Math.Pow(tau, b);

                    // generate delta
                    delta = y * (1.0 - Math.Pow(r, index));

                    //apply
                    _chromosome[i] =
                        _chromosome[i] + delta;

                    //Check limits
                    _chromosome[i] = CircleClamp(_chromosome[i], _min[i], _max[i]);
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
                if (_random.NextDouble() < _mutationRate)
                {
                    //Box-Muller Transformation
                    double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
                    double u2 = 1.0 - _random.NextDouble();
                    double randStdNormal =
                        Math.Sqrt(-2.0 * Math.Log(u1))
                        * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)      
                    double stddev = 1.0;// _mutationBalancer;
                    double mean = 0;
                    double randomn = randStdNormal * stddev + mean;
                    double delta = 0.1 * (_max[i] - _min[i]) * randomn;

                    //Add gaussian
                    _chromosome[i] += delta;

                    //Check limits
                    _chromosome[i] = CircleClamp(_chromosome[i], _min[i], _max[i]);
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
            
            for (int i = 0; i < _size; i++)
            {
                double range = 0.1 * (_max[i] - _min[i]);

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
                    _chromosome[i] =
                        this._chromosome[i]
                        + delta;

                    //Check limits
                    //Check limits
                    _chromosome[i] = CircleClamp(_chromosome[i], _min[i], _max[i]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void MutationPolinominal()
        {
            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;
            double distributionIndex = 20;

            double[] x = this._chromosome;
            for (int i = 0; i < x.Length; i++)
            {
                if (_random.NextDouble() <= _mutationRate)
                {
                    y = x[i];
                    yl = _min[i];// x.GetLowerBound(i);
                    yu = _max[i];// x.GetUpperBound(i);
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = _random.NextDouble();
                    mut_pow = 1.0 / (distributionIndex + 1.0);
                    if (rnd <= 0.5)
                    {
                        xy = 1.0 - delta1;
                        val = 2.0 * rnd + (1.0 - 2.0 * rnd) * (Math.Pow(xy, (distributionIndex + 1.0)));
                        deltaq = Math.Pow(val, mut_pow) - 1.0;
                    }
                    else
                    {
                        xy = 1.0 - delta2;
                        val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * (Math.Pow(xy, (distributionIndex + 1.0)));
                        deltaq = 1.0 - (Math.Pow(val, mut_pow));
                    }
                    y = y + deltaq * (yu - yl);
                    y = CircleClamp(y, yl, yu);

                    x.SetValue(y, i);
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
        void CrossoverSinglePoint(ChromosomeDouble pair) 
        {
            int cross = _random.Next(0, _size - 1); // crossover point. 0 means 'between 0 and 1'.

            for (int i = 0; i <= cross; i++)
            {
                this._chromosome[i] = pair._chromosome[i];
            } // for each gene
        }
        
        void CrossoverTwoPoint(ChromosomeDouble pair)
        {
            int N = this._chromosome.Length;

            double[] parent1 = new double[N];
            double[] parent2 = new double[N];

            double[] child1 = new double[N];
            double[] child2 = new double[N];

            this._chromosome.CopyTo(parent1, 0);
            pair._chromosome.CopyTo(parent2, 0);

            parent1.CopyTo(child1, 0);
            parent2.CopyTo(child2, 0);

            // STEP 1: Get two cutting points
            int crosspoint1 = _random.Next(0, _size - 1);
            int crosspoint2 = _random.Next(0, _size - 1);
            while (crosspoint1 != crosspoint2)
            {
                crosspoint2 = _random.Next(0, _size - 1);
            }
            
            if (crosspoint1 > crosspoint2)
            {
                int swap;
                swap = crosspoint1;
                crosspoint1 = crosspoint2;
                crosspoint2 = swap;
            }

            //
            // STEP 2: Obtain the first child
            for (int j = 0; j < N; j++)
            {
                for (int k = crosspoint1; k <= crosspoint2; k++)
                {
                    child1[k] = parent2[k];
                }                
            }

            // STEP 3: Obtain the second child
            for (int j = 0; j < N; j++)
            {
                for (int k = crosspoint1; k <= crosspoint2; k++)
                {
                    child2[k] = parent1[k];
                }
            }

            child1.CopyTo(parent1, 0);
        }

        /// <summary>
        /// Generate random number beetween c1 and c2
        /// </summary>
        /// <remarks>Radcliffe, 1991</remarks>
        /// <param name="pair"></param>
        void CrossoverFlat(ChromosomeDouble pair) 
        {
            for (int i = 0; i < _size; i++)
            {
                if (_random.NextDouble() < 0.5)
                {
                    this._chromosome[i] = 
                        (pair._chromosome[i] - this._chromosome[i]) * _random.NextDouble()
                        + this._chromosome[i];
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
        void CrossoverUniform(ChromosomeDouble pair) 
        {
            for (int i = 0; i < _size; i++)
            {
                if (_random.NextDouble() < 0.5)
                {
                    this._chromosome[i] = pair._chromosome[i];
                }
            } //for each gene
        }

        /// <summary>
        /// Simple arithmetic crossover method (AMXO) 
        /// 
        /// Michalewicz, 1992
        /// </summary>
        /// <param name="pair"></param>
        void CrossoverArithmeticUniform(ChromosomeDouble pair)
        {
            for (int i = 0; i < this._chromosome.Length; i++)
            {
                this._chromosome[i] =
                    pair._chromosome[i]
                    + _crossoverBalancer * (this._chromosome[i] - pair._chromosome[i]);
            } // for each gene
        }

        /// <summary>
        /// Arithmetic with random crossover balancer (NAMXO) 
        /// 
        /// </summary>
        /// <remarks>Michalewicz, 1992</remarks>
        /// <param name="pair"></param>
        void CrossoverArithmeticNonUniform(ChromosomeDouble pair)
        {
            for (int i = 0; i < this._chromosome.Length; i++)
            {
                this._chromosome[i] =
                    pair._chromosome[i]
                    + _crossoverBalancer
                    * _random.NextDouble()
                    * (this._chromosome[i] - pair._chromosome[i]);
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
        void CrossoverHeuristic(ChromosomeDouble pair)
        {
            double[] c1;
            double[] c2;

            // slect c1 - best parent
            if (this._fitness < pair._fitness)
            {
                c1 = this._chromosome;
                c2 = pair._chromosome;
            }
            else
            {
                c1 = pair._chromosome;
                c2 = this._chromosome;
            }

            //double r = _random.NextDouble();
            for (int i = 0; i < _size; i++)
            {
                this._chromosome[i] =
                    c1[i]
                    + _random.NextDouble() * (c1[i] - c2[i]);
            } // for each gene
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
        void CrossoverExtendedIntermediate(ChromosomeDouble pair)
        {
            double b = (_crossoverBalancer / 2.0) + 1.0;
            double a = -(_crossoverBalancer / 2.0);
            double alpha = 0.0;

            for (int i = 0; i < _size; i++)
            {
                alpha = //best from -0.25 to 1.25
                    (b - a) * _random.NextDouble()
                    + a;

                this._chromosome[i] =
                    this._chromosome[i]
                    + alpha * (pair._chromosome[i] - this._chromosome[i]);

                //Check limits
                _chromosome[i] = CircleClamp(_chromosome[i], _min[i], _max[i]);
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
        void CrossoverExtendedLine(ChromosomeDouble pair)
        {
            double b = _crossoverBalancer / 2.0 + 1.0;
            double a = -_crossoverBalancer / 2.0;
            double alpha = //best from -0.25 to 1.25
                    (b - a) * _random.NextDouble()
                    + a;

            for (int i = 0; i < _size; i++)
            {
                this._chromosome[i] =
                    this._chromosome[i]
                    + alpha * (pair._chromosome[i] - this._chromosome[i]);
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
        void CrossoverBLX_alpha(ChromosomeDouble pair)
        {
            for (int i = 0; i < _size; i++)
            {
                double cmax = Math.Max(this._chromosome[i], pair._chromosome[i]);
                double cmin = Math.Min(this._chromosome[i], pair._chromosome[i]);
                double I = cmax - cmin;

                double b = cmax + I * _crossoverBalancer; // best is 0.5
                double a = cmin - I * _crossoverBalancer; // best is 0.5

                if (b > _max[i]) b = _max[i];
                if (a < _min[i]) a = _min[i];

                double r = 0.0;

                r = _random.NextDouble() * (b - a) + a;
                this._chromosome[i] = r;
            } //for each gene
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        void CrossoverSBX(ChromosomeDouble pair)
        { 
            int i;
            double rand;
            double y1, y2, yL, yu;
            double c1, c2;
            double alpha, beta, betaq;
            double valueX1, valueX2;

            int N = this._chromosome.Length;
            double distributionIndex = 20;
            double EPS = 0;

            double[] x1 = new double[N];
            double[] x2 = new double[N];
            this._chromosome.CopyTo(x1, 0);
            pair._chromosome.CopyTo(x2, 0);

            double[] children1 = new double[N];
            double[] children2 = new double[N];

            if (_random.NextDouble() <= _mutationRate)
            {
                for (i = 0; i < N; i++)
                {
                    valueX1 = x1[i];
                    valueX2 = x2[i];
                    if (_random.NextDouble() <= 0.5)
                    {
                        if (Math.Abs(valueX1 - valueX2) > EPS)
                        {

                            if (valueX1 < valueX2)
                            {
                                y1 = valueX1;
                                y2 = valueX2;
                            }
                            else
                            {
                                y1 = valueX2;
                                y2 = valueX1;
                            }

                            yL = _min[i];// x1.GetLowerBound(i);
                            yu = _max[i];// x1.GetUpperBound(i);
                            rand = _random.NextDouble();
                            beta = 1.0 + (2.0 * (y1 - yL) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                            beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(distributionIndex + 1.0));

                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (distributionIndex + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (distributionIndex + 1.0)));
                            }

                            c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));

                            c1 = CircleClamp(c1, yL, yu);
                            c2 = CircleClamp(c2, yL, yu);

                            if (_random.NextDouble() <= 0.5)
                            {
                                children1.SetValue(c2,i);
                                children2.SetValue(c1, i);
                            }
                            else
                            {
                                children1.SetValue(c1, i);
                                children2.SetValue(c2, i);
                            }
                        }
                        else
                        {
                            children1.SetValue(valueX1, i);
                            children1.SetValue(valueX2, i);
                        }
                    }
                    else
                    {
                        children1.SetValue(valueX2, i);
                        children1.SetValue(valueX1, i);
                    }
                }
            }
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

                case eMutationType.Polyniminal:
                    MutationPolinominal();
                    break;
            }// select mutation

        }

        public void Crossover(IChromosome pair)
        {
            ChromosomeDouble parent = (ChromosomeDouble)pair;
            _crossoverCount++;

            switch (_crossoverType)
            {
                case eCrossoverType.OnePoint:
                    CrossoverSinglePoint(parent);
                    break;

                case eCrossoverType.TwoPoint:
                    CrossoverTwoPoint(parent);
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

                case eCrossoverType.SBX:
                    CrossoverSBX(parent);
                    break;
            } //select mutation
        }

        public IChromosome Clone()
        {
            return new ChromosomeDouble(this);
        }        

        public IChromosome CreateNew()
        {
            ChromosomeDouble c = new ChromosomeDouble(_obctiveFunction);
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
            ChromosomeDouble other = (ChromosomeDouble)pair;
            double result = 0;
            double l = 0;
            double dmax;
            for (int i = 0; i < _size; i++)
            {
                dmax = (_max[i] - _min[i]);
                l = (this._chromosome[i] - other._chromosome[i]) * (this._chromosome[i] - other._chromosome[i]) / (dmax * dmax);
                result = result + l;
            }
            result = Math.Sqrt(result);
            result /= Math.Sqrt(_chromosome.Length);
            return result;
        }

        double CircleClamp(double x, double min, double max)
        {
            if (x > max)
            {
                x = (max - x) + min;
            }

            if (x < min)
            {
                x = max - (min - x);
            }

            return x;
        }

        public void Evaluate()
        {
            _fitness = _function(_chromosome);
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
                s += _chromosome[i].ToString("F2") + " ";
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

        [Obsolete("Что-то старое, разобраться и удалить нахуй")]
        public void Mutation(IChromosome attraction)
        {
            ChromosomeDouble other = attraction as ChromosomeDouble;
            double _gamma = 1;
            double _beta0 = 2;
            double _m = 2;

            double rij = this.Distance(attraction);
            double beta = _beta0 * Math.Exp(-_gamma * Math.Pow(rij, _m));

            double alpha = 0.1 * (1 - _Generation / _GenerationMax);

            for (int k = 0; k < _Length; k++)
            {
                this._chromosome[k] =
                    this._chromosome[k]
                    + beta * _random.NextDouble() * (other._chromosome[k] - this._chromosome[k])
                    + alpha * (2 * _random.NextDouble() - 1);

                if (_chromosome[k] > _max[k]) _chromosome[k] = _max[k];
                if (_chromosome[k] < _min[k]) _chromosome[k] = _min[k];
            }
        }
    }
}
