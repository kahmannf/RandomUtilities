using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil
{
    public class EquationSegment
    {
        public EquationSegment() : this(null, 0.0) { }

        public EquationSegment(char? unknown, SaveDouble valuefactor)
        {
            Unknown = unknown;
            ValueFactor = valuefactor;
        }

        /// <summary>
        /// The letter of the unknown. 
        /// Is null if the segment is a constant value.
        /// </summary>
        public char? Unknown;
        
        public bool Constant => !Unknown.HasValue; 

        /// <summary>
        /// Factor of the Unknown. Value if Constant
        /// </summary>
        public SaveDouble ValueFactor { get; set; }

        public EquationSegment Value => new EquationSegment(this.Unknown, this.ValueFactor);

        public override string ToString()
        {
            string result = ValueFactor.ToString();

            if (!Constant)
                result += Unknown;

            if ((double)ValueFactor < 0.0)
            {
                result = "- " + result.TrimStart('-');
            }
            else
            {
                result = "+ " + result.TrimStart();
            }

            return result.Trim();
        }

        public EquationSegment Negate()
        {
            EquationSegment result = this.Value;
            result.ValueFactor *= -1;
            return result;
        }

        public EquationSegment Divide(SaveDouble divideBy)
        {
            EquationSegment result = this.Value;
            result.ValueFactor /= divideBy;
            return result;
        }

        public EquationSegment AddUp(EquationSegment other)
        {
            if ((!Constant && !other.Constant) && Unknown.Value != other.Unknown.Value)
                throw new InvalidOperationException("Only Cosntant-EquationSegments or Segments with the same Unknown can be added up");

            return new EquationSegment(null, this.ValueFactor + other.ValueFactor);
        }
    }
}
