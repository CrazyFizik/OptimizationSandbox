using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Interfaces
{

    /// <summary>
    /// Reference to multi-objective function at x = arg point
    /// </summary>
    /// <param name="arg"> double array function</param>
    /// <returns>objectives vector</returns>
    public delegate double[] MultiObjectiveFunction(double[] arg);

    /// <summary>
    /// Multi-objective problem representation
    /// </summary>
    interface IMultiObjectiveProblem
    {
        /// <summary>
        ///   Gets the objective function which need Eval
        /// </summary>
        MultiObjectiveFunction _ObjectiveFunction { get; }

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
