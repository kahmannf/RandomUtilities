using RandomUtilities.MathUtil;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    public enum CollisionBehaviors
    {
        /// <summary>
        /// Colliding object absorbs its part of energie
        /// </summary>
        Absorbing,
        /// <summary>
        /// Colliding object reflects its part of energy
        /// </summary>
        Reflecting
    }

    public class Collider
    {
        public Collider() : this(new VectorRoute(), new Vector2())
        {
        }

        /// <summary>
        /// Center to outline is the vector from the center of the gameobject to the outline
        /// </summary>
        /// <param name="outLine"></param>
        /// <param name="centerToOutline"></param>
        public Collider(VectorRoute outLine, Vector2 centerToOutline) : this(outLine, centerToOutline, 1)
        {
        }

        /// <summary>
        /// Center to outline is the vector from the center of the gameobject to the outline
        /// </summary>
        /// <param name="outLine"></param>
        /// <param name="centerToOutline"></param>
        public Collider(VectorRoute outLine, Vector2 centerToOutline, SaveDouble temper)
        {
            _outLine = outLine.Value;
            _centerToOutline = centerToOutline.Value;
            Temper = temper;
            MassEmphasisFromCenter = new Vector2();
            _collisionBehavior = CollisionBehaviors.Absorbing;
        }

        public void Activate(RenderedObject parent)
        {
            _parent = parent;
        }

        private RenderedObject _parent;

        public RenderedObject Parent => _parent;

        private VectorRoute _outLine;

        private Vector2 _centerToOutline;

        private CollisionBehaviors _collisionBehavior;

        public CollisionBehaviors CollisionBehavior => _collisionBehavior;

        /// <summary>
        /// A Vector2 that point from the center of the collider / RO to the mass emphasis of the collider / RO
        /// </summary>
        public Vector2 MassEmphasisFromCenter { get; set; }

        private SaveDouble _temper;
        public SaveDouble Temper
        {
            get => _temper;
            set
            {
                if ((double)value > 1 || (double)value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Temper", "The Temper of a collider has to be larger than 0 but cannot be larger than 1");
                }
                else
                {
                    _temper = value;
                }
            }
        }

        public VectorRoute PositionedOutline
        {
            get
            {
                Vector2 startingPointOfOutline = _centerToOutline.Add(_parent.PhysicalLocation);

                VectorRoute turnThat = _outLine.Shift(startingPointOfOutline);
                
                Vector2 vectorToMassEmphasisFromStartingPoint = _centerToOutline.Reverse().Add(MassEmphasisFromCenter);

                return turnThat.RotateAround(_parent.Rotation, vectorToMassEmphasisFromStartingPoint);
            }
        } 

        public bool Check(Collider compareTo, out Collision collision)
        {
            List<int> baseHits = new List<int>();

            VectorRoute baseOutline = this.PositionedOutline;

            for (int i = 0; i < baseOutline.Count; i++)
            {
                //Vector2 physicalLocation = compareTo._outLine.FromStart(i) //vector from start of route to specific index
                //                                    .Add(compareTo._centerToOutline) //vector from center of RO to specific index
                //                                    .Add(compareTo._parent.PhysicalLocation); //vector from {0,0} to specific index

                if (compareTo.IsInside(baseOutline.TotalPosition(i)))
                {
                    baseHits.Add(i);
                }
            }

            List<int> compHits = new List<int>();

            VectorRoute compOutline = compareTo.PositionedOutline;

            for (int i = 0; i < compOutline.Count; i++)
            {
                //Vector2 physicalLocation = compareTo._outLine.FromStart(i) //vector from start of route to specific index
                //                                    .Add(compareTo._centerToOutline) //vector from center of RO to specific index
                //                                    .Add(compareTo._parent.PhysicalLocation); //vector from {0,0} to specific index

                if (IsInside(compOutline.TotalPosition(i)))
                {
                    compHits.Add(i);
                }
            }

            if (compHits.Count > 0)
            {
                collision = new Collision(this, compareTo, baseHits, compHits, true);
                return true;
            }
            else
            {
                collision = new Collision();
                return false;
            }
        }



        public bool IsInside(Vector2 point)
        {
            //Explanation on Stackoverflow:
            // https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
            //

            VectorRoute outline = PositionedOutline;

            //Excluding to most points with this => performance
            if (point.X < outline.Xmin || point.X > outline.Xmax || point.Y < outline.Ymin || point.Y > outline.Ymax)
            {
                return false;
            }

            //raycast along the x-axis
            V2Straight ray = new V2Straight(new Vector2(outline.Xmin - outline.XPadding, point.Y), point, true);
            
            return DoRayCast(ray).Count % 2 == 1;
        }

        /// <summary>
        /// Casts a ray along the straight and returns the hits
        /// </summary>
        /// <param name="ray"></param>
        /// <returns>The indicies of the sides that where hit</returns>
        public List<int> DoRayCast(V2Straight ray)
        {
            List<int> hits = new List<int>();

            List<V2Straight> relevantColliderSides = PositionedOutline.GetAllSides();

            foreach (V2Straight side in relevantColliderSides)
            {
                if (!ray.IsParallel(side))
                {
                    LinearEquationSystem les = LinearEquationSystem.FromVStraights(ray, 'r', side, 's');

                    if (les.SolveGaussian() is Dictionary<char, SaveDouble> result && result.Keys.Count > 0)//Intersection found
                    {
                        //Check limits here. raycast will otherwise go out on the other side and make hits even at any time

                        if (ray.IsPointAtScaleFactorOnStraight(result['r']))
                        {
                            hits.Add(relevantColliderSides.IndexOf(side));
                        }
                    }

                }
            }

            return hits;
        }

        [Obsolete("outdated", true)]
        public void DrawCollider(System.Drawing.Graphics g, Vector2 cameraOffset, int scale)
        {
            VectorRoute outline = PositionedOutline;

            Vector2 currentLocation = outline.StartingPoint;

            System.Drawing.Pen p = new System.Drawing.Pen(System.Drawing.Color.Red);

            for (int i = 0; i < outline.Count; i++)
            {
                Vector2 nextLocation = outline.FromStart(i).Add(outline.StartingPoint);

                g.DrawLine(p, (System.Drawing.PointF)currentLocation.Subtract(cameraOffset), (System.Drawing.PointF)nextLocation.Subtract(cameraOffset));

                currentLocation = nextLocation.Value;
            }
        }
    }
}
