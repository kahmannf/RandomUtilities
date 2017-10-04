using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil.Visualization
{
    public class CoordSystemDrawerData
    {
        public CoordSystemDrawerData()
        {
            Points = new List<Vector2>();
            Straights = new List<V2Straight>();
            Shapes = new List<VectorRoute>();
            PointColor = System.Drawing.Color.Red;
            StraightColor = System.Drawing.Color.Blue;
            ShapeColor = System.Drawing.Color.Purple;
        }

        public List<Vector2> Points { get; set; }
        public List<V2Straight> Straights { get; set; }
        public List<VectorRoute> Shapes { get; set; }

        public System.Drawing.Color PointColor;
        public System.Drawing.Color StraightColor;
        public System.Drawing.Color ShapeColor;
    }
}
