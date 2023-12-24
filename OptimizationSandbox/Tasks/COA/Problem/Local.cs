using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OptimizationSandbox.Tasks.COA;

namespace OptimizationSandbox.Tasks.COA.Problem
{
    public class Local : Problem
    {
        public Local(Model model) : base(model)
        {
            //throw new Exception();
        }

        public override void SetArgs(double[] args, double[] ub, double[] lb)
        {            
            double dx;
            double dy;

            int index = 0;
            foreach (Channel channel in _model._Design._channels)
            {
                double x0 = channel._cavities[0]._position.X;
                double y0 = channel._cavities[0]._position.Y;
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    c = -channel._mainCoupling[i - 1].GetMax();
                    d = channel._mainCoupling[i - 1].GetMax();

                    a = lb[index];
                    b = ub[index];
                    dx = args[index].ReMap(a, b, c, d);
                    index++;

                    a = lb[index];
                    b = ub[index];
                    dy = args[index].ReMap(a, b, c, d);
                    index++;

                    x0 = x0 + dx;
                    y0 = y0 + dy;

                    _model._X[index-1] = x0;
                    _model._X[index] = y0;
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
                double x0 = channel._cavities[0]._position.X;
                double y0 = channel._cavities[0]._position.Y;
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    x = _model._X[index];                    
                    y = _model._X[index+1];
                    
                    double dx = x - x0;
                    double dy = y - y0;
                    x0 = x;
                    y0 = y;

                    a = -channel._mainCoupling[i - 1].GetMax();
                    b = channel._mainCoupling[i - 1].GetMax();

                    c = lb[index];
                    d = ub[index];
                    result[index] = x.ReMap(a, b, c, d);
                    index++;

                    c = lb[index];
                    d = ub[index];
                    result[index] = y.ReMap(a, b, c, d);
                    index++;
                }
            }
            return result;
        }
    }
}
