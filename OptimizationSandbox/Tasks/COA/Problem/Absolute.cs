using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization.Interfaces;

using OptimizationSandbox.Tasks.COA;

namespace OptimizationSandbox.Tasks.COA.Problem
{
    public class Absolute : Problem
    {

        public Absolute(Model model) : base(model)
        {
            //throw new Exception();
        }

        public override void SetArgs(double[] args, double[] ub, double[] lb)
        {            
            double x;
            double y;

            int index = 0;
            foreach (Channel channel in _model._Design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    c = _model._OX.X;
                    d = _model._OX.Y;
                    a = lb[index];
                    b = ub[index];
                    x = args[index].ReMap(a, b, c, d);
                    _model._X[index] = x;
                    index++;

                    c = _model._OY.X;
                    d = _model._OY.Y;
                    a = lb[index];
                    b = ub[index];
                    y = args[index].ReMap(a, b, c, d);
                    _model._X[index] = y;
                    index++;
                }
            }
        }

        public override double[] GetArgs(double[] ub, double[] lb)
        {            
            double[] result = new double[_valnum];

            double x;
            double y;

            int index = 0;
            foreach (Channel channel in _model._Design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    a = _model._OX.X;
                    b = _model._OX.Y;
                    c = lb[index];
                    d = ub[index];
                    x = _model._X[index];
                    result[index] = x.ReMap(a, b, c, d);
                    index++;

                    a = _model._OY.X;
                    b = _model._OY.Y;
                    c = lb[index];
                    d = ub[index];
                    y = _model._X[index];
                    result[index] = y.ReMap(a, b, c, d);
                    index++;
                }
            }

            return result;
        }

    }
}
