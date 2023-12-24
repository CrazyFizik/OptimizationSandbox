using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Heuristic.GeneticlAlgorithm.Interfaces;
using Optimization.Heuristic.GeneticlAlgorithm.SelectionAlgorithm;
using Optimization.Interfaces;

namespace Optimization.Heuristic.SimulatedAnnealing
{
    /// <summary>
    /// SIMULATED ANNEALING method finds a constrained minimum of a function of several variables.
    /// * A, b, Aeq, beq are not yet implemented *
    /// Attempts to solve problems of the form:
    /// min F(X)  subject to:  X = [LB, UB]
    ///      X
    ///      
    /// Problem Input:
    /// objective   - Objective function
    /// x0          - Starting point
    /// lb          - Lower bound for x
    /// ub          - Upper bound for x
    /// rng         - random generator
    /// solver      - Solver "SA"
    /// options
    /// 
    /// Algorithm Output:
    /// x               - Solution
    /// fval            - Objective function value at the solution
    /// exitflag        - Reason simulannealbnd stopped
    /// iterations      - The number of iterations computed
    /// funccount       - The number of evaluations of the objective function
    /// temperature     - Temperature when the solver terminated
    /// totaltime       - Total time for the solver to run
    /// rng
    /// message
    /// 
    /// Information about current state of the solver:
    /// x           - Current point
    /// fval        - Current objective function value at x
    /// bestx       - Best point found so far
    /// bestfval    - Objective function value at best point
    /// temperature - Current temperature, a vector the same length as x
    /// iteration   - Current iteration
    /// funccount   - Number of function evaluations
    /// t0          - Start time for algorithm
    /// k           - Annealing parameter, a vector the same length as x
    /// 
    /// The flags of current state:
    /// 'init' - Initialization state
    /// 'iter' - Iteration state
    /// 'done' - Final state
    /// 
    /// Options:
    /// AcceptanceFcn
    /// AnnealingFcn
    /// TemperatureFcn
    /// HybridFcn
    /// HybridInterval
    /// InitialTemperature  - 1 or 100
    /// MaxFunEvals         - 3000*numberOfVariables
    /// MaxIter             - Inf
    /// ReannealInterval    - 100
    /// StallIterLimit      - 500*numberOfVariables
    /// TimeLimit           - Inf
    /// TolFun              - 1e-6
    /// </summary>
    public class SimulatedAnnealing : ISolver
    {
        //Problem
        IProblem _problem;
        ISolver _hybridFun;
        ISolver _outputFun;

        //Soltions
        Solution _best;         //best solution in every time
        Solution[] _population; //current population of solutions

        //Points
        double[] _x0;   // Start point        
        double[] _x;    // Current point
        double[] _bestx;// Best point

        //Fval
        double _fval0;      // Start fval
        double _fval;       // Current fval
        double _bestfval;   // Best fval
        double _avgfval;    // Avg fval 
        int _N = 0;

        //Anneal parameters
        double _T0 = 1;
        double _T = 1e-4;//5e-5;
        double _temperature;
        double _coolingRate = 0.99;
        eCoolingScheme _coolingScheme = eCoolingScheme.exponential;
        int _stepsPerT = 20;
        int _popSize = 10;
        int _nMoves = 5;
        int _plotInterval = 32;
        double _sigma = 1.0;
        double _mu = 1.0;

        // Termination
        double _objectiveLimit = 1e-6;  // 1) stops if the best objective function value is less than or equal to the value of ObjectiveLimit
        double _tolFun = 1e-6;  // 2) until the average change in value of the objective function          
        int _stallIterLimit = 64;    // in StallIterLim iterations is less than FunctionTolerance  
        int _maxIter = 1024;  // 3) if the number of iterations exceeds this maximum number of iterations
        int _maxFunEvals = 16384; // 4) maximum number of evaluations of the objective function
        int _maxRestarts = 1;     // 5)

        // Terminaton flags
        bool _start = true;
        bool _exit = false;
        bool _exitFun = false;      // 1
        bool _exitIter = false;     // 3
        bool _exitStall = false;    // 2
        bool _exitRestart = false;  // 5
        bool _exitFunEval = false;  // 4

        // Termination counters
        double _dt = 0;
        int _iterationLocal = 0;
        int _iterationGlobal = 0;
        int _stall = 0;
        internal int _funcount = 0;
        int _startcount = 0;

        // Lam Law
        int _movesAcceptable = 0;
        int _movesTotal = 0;
        double _acceptableRate = 0;

