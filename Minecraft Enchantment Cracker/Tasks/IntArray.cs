using System;
using System.Diagnostics;

namespace Minecraft_Enchantment_Cracker.Tasks
{
    public class IntArray
    {
        private int[] _values;
        private int   _size;

        public IntArray(int startSize = 1000000) { _values = new int[startSize]; }

        public void AddValue(int v)
        {
            if(_size == _values.Length)
            {
                Debug.WriteLine($"Exceeded size: {_size}");
                var newValues = new int[_values.Length + 1000000];
                Array.Copy(_values, newValues, _values.Length);
                _values = newValues;
            }

            _values[_size++] = v;
        }

        public void AddAllValues(int[] v)
        {
            if(_size + v.Length > _values.Length)
            {
                int[] newValues = new int[_values.Length + v.Length];
                Array.Copy(_values, newValues, _values.Length);
                Array.Copy(v, 0, newValues, _values.Length, v.Length);
                _values = newValues;
            }
            else
                Array.Copy(v, 0, _values, _size, v.Length);

            _size += v.Length;
        }

        public int[] GetValues()
        {
            var ret = new int[_size];
            Array.Copy(_values, ret, _size);
            return ret;
        }
    }
}
