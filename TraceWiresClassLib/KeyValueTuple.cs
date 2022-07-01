using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceWiresClassLib
{
    // Мне нужен тип, состоящий из двух изменяемых переменных.
    // Такого нет.
    // Поэтому вот мой.

    public class KeyValueTuple<TKey, TValue>
    {
        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public KeyValueTuple(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
