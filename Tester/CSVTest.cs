using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomUtilities.Serialization.CSV;

namespace Tester
{
    public class CSVTest
    {
        public string UnionDepotNr { get; set; }
        public DateTime Datum { get; set; }
        public string BuchungsArt { get; set; }
        public string Stuecke { get; set; }
        public DoubleCSVParse Abrechnungspreis { get; set; }
        public double Gegenwert { get; set; }
        public string Einheit { get; set; }

        internal static ColumnMapper<CSVTest> GetMapping()
        {
            ColumnMapper<CSVTest> mapper = new ColumnMapper<CSVTest>();

            mapper.Add(0, "UnionDepotNr");
            //mapper.Add(1, "Datum");
            //mapper.Add(2, "BuchungsArt");
            mapper.Add(3, "Stuecke");
            //mapper.Add(4, "Abrechnungspreis");
            //mapper.Add(5, "Gegenwert");
            //mapper.Add(6, "Einheit");

            return mapper;
        }
    }
}
