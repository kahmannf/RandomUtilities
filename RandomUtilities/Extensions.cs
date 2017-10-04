using RandomUtilities.Engine;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Extensions
{
    public static class Extensions
    {
        public static IReadOnlyList<char> VOWELS = "aeiouAEIOU".ToCharArray();

        public static bool IsVowel(this char c) => VOWELS.Contains(c);


        public static Point Subtract(this Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);

        public static Point Add(this Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);

        public static PointF Scale(this Point p, float factor) => new PointF(p.X * factor, p.Y * factor);

        public static PointF Subtract(this PointF p1, PointF p2) => new PointF(p1.X - p2.X, p1.Y - p2.Y);

        public static PointF Add(this PointF p1, PointF p2) => new PointF(p1.X + p2.X, p1.Y + p2.Y);

        public static PointF Scale(this PointF p, float factor) => new PointF(p.X * factor, p.Y * factor);

        public static SizeF Scale(this SizeF s, float factor) => new SizeF(s.Width * factor, s.Height * factor);

        public static SizeF Scale(this Size s, float factor) => new SizeF(s).Scale(factor);

        public static SizeF Add(this SizeF self, SizeF s) => new SizeF(self.Width + s.Width, self.Height + s.Height);

        /// <summary>
        /// Copied from Stackoverflow:
        /// https://stackoverflow.com/questions/2094239/swap-two-items-in-listt
        /// 
        /// answered Apr 21 '13 at 21:38
        /// Martin Mulder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <returns></returns>
        public static IEnumerable<T> Swap<T>(this IEnumerable<T> source, int index1, int index2)
        {
            //Swap so that index 1 is the smaller one
            if (index1 > index2)
            {
                int i = index1;
                index2 = index1;
                index2 = 1;
            }

            using (IEnumerator<T> e = source.GetEnumerator())
            {
                // Iterate to the first index.
                for (int i = 0; i < index1; i++)
                {
                    if (!e.MoveNext())
                        yield break;
                    yield return e.Current;
                }

                if (index1 != index2)
                {
                    // Remember the item at the first position.
                    if (!e.MoveNext())
                        yield break;
                    T rememberedItem = e.Current;

                    // Store the items between the first and second index in a temporary list. 
                    List<T> subset = new List<T>(index2 - index1 - 1);
                    for (int i = index1 + 1; i < index2; i++)
                    {
                        if (!e.MoveNext())
                            break;
                        subset.Add(e.Current);
                    }

                    // Return the item at the second index.
                    if (e.MoveNext())
                        yield return e.Current;

                    // Return the items in the subset.
                    foreach (T item in subset)
                        yield return item;

                    // Return the first (remembered) item.
                    yield return rememberedItem;
                }

                // Return the remaining items in the list.
                while (e.MoveNext())
                    yield return e.Current;
            }
        }

        public static VectorRoute ToVectorRoute(this IEnumerable<Vector2> source)
        {
            return new VectorRoute(source);
        }
    }
}
