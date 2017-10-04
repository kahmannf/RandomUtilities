using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil
{
    public static class Util
    {
        public const double FULL_RADIANT = 2 * Math.PI;

        public const double QUARTER_RADIANT = Math.PI / 2;

        public static T TakeLarger<T>(T value1, T value2)
        {
            if (Comparer<T>.Default is Comparer<T> comp)
                return TakeLarger(value1, value2, comp);

            throw new InvalidOperationException($"Cannot compare values of \"{typeof(T).ToString()}\" because there is no default comparer");
        }

        public static T TakeLarger<T>(T value1, T value2, Comparer<T> comp)
        {
            return comp.Compare(value1, value2) > 0 ? value1 : value2;
        }

        public static T TakeSmaller<T>(T value1, T value2)
        { 
            if (Comparer<T>.Default is Comparer<T> comp)
                return TakeSmaller(value1, value2, comp);

            throw new InvalidOperationException($"Cannot compare values of \"{typeof(T).ToString()}\" because there is no default comparer");
        }

        public static T TakeSmaller<T>(T value1, T value2, Comparer<T> comp)
        {
            return comp.Compare(value1, value2) < 0 ? value1 : value2;
        }

        public static SaveDouble DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static SaveDouble RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        /// <summary>
        /// Return a Representative of the angle speciffied within 0.0 - Util.FULL_RADIANT
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static SaveDouble NormalizeAngle(double angle)
        {
            while (angle < 0.0)
                angle += FULL_RADIANT;

            while (angle > FULL_RADIANT)
                angle -= FULL_RADIANT;

            return angle;
        }
    }
}
