using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OptimizationSandbox.Tasks.COA
{
    public class Channel
    {
        public Cavity[] _cavities;
        public Coupling[] _mainCoupling;
        public Coupling[] _crossCoupling;

        public Color _color;

        Vector2 _startPoint;
        Vector2 _endPoint;

        public Channel()
        {

        }

        public Channel(Color color, int cavityNumber, float radius, Vector2 startPoint, Vector2 endPoint, float minLength, float maxLength)
        {
            _color = color;
            _cavities = new Cavity[cavityNumber];
            _mainCoupling = new Coupling[cavityNumber - 1];
            _startPoint = startPoint;
            _endPoint = endPoint;

            for (int i = 0; i < cavityNumber; i++)
            {
                _cavities[i] = new Cavity(color, radius, i);
            }

            for (int i = 0; i <cavityNumber-1; i++)
            {
                Coupling coupling = new Coupling(_cavities[i], _cavities[i + 1], minLength, maxLength);
                _mainCoupling[i] = coupling;
            }

            Vector2 delta = (endPoint - startPoint) / (cavityNumber - 1);
            for (int i = 0; i < cavityNumber; i++)
            {
                if (i == 0)
                {
                    _cavities[i]._position = startPoint;
                }
                else if (i < cavityNumber-1)
                {
                    _cavities[i]._position = _cavities[i - 1]._position + delta;
                }
                else
                {
                    _cavities[i]._position = endPoint;
                }
            }
        }

        public Channel(Color color, int cavityNumber, float radius, Vector2 start, Vector2 end, float min, float max, int[][] crosscouplingMatrix, float minCC, float maxCC) :
            this(color, cavityNumber, radius, start, end, min, max)
        {
            //Array.Copy(crosscoupling, _crossCoupling, crosscoupling.Length);
            List<Coupling> cc = new List<Coupling>();
            for (int i = 0; i < crosscouplingMatrix.Length; i++)
            {
                int A = crosscouplingMatrix[i][0];
                int B = crosscouplingMatrix[i][1];

                Coupling coupling = new Coupling(_cavities[A], _cavities[B], minCC, maxCC);
                cc.Add(coupling);
            }
            _crossCoupling = cc.ToArray();
        }
    }
}