        //Debug
        List<double>    _convergence = new List<double>();     // The convergence curve
        List<double[]>  _xTrajectory = new List<double[]>(); // Trajectory of solutions positions
        List<double[]>  _pTrajectory = new List<double[]>(); // Trajectory of search direction
        List<Solution>  _results = new List<Solution>();

        #region ISolver implement

        public ISolver _Hybrid
        {
            get
            {
                return _hybridFun;
            }

            set
            {
                _hybridFun = value;
            }
        }

        public IProblem _Problem
        {
            get
            {
                return _problem;
            }

            set
            {
                _problem = value;
            }
        }

        public ISolution _Solution
        {
            get
            {
                return _best;
            }
        }

        public List<double> _ConvergenceCurve
        {
            get
            {
                return _convergence;
            }
        }

        public List<double[]> _Trajectory
        {
            get
            {
                return _xTrajectory;
            }
        }

        public int _MaxStall
        {
            get
            {
                return _stallIterLimit;
            }

            set
            {
                _stallIterLimit = value;
            }
        }

        public List<ISolution> _Solutions
        {
            get
            {
                _results.Sort();
                List<ISolution> solutions = new List<ISolution>();
                foreach (ISolution solution in _results)
                {
                    solutions.Add(solution);
                }
                return solutions;
            }
        }

        #endregion

        public SimulatedAnnealing(IProblem objective, double[][] x0 = null, ISolver hybrid = null, ISolver output = null)
        {
            _problem = objective;
            _hybridFun = hybrid;
            _outputFun = output;

            _maxIter = 100 * _problem._Length;
            _stallIterLimit = 50;// * _problem._Length;
            _maxFunEvals = 3000 * _problem._Length * _nMoves * _popSize;

            Helpers.ConsoleTable.tableWidth = 72;

            _problem._Penalty = 0;
            _best = new Solution(this);
            _bestfval = _best._cost;
            _bestx = _best._position;
            if (x0 != null)
            {
                _popSize = x0.Length + 1;
                _population = new Solution[_popSize];
                _population[0] = _best.Clone();
                for (int i = 1; i < _popSize; i++)
                {
                    _population[i] = new Solution(this);
                    x0[i].CopyTo(_population[i]._position, 0);
                    _population[i].Evaluate();
                }
            }
            else
            {
                _popSize = 2 * _problem._Length + 1;
                _population = new Solution[_popSize];

                double[] x = new double[_problem._Length];
                for (int i = 0; i < _problem._Length - 1; i++)
                {
                    x[i] = (_problem._UB[i] - _problem._LB[i])/ 2;
                }
                _population[0] = new Solution(this);
                x.CopyTo(_population[0]._position, 0);
                _population[0].Evaluate();

                int n = 0;
                for (int i = 1; i < _popSize; i+=2)
                {
                    double tempx = x[n];

                    _population[i] = new Solution(this);
                    x.CopyTo(_population[i]._position, 0);
                    _population[i]._position[n] = tempx + (_problem._UB[n] - tempx) / 2;
                    _population[i].Evaluate();

                    _population[i+1] = new Solution(this);
                    x.CopyTo(_population[i+1]._position, 0);
                    _population[i+1]._position[n] = tempx - (tempx - _problem._LB[n]) / 2;
                    _population[i + 1].Evaluate();

                    n++;
                }
            }
            Start();
        }
        
        public ISolution Solve()
        {
            while(!_exit)
            {
                Step();
            }

            return _best;
        }

        public ISolution Solve(double[] args)
        {
            throw new NotImplementedException();
        }

        #region New Template

        /*
         * Start                    - Preprocess all inputs, validate options, find initial feaasible point
         * Solve                    - Call engine to do the actual algorithm      
         *      Step
         *          CheckExit
         *          NewPoint
         *          HybridFunction
         *          Update
         *          Plot
         *      ReStart
         *      OutputFunction
         *      Plot
         * End                      - Prepare output arguments      
         */

        public bool Start()
        {
            Array.Sort(_population);

            _fval = _population[0]._cost;
            _x = _population[0]._position;

            _fval0 = _population[0]._cost;
            _x0 = _population[0]._position;

            _start = false;
            _stall = 0;

            _temperature = _T0;
            _startcount++;
            _iterationLocal = 0;

            if (_population[0]._cost < _bestfval)
            {
                _best = _population[0].Clone();
                _bestfval = _best._cost;
                _bestx = _best._position;
            }

            PrintStart();

            return true;
        }

        public bool End()
        {


            _best.Evaluate();
            Console.WriteLine("Function Evaluations = " + _funcount);
            Console.Write("Best solution found at: ");
            Console.WriteLine(_best.ToString());

            return true;
        }

