using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OptimizationSandbox.Tasks.COA;
using Optimization.Interfaces;
using System.Drawing;


namespace OptimizationSandbox.Tasks.COA
{
    public enum eArgumentType
    {
        absolute,
        local,
        polar,
        angle
    }

    public class ModelOld : IModel
    {
        Function _function;
        public Function _ObjectiveFunction
        {
            get
            {
                return _function;
            }
        }

        int _valnum = 0;
        public int _Length
        {
            get
            {

                return _valnum;
            }
        }

        public eArgumentType type = eArgumentType.absolute;

        Output _out = new Output();
        public Output _Out
        {
            get
            {
                return _out;
            }
        }
        

        Design _design;
        public Design _Design
        {
            get
            {
                return _design;
            }
            set
            {
                Init(value);
            }
        }

        /// <summary>
        /// Initial point
        /// </summary>
        public double[] _X0
        {
            get
            {
                double[] x = new double[_Length];

                switch (type)
                {
                    case eArgumentType.absolute:
                        x = GetAbsolute();
                        break;

                    case eArgumentType.local:
                        x = GetLocal();
                        break;

                    case eArgumentType.polar:
                        x = GetPolar();
                        break;

                    case eArgumentType.angle:
                        throw new Exception();
                        break;
                }

                return x;
            }
        }

        public bool IsNormalize = true;

        #region Boundary

        double _minGrid;
        double _maxGrid;

        double _min = 0;
        double _max = 1;
        public double _LB
        {
            get
            {
                return _min;
            }
        }

        public double _UB
        {
            get
            {
                return _max;
            }
        }

        public double[] _X
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

        double _areaMax;
        double _areaMin;
        Vector2 _lb = new Vector2();
        Vector2 _ub = new Vector2();

        #endregion

        #region Mesh
        double _step = 1;
        short[][] _grid;
        short[][] _allocateGrid;
        int _size;
        int _freeArea;

        short _free = 0;
        short _obstacle = 1;
        short _border = 2;
        short _cavity = 4;
        #endregion

        #region Constructors

        public ModelOld(Design design, double step, eArgumentType type)
        {
            this._step = step;
            this.type = type;


            if (type == eArgumentType.local)
            {
                _max = 1;
                _min = -1;
            }
            else
            {
                _min = 0;
                _max = 1;
            }

            Init(design);
        }

        /// <summary>
        /// Init mesh and another parameters
        /// </summary>
        /// <param name="design"></param>
        void Init(Design design)
        {
            _design = design;
            _function = Solve;

            _grid = GenerateMesh(_step);
            _grid = SetBorder(ref _grid, _step, _size, _border);
            _grid = SetObstacles(ref _grid, _step, _size, _obstacle);
            _freeArea = CalcFreeArea(ref _grid, _free);
            _allocateGrid = GenerateMesh(_step);

            _valnum = 0;
            foreach (Channel channel in _design._channels)
            {
                _valnum += channel._cavities.Length - 2;
            }
            if (type != eArgumentType.angle)
                _valnum *= 2;
        }

        /// <summary>
        /// Generate meshgrid
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        short[][] GenerateMesh(double step)
        {
            double max = _design._border._vertexes[0].X;
            double min = _design._border._vertexes[0].X;
            foreach (Vector2 point in _design._border._vertexes)
            {
                if (point.X > max)
                    max = point.X;
                if (point.Y > max)
                    max = point.Y;

                if (point.X < min)
                    min = point.X;
                if (point.Y < min)
                    min = point.Y;
            }
            _maxGrid = max;
            _minGrid = min;

            double delta = _maxGrid - _minGrid;
            _areaMin = _minGrid - delta / 2;
            _areaMax = _maxGrid + delta / 2;

            _lb = new Vector2((float)_areaMin, (float)_areaMin);
            _ub = new Vector2((float)_areaMax, (float)_areaMax);

            double range = _areaMax - _areaMin;
            int length = 1 + (int)Math.Round(range / step);
            _size = length;

            short[][] grid = new short[length][];

            for (int i = 0; i < length; i++)
            {
                grid[i] = new short[length];
            }

            return grid;
        }

        #endregion

        #region Solve

        public double Solve(double[] args)
        {
            double result = 0;

            switch (type)
            {
                case eArgumentType.absolute:
                    result = SolveAbsolute(args);
                    break;

                case eArgumentType.local:
                    result = SolveLocal(args);
                    break;

                case eArgumentType.polar:
                    result = SolvePolar(args);
                    break;

                case eArgumentType.angle:
                    result = SolveAngle(args);
                    break;
            }

            return result;
        }

