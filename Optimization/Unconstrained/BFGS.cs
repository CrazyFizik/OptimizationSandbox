using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization.Interfaces;

namespace Optimization.Unconstrained
{
    /// <summary>
    /// 
    /// Input:
    /// function        Pointer to Objective function
    /// x0              Initial point for x
    /// Aineq           Matrix for linear inequality constraints
    /// bineq           Vector for linear inequality constraints
    /// Aeq             Matrix for linear equality constraints
    /// beq             Vector for linear equality constraints
    /// lb              Vector of lower bounds
    /// ub              Vector of upper bounds
    /// nonlcon         Nonlinear constraint function
    /// solver          "BFGS"
    /// options         Algorithm specific options
    /// 
    /// Parameters:
    /// DerivativeCheck = off
    /// DiffMaxChange   = Inf
    /// DiffMinChange   = 0
    /// MaxFunEvals     = 100*numberOfVariables
    /// MaxIter         = 1000
    /// TolCon          = 1e-6
    /// TolFun          = 1e-6
    /// TolX            = 1e-10
    /// EPS             = 2.5e-11
    /// Hessian         = Hessian
    /// </summary>
    public class BFGS : ISolver
    {
        static Random _random = new Random(0);

        public IProblem _objective;

        public delegate double[] GradientFunction(Function f, double[] arg, double EPS);
        public GradientFunction _gradient;
        public Function _function;

        int _iteration = 0;
        int _maxIterations = 1024; // The number of iterations that were performed

        double _eps = 1e-6;
        double _tolX;
        double _tolG;
        double _stpmax = 100;
        double _mu = 1e-4;

        double[] _x0; // Start point
        double _fval0;

        List<double> _convergence = new List<double>();     // The convergence curve
        List<double[]> _xTrajectory = new List<double[]>(); // Trajectory of solutions positions
        List<double[]> _pTrajectory = new List<double[]>(); // Trajectory of search direction

        #region ISolver implement

        public ISolver _Hybrid
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

        public IProblem _Problem
        {
            get
            {
                return _objective;
            }

            set
            {
                _objective = value;
            }
        }

        public ISolution _Solution
        {
            get
            {
                return new Solution(_x0, _fval0);
            }
        }

