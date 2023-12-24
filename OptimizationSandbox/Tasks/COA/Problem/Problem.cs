using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;

namespace OptimizationSandbox.Tasks.COA.Problem
{
    public abstract class Problem : IProblem
    {
        protected delegate double[] Objectives();
        protected Objectives _objectives;

        protected Model _model;
        protected Function _function;
        protected double _penalty = 0.0;

        protected int _valnum;

        protected double[] _x0;
        protected double[] _ub;
        protected double[] _lb;

        protected double _norm = 1;

        protected double a;
        protected double b;
        protected double d;
        protected double c;

        public Problem(Model model)
        {
            _model = model;
            _objectives = _model.Evaluate;
            _function = Evaluate;

            Init();
        }

        protected virtual void Init()
        {
            _valnum = _model._X.Length;
            _lb = new double[_valnum];
            _ub = new double[_valnum];
            for (int i = 0; i < _valnum; i++)
            {
                _lb[i] = 0;
                _ub[i] = 1;
            }            
            _x0 = GetArgs(_ub, _lb);
        }

        public double Evaluate(double[] arg)
        {
            double result;
            double[] fval;

            SetArgs(arg, _ub, _lb);
            //_x0 = GetArgs(_ub, _lb);
            fval = _objectives();
            double f = fval[0];
            double c = fval[1] + fval[2];

            result = f + _penalty * c * c;

            return result;
        }

        public abstract void SetArgs(double[] args, double[] ub, double[] lb);
        public abstract double[] GetArgs(double[] ub, double[] lb);

        #region Interface implement

        public int _Length
        {
            get
            {
                return _valnum;
            }
        }

        public Function _ObjectiveFunction
        {
            get
            {
                return _function;
            }
        }

        public double[] _X0
        {
            get
            {
                _x0 = GetArgs(_ub, _lb);
                return _x0;
            }
        }

        public double[] _LB
        {
            get
            {
                return _lb;
            }
        }

        public double[] _UB
        {
            get
            {
                return _ub;
            }
        }

        public double _Norm
        {
            get
            {
                return _norm;
            }

            set
            {
                _norm = value;
            }
        }

        public double _Penalty
        {
            get
            {
                return _penalty;
            }

            set
            {
                _penalty = value;
            }
        }

        public void Save(string name)
        {
            _model.Save(name);
        }

        #endregion
    }
}
