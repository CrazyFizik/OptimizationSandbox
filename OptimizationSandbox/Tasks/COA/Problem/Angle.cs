using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OptimizationSandbox.Tasks.COA;

namespace OptimizationSandbox.Tasks.COA.Problem
{
    public class Angle : Problem
    {
        public Angle(Model model) : base(model)
        {
            //throw new Exception();
        }

        protected override void Init()
        {
            _lb = new double[_valnum];
            _ub = new double[_valnum];
            for (int i = 0; i < _x0.Length; i++)
            {
                _lb[i] = 0;
                _ub[i] = 1;
            }

            _valnum = _model._X.Length / 2;
            _x0 = GetArgs(_ub, _lb);
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
                    double r = channel._mainCoupling[i - 1].GetMin();

                    c = 0;
                    d = 2 * Math.PI;
                    a = lb[index];
                    b = ub[index];
                    double alpha = args[index].ReMap(a, b, c, d);
                    index++;

                    dx = r * Math.Cos(alpha);
                    dy = r * Math.Sin(alpha);

                    x0 = x0 + dx;
                    y0 = y0 + dy;

                    _model._X[2 * index] = x0;
                    _model._X[2 * index + 1] = y0;
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
                    x = _model._X[2 * index];
                    y = _model._X[2 * index + 1];
                    double dx = x - x0;
                    double dy = y - y0;
                    x0 = x;
                    y0 = y;

                    double r = channel._mainCoupling[i - 1].GetMin();

                    double alpha = Math.Atan2(dy, dx);
                    if (alpha < 0) alpha += 2 * Math.PI;

                    a = 0;
                    b = 2 * Math.PI;
                    c = lb[index];
                    d = ub[index];
                    alpha = alpha.ReMap(a, b, c, d);

                    result[index] = alpha;
                    index++;
                }
            }
            return result;
        }
    }
}
