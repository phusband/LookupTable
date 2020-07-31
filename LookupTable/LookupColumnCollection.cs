using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookup
{
    public sealed class LookupColumnCollection : IEnumerable<ILookupColumn>
    {
        #region Properties

        private Dictionary<string, int> _columnMap;
        private List<ILookupColumn> _columns;

        public ILookupColumn this[string name]
        {
            get => this[IndexOf(name)];
            private set => _columns[IndexOf(name)] = value;
        }

        public ILookupColumn this[int index]
        {
            get => GetAt(index);
            private set => _columns[index] = value;
        }

        public int Count => _columns.Count;

        public LookupTable Table { get; }

        #endregion

        #region Construtors

        internal LookupColumnCollection(LookupTable table)
        {
            _columnMap = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            _columns = new List<ILookupColumn>();

            Table = table;
        }

        #endregion

        #region Methods

        private void UpdateOrdinals()
        {
            _columnMap = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

            int i = 0;
            foreach (ILookupColumn column in _columns)
            {
                _columnMap.Add(column.Name, i++);
            }
        }


        public ILookupColumn Get(string columnName) => GetAt(IndexOf(columnName));

        public LookupColumn<T> Get<T>(string columnName) => GetAt<T>(IndexOf(columnName));

        public ILookupColumn GetAt(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            return _columns[columnIndex];
        }

        public LookupColumn<T> GetAt<T>(int columnIndex)
        {
            ILookupColumn iColumn = GetAt(columnIndex);
            if (!(iColumn is LookupColumn<T> column))
                throw new Exception($"Invalid type '{typeof(T).FullName}' specified for column at {columnIndex}.");

            return column;
        }

        public void Add(ILookupColumn column)
        {
            if (_columnMap.ContainsKey(column.Name))
                throw new Exception($"Column {column.Name} already exists.");

            _columns.Add(column);
            _columnMap.Add(column.Name, _columnMap.Count);
        }

        public LookupColumn<T> Add<T>(string name, IEqualityComparer<T> comparer = null)
        {
            var column = new LookupColumn<T>(this.Table, name, comparer);
            Add(column);

            return column;
        }

        public void Clear()
        {
            _columns.Clear();
            _columnMap.Clear();
        }

        public bool Contains(string columnName) => _columnMap.ContainsKey(columnName);

        public int IndexOf(string columnName)
        {
            if (_columnMap.TryGetValue(columnName, out int columnIndex))
                return columnIndex;

            throw new KeyNotFoundException($"Column '{columnName}' does not exist.");
        }

        public void Insert(int index, ILookupColumn item)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException(nameof(index));

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Insert the new column and recreate the column name/ordinal map
            _columns.Insert(index, item);
            UpdateOrdinals();
        }

        public void Insert<T>(int index, string name, IEqualityComparer<T> comparer = null)
        {
            var column = new LookupColumn<T>(this.Table, name, comparer);
            Insert(index, column);
        }

        public bool Remove(ILookupColumn item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            bool removed = _columns.Remove(item);

            UpdateOrdinals();
            return removed;
        }

        public bool Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return Remove(GetAt(IndexOf(name)));
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException(nameof(index));

            var column = GetAt(index);
            Remove(column);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<ILookupColumn> GetEnumerator() => _columns?.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