        public List<ISolution> _Solutions
        {
            get
            {
                List<ISolution> solutions = new List<ISolution>();
                solutions.Add(new Solution(_x0, _fval0));
                return solutions;
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
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        [Obsolete("Добавить гибридную функцию")]
        public BFGS(IProblem objective)
        {
            _objective = objective;
            _function = objective._ObjectiveFunction;
            _gradient = Gradient;

            _x0 = objective._X0;
            if (_x0 == null)
            {
                _x0 = new double[objective._Length];
                for (int i = 0; i < objective._Length; i++)
                {
                    _x0[i] = _random.NextDouble() * (objective._UB[i] - objective._LB[i]) + objective._LB[i];
                }
            }
        }

        public ISolution Solve()
        {
            Minimize();
            Solution solution = new Solution(_x0, _fval0);
            return solution;
        }


        public ISolution Solve(double[] args)
        {
            args.CopyTo(_x0, 0);
            return Solve();
        }

        public double[] Minimize()
        {
            bool start = true;
            bool spurious = false;
            bool end = false;

            int n;

            double eps;
            double mu;
            double tolx;
            double tolg;
            double stpmax;
            double norm;

            double f0;
            double fval;
            double[] x0;
            double[] x;
            double[] p;
            double[] g;
            double[][] Hessian;

            n = _x0.Length;
            x0 = _x0;
            x = new double[n];
            g = new double[n];
            p = new double[n];
            Hessian = new double[n][];

            stpmax = _stpmax;
            eps = _eps;
            tolx = 4 * eps;
            tolg = 4 * eps;
            mu = _mu;

            //Mainloop   
            fval = f0 = _function(x0);
            g = _gradient(_function, x0, _eps);
            //Console.WriteLine("starting Point: x[0] = " + x0[0] + " x[1] = " + x0[1]);
            //Console.WriteLine("starting Grads: g[0] = " + g[0] + " g[1] = " + g[1]);
            //Console.WriteLine("Starting F = " + fval);
            //Console.WriteLine("\nBegin\n");
            StringBuilder report = new StringBuilder();

            double normF = 1.0;
            double normG = 1.0;

            normF = fval;
            //normG = fval;

            fval = fval / normF;
            f0 = f0 / normF;
            //for (int i = 0; i < n; i++)
            //{
            //    g[i] = g[i] / normG;
            //}

            _iteration = 0;
            for (int iteration = 0; iteration < _maxIterations; iteration++)
            {
                _Problem._Penalty = iteration; // ????
                _iteration++;
                report.Clear();
                report.Append(" Iteration = " + _iteration + " ");

                // First approximation
                if (start)
                {
                    //fval = f0 = _function(x0);
                    g = _gradient(_function, x0, _eps);
                    report.Append(" Hessian init ");
                    norm = 0;
                    for (int i = 0; i < n; i++)
                    {
                        Hessian[i] = new double[n];
                        for (int j = 0; j < n; j++)
                        {
                            Hessian[i][j] = 0;
                        }
                        Hessian[i][i] = 1;
                        p[i] = -g[i];
                        norm += x0[i] * x0[i];
                    }
                    stpmax = _stpmax * Math.Max(Math.Sqrt(norm), (double)n);
                    start = false;
                    spurious = false;
                    end = false;
                }

                string search = LineSearch(_function, _gradient, ref x0, f0, ref x, ref fval, ref p, ref g, eps, tolx, stpmax, mu, ref spurious, ref start, ref end, normF);

                //The new function evaluation occurs in LineSearch; save the function value in f0 for the
                //next line search.It is usually safe to ignore the value of check.
                _fval0 = f0 = fval; // store old fval

                string direction = UpdateDirection(_function, _gradient, ref p, ref x0, ref x, ref g, ref Hessian, ref fval, eps, tolx, tolg, ref end, normG);

                _convergence.Add(fval);
                _xTrajectory.Add(x);
                _pTrajectory.Add(p);
                
                report.Append(search);
                report.Append(direction);
                report.Insert(0, "F = " + fval.ToString());
                //Console.WriteLine(report);
                if (end && !start) break;
            }
            //Console.WriteLine("\nEnd\n");

            //Console.WriteLine("Iterations = " + _iteration);
            //Console.WriteLine("Function min value = " + fval);
            //Console.WriteLine("Minimized at x0 = [" + String.Join(",", x) + "]");
          
            _fval0 = _function(x);

            return x;
        }

        /// <summary>
        /// The Broyden-Fletcher-Goldfarb-Shanno variant of DavidonFletcher-Powell minimization 
        /// is performed on a function whose value and gradient are provided
        /// by a functor gradient.
        /// 
        /// The routine LineSearch() is previouscalled to perform approximate line minimizations
        /// </summary>
        /// <param name="function">Pointer to objective function</param>
        /// <param name="gradient">Pointer to gradient of objective function</param>
        /// <param name="p">Search direction (Newton step)</param>
        /// <param name="x0">Start point (same as x)</param>
        /// <param name="x">Solution point x = x0</param>
        /// <param name="g">Gradient's value</param>
        /// <param name="Hessian">Hessian approximation</param>
        /// <param name="fval">The minimum value of objective function</param>
        /// <param name="eps">Machine precision</param>
        /// <param name="tolx">The convergence criterion on x values</param>
        /// <param name="tolg">Zero-gradient criterion</param>
        /// <param name="end">The flag of the completion of the algorithm</param>
        public string UpdateDirection(
            Function function,
            GradientFunction gradient,
            ref double[] p,
            ref double[] x0,
            ref double[] x,
            ref double[] g,
            ref double[][] Hessian,
            ref double fval,
            double eps,
            double tolx,
            double tolg,
            ref bool end,
            double normG = 1.0)
        {
            StringBuilder report = new StringBuilder();

            //========================================

            double test = 0;
            double norm = 0;
            double EPS = eps;
            double TOLX = tolx;
            double TOLG = tolg;

            int n = p.Length;

            double[] s = new double[n];
            double[] y = new double[n];
            double[] Hy = new double[n];
            double[] u = new double[n];

            double normS, normY;
            double alpha, beta;

            for (int i = 0; i < n; i++)
            {
                //Update the line direction
                p[i] = x[i] - x0[i];
                s[i] = x[i] - x0[i];
                //Update start position
                x0[i] = x[i];
            }

            // Test convergence on delta x
            test = 0.0;
            for (int i = 0; i < n; i++)
            {
                double temp = Math.Abs(p[i]) / Math.Max(Math.Abs(p[i]), 1.0);
                if (temp > test) test = temp;
            }
            if (test < TOLX)
            {
                //Console.WriteLine("Exiting when test = " + test + " < xtol = " + TOLX);
                report.Append("\nExiting when test = " + test + " < xtol = " + TOLX);
                end = true;
                return report.ToString();
            }

            // Calculate gradient
            for (int i = 0; i < n; i++)
            {
                y[i] = g[i]; //Save the old gradient
            }
            g = gradient(function, x0, EPS); // get the new gradient
            for (int i = 0; i < n; i++)
            {
                g[i] = g[i] / normG;
            }

            //Test for convergence on zero gradient
            test = 0.0;
            norm = Math.Max(fval, 1.0);
            for (int i = 0; i < n; i++)
            {
                double temp = Math.Abs(g[i]) * Math.Max(Math.Abs(x0[i]), 1.0) / norm;
                if (temp > test) test = temp;
            }
            if (test < TOLG)
            {
                //Console.WriteLine("Exiting when test = " + test + " < gtol = " + TOLG);
                report.Append("\nExiting when test = " + test + " < gtol = " + TOLG);
                end = true;
                return report.ToString();
            }

            //Compute difference of gradients
            for (int i = 0; i < n; i++)
            {
                y[i] = g[i] - y[i];
            }
            //and difference times current matrix
            for (int i = 0; i < n; i++)
            {
                Hy[i] = 0.0;
                for (int j = 0; j < n; j++)
                {
                    Hy[i] += Hessian[i][j] * y[j];
                }
            }

            //Calculate dot products for the denomifornators 
            alpha = beta = 0.0;
            normS = normY = 0.0;
            for (int i = 0; i < n; i++)
            {
                alpha += y[i] * s[i];   // Scalar product of y^T * s
                beta += y[i] * Hy[i];   // Scalar product of y^T * (H * y)
                normY += y[i] * y[i];
                normS += s[i] * s[i];
            }

            // Update Hessian
            if (alpha > Math.Sqrt(EPS * normS * normY)) //Skip update if alpha not sufficiently postive
            {
                //The vector that makes BFGS different from DFP
                for (int i = 0; i < n; i++)
                {
                    u[i] = s[i] / alpha - Hy[i] / beta;
                }

                // The BFGS updating formula:
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        //DFP terms
                        double A = s[i] * s[j] / alpha;     // Outer product:   s*s^T
                        double B = Hy[i] * Hy[j] / beta;    // Outer product:   of Hy * Hy^T
                        //BFGS term
                        double C = beta * u[i] * u[j];      // Outer product:   u * U^T
                        //Update Hessian
                        Hessian[i][j] = Hessian[i][j] + A - B + C;
                        Hessian[j][i] = Hessian[i][j];
                    }                    
                }
            } // Hessian updating

            //Console.WriteLine("H:");
            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = 0; j < n; j++)
            //    {
            //        Console.Write(Hessian[i][j] + " ");
            //    }
            //    Console.WriteLine();
            //}

