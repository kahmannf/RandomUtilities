using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities
{
    public class PointTranslator
    {
        public PointTranslator()
        {
            CoordSystemOffsetToScreen = new Vector2();
        }

        /// <summary>
        /// Vector from the RenderOrigin of the Graphics to {0,0} of the coordinate system
        /// </summary>
        public Vector2 CoordSystemOffsetToScreen { get; set; }

        public Point CoordinateToGraphicsPoint(int xcoord, int ycoord)
        {
            return new Point(xcoord + Convert.ToInt32(CoordSystemOffsetToScreen.X), (-ycoord + Convert.ToInt32(CoordSystemOffsetToScreen.Y)));
        }

        public Point CoordinateToGraphicsPoint(Vector2 coords)
        {
            return CoordinateToGraphicsPoint(Convert.ToInt32(coords.X), Convert.ToInt32(coords.Y));
        }

        public Vector2 GraphicsPointsToVector2(Point p)
        {
            return new Vector2(p.X - Convert.ToInt32(CoordSystemOffsetToScreen.X), -(p.Y - Convert.ToInt32(CoordSystemOffsetToScreen.Y)));
        }
    }
}
