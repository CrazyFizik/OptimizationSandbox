using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Optimization.Interfaces;
using System.Drawing;


namespace OptimizationSandbox.Tasks.COA
{
    public class Model : IModel
    {
        #region Interface implementation

        int _length;
        double[] _x;
        double[] _radii;

        public double[] _X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        public double[] Evaluate()
        {
            CalcMetrics();
            double[] reslut =
                {
                _out._conflictedArea * _step * _step,
                _out._mainCouplingConflict,
                _out._crossCouplingConflict,                
                _out._coveredArea,
                _out._freeArea
                };

            return reslut;
        }

        public double[] Evaluate(double[] arg)
        {
            int index = 0;
            float x;
            float y;

            foreach (Channel channel in _Design._channels)
            {
                for (int i = 1; i < channel._cavities.Length - 1; i++)
                {
                    x = (float)arg[index];
                    index++;
                    y = (float)arg[index];
                    index++;

                    channel._cavities[i]._position = new Vector2(x, y);
                }
            }

            return Evaluate();
        }

        #endregion

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

        Output _out = new Output();
        public Output _Out
        {
            get
            {
                return _out;
            }
        }

        #region Boundary

        double _minGrid;
        double _maxGrid;

        double _areaMax;
        double _areaMin;

        Vector2 _lb = new Vector2();
        Vector2 _ub = new Vector2();

        public double _Max
        {
            get
            {
                return _areaMax;
            }
        }

        public double _Min
        {
            get
            {
                return _areaMin;
            }
        }

        Vector2 _ox = new Vector2();
        Vector2 _oy = new Vector2();

        public Vector2 _OX
        {
            get
            {
                return _ox;
            }
        }

        public Vector2 _OY
        {
            get
            {
                return _oy;
            }
        }



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

        public Model(Design design, double step)
        {
            this._step = step;
            Init(design);
        }

        /// <summary>
        /// Init mesh and another parameters
        /// </summary>
        /// <param name="design"></param>
        void Init(Design design)
        {
            _design = design;
            GetDesign();

            // Generate grid
            _grid = GenerateMesh(_step);
            _grid = SetBorder(ref _grid, _step, _size, _border);
            _grid = SetObstacles(ref _grid, _step, _size, _obstacle);
            _freeArea = CalcFreeArea(ref _grid, _free);
            _allocateGrid = GenerateMesh(_step);

            // Check
            var favl = CalcMetrics();
            string path = "Initial" + DateTime.Now.ToString("yyyy.mm.dd.HH.mm");
            Save(path);
        }

        /// <summary>
        /// Generate meshgrid
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        short[][] GenerateMesh(double step)
        {
            _ox = new Vector2(_design._border._vertexes[0].X, _design._border._vertexes[0].X);
            _oy = new Vector2(_design._border._vertexes[0].Y, _design._border._vertexes[0].Y);

            foreach (Vector2 point in _design._border._vertexes)
            {
                if (point.X > _ox.Y)
                {
                    _ox.Y = point.X;
                }
                if (point.X < _ox.X)
                {
                    _ox.X = point.X;
                }

                if (point.Y > _oy.Y)
                {
                    _oy.Y = point.Y;
                }
                if (point.Y < _oy.X)
                {
                    _ox.X = point.Y;
                }
            }

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

        #region Design Refresh

        void GetDesign()
        {
            // coordinates and radii
            int N = 0;
            foreach (Channel channel in _design._channels)
            {
                N += channel._cavities.Length - 2;
            }
            _length = 2 * N;
            _x = new double[_length];
            _radii = new double[N];
            int index = 0;
            int n = 0;
            foreach (Channel channel in _design._channels)
            {
                int start = 1;
                int end = channel._cavities.Length - 1;
                for (int i = start; i < end; i++)
                {
                    Cavity cavity = channel._cavities[i];
                    _x[index++] = cavity._position.X;
                    _x[index++] = cavity._position.Y;
                    _radii[n++] = cavity._radius;
                }
            }
        }

        void SetDesign()
        {
            int index = 0;
            int n = 0;
            foreach (Channel channel in _design._channels)
            {
                int start = 1;
                int end = channel._cavities.Length - 1;
                for (int i = start; i < end; i++)
                {
                    Cavity cavity = channel._cavities[i];
                    cavity._position.X = (float)_x[index++];
                    cavity._position.Y = (float)_x[index++];
                    cavity._radius = (float)_radii[n++];
                }
            }
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
            double ix;
            double iy;
            int N;
            double radius;
            int row0;
            int column0;

            foreach (Channel channel in _design._channels)
            {
                foreach (Cavity cavity in channel._cavities)
                {
                    // Coordinates and step
                    x0 = cavity._position.X - _lb.X;
                    y0 = cavity._position.Y - _lb.Y;
                    radius = cavity._radius;
                    N = (int)Math.Round(2 * radius / step);

                    // Coordinates and step
                    //x0 = x - _lb.X;
                    //y0 = y - _lb.Y;
                    //N = (int)Math.Round(2 * radius / step);

                    // Mesh
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
                            ix = column * step;
                            iy = row * step;

                            if (Utils.InCircle(ix - x0, iy - y0, radius))
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
                                calc = calc + 1;// + ptrptr[row][column];
                            }
                        }
                    }
                }
            }

            return calc;
        }
        #endregion

        double CalcMetrics()
        {
            SetDesign();
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

            _out._conflictedArea = CalcConflictArea(ref _allocateGrid, _cavity);
            _out._coveredArea = 0;// _step * _step * CalcCoveredArea(ref _allocateGrid, _cavity);
            _out._freeArea = 0;// _freeArea;

            return _out._result;
        }

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
    }
}
