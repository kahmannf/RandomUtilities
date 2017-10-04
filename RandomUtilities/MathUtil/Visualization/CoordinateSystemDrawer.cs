using RandomUtilities.Extensions;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil.Visualization
{
    public delegate CoordSystemDrawerData CoordSystemDataEventDelegate();
    public delegate Graphics CoordSystemUpdateEventDelegate(Bitmap current);
    public enum CoordinateSystemDrawerModes
    {
        /// <summary>
        /// Coordinate Drawer will draw tha Data 
        /// which was added manually by calling a Add Methode
        /// </summary>
        FromStaticData,
        /// <summary>
        /// CoordinateDrawer will request the Data with the "DrawDataRequest" event
        /// </summary>
        FromDynamicData
    }
    public class CoordinateSystemDrawer
    {
        public event CoordSystemDataEventDelegate DrawDataRequest;
        public event CoordSystemUpdateEventDelegate CurrentChanged;

        public CoordinateSystemDrawerModes Mode { get; set; }

        public CoordinateSystemDrawer() : this(CoordinateSystemDrawerModes.FromStaticData)
        {
        }

        public CoordinateSystemDrawer(CoordinateSystemDrawerModes mode)
        {
            _gridColor = Color.LightBlue;
            GridSpacing = 10;
            PixelPerPoint = 1;
            translator = new PointTranslator();
            SetRenderSize(new Vector2(1920, 1080));
            Mode = mode;
            _staticDrawData = new CoordSystemDrawerData();
            CreatePens();
            Draw(_staticDrawData);
        }

        private void CreatePens()
        {
            _axisPen = new Pen(_gridColor, 2f);
            _gridPen = new Pen(_gridColor, 1f);
        }

        #region Bounds

        private Vector2 _topLeftCorner => translator.GraphicsPointsToVector2(new Point(0, 0));
        private Vector2 _topRightCorner => translator.GraphicsPointsToVector2(new Point(Convert.ToInt32(_renderSize.X), 0));
        private Vector2 _bottomLeftCorner => translator.GraphicsPointsToVector2(new Point(0, Convert.ToInt32(_renderSize.Y)));
        private Vector2 _bottomRightCorner => translator.GraphicsPointsToVector2(new Point(Convert.ToInt32(_renderSize.X), Convert.ToInt32(_renderSize.Y)));


        public V2Straight UpperBound => new V2Straight(_topLeftCorner, _topRightCorner, true);

        public V2Straight LowerBound => new V2Straight(_bottomLeftCorner, _bottomRightCorner, true);

        public V2Straight LeftBound => new V2Straight(_topLeftCorner, _bottomLeftCorner, true);

        public V2Straight RightBound => new V2Straight(_topRightCorner, _bottomRightCorner, true);

        public V2Straight[] Bounds => new V2Straight[] { UpperBound, LowerBound, LeftBound, RightBound };

        #endregion


        private Bitmap _current;
        public Bitmap Current => _current;

        public bool DrawGrid { get; set; }

        private int _gridSpacing;
        public int GridSpacing
        {
            get => _gridSpacing;
            set
            {
                if (value > 0)
                    _gridSpacing = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector2 _renderSize;

        public Vector2 RenderSize => _renderSize.Value;


        private double _pixelPerPoint;

        /// <summary>
        /// Amount of Pixel per Point.
        /// </summary>
        public double PixelPerPoint
        {
            get { return _pixelPerPoint; }
            set
            {
                if (value > 0.0)
                {
                    _pixelPerPoint = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }


        public void SetRenderSize(Vector2 size)
        {
            if (size.X < 2 || size.Y < 2)
            {
                throw new ArgumentException("The X and Y components of the size must be 2 or larger", "size");
            }

            _renderSize = size.Value;
            translator.CoordSystemOffsetToScreen = RenderSize.Scale(0.5);
        }

        public void ForceRefresh()
        {
            switch (Mode)
            {
                case CoordinateSystemDrawerModes.FromStaticData:
                    Draw(_staticDrawData);
                    break;
                case CoordinateSystemDrawerModes.FromDynamicData:
                    if (DrawDataRequest?.Invoke() is CoordSystemDrawerData data)
                    {
                        Draw(data);
                    }
                    else
                    {
                        DrawEmpty();
                    }
                    break;
            }
        }

        private CoordSystemDrawerData _staticDrawData;

        public void AddPoint(Vector2 vector)
        {
            if (!(_staticDrawData.Points.Any(x => x == vector)))
            {
                _staticDrawData.Points.Add(vector);
            }
        }

        public void AddStraight(V2Straight straight)
        {
            if (!(_staticDrawData.Straights.Any(x => x == straight)))
            {
                _staticDrawData.Straights.Add(straight);
            }
        }

        public void AddShape(VectorRoute shape)
        {
            if (!(_staticDrawData.Shapes.Any(x => x == shape)))
            {
                _staticDrawData.Shapes.Add(shape);
            }
        }

        private Color _gridColor;
        public Color GridColor
        {
            get => _gridColor;
            set
            {
                _gridColor = value;
                CreatePens();
            }
        }

        private Pen _axisPen;
        private Pen _gridPen;

        private void DrawEmpty()
        {
            //Get even width and heigth
            int xint = (((int)_renderSize.X) / 2) * 2;
            int yint = (((int)_renderSize.Y) / 2) * 2;

            Bitmap next = new Bitmap(xint, yint);

            using (Graphics g = Graphics.FromImage(next))
            {
                //backgroud
                g.FillRectangle(new SolidBrush(Color.White), new RectangleF(new Point(0, 0), new Size(xint, yint)));

                //x-axis
                g.DrawLine(_axisPen, new Point(0, yint / 2), new Point(xint, yint / 2));

                //y-axis
                g.DrawLine(_axisPen, new Point(xint / 2, 0), new Point(xint / 2, yint));

                if (DrawGrid)
                {
                    int gridSpacing = Convert.ToInt32(GridSpacing * PixelPerPoint);

                    //grid parallel to x-axis and below x-axis
                    for (int y = (yint / 2) + gridSpacing; y < yint; y += gridSpacing)
                    {
                        g.DrawLine(_gridPen, new Point(0, y), new Point(xint, y));
                    }

                    //grid parallel to x-axis and above x-axis
                    for (int y = (yint / 2) - gridSpacing; y >= 0; y -= gridSpacing)
                    {
                        g.DrawLine(_gridPen, new Point(0, y), new Point(xint, y));
                    }

                    //grid parallel to y-axis and right to y-axis
                    for (int x = (xint / 2) + gridSpacing; x < xint; x += gridSpacing)
                    {
                        g.DrawLine(_gridPen, new Point(x, 0), new Point(x, yint));
                    }

                    //grid parallel to y-axis and laft to y-axis
                    for (int x = (xint / 2) - gridSpacing; x >= 0; x -= gridSpacing)
                    {
                        g.DrawLine(_gridPen, new Point(x, 0), new Point(x, yint));
                    }
                }
            }

            Bitmap old = _current;

            _current = next;

            if (old != null)
            {
                old.Dispose();
                old = null;
            }
        }

        PointTranslator translator;

        private void DrawPoint(Vector2 vector, Color color, Size pointSize, Graphics g)
        {

            Point location = translator.CoordinateToGraphicsPoint(vector.Scale(PixelPerPoint)).Subtract(new Point(pointSize.Width / 2, pointSize.Height / 2));

            g.FillRectangle(new SolidBrush(color), new RectangleF(location, pointSize));
        }

        private void DrawStraight(V2Straight straight, Color color, Size pointSize, int straightThickness, Graphics g)
        {
            if (straight.IsLimited)
            {
                Point start = translator.CoordinateToGraphicsPoint(straight.Start.Scale(PixelPerPoint));
                Point end = translator.CoordinateToGraphicsPoint(straight.End.Scale(PixelPerPoint));

                g.DrawLine(new Pen(color), start, end);

                DrawPoint(straight.Start, color, pointSize, g);
                DrawPoint(straight.End, color, pointSize, g);
            }
            else
            {
                List<Vector2> resultPoints = new List<Vector2>();

                foreach (V2Straight s in Bounds)
                {
                    if (s.TryGetIntersection(straight, out Vector2 result) && s.IsPointOnStraight(result))
                    {
                        resultPoints.Add(result);
                    }
                }

                if (resultPoints.Count == 2)
                {
                    Point start = translator.CoordinateToGraphicsPoint(resultPoints[0]);
                    Point end = translator.CoordinateToGraphicsPoint(resultPoints[1]);

                    g.DrawLine(new Pen(color), start, end);
                }
                else
                {
                    throw new Exception("Something went wrong: Ended up with more than two intersections for the bound calculation");
                }
            }
        }

        private void DrawShape(VectorRoute shape, Color color, Size pointSize, int straightsThickness, Graphics g)
        {
            if (shape.Count == 1)
            {
                DrawPoint(shape.TotalPosition(0), color, pointSize, g);
            }
            else if(shape.Count > 1)
            {

                foreach (V2Straight side in shape.GetAllSides())
                {
                    DrawStraight(side, color, pointSize, straightsThickness, g);
                }
            }
        }

        private void Draw(CoordSystemDrawerData data)
        {
            try
            {
                DrawEmpty();

                using (Graphics g = Graphics.FromImage(_current))
                {
                    Size pointSize = new Size(4, 4);

                    foreach (Vector2 v in data.Points)
                    {
                        DrawPoint(v, data.PointColor, pointSize, g);
                    }

                    Size straightsPointSize = new Size(2, 2);

                    int straightThickness = 1;

                    foreach (V2Straight straight in data.Straights)
                    {
                        DrawStraight(straight, data.StraightColor, straightsPointSize, straightThickness, g);
                    }

                    foreach (VectorRoute route in data.Shapes)
                    {
                        DrawShape(route, data.ShapeColor, straightsPointSize, straightThickness, g);
                    }
                }


                using (Graphics g = CurrentChanged?.Invoke(_current))
                {
                    g?.DrawImage(_current, new PointF(0, 0));
                }
            }
            catch (InvalidOperationException ex) { }
        }
    }
}