        public double SolvePolar(double[] args)
        {
            int index = 0;
            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    double r = args[index]
                        * (channel._mainCoupling[i - 1].GetMax() - channel._mainCoupling[i - 1].GetMin())
                        + channel._mainCoupling[i - 1].GetMin();
                    index++;

                    //double r = channel._mainCoupling[i - 1].GetMin();

                    double alpha = 2 * Math.PI * args[index];
                    index++;

                    double x = r * Math.Cos(alpha);
                    double y = r * Math.Sin(alpha);

                    Vector2 delta = new Vector2(
                        (float)(x),
                        (float)(y));

                    channel._cavities[i]._position =
                        channel._cavities[i - 1]._position
                        + delta;
                }
            }

            return CalcMetrics();
        }

        public double SolveAngle(double[] args)
        {
            int index = 0;
            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    double r = channel._mainCoupling[i - 1].GetMin();

                    double alpha = 2 * Math.PI * args[index];
                    index++;

                    double x = r * Math.Cos(alpha);
                    double y = r * Math.Sin(alpha);

                    Vector2 delta = new Vector2(
                        (float)(x),
                        (float)(y));

                    channel._cavities[i]._position =
                        channel._cavities[i - 1]._position
                        + delta;
                }
            }

            return CalcMetrics();
        }

        public double SolveLocal(double[] args)
        {
            int index = 0;
            float x;
            float y;
            double norm;

            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    norm = channel._mainCoupling[i].GetMax();

                    x = (float)(args[index] * norm);
                    index++;
                    y = (float)(args[index] * norm);
                    index++;

                    channel._cavities[i]._position =
                        new Vector2(x, y)
                        + channel._cavities[i - 1]._position;
                }
            }

            return CalcMetrics();
        }

        public double SolveAbsolute(double[] args)
        {
            int index = 0;
            float x;
            float y;

            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {

                    //x = (float)args[index];
                    //index++;
                    //y = (float)args[index];
                    //index++;

                    x = (float)(args[index]
                        * (_maxGrid - _minGrid) + _minGrid);
                    index++;

                    y = (float)(args[index]
                        * (_maxGrid - _minGrid) + _minGrid);
                    index++;

                    channel._cavities[i]._position = new Vector2(x, y);
                }
            }

            return CalcMetrics();
        }

        public double CalcMetrics()
        {
            _allocateGrid = SetCavity(ref _grid, ref _allocateGrid, _step, _size, _cavity);

            _out.Reset();
            double delta = 0;
            foreach (Channel channel in _design._channels)
            {
                foreach (Coupling coupling in channel._mainCoupling)
                {
                    delta =
                        coupling.GetConflictLength();
                    _out._mainCouplingConflict += delta;
                }

                if (channel._crossCoupling != null)
                {
                    foreach (Coupling coupling in channel._crossCoupling)
                    {
                        delta =
                            coupling.GetConflictLength();
                        _out._crossCouplingConflict += delta;
                    }
                }
            }
            _out._crossCouplingConflict *=  _out._crossCouplingConflict;
            _out._mainCouplingConflict *=  _out._mainCouplingConflict;

            _out._conflictedArea =  CalcConflictArea(ref _allocateGrid, _cavity);
            _out._coveredArea = 0;// _step * _step * CalcCoveredArea(ref _allocateGrid, _cavity);
            _out._freeArea = 0;// _freeArea;

            return _out._result;
        }

        #endregion

        #region Gets

        public double[] GetPolar()
        {
            int index = 0;
            double[] result = new double[_Length];
            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    Cavity cavity = channel._cavities[i]; Cavity prevCavity = channel._cavities[i - 1]; Vector2 localPosition = cavity._position - prevCavity._position;
                    double r = Math.Sqrt(localPosition.X * localPosition.X + localPosition.Y * localPosition.Y);
                    r = (r - channel._mainCoupling[i - 1].GetMin())
                        / (channel._mainCoupling[i - 1].GetMax() - channel._mainCoupling[i - 1].GetMin());

                    double alpha = Math.Atan2(localPosition.Y, localPosition.X);
                    if (alpha < 0) alpha += 2 * Math.PI;
                    alpha = alpha / (2 * Math.PI);

                    result[index] = r;
                    index++;
                    result[index] = alpha;
                    index++;
                }
            }
            return result;
        }

        public double[] GetLocal()
        {
            throw new Exception();
        }

        public double[] GetAbsolute()
        {
            int index = 0;
            double[] result = new double[_valnum];

            foreach (Channel channel in _design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    Cavity cavity = channel._cavities[i];

                    result[index] = (float) 
                        (cavity._position.X - _minGrid) / (_maxGrid - _minGrid);
                    index++;

                    result[index] = (float)
                        (cavity._position.Y - _minGrid) / (_maxGrid - _minGrid);
                    index++;

                }
            }

            return result;
        }

        #endregion

        #region Sets

        short[][] SetBorder(ref short[][] tempgrid, double step, int size, short pattern)
        {
            double x0;
            double y0;
            double x;
            double y;

            x0 = 0 - _lb.X;// _design._border._vertexes[0]._position.X - _lb.X;
            y0 = 0 - _lb.Y;// _design._border._vertexes[0]._position.Y - _lb.Y;

            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    x = column * step - x0;
                    y = row * step - y0;

                    if (!Utils.InPolygon(_design._border._vertexes.ToArray<Vector2>(), new Vector2((float)x, (float)y)))
                    {
                        tempgrid[row][column] += pattern;
                    }
                }
            }
            return tempgrid;
        }

        short[][] SetObstacles(ref Int16[][] tempgrid, double step, int size, short pattern)
        {
            double x0;
            double y0;
            double x;
            double y;
            int N;
            double max;
            int row0;
            int column0;

            foreach (Obstacle obstacle in _design._obstacles)
            {
                if (obstacle is Polygon)
                {
                    Polygon polygon = obstacle as Polygon;

                    max = 0;
                    foreach (Vector2 point in polygon._vertexes)
                    {
                        if (point.X > max)
                            max = point.X;

                        if (point.Y > max)
                            max = point.Y;
                    }
                    N = (int)Math.Round(max / step);

                    x0 = polygon._position.X - _lb.X;
                    y0 = polygon._position.Y - _lb.Y;

                    row0 = (int)Math.Round(y0 / step);
                    column0 = (int)Math.Round(x0 / step);

                    for (int row = row0; row < N + row0; row++)
                    {
                        for (int column = column0; column < N + column0; column++)
                        {
                            x = column * step - x0;
                            y = row * step - y0;

                            if (Utils.InPolygon(polygon._vertexes.ToArray<Vector2>(), new Vector2((float)x, (float)y)))
                            {
                                tempgrid[row][column] += pattern;
                            }
                        }
                    }
                }

                if (obstacle is Screw)
                {
                    Screw screw = obstacle as Screw;

                    x0 = screw._position.X - _lb.X;
                    y0 = screw._position.Y - _lb.Y;
                    max = 2 * screw._radius;
                    N = (int)Math.Round(max / step);

                    row0 = (int)Math.Round((y0 - max / 2) / step);
                    if (row0 < 0) row0 = 0;
                    column0 = (int)Math.Round((x0 - max / 2) / step);
                    if (column0 < 0) column0 = 0;

                    for (int row = row0; row < N + row0; row++)
                    {
                        for (int column = column0; column < N + column0; column++)
                        {
                            x = column * step;
                            y = row * step;

                            if (Utils.InCircle(x - x0, y - y0, max / 2))
                            {
                                tempgrid[row][column] += pattern;
                            }
                        }
                    }
                }
            }
            return tempgrid;
        }

        short[][] SetCavity(ref short[][] grid, ref short[][] tempgrid, double step, int size, short pattern)
        {
            for (int i = 0; i < size; i++)
            {
                grid[i].CopyTo(tempgrid[i], 0);
            }

            double x0;
            double y0;
            double x;
            double y;
            int N;
            double radius;
            int row0;
            int column0;

            foreach (Channel channel in _design._channels)
            {
                foreach (Cavity cavity in channel._cavities)
                {
                    x0 = cavity._position.X - _lb.X;
                    y0 = cavity._position.Y - _lb.Y;
                    radius = cavity._radius;
                    N = (int)Math.Round(2 * radius / step);

                    row0 = (int)Math.Round((y0 - radius) / step);
                    if (row0 < 0) row0 = 0;

                    column0 = (int)Math.Round((x0 - radius) / step);
                    if (column0 < 0) column0 = 0;

                    int rowMax = N + row0;
                    if (rowMax > size) rowMax = size;

                    int columnMax = N + column0;
                    if (columnMax > size) columnMax = size;

                    for (int row = row0; row < rowMax; row++)
                    {
                        for (int column = column0; column < columnMax; column++)
                        {
                            x = column * step;
                            y = row * step;

                            if (Utils.InCircle(x - x0, y - y0, radius))
                            {
                                tempgrid[row][column] += pattern;
                            }
                        }
                    }
                }

            }
            return tempgrid;
        }

        #endregion

        #region Calcs
        unsafe static int CalcCoveredArea(ref Int16[][] grid, short pattern)
        {
            int Rows = grid.Length;
            int calc = 0;

            fixed (short* junk = &grid[0][0])
            {

                short*[] arrayofptr = new short*[grid.Length];
                for (int i = 0; i < grid.Length; i++)
                {
                    fixed (short* ptr = &grid[i][0])
                    {
                        arrayofptr[i] = ptr;
                    }
                }

                fixed (short** ptrptr = &arrayofptr[0])
                {
                    for (int row = 0; row < Rows; row++)
                    {
                        for (int column = 0; column < Rows; column++)
                        {
                            if (ptrptr[row][column] == pattern)
                            {
                                calc++;
                            }
                        }
                    }
                }
            }

            return calc;
        }

        unsafe static int CalcFreeArea(ref short[][] grid, short pattern)
        {
            int Rows = grid.Length;
            int calc = 0;

            fixed (short* junk = &grid[0][0])
            {

                short*[] arrayofptr = new short*[grid.Length];
                for (int i = 0; i < grid.Length; i++)
                {
                    fixed (short* ptr = &grid[i][0])
                    {
                        arrayofptr[i] = ptr;
                    }
                }

                fixed (short** ptrptr = &arrayofptr[0])
                {
                    for (int row = 0; row < Rows; row++)
                    {
                        for (int column = 0; column < Rows; column++)
                        {
                            if (ptrptr[row][column] == pattern)
                            {
                                calc++;
                            }
                        }
                    }
                }
            }

            return calc;
        }

        unsafe static int CalcConflictArea(ref short[][] grid, short pattern)
        {
            int Rows = grid.Length;
            int calc = 0;

            fixed (short* junk = &grid[0][0])
            {

                short*[] arrayofptr = new short*[grid.Length];
                for (int i = 0; i < grid.Length; i++)
                {
                    fixed (short* ptr = &grid[i][0])
                    {
                        arrayofptr[i] = ptr;
                    }
                }

                fixed (short** ptrptr = &arrayofptr[0])
                {
                    for (int row = 0; row < Rows; row++)
                    {
                        for (int column = 0; column < Rows; column++)
                        {
                            if (ptrptr[row][column] > pattern)
                            {
                                calc = calc + ptrptr[row][column];
                            }
                        }
                    }
                }
            }

            return calc;
        }
        #endregion

        private Image CreateImage(Int16[][] data)
        {
            short max = data[0][0];
            short min = data[0][0];
            for (int row = 0; row < data.GetLength(0); row++)
            {
                for (int column = 0; column < data[row].GetLength(0); column++)
                {
                    if (data[row][column] > max)
                        max = data[row][column];

                    if (data[row][column] < min)
                        min = data[row][column];
                }
            }
            int range = max - min;
            byte v;

            Bitmap bm = new Bitmap(data.GetLength(0), data[0].GetLength(0));
            System.Drawing.Imaging.BitmapData bd = bm.LockBits(
                new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // This is much faster than calling Bitmap.SetPixel() for each pixel.
            unsafe
            {
                byte* ptr = (byte*)bd.Scan0;
                for (int j = 0; j < bd.Height; j++)
                {
                    for (int i = 0; i < bd.Width; i++)
                    {
                        v = (byte)(255 * (data[bd.Height - 1 - j][i] - min) / range);
                        ptr[0] = v;
                        ptr[1] = v;
                        ptr[2] = v;
                        ptr[3] = (byte)255;
                        ptr += 4;
                    }
                    ptr += (bd.Stride - (bd.Width * 4));
                }
            }

            bm.UnlockBits(bd);
            return bm;
        }

        public void Save(string path)
        {
            path = path + ".jpg";
            Image image = CreateImage(_allocateGrid);
            image.Save(path);
        }

        public double[] Evaluate()
        {
            throw new NotImplementedException();
        }

        public double[] Evaluate(double[] arg)
        {
            throw new NotImplementedException();
        }
    }

    

    
}
