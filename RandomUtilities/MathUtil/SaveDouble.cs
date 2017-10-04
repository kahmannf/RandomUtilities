using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil
{
    public struct SaveDouble : IComparable<SaveDouble>, IComparable
    {
        /// <summary>
        /// one tenth of a degree-second 
        /// (which is in latitudinal distance, and equivalently of longitudinal distance at the equator,
        /// a difference of approximately ten feet; that's about the accuracy of civilian GPS)
        /// 
        /// Copied from stackoverflow:
        /// https://stackoverflow.com/questions/10096930/how-do-i-know-if-two-line-segments-are-near-collinear
        /// 
        /// 
        /// I know its totally unrelated to double rounding Errors, but im going to use it until i have a better value
        /// Also, this struct was originally created for vector-calculations which makes this semi-ok....
        /// </summary>
        public const double SAVEDOUBLE_EXPECTED_ERROR = 0.000027;

        public double HighestValue => Value + (SAVEDOUBLE_EXPECTED_ERROR / 2);
        public double LowestValue => Value - (SAVEDOUBLE_EXPECTED_ERROR / 2);

        public double Value;

        public SaveDouble(double d)
        {
            Value = d;
        }

        #region Operators

        public static implicit operator SaveDouble(double d)
        {
            return new SaveDouble(d);
        }

        public static implicit operator SaveDouble(int i)
        {
            return new SaveDouble(i);
        }

        public static implicit operator double(SaveDouble sd)
        {
            return sd.Value;
        }

        public static explicit operator int(SaveDouble d)
        {
            return Convert.ToInt32(d.Value);
        }

        public static SaveDouble operator *(SaveDouble sd1, SaveDouble sd2)
        {
            return new SaveDouble(sd1.Value * sd2.Value);
        }

        public static SaveDouble operator /(SaveDouble sd1, SaveDouble sd2)
        {
            return new SaveDouble(sd1.Value / sd2.Value);
        }

        public static SaveDouble operator +(SaveDouble sd1, SaveDouble sd2)
        {
            return new SaveDouble(sd1.Value + sd2.Value);
        }

        public static SaveDouble operator -(SaveDouble sd1, SaveDouble sd2)
        {
            return new SaveDouble(sd1.Value - sd2.Value);
        }

        public static bool operator <(SaveDouble sd1, SaveDouble sd2)
        {
            return sd1 != sd2 && sd1.LowestValue < sd2.HighestValue;
        }

        public static bool operator >(SaveDouble sd1, SaveDouble sd2)
        {
            return sd1 != sd2 && sd1.HighestValue > sd2.LowestValue;
        }

        public static bool operator ==(SaveDouble sd1, SaveDouble sd2)
        {
            return sd1.LowestValue < sd2.HighestValue && sd1.HighestValue > sd2.LowestValue;
        }

        public static bool operator !=(SaveDouble sd1, SaveDouble sd2)
        {
            return sd1.LowestValue > sd2.HighestValue || sd1.HighestValue < sd2.LowestValue;
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        #region IComparable & IComparable<SaveDouble> Member

        public int CompareTo(SaveDouble other)
        {
            return Comparer<double>.Default.Compare(Value, other);
        }

        public int CompareTo(object obj)
        {
            if (obj is SaveDouble sd)
                return CompareTo(sd);
            else
            {
                throw new InvalidOperationException("Can not compare this value to a SaveDouble");
            }
        }
        
        #endregion
    }
}
