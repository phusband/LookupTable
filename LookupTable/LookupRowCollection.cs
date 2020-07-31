using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lookup
{
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

        public LookupRow Add()
        {
            int newIndex = Count;

            // Adding a rows means iterating through all columns adding a null value
            foreach (ILookupColumn column in Table.Columns)
                column.SetAt(newIndex, null);

            Count++;
            return new LookupRow(this.Table, newIndex);
        }

        public void Remove(LookupRow item) => RemoveAt(item.Index);

        public void RemoveAt(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= Count)
                throw new ArgumentNullException(nameof(rowIndex));

            // Removing a row means iterating through all columns removing the indexes
            foreach (ILookupColumn column in Table.Columns)
                column.RemoveAt(rowIndex);

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

        public LookupRow FindFirstRow<T>(string columnName, T value) => FindFirstRow(Table.Columns.IndexOf(columnName), value);

        public LookupRow FindFirstRow<T>(int columnIndex, T value) => FindRows(columnIndex, value).FirstOrDefault();

        public LookupRow FindLastRow<T>(string columnName, T value) => FindLastRow(Table.Columns.IndexOf(columnName), value);

        public LookupRow FindLastRow<T>(int columnIndex, T value) => FindRows(columnIndex, value).LastOrDefault();

        public IEnumerable<LookupRow> FindRows<T>(string columnName, T value) => FindRows(Table.Columns.IndexOf(columnName), value);

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
}
