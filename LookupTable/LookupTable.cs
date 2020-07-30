using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Lookup
{
    public class LookupTable
    {
        #region Properties

        public LookupColumnCollection Columns { get; }

        public string Name { get; set; }

        public LookupRowCollection Rows { get; }

        #endregion

        #region Constructors

        public LookupTable()
        {
            Columns = new LookupColumnCollection(this);
            Rows = new LookupRowCollection(this);
        }

        public LookupTable(string name)
            : this()
        {
            Name = name;
        }

        public LookupTable(DataTable table)
            : this()
        {
            //table.cr
        }


        #endregion

        #region Methods

        public void Clear()
        {
            Columns.Clear();
            Rows.Clear();
        }

        internal object[] GetItems(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            object[] items = new object[Columns.Count];
            for (int i = 0; i < Columns.Count; i++)
            {
                ILookupColumn column = Columns[i];
                items[i] = column.GetAt(rowIndex);
            }

            return items;
        }

        internal T GetItem<T>(int columnIndex, int rowIndex) => Columns.GetAt<T>(columnIndex).GetAt(rowIndex);

        internal bool IsNull(int columnIndex, int rowIndex) => Columns.GetAt(columnIndex).IsNull(rowIndex);

        internal bool IsValidRowIndex(int rowIndex) => rowIndex >= 0 && rowIndex <= this.Rows.Count;

        internal void SetItem<T>(int columnIndex, int rowIndex, T value) => Columns.GetAt<T>(columnIndex).SetAt(rowIndex, value);

        #endregion
    }

    public class LookupRowCollection : IEnumerable<LookupRow>
    {

        #region Properties

        public LookupTable Table { get; }

        public int Count { get; private set; }

        #endregion

        #region Constructors

        internal LookupRowCollection(LookupTable table)
        {
            Table = table;

            Count = 0;
        }

        #endregion

        #region Methods

        public LookupRow Add(LookupRow item)
        {
            int newIndex = Count;

            // Adding a rows means iterating through all columns adding a null value
            foreach (ILookupColumn column in Table.Columns)
                column.SetAt(newIndex, null);

            Count++;
            return new LookupRow(this.Table, newIndex);
        }

        public void Remove(LookupRow item)
        {
            int oldIndex = item.Index;

            // Adding a rows means iterating through all columns removing the indexes
            foreach (ILookupColumn column in Table.Columns)
                column.RemoveAt(oldIndex);

            Count--;
        }

        public void Clear()
        {
            // Clearing the rows means clearing all columns
            for (int i = Table.Columns.Count - 1; i >= 0; i--)
            {
                Table.Columns[i].Clear();
            }

            Count = 0;
        }

        public LookupRow FindFirstRow<T>(string columnName, T value) => FindFirstRow(Table.Columns.GetIndex(columnName), value);

        public LookupRow FindFirstRow<T>(int columnIndex, T value) => FindRows(columnIndex, value).FirstOrDefault();

        public LookupRow FindLastRow<T>(string columnName, T value) => FindLastRow(Table.Columns.GetIndex(columnName), value);

        public LookupRow FindLastRow<T>(int columnIndex, T value) => FindRows(columnIndex, value).LastOrDefault();

        public IEnumerable<LookupRow> FindRows<T>(string columnName, T value) => FindRows(Table.Columns.GetIndex(columnName), value);

        public IEnumerable<LookupRow> FindRows<T>(int columnIndex, T value)
        {
            var column = Table.Columns.GetAt<T>(columnIndex);
            var indexes = column.FindIndexes(value).OrderBy(i => i);

            return indexes.OrderBy(i => i)
                          .Select(i => new LookupRow(this.Table, i));
        }

        #endregion

        #region IEnumerable

        public IEnumerator<LookupRow> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new LookupRow(this.Table, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public class LookupRow
    {
        #region Properties

        public LookupTable Table { get; }

        public int Index { get; }

        public object[] Items => GetItems();

        #endregion

        #region Constructors

        internal LookupRow(LookupTable table, int index)
        {
            Index = index;
            Table = table;
        }

        #endregion

        #region Methods

        public T Get<T>(string columnName) => Get<T>(Table.Columns.GetIndex(columnName));

        public T Get<T>(int columnIndex) => Table.GetItem<T>(columnIndex, this.Index);

        public object[] GetItems() => Table?.GetItems(this.Index);

        public bool IsNull(string columnName) => IsNull(Table.Columns.GetIndex(columnName));

        public bool IsNull(int columnIndex) => Table.IsNull(columnIndex, this.Index);

        public void Set<T>(string columnName, T value) => Set<T>(Table.Columns.GetIndex(columnName), value);

        public void Set<T>(int columnIndex, T value) => Table.SetItem<T>(columnIndex, this.Index, value);

        #endregion
    }

    public class LookupColumnCollection : IEnumerable<ILookupColumn>
    {
        private Dictionary<string, int> _columnMap;
        private List<ILookupColumn> _columns;

        public ILookupColumn this[string name]
        { 
            get => this[GetIndex(name)];
            set => throw new NotImplementedException();
        }

        public ILookupColumn this[int index]
        {
            get => GetAt(index);
            set => throw new NotImplementedException();
        }

        public int Count => _columns.Count;

        public LookupTable Table { get; }

        internal LookupColumnCollection(LookupTable table)
        {
            _columnMap = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            _columns = new List<ILookupColumn>();

            Table = table;
        }

        public int GetIndex(string columnName)
        {
            if (_columnMap.TryGetValue(columnName, out int columnIndex))
                return columnIndex;

            throw new KeyNotFoundException($"Column '{columnName}' does not exist.");
        }

        internal ILookupColumn GetAt(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count)
                throw new ArgumentOutOfRangeException(nameof(columnIndex));

            return _columns[columnIndex];
        }

        internal LookupColumn<T> GetAt<T>(int columnIndex)
        {
            ILookupColumn iColumn = GetAt(columnIndex);
            if (!(iColumn is LookupColumn<T> column))
                throw new Exception($"Invalid type '{typeof(T).FullName}' specified for column at {columnIndex}.");

            return column;
        }



        public LookupColumn<T> Add<T>(string name)
        {
            if (_columnMap.ContainsKey(name))
                throw new Exception($"Column {name} already exists.");

            return null;
            //var column = new LookupColumn<T>(this.Table, )

            //throw new NotImplementedException();
        }

        //public LookupColumn<T> Add

        public void Clear()
        {
            _columns.Clear();
            _columnMap.Clear();
        }

        public bool Contains(ILookupColumn item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ILookupColumn[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ILookupColumn> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(ILookupColumn item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ILookupColumn item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ILookupColumn item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

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

        public int Ordinal { get; }

        public LookupTable Table { get; }

        public IEqualityComparer<T> Comparer { get; }

        #endregion

        #region Constructors

        internal LookupColumn(LookupTable table)
            : this(table, EqualityComparer<T>.Default) { }

        internal LookupColumn(LookupTable table, IEqualityComparer<T> comparer)
        {
            Comparer = comparer;

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
            if (Table.IsValidRowIndex(rowIndex))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            // Check stored values first
            if (_valueMap.TryGetValue(rowIndex, out T value))
                return value;

            // Return NullValue when applicable
            if (!CanStoreNullValues && _nullMap.Contains(rowIndex))
                return NullValue;

            throw new Exception($"Unable to retrieve value at row {rowIndex}.");
        }

        public bool IsNull(int rowIndex)
        {
            if (!CanStoreNullValues)
                return _nullMap.Contains(rowIndex);

            return GetAt(rowIndex) == null;
        }

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
            if (Table.IsValidRowIndex(rowIndex))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

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
                if (!CanStoreNullValues)
                {
                    SetNull(rowIndex);
                }
                else
                {
                    SetAt(rowIndex, default(T));
                }
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