using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil
{
    public class Equation
    {
        public Equation() : this(new List<EquationSegment>(), new List<EquationSegment>()) { }

        public Equation(IEnumerable<EquationSegment> leftSide, IEnumerable<EquationSegment> rigthSide)
        {
            _leftSide = new List<EquationSegment>(leftSide.Select(x => x.Value));
            _rigthSide = new List<EquationSegment>(rigthSide.Select(x => x.Value));
        }

        private List<EquationSegment> _leftSide;
        private List<EquationSegment> _rigthSide;

        public int UnknownCount => Unknowns.Count();

        public IEnumerable<EquationSegment> Unknowns => _leftSide.Where(x => !x.Constant).Union(_rigthSide.Where(x => !x.Constant));

        public IEnumerable<EquationSegment> Constants => _leftSide.Where(x => x.Constant).Union(_rigthSide.Where(x => x.Constant));

        public Equation Value => new Equation(_leftSide, _rigthSide);

        /// <summary>
        /// Sorts the Equation.
        /// all but the last unknown (alphabetically) are moved to the left side,
        /// the last one to the right
        /// </summary>
        public Equation Normalize()
        {
            Equation equ = this.Value;
            equ.NormalizeInternal();
            return equ;
        }

        /// <summary>
        /// Sorts the Equation.
        /// all but the specified unknown are moved to the left side,
        /// the specified one to the right
        /// </summary>
        public Equation Normalize(char c)
        {
            Equation equ = this.Value;
            equ.NormalizeInternal(c);
            return equ;
        }

        public Equation Normalize(IEnumerable<char> allUnknowns)
        {
            Equation equ = this.Value;
            equ.AddUnknowns(allUnknowns);
            equ.NormalizeInternal();
            return equ;
        }

        public Equation Normalize(IEnumerable<char> allUnknowns, char target)
        {
            Equation equ = this.Value;
            equ.AddUnknowns(allUnknowns);
            equ.NormalizeInternal(target);
            return equ;
        }

        private void AddUnknowns(IEnumerable<char> allUnknowns)
        {
            foreach (char unknown in allUnknowns)
            {
                if (!(Unknowns.FirstOrDefault(x => x.Unknown.Value == unknown) is EquationSegment))
                {
                    _leftSide.Add(new EquationSegment(unknown, 0.0));
                }
            }
        }

        private void NormalizeInternal()
        {
            //Create List here because the Enumerable is going to change
            List<char> unknowns = Unknowns.Select(x => x.Unknown.Value).ToList();

            int count = UnknownCount;

            List<EquationSegment> newLeftSide = new List<EquationSegment>();

            foreach (char unknown in unknowns.OrderBy(x => x))
            {
                IEnumerable<EquationSegment> segments = RemoveFromEquation(unknown, Sides.Left);

                EquationSegment finalSegment = segments.Aggregate((x, y) => x.AddUp(y));

                newLeftSide.Add(finalSegment);
            }


            IEnumerable<EquationSegment> constants = RemoveFromEquation(null, Sides.Right); 

            _rigthSide.Clear();

            if (constants.Count() > 0)
                _rigthSide.Add(constants.Aggregate((x, y) => x.AddUp(y)));
            else
                _rigthSide.Add(new EquationSegment(null, 0.0));

            _leftSide = newLeftSide.OrderBy(x => x.Constant ? char.MaxValue : x.Unknown.Value).ToList();
        }

        private void NormalizeInternal(char c)
        {
            NormalizeInternal();
            MoveToSideInternal(Sides.Right, Unknowns.First(x => x.Unknown.Value == c));
        }

        private enum Sides { Left, Right }

        private void MoveToSideInternal(Sides side, EquationSegment segment)
        {
            List<EquationSegment> destination, other;

            if (side == Sides.Left)
            {
                other = _rigthSide;
                destination = _leftSide;
            }
            else
            {
                other = _leftSide;
                destination = _rigthSide;
            }

            if (!destination.Contains(segment))
            {
                other.Remove(segment);
                destination.Add(segment.Negate());
            }
        }

        /// <summary>
        /// Removes all values for that unknown from the Equation
        /// all Segemnts will be unified for one side (unificationSide)
        /// => they can be added on that side again
        /// </summary>
        /// <param name="c"></param>
        /// <param name="unificationSide"></param>
        /// <returns></returns>
        private IEnumerable<EquationSegment> RemoveFromEquation(char? c, Sides unificationSide)
        {
            List<EquationSegment> segments;

            if (c.HasValue)
                segments = new List<EquationSegment>(Unknowns.Where(x => x.Unknown.Value == c));
            else
                segments = new List<EquationSegment>(Constants);

            List<EquationSegment> result = new List<EquationSegment>();

            List<EquationSegment> destination, other;

            if (unificationSide == Sides.Left)
            {
                destination = _leftSide;
                other = _rigthSide;
            }
            else
            {
                destination = _rigthSide;
                other = _leftSide;
            }

            foreach (EquationSegment segment in segments)
            {
                if (destination.Contains(segment))
                {
                    destination.Remove(segment);
                    result.Add(segment);
                }
                else
                {
                    other.Remove(segment);
                    result.Add(segment.Negate());
                }
            }

            return result;
        }

        public override string ToString()
        {
            string result = string.Empty;

            if (_leftSide.Count == 0)
            {
                result += "0 ";
            }
            else
            {
                result += _leftSide[0].ToString().TrimStart('+');

                result += " ";

                foreach (EquationSegment es in _leftSide.Skip(1))
                {
                    result += es.ToString() + " ";
                }
            }

            result += "= ";

            if (_rigthSide.Count == 0)
            {
                result += "0";
            }
            else
            {
                foreach (EquationSegment es in _rigthSide)
                {
                    result += es.ToString() + " ";
                }
            }

            return result.Trim();
        }

        public static Equation FromString(string s)
        {
            if (!s.Contains('=') || s.IndexOf('=') != s.LastIndexOf('='))
            {
                throw new InvalidCastException("There was more ore less than one \"=\"");
            }

            string[] sides = s.Split('=');

            Func<string, List<EquationSegment>> parseSide = (string side) =>
            {
                List<EquationSegment> result = new List<EquationSegment>();

                //Whitespace after string is required to parse alst argument if it is only a number;
                side = side.Trim() + " ";

                //if first Segment is negative, new instance qill be created inside the for-loop
                EquationSegment current = side[0] == '-' ? null : new EquationSegment(null, 1);

                string digitString = string.Empty;

                bool negative = false;

                for (int i = 0; i < side.Length; i++)
                {
                    if (side[i] == '-' || side[i] == '+')
                    {
                        if(current != null)
                            result.Add(current);

                        current = new EquationSegment(null, 1);

                        if (side[i] == '-')
                            negative = true;
                    }
                    else if (char.IsDigit(side[i]))
                        digitString += side[i];
                    else
                    {
                        if (!string.IsNullOrEmpty(digitString))
                        {
                            current.ValueFactor = Int32.Parse(digitString);
                            if (negative)
                            {
                                current = current.Negate();
                                negative = false;
                            }
                            digitString = string.Empty;
                        }

                        if (char.IsLetter(side[i]))
                        {
                            current.Unknown = side[i];
                        } 
                    }

                    if (i == side.Length - 1)
                        result.Add(current);
                }

                return result;
                
            };

            return new Equation(parseSide(sides[0]), parseSide(sides[1]));
        }

        public bool TrySolve(out SaveDouble result)
        {
            result = 0.0;
            if (this.UnknownCount != 1)
            {
                return false;
            }
            else
            {
                Equation equ = this.Normalize();

                if (equ[0].ValueFactor == 0)
                    return false;

                result = equ[1].ValueFactor / equ[0].ValueFactor;
                return true;
            }
        }

        public EquationSegment this[int index]
        {
            get
            {
                if (index > _leftSide.Count - 1)
                    return _rigthSide[index - _leftSide.Count];
                else
                    return _leftSide[index];
            }
        }
    }
}
