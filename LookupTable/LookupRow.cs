using System.Linq;

namespace Lookup
{
    public sealed class LookupRow
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

        public T Get<T>(string columnName) => Get<T>(Table.Columns.IndexOf(columnName));

        public T Get<T>(int columnIndex) => Table.GetItem<T>(columnIndex, this.Index);

        public object[] GetItems() => Table?.GetItems(this.Index);

        public bool HasValue<T>(string columnName, T value) => HasValue<T>(Table.Columns.IndexOf(columnName), value);

        public bool HasValue<T>(int columnIndex, T value) => Table.Columns.GetAt<T>(columnIndex).FindIndexes(value).Contains(this.Index);

        public bool IsNull(string columnName) => IsNull(Table.Columns.IndexOf(columnName));

        public bool IsNull(int columnIndex) => Table.IsNull(columnIndex, this.Index);

        public void Set<T>(string columnName, T value) => Set<T>(Table.Columns.IndexOf(columnName), value);

        public void Set<T>(int columnIndex, T value) => Table.SetItem<T>(columnIndex, this.Index, value);

        public void SetNull(string columnName) => SetNull(Table.Columns.IndexOf(columnName));

        public void SetNull(int columnIndex) => Table.SetNull(columnIndex, this.Index);

        public override string ToString() => string.Join(", ", GetItems().Select(o => o.ToString()));

        #endregion
    }
}
