using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Interfaces
{
    /// <summary>
    /// Simple problem model
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Point
        /// </summary>
        double[] _X
        {
            get;
            set;
        }

        /// <summary>
        /// Model Evaluator
        /// </summary>
        /// <returns>Values of objective function</returns>
        double[] Evaluate();

        /// <summary>
        /// Model Evaluator at poin x = arg
        /// </summary>
        /// <param name="arg">Point</param>
        /// <returns>Values of objective function</returns>
        double[] Evaluate(double[] arg);
    }
}
