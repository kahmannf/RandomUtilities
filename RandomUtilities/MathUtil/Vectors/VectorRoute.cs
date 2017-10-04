using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.MathUtil.Vectors
{
    public class VectorRoute : IEnumerable<Vector2>
    {
        public VectorRoute()
        {
            _vectors = new List<Vector2>();
            _startingPoint = new Vector2();
        }

        public VectorRoute(IEnumerable<Vector2> vectors) : this()
        {
            AppendRange(vectors);
        }

        private VectorRoute(IEnumerable<Vector2> vectors, Vector2 startingPoint) : this(vectors)
        {
            _startingPoint = startingPoint.Value;

            RecalculateProperties();
        }

        private List<Vector2> _vectors;
        private Vector2 _startingPoint;

        public Vector2 StartingPoint => _startingPoint.Value;

        public void AppendRange(IEnumerable<Vector2> vectors)
        {
            foreach (Vector2 v in vectors)
            {
                AppendInternal(v, true);
            }

            RecalculateProperties();
        }

        public void Append(Vector2 v)
        {
            AppendInternal(v, false);
        }

        private void AppendInternal(Vector2 v, bool supressUpdate)
        {
            _vectors.Add(v.Value);
            if (!supressUpdate)
                RecalculateProperties();
        }

        public void InsertAt(int index, Vector2 v)
        {
            _vectors.Insert(index, v.Value);
            RecalculateProperties();
        }

        /// <summary>
        /// Adds all vectors to the specified index up
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 FromStart(int i)
        {
            Vector2 result = new Vector2();

            for (int j = 0; j <= i; j++)
            {
                result = result.Add(this[j]);
            }

            return result;
        }

        public void RemoveAt(int index)
        {
            _vectors.RemoveAt(index);
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return _vectors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void RecalculateProperties()
        {
            if (this.Count > 0)
                _sum = this.Aggregate((v1, v2) => v1.Add(v2));
            else
                _sum = new Vector2();

            Vector2 currentLocation = _startingPoint.Value;

            _xmin = currentLocation.X;
            _xmax = currentLocation.X;
            _ymin = currentLocation.Y;
            _ymax = currentLocation.Y;

            for (int i = 0; i < _vectors.Count; i++)
            {
                currentLocation = TotalPosition(i);

                _xmax = Util.TakeLarger(_xmax, currentLocation.X);
                _ymax = Util.TakeLarger(_ymax, currentLocation.Y);

                _xmin = Util.TakeSmaller(_xmin, currentLocation.X);
                _ymin = Util.TakeSmaller(_ymin, currentLocation.Y);
            }
        }

        /// <summary>
        /// Rotates to Route around a point that is describes as startingpoint + pointFromStartToTurn
        /// </summary>
        /// <param name="radAngle"></param>
        /// <param name="pointFromStartToTurn"></param>
        /// <returns></returns>
        internal VectorRoute RotateAround(SaveDouble radAngle, Vector2 pointFromStartToTurn)
        {
            //dont mess with this!!!!
            //SERIOUSLY!!!
            #region Tried an explanation:

            //vector s is the vector to the starting point (this.startingPoint)
            //vector t is the vector from the starting point to the point which this is turned around (pointFromStartToTurn)
            //vector t' is t after the rotation.
            //Point p is the starting point
            //Point c is the rotationcenter

            //to get t' we have to set the startingPoint of the result befor turning to t

            //Befor the rotation, s points to p and t point from p to c
            //After the rotation, t still points to c because neither of them changed
            //s doenst point to p anymore (except if rotationcenter was p, but that doenst matter) because p was turned around c
            //t' points from p to c (direction is important)!!!!
            //calculation is by the route s->t-> (you are a c now) reverse t' -> voila welcome to p, the new starting point

            #endregion

            VectorRoute result = new VectorRoute(_vectors, pointFromStartToTurn);

            result = result.Rotate(radAngle);
            
            result._startingPoint = this._startingPoint.Add(pointFromStartToTurn).Add(result.StartingPoint.Reverse());

            result.RecalculateProperties();

            return result;
        }

        /// <summary>
        /// Rotates the Route around the startingpoint
        /// </summary>
        /// <param name="radAngle"></param>
        /// <returns></returns>
        public VectorRoute Rotate(SaveDouble radAngle)
        {
            return new VectorRoute(_vectors.Select(x => x.Rotate(radAngle)), _startingPoint.Rotate(radAngle)); 
        }

        /// <summary>
        /// Retirun the total position of a Point on the route
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 TotalPosition(int index)
        {
            return (FromStart(index).Add(_startingPoint));
        }

        private Vector2 _sum;

        public Vector2 Sum => _sum;

        public int Count => _vectors.Count;

        public bool ClosedRoute => this._vectors.Count > 0 && this.Sum.NullVector;

        public Vector2 this[int index]
        {
            get
            {
                return _vectors[index];
            }
            set
            {
                _vectors[index] = value;
            }
        }

        private SaveDouble _xmax;
        private SaveDouble _ymax;

        private SaveDouble _xmin;
        private SaveDouble _ymin;

        public SaveDouble Xmax => _xmax;

        public SaveDouble Ymax => _ymax;

        public SaveDouble Xmin => _xmin;

        public SaveDouble Ymin => _ymin;

        /// <summary>
        /// A padding that is 1% of the max width of the collider
        /// </summary>
        public SaveDouble XPadding => ((Xmax - Xmin) / 100);

        /// <summary>
        /// A padding that is 1% if the max height of the collider
        /// </summary>
        public SaveDouble YPadding => ((Ymax - Ymin) / 100);

        public SaveDouble MaxPadding => Util.TakeLarger(XPadding, XPadding);

        public VectorRoute Value => new VectorRoute(this._vectors.Select(x => x.Value), _startingPoint.Value);

        public VectorRoute Shift(Vector2 v)
        {
            return new VectorRoute(_vectors, _startingPoint.Add(v));
        }

        /// <summary>
        /// Returns the absolute location of all sides baste on the starting point
        /// </summary>
        /// <returns></returns>
        public List<V2Straight> GetAllSides()
        {
            int requiered_vectors = 2; //Todo: overthink this. are two enough or does it have to be 3???
            if (_vectors.Count < requiered_vectors)
                throw new InvalidOperationException("Can not calculate sides with less that " + requiered_vectors + " vectors");

            Vector2 currentLocation = _startingPoint.Value;


            List<V2Straight> result = new List<V2Straight>();
            
            for (int i = 0; i < _vectors.Count; i++)
            {
                Vector2 nextLocation = currentLocation.Add(_vectors[i]);
                result.Add(new V2Straight(currentLocation.Value, nextLocation, true));
                currentLocation = nextLocation;
            }

            return result;
        } 
    }
}
