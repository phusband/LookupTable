using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lookup;

namespace LookupTableConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new LookupTable("TestTable");
            test.Columns.Add<int>("Integers");
            test.Columns.Add<string>("Strings");
            test.Columns.Add<double>("Doubles");

            test.Columns.Remove("strings");
            test.Columns.RemoveAt(0);
            test.Columns.Insert<int>(0, "Integers");
            test.Columns.Insert<string>(1, "Strings");

            for (int i = 1; i < 100; i++)
            {
                var newRow = test.Rows.Add();

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
                    newRow.Set("Doubles", (double)i);
                }


                
                
            }

        }
    }
}