        public bool Step()
        {
            // Restart
            if (_start & !_exit)
            {
                Start();
            }

            _iterationLocal++;
            _iterationGlobal++;

            _fval0 = _fval;
            _x0 = _x;

            // Solve
            DateTime time = DateTime.Now;
            NewPoint();
            Hybrid();
            Update();
            _dt = (DateTime.Now - time).TotalMilliseconds;
            PrintIterationInfo();
            // End solve
            CheckExit();

            return _exit;
        }

        void CheckExit()
        {
            // Check exit
            _exitStall = _stall > _stallIterLimit;
            _exitFun = _bestfval <= _objectiveLimit;
            _exitIter = _iterationGlobal >= _maxIter;
            _exitFunEval = _funcount >= _maxFunEvals;
            _exitRestart = _startcount > _maxRestarts + 1;

            _exit = _exitFun | _exitIter | _exitRestart | _exitRestart | _exitFunEval;
            _start = _temperature <= _T | _exitStall | _exitFun | _exitIter | _exitFunEval;

            // Add solutions
            //if (_exit || _start)
            //{
            //    foreach (Solution solution in _population)
            //    {
            //        _results.Add(solution.Clone());
            //    }
            //}

            // Stop
            if (_start)
            {
                PrintStopReason();
            }

            //End
            if (_exit)
            {
                foreach (Solution solution in _population)
                {
                    _results.Add(solution.Clone());
                }
                _results.Add(_best.Clone());
                _results.Sort();
                PrintExitReason();
                End();
            }
        }

        void NewPoint()
        {
            _movesAcceptable = 0;
            _movesTotal = 0;
            _N = 0;

            double relativeT = _temperature / _T0;
            
            for (int i = 0; i < _stepsPerT; i++)
            {
                List<Solution> solutions = new List<Solution>();
                for (int j = 0; j < _popSize * _nMoves; j++)
                {
                    //copy curent solution
                    int index = j % _popSize;
                    Solution solution = _population[index].Clone(); // 12 ms
                    solution._temperature = _temperature;
                    _problem._Penalty = 1 - relativeT;

                    //move solution to new position
                    double sigma = _sigma * relativeT;
                    solution.AnnealingFast(sigma);
                    double mu = _mu; // Math.Max(_temperature / _T0, 0.5);
                    solution.Swap(mu);

                    //calc cost function
                    solution.Evaluate(); // 8 ms
                    solutions.Add(solution);

                    _N++;
                    _avgfval = (solution._cost + _N * _avgfval) / (_N + 1);
                    _movesTotal++;
                }

                solutions.Sort();

                // Aceptable routine
                for (int j = 0; j < _popSize; j++)
                {
                    // check transition to new position
                    if (solutions[j]._cost <= _population[j]._cost)
                    {
                        _population[j] = solutions[j]; // transition to good new soltion
                        _movesAcceptable++;
                    }
                    else
                    {
                        double dE = solutions[j]._cost - _population[j]._cost;// Math.Abs(solutions[j]._cost - _population[j]._cost);
                        dE /= _population[j]._cost; // energy
                        double p = Math.Exp(-dE / _temperature);
                        if (Solution._random.NextDouble() <= p)
                        {
                            _population[j] = solutions[j]; // transition to worst new solution
                            _movesAcceptable++;
                        }
                    }

                } //each solution in population

            } // each step

            _acceptableRate = (double)_movesAcceptable / _movesTotal;

        }

        void Update()
        {
            Array.Sort(_population);
            _fval = _population[0]._cost;
            _x = _population[0]._position;
            // History
            _convergence.Add(_population[0]._cost);
            //_xTrajectory.Add(_population[0]._position);
            //_pTrajectory.Add(_population[0]._direction);

            if (_fval < _bestfval)
            {
                _best = _population[0].Clone();
                _bestx = _best._position;
                _bestfval = _best._cost;
            }

            // Cooling
            _temperature = _temperature * _coolingRate;
            //if (_adaptive & _fval < _fval0)
            //{
            //    // Do nothing
            //}
            //else
            //{
            //    double t = _temperature;
            //    _temperature = Cooling(_coolingScheme, _T0, _temperature, _coolingRate, _schedule, _MaxIterations);
            //    if (_temperature < t)
            //    {
            //        _schedule++;
            //    }
            //}

            if (Math.Abs(_fval - _fval0) < _tolFun || _movesAcceptable == 0)
            {
                _stall++;
            }
            else
            {
                _stall = 0;
            }
        }

        void Plot()
        {

        }

