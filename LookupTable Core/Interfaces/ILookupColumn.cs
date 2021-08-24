using System;

namespace Lookup.Interfaces
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
        bool IsNull(int rowIndex);
        void RemoveAt(int rowIndex);
        void SetAt(int rowIndex, object value);
        void SetNull(int rowIndex);

    }
}