using RandomUtilities.Serialization.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvPathFormat = @"C:\Users\{0}\Downloads\Umsaetze_98845548_2017-10-23_1508765609142.csv";

            string csvPath = string.Format(csvPathFormat, "Username");

            ColumnMapper<CSVTest> mapping = CSVTest.GetMapping();

            CSVReader<CSVTest> reader = new CSVReader<CSVTest>(mapping);

            reader.HandleUnknownType += (type, value) =>
            {
                if (type == typeof(DoubleCSVParse))
                {
                    return DoubleCSVParse.FromCSVString(value);
                }
                else if (type == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, out DateTime date))
                    {
                        return date;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                {
                    return null;
                }
            };

            reader.HeaderRow = true;

            Stream s = new FileStream(csvPath, FileMode.Open);



            CSVTest[] result = reader.Read(s);
        }
    }
}
