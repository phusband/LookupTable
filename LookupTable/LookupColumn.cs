using System;
using System.Collections.Generic;
using System.Linq;

namespace Lookup
{
    public interface ILookupColumn
    {
        Type DataType { get; }
        bool IsNullableType { get; }
        bool IsValueType { get; }
        string Name { get; }
        int Ordinal { get; }
        LookupTable Table { get; }

        void Clear();
        void ClearData();
        object GetAt(int rowIndex);
        void RemoveAt(int rowIndex);
        void SetAt(int rowIndex, object value);
        void SetNull(int rowIndex);
        bool IsNull(int rowIndex);
    }

    public class LookupColumn<T> : ILookupColumn
    {
        #region Properties

        private HashSet<int> _nullMap;
        private Dictionary<T, HashSet<int>> _indexMap;
        private Dictionary<int, T> _valueMap;

        internal bool CanStoreNullValues => !IsValueType || IsNullableType;

        public Type DataType { get; }

        public bool IsValueType { get; }

        public bool IsNullableType { get; }

        public T NullValue { get; set; }

        public string Name { get; }

        public int Ordinal => Table.Columns.IndexOf(Name);

        public LookupTable Table { get; }

        public IEqualityComparer<T> Comparer { get; }

        #endregion

        #region Constructors

        internal LookupColumn(LookupTable table, string name)
            : this(table, name, null) { }

        internal LookupColumn(LookupTable table, string name, IEqualityComparer<T> comparer)
        {
            Name = name;
            Comparer = comparer ?? EqualityComparer<T>.Default;

            DataType = typeof(T);
            IsValueType = DataType.IsValueType;
            IsNullableType = (Nullable.GetUnderlyingType(DataType) == null);
            NullValue = default(T);

            _nullMap = new HashSet<int>();
            _indexMap = new Dictionary<T, HashSet<int>>(Comparer);
            _valueMap = new Dictionary<int, T>();

            Table = table;
        }

        #endregion

        #region Methods

        public void Clear()
        {
            // Clearing a column means wiping all stored index information
            _nullMap.Clear();
            _indexMap.Clear();
            _valueMap.Clear();
        }

        public void ClearData()
        {
            // Clearing data for a column means setting null values for all rows
            for (int i = 0; i < Table.Rows.Count; i++)
            {
                SetNull(i);
            }
        }

        public bool Contains(T value) => _indexMap.ContainsKey(value);

        public int FindFirstIndex(T value) => FindIndexes(value).Min();

        public int FindLastIndex(T value) => FindIndexes(value).Max();

        public HashSet<int> FindIndexes(T value)
        {
            if (_indexMap.TryGetValue(value, out HashSet<int> matches))
            {
                return new HashSet<int>(matches);
            }

            return null;
        }

        public HashSet<int> FindNull() => new HashSet<int>(_nullMap);

        public T GetAt(int rowIndex)
        {
            if (!Table.IsValidRowIndex(rowIndex))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            // Check stored values first
            if (_valueMap.TryGetValue(rowIndex, out T value))
                return value;

            // Return NullValue when applicable
            if (_nullMap.Contains(rowIndex))
                return NullValue;

            throw new Exception($"Unable to retrieve value at row {rowIndex}.");
        }

        public bool IsNull(int rowIndex) => _nullMap.Contains(rowIndex);

        private void RemoveValueAt(int rowIndex)
        {
            // Remove the value and the stored row index
            if (_valueMap.TryGetValue(rowIndex, out T oldValue) &&
                _indexMap.TryGetValue(oldValue, out HashSet<int> indexes))
            {
                indexes.Remove(rowIndex);
                _valueMap.Remove(rowIndex);
            }
        }

        private void RemoveNullAt(int rowIndex) => _nullMap.Remove(rowIndex);

        public void SetAt(int rowIndex, T value)
        {
            if (!Table.IsValidRowIndex(rowIndex))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            if (value == null)
            {
                SetNull(rowIndex);
                return;
            }

            if (_valueMap.TryGetValue(rowIndex, out T oldValue) &&
                _indexMap.TryGetValue(oldValue, out HashSet<int> indexes))
            {
                if (Comparer.Equals(value, oldValue))
                    return; // Don't set the same value twice

                indexes.Remove(rowIndex);
            }

            // Both the value and rowindex need to be stored seperately
            _valueMap[rowIndex] = value;
            if (!_indexMap.TryGetValue(value, out indexes))
            {
                indexes = new HashSet<int>();
                _indexMap.Add(value, indexes);
            }

            // Remove any null map references to this row
            RemoveNullAt(rowIndex);

            indexes.Add(rowIndex);
        }

        public void SetNull(int rowIndex)
        {
            if (IsNull(rowIndex))
                return; // Don't set the same value twice

            // Remove any value map references to this row after adding
            RemoveValueAt(rowIndex);

            _nullMap.Add(rowIndex);
        }

        public bool TryGetValue(int index, out T value) => _valueMap.TryGetValue(index, out value);

        public override string ToString()
        {
            return $"{Name} ({DataType.Name})";
        }

        #endregion

        #region ILookupColumn

        object ILookupColumn.GetAt(int rowIndex)
        {
            if (IsNull(rowIndex))
                return DBNull.Value;

            return GetAt(rowIndex);
        }

        void ILookupColumn.SetAt(int rowIndex, object value)
        {
            if (value == null)
            {
                SetNull(rowIndex);
            }
            else
            {
                SetAt(rowIndex, (T)value);
            }
        }

        void ILookupColumn.RemoveAt(int rowIndex)
        {
            RemoveValueAt(rowIndex);
            RemoveNullAt(rowIndex);
        }

        #endregion
    }
}
