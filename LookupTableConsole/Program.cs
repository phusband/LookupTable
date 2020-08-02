using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lookup;

using System.Diagnostics;

namespace LookupTableConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var size = 10000;
            LookupTableHasValue(size);
            DataTableSelect(size);
            DataTableEnumerable(size);
            
            Console.ReadLine();

        }

        static DataTable CreateDataTable(int size)
        {
            var table = new DataTable("DataTable");
            table.Columns.Add("Integers", typeof(int));
            table.Columns.Add("Strings", typeof(string));
            table.Columns.Add("Doubles", typeof(double));

            table.BeginLoadData();

            for (int i = 1; i < size; i++)
            {
                object[] items = new object[3];

                if (i % 5 == 0)
                {
                    items[0] = i;
                    items[1] = i.ToString();
                    items[2] = DBNull.Value;
                }
                else
                {
                    items[0] = i;
                    items[1] = i.ToString();
                    items[2] = (i + Math.PI) / 2;
                }

                table.Rows.Add(items);
            }

            table.EndLoadData();
            return table;
        }

        static bool DataTableSelect(int size)
        {
            var dataTable = CreateDataTable(size);
            var sw = Stopwatch.StartNew();
            var selectedAll = dataTable.Select("Strings = '5000'").Any() &&
                              dataTable.Select("Integers = 5000").Any() &&
                              dataTable.Select("Strings = '250'").Any() &&
                              dataTable.Select("Integers = 250").Any() &&
                              dataTable.Select("Strings = '9250'").Any() &&
                              dataTable.Select("Integers = 9250").Any();

            Console.WriteLine($"DataTable Select:\t{sw.ElapsedTicks}");
            return selectedAll;
        }

        static bool DataTableEnumerable(int size)
        {            
            var dataTable2 = CreateDataTable(size);
            var sw = Stopwatch.StartNew();
            var foundAll = dataTable2.AsEnumerable().Any(r => r.Field<string>("Strings") == "5000") &&
                           dataTable2.AsEnumerable().Any(r => r.Field<int>("Integers") == 5000) &&
                           dataTable2.AsEnumerable().Any(r => r.Field<string>("Strings") == "250") &&
                           dataTable2.AsEnumerable().Any(r => r.Field<int>("Integers") == 250) &&
                           dataTable2.AsEnumerable().Any(r => r.Field<string>("Strings") == "9250") &&
                           dataTable2.AsEnumerable().Any(r => r.Field<int>("Integers") == 9250);

            Console.WriteLine($"DataTable Enumerable:\t{sw.ElapsedTicks}");
            return foundAll;
        }

        static bool LookupTableHasValue(int size)
        {            
            var lookupTable = CreateLookupTable(size);
            var sw = Stopwatch.StartNew();
            var lookedUpAll = lookupTable.Columns.Get<string>("Strings").HasValue("5000") &&
                              lookupTable.Columns.Get<int>("Integers").HasValue(5000) &&
                              lookupTable.Columns.Get<string>("Strings").HasValue("250") &&
                              lookupTable.Columns.Get<int>("Integers").HasValue(250) &&
                              lookupTable.Columns.Get<string>("Strings").HasValue("9250") &&
                              lookupTable.Columns.Get<int>("Integers").HasValue(9250);

            Console.WriteLine($"LookupTable HasValue:\t{sw.ElapsedTicks}");
            return lookedUpAll;
        }

        static LookupTable CreateLookupTable(int size)
        {
            var table = new LookupTable("LookupTable", size);
            table.Columns.Add<int>("Integers");
            table.Columns.Add<string>("Strings");
            table.Columns.Add<double>("Doubles");

            for (int i = 1; i < size; i++)
            {
                var newRow = table.Rows.Add();
                if (i % 5 == 0)
                {
                    newRow.Set("Integers", i);
                    newRow.Set("Strings", i.ToString());
                    newRow.SetNull("Doubles");
                }
                else
                {
                    newRow.Set("Integers", i);
                    newRow.Set("Strings", i.ToString());
                    newRow.Set("Doubles", (i + Math.PI) / 2);
                }
            }

            return table;
        }

        static LookupTable CreateLookupTableFromDataTable(DataTable table)
        {
            return new LookupTable(table);
        }
    }
}