        void Hybrid()
        {
            if (_hybridFun != null)
            {
                for (int i = 0; i < _population.Length; i++)
                {
                    Solution solution = _population[i].Clone();
                    solution.Evaluate(); // Update problem

                    double f0 = solution._cost;
                    double f = _hybridFun.Solve(_hybridFun._Problem._X0)._Fval;

                    solution.Evaluate(_problem._X0);
                    _population[i] = solution;
                    //Console.WriteLine("F0 = " + f0 + " F1 = " + f + " F = " + solution._cost);
                }
            }
        }

        void Output()
        {
            if (_outputFun != null)
            {
                for (int i = 0; i < _population.Length; i++)
                {
                    Solution solution = _population[i].Clone();
                    solution.Evaluate(); // Update problem

                    double f0 = solution._cost;
                    double f = _hybridFun.Solve(_hybridFun._Problem._X0)._Fval;

                    solution.Evaluate(_problem._X0);
                    _population[i] = solution;
                    //Console.WriteLine("F0 = " + f0 + " F1 = " + f + " F = " + solution._cost);
                }
            }
        }

        #endregion

        #region Console
        /// <summary>
        /// 1) Iteration — Iteration number
        /// 2) f - count — Cumulative number of objective function evaluations
        /// 3) Best f(x) — Best objective function value
        /// 4) Current f(x) — Current objective function value
        /// 5) Temperature — Mean temperature function value
        /// </summary>
        void PrintIterationHeader()
        {
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow(
                "Iteration",
                "f-count",
                "dt",
                "T",
                "Best Fval",
                "Fval",
                "Rate");
            Helpers.ConsoleTable.PrintLine();
        }

        /// <summary>
        /// 1) Iteration — Iteration number
        /// 2) f - count — Cumulative number of objective function evaluations
        /// 3) Best f(x) — Best objective function value
        /// 4) Current f(x) — Current objective function value
        /// 5) Temperature — Mean temperature function value
        /// </summary>
        void PrintIterationInfo()
        {
            if (_iterationGlobal % _plotInterval == 0)
            {
                PrintIterationHeader();
            }

            Helpers.ConsoleTable.PrintRow(
                _iterationGlobal.ToString() + "/" + _maxIter.ToString(),
                _funcount.ToString(),
                _dt.ToString("F2"),
                _temperature.ToString("F5"),
                _bestfval.ToString("F2"),
                _fval.ToString("F2"),
                _acceptableRate.ToString("F2"));
        }

        /// <summary>
        /// 
        /// </summary>
        void PrintStart()
        {
            Console.WriteLine("Start : " + _startcount.ToString() + "/" + (_maxRestarts + 1).ToString());
            Console.WriteLine("Tolerance : " + _tolFun.ToString());
            Console.WriteLine("Objective limit : " + _objectiveLimit.ToString());
            Console.WriteLine("Stalls limit : " + _stallIterLimit);
            Console.WriteLine("Iteraions limit : " + _maxIter.ToString());
            Console.WriteLine("Function evaluations limit : " + _maxFunEvals);
            Console.WriteLine("Solutions:");
            for (int i = 0; i < _popSize; i++)
            {
                Console.WriteLine(_population[i].ToString());
            }

            PrintIterationHeader();
        }

        void PrintStopReason()
        {
            // Stop reason message
            Console.WriteLine();
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow("STOP REASON");
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow(
                "Temperature",
                "Stall",
                "Fun",
                "Iter",
                "FunEval");
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow(
                (_temperature <= _T).ToString(),
                _exitStall.ToString(),
                _exitFun.ToString(),
                _exitIter.ToString(),
                _exitFunEval.ToString());
            Helpers.ConsoleTable.PrintLine();
            Console.WriteLine();

            //Helpers.ConsoleTable.PrintLine();
            //Helpers.ConsoleTable.PrintRow("SOLUTIONS");
            //Helpers.ConsoleTable.PrintLine();
            //for (int i = 0; i < _popSize; i++)
            //{
            //    Console.WriteLine(_population[i].ToString());
            //}
            //Helpers.ConsoleTable.PrintLine();
        }

        /// <summary>
        /// 
        /// </summary>
        void PrintExitReason()
        {
            // Exit reason message
            Console.WriteLine();
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow("EXIT REASON");
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow(
                "Function limit",
                "Iteration limit",
                "Restarts limit",
                "Staller limit",
                "Fun count");
            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow(
                _exitFun.ToString(),
                _exitIter.ToString(),
                _exitRestart.ToString(),
                _exitStall.ToString(),
                _funcount.ToString());
            Helpers.ConsoleTable.PrintLine();
            Console.WriteLine();

            Helpers.ConsoleTable.PrintLine();
            Helpers.ConsoleTable.PrintRow("SOLUTIONS");
            Helpers.ConsoleTable.PrintLine();
            for (int i = 0; i < _popSize; i++)
            {
                Console.WriteLine(_population[i].ToString());
            }
            Helpers.ConsoleTable.PrintLine();
        }
        #endregion