            // Now calculate the next direction to go
            for (int i = 0; i < n; i++)
            {
                p[i] = 0.0;
                for (int j = 0; j < n; j++)
                {
                    p[i] -= Hessian[i][j] * g[j];
                }
            } //and go back for another iteration

            return String.Empty;
        }

        [Obsolete("Вывести в отдельны класс")]
        /// <summary>
        ///Backtracking line search, a search scheme based on the Armijo–Goldstein condition.
        ///
        /// Given an n-dimensional point x0[0..n-1], the value of the function and gradient there, f0
        ///and g[0..n - 1], and a direction p[0..n - 1], finds a new point x[0..n - 1] along the direction
        ///p from x0 where the function or functor func has decreased “sufficiently.” 
        /// </summary>
        /// <param name="function">Pointer to objective function</param>
        /// <param name="gradient">>Pointer to gradient of objective function</param>
        /// <param name="x0">Start point</param>
        /// <param name="x">New point</param>
        /// <param name="fval">FUnction value at x</param>
        /// <param name="p">Newton step (direction)</param>
        /// <param name="g">Gradient value</param>
        /// <param name="f0">Old function value at x0 point</param>
        /// <param name="eps">Machine precision</param>
        /// <param name="tolx">The convergence criterion on delta x</param>
        /// <param name="stpmax">Is an input quantity that limits the length of the steps so that you do not try to evaluate the function in regions where it is undefined or subject to overflow</param>
        /// <param name="mu">Ensures sufficient decrease in function value</param>
        /// <param name="spurious">Spurious solution flag (need restart from another point)</param>
        /// <param name="start">Hessian starting aprroximation flag</param>
        /// <param name="end">The flag of the completion of the algorithm</param>
        public string LineSearch(
            Function function,
            GradientFunction gradient,
            ref double[] x0,
            double f0,            
            ref double[] x,
            ref double fval,
            ref double[] p,
            ref double[] g,
            double eps,
            double tolx,
            double stpmax,
            double mu, 
            ref bool spurious,
            ref bool start,
            ref bool end,
            double normF = 1.0)
        {

            StringBuilder report = new StringBuilder();

            //==========================================

            int n;

            double MU, TOLX;
            double slope;                               // The local slope of the function in search direction
            double lambda, l1, l2, lambdaMin, theta;    // The step size
            double f1, f2;
            double a, b;
            double rhs1, rhs2;
            double norm = 0;

            //==========================================

            n = x0.Length;
            MU = mu;
            TOLX = tolx;

            //Scale if attempted step is too big.
            for (int i = 0; i < n; i++)
            {
                norm += p[i] * p[i];
            }
            norm = Math.Sqrt(norm);
            if (norm > stpmax)
            {
                for (int i = 0; i < n; i++)
                {
                    p[i] = p[i] / norm;
                }
            }

            //Check curvature condition (Wolfe condition 1)
            slope = 0;
            for (int i = 0; i < n; i++)
            {
                slope += g[i] * p[i];
            }
            if (slope >= 0)
            {
                //throw new Exception("Roundoff problem");
                start = true;

                //Console.WriteLine("=================================================");
                //Console.WriteLine("Wolfe condition not executed: ");
                //if (slope > 0) Console.Write("Roundoff problem");
                //else Console.Write("The solution is degenerated");
                //Console.WriteLine(" | slope = " + slope.ToString());

                //Console.WriteLine("Position: [" + String.Join(",", x0) + "]");
                //Console.WriteLine("Direction: [" + String.Join(",", p) + "]");
                //Console.WriteLine("Gradiend: [" + String.Join(",", g) + "]");

                //Console.WriteLine("=================================================");

                report.Append(" Wolfe condition not executed: ");
                if (slope > 0) report.Append("Roundoff problem");
                else report.Append("The solution is degenerated");
                report.Append(" | slope = " + slope.ToString("F0"));
            }

            //Compute lambda min
            norm = 0;
            for (int i = 0; i < n; i++)
            {
                double temp = Math.Abs(p[i]) / Math.Max(Math.Abs(x0[i]), 1.0);
                if (temp > norm) norm = temp;
            }
            lambdaMin = TOLX / norm;
            double l0 = 1;
            lambda = l1 = l0;
            l2 = 0;
            f1 = f2 = 0;
            for (;;)
            {
                for (int i = 0; i < n; i++)
                {
                    x[i] = x0[i] + lambda * p[i];
                }
                fval = function(x)/normF;
                f1 = fval;

                if (lambda < lambdaMin)
                {
                    //Convergence on x. 
                    //For zero finding, the calling program should verify the convergence
                    for (int i = 0; i < n; i++)
                    {
                        x[i] = x0[i];
                    }

                    //Console.WriteLine("Solution is spurious");
                    report.Append(" Solution is spurious");

                    spurious = true;
                    end = true;
                    return report.ToString(); ;
                }
                else if (fval <= f0 + MU * lambda * slope) // Armijo rule (Wolfe condition 2)
                {
                    //Sufficient function decrease
                    //Console.WriteLine("Armijo–Goldstein test condition is fulfilled");
                    return report.ToString(); ;
                }
                else
                { //Backtrack.
                    if (lambda == l0) //First time
                    {
                        lambda = -slope / (2.0 * (fval - f0 - slope));
                    }
                    else //Subsequent backtracks
                    {
                        rhs1 = f1 - f0 - l1 * slope;
                        rhs2 = f2 - f0 - l2 * slope;
                        double minor = (l1 - l2);

                        a = rhs1 / (l1 * l1) - rhs2 / (l2 * l2);
                        a = a / minor;

                        b = -l2 * rhs1 / (l1 * l1) + l1 * rhs2 / (l2 * l2);
                        b = b / minor;

                        if (a == 0.0)
                        {
                            lambda = -slope / (2.0 * b);
                        }
                        else
                        {
                            theta = b * b - 3.0 * a * slope;
                            if (theta < 0)
                            {
                                lambda = 0.5 * l1;
                            }
                            else if (b <= 0)
                            {
                                lambda = -b + Math.Sqrt(theta);
                                lambda = lambda / (3.0 * a);
                            }
                            else
                            {
                                lambda = -slope / (b + Math.Sqrt(theta));
                            }

                            if (lambda > 0.5 * l1)
                            {                                
                                lambda = 0.5 * l1; // l <= 0.5 * l1
                            }
                        }
                    } //Subsequent backtracks
                } //Backtrack
                f2 = fval;
                l2 = l1;                
                lambda = l1 = Math.Max(lambda, 0.1 * l1); // l > 0.1 * l1
            } // Try again

            return report.ToString();
        }

        [Obsolete("Вывести в отдельный класс")]
        /// <summary>
        /// Gradient of function f  at point x = [arg]
        /// </summary>
        /// <param name="f">Pointer to objective function</param>
        /// <param name="arg">Point</param>
        /// <param name="EPS">Numeric precision</param>
        /// <returns>gradient(f)</returns>
        public double[] Gradient(Function f, double[] arg, double EPS)
        {
            int n = arg.Length;
            double[] x = new double[n];
            double[] result = new double[n];
            arg.CopyTo(x, 0);

            for (int i = 0; i < n; i++)
            {
                double x0 = x[i];
                double dx = Math.Abs(EPS * x0);  //Math.Sqrt(EPS) * Math.Abs(x0);               
                double h = 0;

                x[i] = x0 + dx;
                dx = x[i] - x0;
                h += dx;
                double f2 = f(x);

                x[i] = x0 - dx;
                dx = x0 - x[i];
                h += dx;
                double f0 = f(x);

                result[i] = (f2 - f0) / h;
            }
            return result;
        }

    }
}
