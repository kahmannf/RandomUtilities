using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil.Vectors
{
    public class V2Straight
    {
        private Vector2 _v1;
        private Vector2 _v2;

        public Vector2 Start => _isLimited ? _v1.Value : throw new InvalidOperationException("This straight is not limited and has no start");
        public Vector2 End => _isLimited ? _v2.Value : throw new InvalidOperationException("This straight is not limited and has no end");

        public Tuple<Vector2, Vector2> TwoPoints => new Tuple<Vector2, Vector2>(_baseVector.Value, PointAtScaleFactor(1.0));   

        private Vector2 _baseVector;
        private Vector2 _scaleVector;

        private bool _isLimited;

        /// <summary>
        /// A Vector which contains the maximum possible x and y value for a vector on this line
        /// </summary>
        public Vector2 MaxLimit
        {
            get
            {
                if (!_isLimited)
                    return new Vector2(double.MaxValue, double.MaxValue);
                else
                    return new Vector2(Util.TakeLarger(_v1.X, _v2.X), Util.TakeLarger(_v1.Y, _v2.Y));
            }
        }

        /// <summary>
        /// A Vector which contains the minimum possible x and y value for a vector on this line
        /// </summary>
        public Vector2 MinLimit
        {
            get
            {
                if (!_isLimited)
                    return new Vector2(double.MinValue, double.MinValue);
                else
                    return new Vector2(Util.TakeSmaller(_v1.X, _v2.X), Util.TakeSmaller(_v1.Y, _v2.Y));
            }
        }

        public bool IsLimited => _isLimited;

        /// <summary>
        /// Returns a list of EquationSegments that can be used in a linear EquationSystem
        /// </summary>
        /// <param name="variableChar"></param>
        /// <returns></returns>
        public List<EquationSegment> GetXSegments(char variableChar) => 
            new List<EquationSegment>()
            {
                new EquationSegment(null, _baseVector.X),
                new EquationSegment(variableChar, _scaleVector.X)
            };

        /// <summary>
        /// Returns a list of EquationSegments that can be used in a linear EquationSystem
        /// </summary>
        /// <param name="variableChar"></param>
        /// <returns></returns>
        public List<EquationSegment> GetYSegments(char variableChar) =>
            new List<EquationSegment>()
            {
                new EquationSegment(null, _baseVector.Y),
                new EquationSegment(variableChar, _scaleVector.Y)
            };

        private V2Straight(Vector2 baseVector)
        {
            _isLimited = false;
            _baseVector = baseVector.Value;
        }

        public V2Straight(Vector2 baseVector, Vector2 pointOnStraight) : this(baseVector, pointOnStraight, false)
        {
        }

        public V2Straight(Vector2 baseVector, Vector2 pointOnStraight, bool limitStraight)
        {
            if (baseVector == pointOnStraight) // _scaleVector would be a NullVector which would not make sense
            {
                throw new ArgumentException("The two vectors can not have the same value!");
            }

            _isLimited = limitStraight;

            _v1 = baseVector.Value;
            _v2 = pointOnStraight.Value;

            _baseVector = baseVector.Value;

            _scaleVector = baseVector.Subtract(pointOnStraight).AsUnitVector();
            
            //if (_scaleVector.Magnitude < 0.0)
            //    _scaleVector = _scaleVector.Reverse();
        }

        public SaveDouble GetDegree(V2Straight compareTo)
        {
            return GetDegree(compareTo._scaleVector);
        }

        public SaveDouble GetDegree(Vector2 vector)
        {
            return _scaleVector.GetCosinAngle(vector);
        }

        public bool IsParallel(V2Straight compareTo)
        {
            return this.IsParallel(compareTo._scaleVector);
        }

        internal bool IsPointAtScaleFactorOnStraight(double scaleFactor)
        {
            return this.IsPointOnStraight(this.PointAtScaleFactor(scaleFactor));
        }

        public bool IsParallel(Vector2 compareTo)
        {
            return _scaleVector.IsParallel(compareTo);
        }

        public Vector2 GetIntersection(V2Straight otherStraight)
        {
            if (IsParallel(otherStraight))
                throw new InvalidOperationException("Cannot get the intersection of two parallel straights");

            LinearEquationSystem les = LinearEquationSystem.FromVStraights(this, 'r', otherStraight, 's');

            Dictionary<char, SaveDouble> result = les.SolveGaussian();

            if (result.ContainsKey('r'))
            {
                return this.PointAtScaleFactor(result['r']);
            }
            else if (result.ContainsKey('s')) //Should not be possible to end up here, but just in case....
            {
                return otherStraight.PointAtScaleFactor(result['s']);
            }
            else
            {
                throw new Exception("Could not solve to LinearEquationSystem for these two straights.");
            }
        }

        public bool TryGetIntersection(V2Straight otherStraight, out Vector2 intersection)
        {
            if (IsParallel(otherStraight))
            {
                intersection = null;
                return false;
            }

            LinearEquationSystem les = LinearEquationSystem.FromVStraights(this, 'r', otherStraight, 's');

            Dictionary<char, SaveDouble> result = les.SolveGaussian();

            if (result.ContainsKey('r'))
            {
                intersection = this.PointAtScaleFactor(result['r']);
                return true;
            }
            else if (result.ContainsKey('s')) //Should not be possible to end up here, but just in case....
            {
                intersection = otherStraight.PointAtScaleFactor(result['s']);
                return true;
            }
            else
            {
                //intersection = null;
                //return false;
                throw new Exception("Could solve the LinearEquations");
            }
        }

        public Vector2 PointAtScaleFactor(double scaleFactor)
        {
            return _baseVector.Add(_scaleVector.Scale(scaleFactor));
        }

        public bool IsPointOnStraight(Vector2 point)
        {
            //If limited, use this quick check to exclude some points
            if (_isLimited && (point.X < MinLimit.X || point.Y < MinLimit.Y || point.X > MaxLimit.X || point.Y > MaxLimit.Y))
                return false;

            #region Stolen code. Not sure if that work correct

            ////Stackoverflow : 
            ////https://stackoverflow.com/questions/907390/how-can-i-tell-if-a-point-belongs-to-a-certain-line
            ////
            ////Given:

            ////Point p(X= 4, Y= 5)
            ////Line l(Slope= 1, YIntersect= 1)
            ////Plug in X and Y:

            ////Y = Slope * X + YIntersect
            ////=> 5 = 1 * 4 + 1
            ////=> 5 = 5

            ////If your lines are represented in (X1, Y1),(X2, Y2) form, then you can calculate slope with:

            ////Slope = (y1 - y2) / (x1 - x2)
            ////And then get the Y - Intersect with this:

            ////Y - Intersect = -Slope * X1 + Y1;

            ////You'll have to check that x1 - x2 is not 0. 
            ////If it is, then checking if the point is on the line is a simple matter of checking if the 
            ////Y value in your point is equal to either x1 or x2. Also, check that the X of the point is not 'x1' or 'x2'

            //Tuple<Vector2, Vector2> pointsOnLine = TwoPoints;

            //if (pointsOnLine.Item1.X - pointsOnLine.Item2.X == 0.0)
            //{
            //    return point.Y == pointsOnLine.Item1.X || point.Y == pointsOnLine.Item2.X;
            //}

            //SaveDouble slope = (pointsOnLine.Item1.Y - pointsOnLine.Item2.Y) / (pointsOnLine.Item1.X - pointsOnLine.Item2.X);

            //SaveDouble yintesect = -slope * pointsOnLine.Item1.X + pointsOnLine.Item1.Y;

            //return point.Y == (slope * point.X + yintesect);

            #endregion


            Equation equ1 = new Equation(new EquationSegment[] { new EquationSegment(null, _baseVector.X), new EquationSegment('r', _scaleVector.X) }, new EquationSegment[] { new EquationSegment(null, point.X) });
            Equation equ2 = new Equation(new EquationSegment[] { new EquationSegment(null, _baseVector.Y), new EquationSegment('r', _scaleVector.Y) }, new EquationSegment[] { new EquationSegment(null, point.Y) });

            if (equ1.TrySolve(out SaveDouble sd1) && equ1.TrySolve(out SaveDouble sd2) && sd1 == sd2)
                return true;

            return false;
        }

        public static V2Straight FromBaseAndScaleVector(Vector2 baseVector, Vector2 scaleVector)
        {
            V2Straight result = new V2Straight(baseVector);

            result._scaleVector = scaleVector.AsUnitVector();

            return result;
        }
    }
}
