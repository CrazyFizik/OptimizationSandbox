using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationSandbox.Tasks.COA
{
    public class Vertexes : IList<Vector2>
    {
        List<Vector2> _vertexes = new List<Vector2>();

        public Vector2 this[int index]
        {
            get
            {
                return _vertexes[index];
            }

            set
            {
                _vertexes[index]=value;
            }
        }

        public int Count
        {
            get
            {
                return _vertexes.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(Vector2 item)
        {
            _vertexes.Add(item);
        }

        public void Clear()
        {
            _vertexes.Clear();
        }

        public bool Contains(Vector2 item)
        {
            return _vertexes.Contains(item);
        }

        public void CopyTo(Vector2[] array, int arrayIndex)
        {
            _vertexes.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return _vertexes.GetEnumerator();
        }

        public int IndexOf(Vector2 item)
        {
            return _vertexes.IndexOf(item);
        }

        public void Insert(int index, Vector2 item)
        {
            _vertexes.Insert(index, item);
        }

        public bool Remove(Vector2 item)
        {
            return _vertexes.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _vertexes.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _vertexes.GetEnumerator();
        }
    }
}
