using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Interfaces
{
    /// <summary>
    /// State machine Callback
    /// </summary>
    /// <returns>State-machine info</returns>
    public delegate IStateMachineInfo Callback();

    /// <summary>
    /// Simple state machine info
    /// </summary>
    public interface IStateMachineInfo
    {
        Callback _Callback
        {
            get;
        }

        double _Fval
        {
            get;
        }

        double[] _Fvals
        {
            get;
        }

        int _Iteration
        {
            get;
        }

        int _IterationsMax
        {
            get;
            set;
        }

        int _EvaluationCount
        {
            get;
        }
    }
}
