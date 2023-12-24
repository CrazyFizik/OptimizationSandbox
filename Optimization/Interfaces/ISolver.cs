using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Interfaces
{
    /// <summary>
    /// Simple single-objective solver
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Hybrid function
        /// nullable
        /// </summary>
        ISolver _Hybrid
        {
            get;
            set;
        }

        /// <summary>
        /// Problem representation
        /// </summary>
        IProblem _Problem
        {
            get;
            set;
        }

        /// <summary>
        /// Current best solution
        /// </summary>
        ISolution _Solution
        {
            get;
        }

        List<ISolution> _Solutions
        {
            get;
        }

        /// <summary>
        /// History of all fvals
        /// </summary>
        List<double> _ConvergenceCurve
        {
            get;
        }

        /// <summary>
        /// Solution trakectory
        /// </summary>
        List<double[]> _Trajectory
        {
            get;
        }

        /// <summary>
        /// Number of similary iteration before end
        /// </summary>
        int _MaxStall
        {
            get;
            set;
        }

        ISolution Solve();

        ISolution Solve(double[] args);
    }
}
