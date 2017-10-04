using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomUtilities.MathUtil;

namespace RandomUtilities.MathUtil.Vectors
{
    public class Vector2
    {
        public SaveDouble X { get; set; }
        public SaveDouble Y { get; set; }

        public SaveDouble Magnitude
        {
            get
            {
                if (NullVector)
                    return 0.0;
                else
                {
                    return Math.Sqrt(X * X + Y * Y);
                }
            }
        }

        public bool NullVector => X == 0.0 && Y == 0.0;

        public Vector2 Reverse()
        {
            return new Vector2().Subtract(this);
        }

        public bool IsParallel(Vector2 compareTo)
        {
            SaveDouble angle = this.GetCosinAngle(compareTo);
            return angle * angle == 1;
        }

        public SaveDouble ScalarMultiply(Vector2 other)
        {
            return this.X * other.X + this.Y * other.Y;
        }

        public SaveDouble GetCosinAngle(Vector2 compareTo)
        {
            return (this.ScalarMultiply(compareTo) / (this.Magnitude * compareTo.Magnitude));
        }

        public SaveDouble GetAngle(Vector2 compareTo)
        {
            SaveDouble result = Math.Acos(GetCosinAngle(compareTo));

            if (this.Rotate(result).GetCosinAngle(compareTo) == 1)
                return Util.NormalizeAngle(result + Math.PI);

            return result;
        }

        /// <summary>
        /// Returns a Copy of the current Vector
        /// </summary>
        public Vector2 Value { get { return new Vector2(X, Y); } }

        public Vector2() : this(0.0, 0.0) { }
        
        public Vector2(Size size) : this(size.Width, size.Height) { }
        public Vector2(SizeF size) : this(size.Width, size.Height) { }

        public Vector2(PointF p) : this(p.X, p.Y) { }

        public Vector2(SaveDouble x, SaveDouble y)
        {
            X = x;
            Y = y;
        }

        public Vector2 Scale(double d)
        {
            return new Vector2(X * d, Y * d);
        }

        public Vector2 Add(Vector2 v)
        {
            return new Vector2(X + v.X, Y + v.Y);
        }

        public Vector2 Subtract(Vector2 v)
        {
            return new Vector2(X - v.X, Y - v.Y);
        }

        #region Operators

        public static explicit operator PointF(Vector2 v)
        {
            try
            {
                return new PointF((float)v.X, (float)v.Y);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException("Casting the Vector to a PointF failed. See InnerException for details.", ex);
            }
        }

        public static explicit operator SizeF(Vector2 v)
        {
            try
            {
                return new SizeF((float)v.X, (float)v.Y);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException("Casting the Vector to a SizeF failed. See InnerException for details.", ex);
            }
        }

        public static implicit operator Vector2(Size s)
        {
            return new Vector2(s.Width, s.Height);
        }

        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        public Vector2 AsUnitVector()
        {
            SaveDouble length = Magnitude;

            if (length == 1.0 || length == 0.0)
                return this;

            return this.Scale(1 / length);
        }

        public Vector2 WithLength(double length)
        {
            return this.AsUnitVector().Scale(length);
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}";
        }

        public Vector2 Rotate(double angle)
        {

            //Calculation copied from:
            //http://www.chemgapedia.de/vsengine/vlu/vsc/de/ma/1/mc/ma_11/ma_11_03/ma_11_03_02.vlu.html

            angle = Util.NormalizeAngle(angle);

            //         x' = x * cos angle - y * sin angle 
            SaveDouble x = this.X * Math.Cos(angle) - this.Y * Math.Sin(angle);

            //         y = x * sin angle + y * cos angle 
            SaveDouble y = this.X * Math.Sin(angle) + this.Y * Math.Cos(angle);

            return new Vector2(x, y);
        }

        public SaveDouble GetCrossProduct(Vector2 other)
        {
            return this.X * other.Y - other.X * this.Y;
        }
        

        public Vector2 GetComponentVectorWithAngle(SaveDouble angle)
        {
            if (angle > Util.FULL_RADIANT || angle < 0.0)
            {
                throw new ArgumentOutOfRangeException("angle", "The angle cannot be smaller than zero or larger than Util.FULL_RADIANT (2 * Math.PI)");
            }

            if (angle == 0.0 || angle == Util.FULL_RADIANT)
                return this.Value;
            
            //180 degrees
            if(angle == Math.PI)
                return this.Reverse();

            //orthogonal (90/270 degrees)
            if (angle == Util.QUARTER_RADIANT || angle == 3 * Util.QUARTER_RADIANT)
                return new Vector2(0, 0);

            SaveDouble angleOrtho;

            if (angle < Util.QUARTER_RADIANT)
            {
                angleOrtho = 3 * Util.QUARTER_RADIANT + angle;
            }
            else if (angle < Math.PI) // larger than Util.QUARTER_RADIANT
            {
                angleOrtho = angle + Util.QUARTER_RADIANT;
            }
            else if (angle < Util.QUARTER_RADIANT + Math.PI) // larger than Math.PI
            {
                angleOrtho = angle - Util.QUARTER_RADIANT;
            }
            else // angle smaller than Util.FULL_RADIANT && angle larger than Math.PI + Util.QUARTER_RADIANT            {
            {
                angleOrtho = angle - (Util.QUARTER_RADIANT + Math.PI);
            }

            Vector2 componentDirectionVector = this.Rotate(angle).AsUnitVector();
            Vector2 orthoComponentDirectionVector = this.Rotate(angleOrtho).AsUnitVector();

            V2Straight componentStraight = new V2Straight(new Vector2(0, 0), componentDirectionVector.Reverse());
            V2Straight orthoComponentStraight = new V2Straight(this, orthoComponentDirectionVector.Add(this));

            Vector2 intersection = componentStraight.GetIntersection(orthoComponentStraight);

            return intersection;
        }
    }
}
