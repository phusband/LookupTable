using System;
using System.Data;
using Lookup.Interfaces;

namespace Lookup
{
    public class LookupTable
    {
        #region Properties

        public LookupColumnCollection Columns { get; }

        public int DefaultColumnSize { get; set; }

        public string Name { get; set; }

        public LookupRowCollection Rows { get; }

        #endregion

        #region Constructors

        public LookupTable(int size = 0)
        {
            DefaultColumnSize = size > 0 
                ? size
                : 1000;

            Columns = new LookupColumnCollection(this);
            Rows = new LookupRowCollection(this);
        }

        public LookupTable(string name, int size = 0) : this(size)
        {
            Name = name;
        }

        public LookupTable(DataTable table) : this()
        {
            this.Name = table.TableName;
            DefaultColumnSize = table.Rows.Count;

            foreach (DataColumn dataColumn in table.Columns)
            {
                var column = LookupColumn.CreateFromType( dataColumn.DataType, this, dataColumn.ColumnName);
                this.Columns.Add(column);
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                this.Rows.Add();
                foreach(ILookupColumn column in this.Columns)
                    column.SetAt(i, table.Rows[i][column.Ordinal]);
            }
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

        internal void SetNull(int columnIndex, int rowIndex) => Columns.GetAt(columnIndex).SetNull(rowIndex);

        #endregion
    }
}