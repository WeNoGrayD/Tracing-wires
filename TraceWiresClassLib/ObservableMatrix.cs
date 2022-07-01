using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace TraceWiresClassLib
{
    public class ObservableMatrix<T> : IEnumerable, IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged
        where T : IEquatable<T>
    {
        public T[,] _content;

        private const string IndexerName = "Item[]";

        // Индексатор, дающий доступ к значениям словаря по ключу.

        public T this[int i, int j]
        {
            get { return _content[i, j]; }
            set
            {
                if (!_content[i, j]?.Equals(value) ?? true)
                {
                    _content[i, j] = value;
                    OnPropertyChanged(IndexerName);
                }
            }
        }

        // Конструктор.

        public ObservableMatrix(int size1, int size2) : base()
        {
            _content = new T[size1, size2];
        }

        // Уведомление подписчиков на событие изменения свойства.

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
            
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)_content.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
    }
}
