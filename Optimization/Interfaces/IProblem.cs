using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Optimization.Interfaces
{
    /// <summary>
    /// Reference to single-objective function at x = arg point
    /// </summary>
    /// <param name="arg"> double array argument</param>
    /// <returns>objective value</returns>
    public delegate double Function(double[] arg);

    [Obsolete("Интерфейс не завершен")]
    /// <summary>
    /// Single-objective problem interface:
    /// 
    /// Properties and pointer:
    /// function        Pointer to Objective function
    /// x0              Initial point for x
    /// Aineq           Matrix for linear inequality constraints
    /// bineq           Vector for linear inequality constraints
    /// Aeq             Matrix for linear equality constraints
    /// beq             Vector for linear equality constraints
    /// lb              Vector of lower bounds
    /// ub              Vector of upper bounds
    /// nonlcon         Nonlinear constraint function
    /// solver          Called solver
    /// options         Algorithm specific options
    /// 
    /// Methods:
    /// ToSrtring
    /// Save
    /// </summary>
    public interface IProblem
    {

        ///// <summary>
        /////   Gets input variable's labels for the function.
        ///// </summary>
        ///// 
        //IDictionary<string, int> _Variables { get; }

        ///// <summary>
        /////   Gets the index of each input variable in the function.
        ///// </summary>
        ///// 
        //IDictionary<int, string> _Indices { get; }

        /// <summary>
        ///   Gets the objective function which need Eval
        /// </summary>
        Function _ObjectiveFunction { get; }

        /// <summary>
        ///   Gets the number of input variables for the function.
        /// </summary>
        int _Length { get; }

        /// <summary>
        /// Initial point
        /// </summary>
        double[] _X0 { get; }

        /// <summary>
        /// Return max value from Definition Area
        /// </summary>
        double[] _UB { get; }

        /// <summary>
        /// Return min value from Definition Area
        /// </summary>
        double[] _LB { get; }

        double _Penalty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        double _Norm { get; set; }

        /// <summary>
        /// Text information of problem
        /// </summary>
        /// <returns>info</returns>
        string ToString();

        /// <summary>
        /// Save model
        /// </summary>
        /// <param name="name">Path</param>
        void Save(string name);
    }
}
