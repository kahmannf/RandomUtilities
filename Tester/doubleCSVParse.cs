using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    public class DoubleCSVParse
    {
        public double Value { get; set; }

        public static DoubleCSVParse FromCSVString(string s)
        {
            s = s.TrimEnd(" EUR".ToCharArray());

            if (double.TryParse(s, out double d))
            {
                return new DoubleCSVParse() { Value = d };
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