        #region Temperature RefactorThis
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="iteration">greater than 1 is faster</param>
        /// <returns></returns>
        double TemperatureFast(double t0, int iteration)
        {
            return t0 / (double)iteration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="iteration">greater than 1 is fater</param>
        /// <returns></returns>
        double TemepertaureExp(double t0, int iteration)
        {
            return t0 * Math.Pow(_coolingRate, iteration + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="iteration">greater than 1 is fater</param>
        /// <returns></returns>
        double TemperatureBoltzmann(double t0, int iteration)
        {
            return t0 / Math.Log(iteration + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="acceptableRate"></param>
        /// <returns></returns>
        double TemperatureLam(double t, double targetRate, double acceptableRate, double k)
        {
            return t = t * (1 - (acceptableRate - targetRate) / k);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="k"></param>
        /// <param name="startCost"></param>
        /// <param name="endCost"></param>
        /// <returns></returns>
        double TemperatureAdaptive(double t, int k, double startCost, double endCost)
        {
            if (endCost > startCost)
                return t * _coolingRate;
            else
                return t;
        }
        #endregion

    }

    public enum eCoolingScheme
    {
        exponential,
        fast,
        boltzman,
        lam,
        adaptive
    }

    /// <summary>
    /// 
    ///PARAMETERS
    ///
    /// TolFun               - Termination tolerance on function value
    ///                         [positive scalar | {1e-6} ] 
    /// MaxIter              - Maximum number of iterations allowed 
    ///                         [positive scalar | {Inf} ]
    /// MaxFunEvals          - Maximum number of function(objective)
    ///                         evaluations allowed
    ///                         [positive scalar | {3000*numberOfVariables} ]
    /// TimeLimit            - Total time(in seconds) allowed for optimization
    ///                        [positive scalar | {Inf} ]
    ///
    /// ObjectiveLimit       - Minimum objective function value desired 
    ///                        [scalar | {-Inf} ]
    /// StallIterLimit       - Number of iterations over which average
    ///                         change in objective function value at current
    ///                        point is less than options.TolFun 
    ///                        [positive scalar | {'500*numberOfVariables'} ]
    ///
    /// DataType             - The type of decision variable 
    ///                        [ 'custom' | {'double'} ]
    ///
    /// InitialTemperature   - Initial temperature at start
    ///                          [positive scalar | {100} ]
    ///
    /// ReannealInterval     - Interval for Reannealing 
    ///                          [positive integer | {100} ]
    ///
    /// AnnealingFcn         - Function used to generate new points 
    ///                          [function_handle | @annealingboltz | 
    ///                            {@annealingfast} ]
    ///
    /// TemperatureFcn       - Function used to update temperature schedule
    ///                          [function_handle  | @temperatureboltz | 
    ///                           @temperaturefast | {@temperatureexp} ]
    ///
    /// AcceptanceFcn        - Function used to determine if a new point is
    ///                          accepted or not
    ///                          [function_handle | {@acceptancesa}
    ///          
    /// HybridFcn            - Automatically run HybridFcn(another
    ///                         optimization function) during or at the end of
    ///                         iterations of the solver
    ///                         [@fminsearch | @patternsearch | @fminunc |
    ///                           @fmincon | {[]} ]
    ///
    /// HybridInterval       - Interval(if not 'end' or 'never') at which
    ///                          HybridFcn is called 
    ///                          [positive integer | 'never' | {'end'} ]
    ///
    /// Display              - Controls the level of display 
    ///                         [ 'off' | 'iter' | 'diagnose' | {'final'} ]
    ///
    /// DisplayInterval      - Interval for iterative display
    ///                         [positive integer | {10} ]
    ///
    /// OutputFcns           - Function(s) gets iterative data and can change
    ///                          options at run time
    ///                          [function handle or cell array of function 
    ///                             handles | {[]} ]
    /// PlotFcns             - Plot function(s) called during iterations 
    ///                          [function handle or cell array of function 
    ///                            handles | @saplotbestf | @saplotbestx | 
    ///                            @saplotf | @saplotstopping | 
    ///                            @saplottemperature | {[]} ]
    /// PlotInterval         - Interval at which PlotFcns are called
    ///                          [positive integer {1} ] 
    /// </summary>
    public struct Parameters
    {

    }
}
