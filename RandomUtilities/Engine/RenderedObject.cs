using RandomUtilities.Extensions;
using RandomUtilities.MathUtil;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    public class RenderedObject : IDisposable
    {
        public const double TICK_SECOND_REALATION = 1 / 1000.0;
        
        public RenderedObject()
        {
            _colliders = new List<Collider>();
            _velocity = new Vector2();
            _activeForce = new Vector2();
            PhysicalLocation = new Vector2();
            _mass = 1.0;
            _torque = 0.0;
            _angularVelocity = 0.0;
            _momentOfInertia = 1;
            IsFixed = true;
        }

        public RenderedObject(Sprite s) : this()
        {
            this._sprite = s;
        }

        public RenderedObject(Sprite s, Collider col) : this(s)
        {
            AddCollider(col, true);
        }

        public void AddCollider(Collider col, bool mainCollider)
        {
            col.Activate(this);
            _colliders.Add(col);
            CanCollide = true;
            if (mainCollider)
                this._mainCollider = col;
        }

        public string Layer { get; set; }

        private long _lastPhysicsCalculation = 0;


        #region Physical Properties
        
        /// <summary>
        /// The Position of the center of the RenderedObject
        /// </summary>
        public Vector2 PhysicalLocation { get; set; }
        public Vector2 MassEmphasis => this.PhysicalLocation.Add(ActiveCollider.MassEmphasisFromCenter);

        public Vector2 PhysicalSize { get; set; }

        public string Name { get; set; }

        public bool CanCollide { get; set; } = false;

        private double _mass;

        /// <summary>
        /// Used to calculate angular velocity from torque (german "Trägheitsmoment")
        /// </summary>
        private double _momentOfInertia;

        private Vector2 _velocity;
        public Vector2 Velocity => _velocity.Value;

        private Vector2 _activeForce;

        /// <summary>
        /// Negative torque is clockwise.
        /// </summary>
        private SaveDouble _torque;

        /// <summary>
        /// The angular Velecity (w / lower case omega) (german "Winkelgeschwindigkeit")
        /// definde as w = 2 * PI * f = (2 * PI) / T
        /// Negative value is clockwise
        /// </summary>
        private SaveDouble _angularVelocity;

        /// <summary>
        /// Represents how often this object will spin around its mass emphasis withing one second
        /// </summary>
        public SaveDouble CircularFrequenzy => _angularVelocity / Util.FULL_RADIANT;

        /// <summary>
        /// The angle (rad) of the RanderedObject relative to it's default rotation (clockwise)
        /// </summary>
        private SaveDouble _rotation;

        /// <summary>
        /// The angle (rad) of the RanderedObject relative to it's default rotation (clockwise)
        /// </summary>
        public SaveDouble Rotation
        {
            get => _rotation;
            set
            {
                _rotation = Util.NormalizeAngle(value);
            }
        }

        private List<Collider> _colliders;

        private Collider _mainCollider;

        public IEnumerable<Collider> Colliders => _colliders.ToList();

        /// <summary>
        /// If true, this object will not move on collisions
        /// </summary>
        public bool IsFixed { get; set; }

        public Collider ActiveCollider => _mainCollider;

        #endregion

        #region Physical Manipulation

        /// <summary>
        /// Applies force to the mass emphasis
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(Vector2 force)
        {
            _activeForce = _activeForce.Add(force);
        }

        /// <summary>
        /// Applies a force to the RenderedObject at a point specified by ME + distanceFromME
        /// </summary>
        /// <param name="force">the force</param>
        /// <param name="distanceToME">a vector pointing from the mass emphasis to the point where the force shoud be applied</param>
        public void ApplyForce(Vector2 force, Vector2 distanceFromME)
        {
            //torque: http://www.maschinenbau-wissen.de/skript3/mechanik/kinetik/292-hebelgesetz

            SaveDouble totalAngle = Math.Acos(distanceFromME.GetCosinAngle(force));

            //no torque needed (0° and 180°)
            if (totalAngle == Math.PI || totalAngle == 0.0)
            {
                ApplyForce(force);
                return;
            }

            SaveDouble torqueDirectionAngle = totalAngle < Math.PI ? Util.QUARTER_RADIANT : Math.PI + Util.QUARTER_RADIANT;

            SaveDouble forceToTorqueAngle;

            if (totalAngle == Util.QUARTER_RADIANT || totalAngle == Util.QUARTER_RADIANT + Math.PI)
                forceToTorqueAngle = 0.0;
            else if (totalAngle < Util.QUARTER_RADIANT)
                forceToTorqueAngle = Util.QUARTER_RADIANT - totalAngle;
            else if (totalAngle < Math.PI)
                forceToTorqueAngle = Util.FULL_RADIANT - (totalAngle - Util.QUARTER_RADIANT);
            else if (totalAngle < Math.PI + Util.QUARTER_RADIANT)
                forceToTorqueAngle = Math.PI + Util.QUARTER_RADIANT - totalAngle;
            else
                forceToTorqueAngle = Util.FULL_RADIANT - (totalAngle - (Math.PI + Util.QUARTER_RADIANT));

            Vector2 addedTorgueVector = force.GetComponentVectorWithAngle(forceToTorqueAngle);

            Vector2 addedForce = force.Subtract(addedTorgueVector);

            ApplyForce(addedForce);

            SaveDouble addedTorgue = addedTorgueVector.Magnitude * distanceFromME.Magnitude;

            //check if the the torgue is positive or negative. cannot dertime befor becaus cosin rande is from 0 to Math.PI
            if (distanceFromME.Rotate(Util.QUARTER_RADIANT).GetCosinAngle(force.Rotate(forceToTorqueAngle)) == 1)
                addedTorgue = -addedTorgue;

            _torque += addedTorgue;
        }

        public void ClearTorque()
        {
            _torque = 0.0;
        }

        public void ClearAngularVelocity()
        {
            _angularVelocity = 0.0;
        }

        public void ClearForce()
        {
            _activeForce = new Vector2();
        }

        public void AddVelocity(Vector2 velocity)
        {
            _velocity = _velocity.Add(velocity);
        }

        public void AddAngularVelocity(SaveDouble angVelocity)
        {
            _angularVelocity += angVelocity;
        }

        public void ClearVelocity()
        {
            _velocity = new Vector2();
        }

        public void ClearAllForceAndVelocity()
        {
            ClearAngularVelocity();
            ClearForce();
            ClearTorque();
            ClearVelocity();
        }

        public void Rotate(SaveDouble radRotation)
        {
            double newRotation = Rotation + radRotation;
            
            Rotation = newRotation;
        }


        #endregion


        /// <summary>
        /// Renders the RenderedObjects CurrentState to a Graphics object
        /// </summary>
        /// <param name="g"></param>
        /// <param name="offset">Offset of the graphicsObject to {0,0} of the CoordSystem. Postion of the RenderedObject relativ to the (0,0) of the Graphics object</param>
        /// <param name="scale"></param>
        public void Render(Graphics g, Vector2 offset, float scale)
        {

            SpriteFrame frame = _sprite.Current;

            //Vector2 frameLocation = offset.Add(frame.Offset);
            PointTranslator translator = new PointTranslator()
            {
                CoordSystemOffsetToScreen = offset
            };

            SizeF renderSize = frame.SpritePart.Size.Scale(scale);

            //A padding in the final bitmap (50% of frame on each side, 100% for each axis) for rotation
            //with a mass emphasis that is futher that 25% of the objects larges side away, 
            //the object will not be rendered properely
            //Todo: Consider dynamic calculation of padding
            //benchmark what is faster and how much more memory is used by larger paddings
            //Quick note: this should really be fixed. massive impact on performance
            SaveDouble finalBitmapPadding = (Util.TakeLarger(renderSize.Width, renderSize.Height)) * 1;

            //size of the finalBitmap (size of original bitmap + padding)
            Size sizeWithPadding = new Size((int)(renderSize.Width + finalBitmapPadding), (int)(renderSize.Height + finalBitmapPadding));

            //bitmap htat will be drawn onto the main graphics object
            Bitmap finalBitmap = new Bitmap(sizeWithPadding.Width, sizeWithPadding.Height);

            Graphics bitMapG = Graphics.FromImage(finalBitmap);

            //location of the spriteframe on the final bitmap
            PointF paddingOffset = new PointF((float)finalBitmapPadding / 2, (float)finalBitmapPadding / 2);

            
            //Rotation of Bitmap:
            //https://stackoverflow.com/questions/26525965/rotate-a-graphics-bitmap-at-its-center
            if (this.Rotation != 0.0)
            {
                //Assuming that sizeWtihPadding / 2 represents center point of bitmap (because of following offset shifting)
                Vector2 shiftTranslate = _mainCollider.MassEmphasisFromCenter.Add(new Vector2(sizeWithPadding).Scale(0.5));

                //set the center of the graphcis object to mass ephasis of the main collider (prepareation for rotations)
                bitMapG.TranslateTransform((float)shiftTranslate.X, (float)shiftTranslate.Y);

                //Rotates the world transformation, prepending the rotation matrix to the world transformation matrix.
                // Caution, this methode takes degrees, not radiants
                //=> https://msdn.microsoft.com/de-de/library/a0z3f662(v=vs.110).aspx
                //negative angle to archive rotation that is not clockwise (to match vektor rotation)
                bitMapG.RotateTransform(-(float)Util.RadianToDegree(this.Rotation));

                bitMapG.TranslateTransform(-(float)shiftTranslate.X, -(float)shiftTranslate.Y);
            }

            //target rectangle for the original bitmap (includes size transform and padding offset already)
            RectangleF targeRectanlgeOriginalBitmap = new RectangleF(paddingOffset, renderSize);

            //draw original bitmap on final bitmap
            bitMapG.DrawImage(frame.SpritePart, targeRectanlgeOriginalBitmap);

            Vector2 totalFrameOffset = frame.Offset.Scale(scale)
                                  .Add(new Vector2(-paddingOffset.X, paddingOffset.Y)); //negation of x because we are working with real coordinates here and not Graphics coordinates

            //Subtract the padding offset from the originalBitmaps location to avoid unwanted offset. Also add original fram offset here
            Vector2 finalBitmapLocation = this.PhysicalLocation.Scale(scale).Add(totalFrameOffset);
 
            RectangleF targeRectangleFinalBitmap = new RectangleF(translator.CoordinateToGraphicsPoint(finalBitmapLocation), renderSize.Add(new SizeF((float)finalBitmapPadding, (float)finalBitmapPadding)));

            g.DrawImage(finalBitmap, targeRectangleFinalBitmap);

            bitMapG.Dispose();
            finalBitmap.Dispose();
        }

        [Obsolete("outdated", true)]
        public void DrawColliders(Graphics g, Vector2 cameraOffset, int scale) => _colliders.ForEach(c => c.DrawCollider(g, cameraOffset, scale));

        /// <summary>
        /// Preset assumption:
        /// 
        /// The timeintervall(deltaTime dt) used to calculate the physics is small enough to use
        /// all formulars for velocity(constant velocity and evenly accelerated velocity)
        /// 
        /// </summary>
        /// <param name="allColliders"></param>
        /// <param name = "deltaTime" ></ param >
        internal void CalculatePhysics(IEnumerable<Collider> allColliders, double deltaTime)
        {
            if (_mainCollider == null)
                throw new InvalidOperationException("Cannot calculate physics when maincollider is null");

            //long ticks = Environment.TickCount - _lastPhysicsCalculation;
            //_lastPhysicsCalculation = Environment.TickCount;

            Vector2 oldLocation = this.PhysicalLocation.Value;

            Vector2 oldVelocity = this._velocity.Value;

            CalculateMovement(deltaTime);

            foreach (Collider anyCollider in allColliders)
            {
                foreach (Collider colliderRO in _colliders)
                {
                    if (colliderRO.Check(anyCollider, out Collision collision))
                    {
                        //!note: "anyCollider" might not be the active collider of the collisionPartner
                        //and may not be valid for some physics-calculation regarding mass emphasis, etc.

                        RenderedObject collisionPartner = anyCollider.Parent;

                        //concept: get velocity into collision direction for both ROs (seperate for circular/normal)
                        // note for that => may not be the same direction. best raycast for both velocities
                        //                  and check if thy hit the other collider
                        //                  probably need full velocity if it is a hit, not only a part
                        //                  !!still to be proven/tested!!!
                        //subtract this velocity from its parent RO
                        //Calculate kinetic energy from subtracted velocity (still seperate)
                        //
                        //Unclear part: do some crazy geometric calculation to get new directions
                        //
                        //recalulate velocities from kinectic energy => subtract internal energy absorbed based on RO Propeties
                        //apply proper directions
                        //add velocity to ROs
                        //FIN!

                        if (collision.HitCount == 1)
                        {
                            if (collision.BaseColliderOutlineHitIndex.Count > 0)
                            {
                                //self-explanatory
                                Vector2 hitPointTotalPosition = collision.BaseCollider.PositionedOutline.TotalPosition(collision.BaseColliderOutlineHitIndex[0]);
                                Vector2 velocity = this._velocity;
                                Vector2 circularVelocity = this.GetCircularVelocity(hitPointTotalPosition.Subtract(collision.BaseCollider.Parent.MassEmphasis));

                                //Straight along velocity
                                V2Straight veloRay = V2Straight.FromBaseAndScaleVector(hitPointTotalPosition, velocity);

                                //Straight along circular velocity
                                V2Straight circVeloRay = V2Straight.FromBaseAndScaleVector(hitPointTotalPosition, circularVelocity);

                                //all sides of the collider, in total coordinates
                                List<V2Straight> allSides = collision.ComparedCollider.PositionedOutline.GetAllSides();

                                //Indecies of the sides hit by the veloRay
                                List<int> hitIndexVelo = collision.ComparedCollider.DoRayCast(veloRay);

                                //sides hit by the veloRay
                                IEnumerable<V2Straight> hitSidesVelo = from hitIndex in hitIndexVelo
                                                                       select allSides[hitIndex];

                                //Indecies of the sides hit by the cirVeloRay
                                List<int> hitIndexCircVelo = collision.ComparedCollider.DoRayCast(circVeloRay);

                                //sides hit by the cirVeloRay
                                IEnumerable<V2Straight> hitSidesCircVelo = from hitIndex in hitIndexCircVelo
                                                                           select allSides[hitIndex];

                                if (collision.Behavior == CollisionResultingBehavior.FullAbsorbing)
                                {

                                }
                                
                            }
                        }


                        #region prior attempt: //keep fro recycling reasons

                        //if (collision.HitCount == 1)//a corner hit an edge. simple calculations here to avoid benchmark 
                        //                            //issues on simple collisions
                        //{
                        //    if (collision.BaseColliderOutlineHitIndex.Count > 0)//corner of this object, edge of other object
                        //    {
                        //        //Assumption: circular movement is small enough to be considered straight 
                        //        // => vector of total position change will be used as movementdirection

                        //        //calculate both velocities/forces (circular and normal) seperatly


                        //    }
                        //}

                        ////Get the real point of collision here
                        //if (collision.BaseColliderOutlineHitIndex.Count > 0)
                        //{
                        //    throw new NotImplementedException();
                        //}
                        //else //c.CompColliderOutlineHitIndex.Count > 0
                        //{
                        //    //Calculate the side of the base collider which hit a point;
                        //    //Maybe a raycast from the point alont the negavite velocity
                        //    // => first intersection with outline is hit;
                        //    // + the distance is known
                        //    throw new NotImplementedException();
                        //}


                        //if (_angularVelocity.Value == 0.0)
                        //{

                        //}
                        //else
                        //{
                        //    throw new NotImplementedException("Collisions for spinning Objects are not implemented yet");
                        //}

                        #endregion
                    }
                }
            }

        }

        /// <summary>
        /// Recalculate positioning and rotations for delta time seconds passed
        /// </summary>
        /// <param name="deltaTime">The passed time in seconds</param>
        private void CalculateMovement(double deltaTime)
        {
            //double deltaTime = ticks * TICK_SECOND_REALATION;

            //      F = m * a    =>    a = F / m
            double accelerationValue = _activeForce.Magnitude / _mass;

            //      dv = a * dt; 
            Vector2 deltaVelocity = _activeForce.WithLength(accelerationValue * deltaTime);

            //      v = dv + v0
            this._velocity = _velocity.Add(deltaVelocity);

            this.PhysicalLocation = PhysicalLocation.Add(_velocity.Scale(deltaTime));

            //Torque and angular velocity formulars: 
            //http://dodo.fb06.fh-muenchen.de/lab_didaktik/pdf/web-massentraegheitsmoment.pdf

            // M: torque  
            // J: moment of inertia

            // M = J * a => a = M / J

            double angularAccerleation = _torque / _momentOfInertia;

            // a = dw / dt => dw = a * dt

            double deltaAngularVelocity = angularAccerleation * deltaTime;

            _angularVelocity += deltaAngularVelocity;

            //w = delta angle / dt => deltaAngle = w * dt

            double deltaAngle = _angularVelocity * deltaTime;

            //angle = delta angle + angle0

            double newRotation = deltaAngle + this._rotation;

            this.Rotation = newRotation;
        }

        public Vector2 GetCircularVelocity(Vector2 vectorFromME)
        {
            //mass emphasis does not appear in this methode because the returned vector 
            //is the same regardsless from which sarting point calculated

            if (_angularVelocity == 0.0)
                return new Vector2();

            //https://www.leifiphysik.de/mechanik/kreisbewegung

            //if the circular velocity is orthogonal to the radius-vector this is valid:
            //v = w * r
            // => only circles are calculated here, so this is the case

            //Create a vector pointing upwards (positive y), because that is defined aus deltaAngle = 0.0
            Vector2 result = new Vector2(0, 1);

            //set the Magnitude of the vector to the velocities value
            result = result.WithLength(_angularVelocity * vectorFromME.Magnitude);

            //rotate the vector by the ROs rotation plus difference from 0.0 degrees 
            //to radius vector to get the angle of the radius-vector
            //then by 90° degrees for to get the circular velocity vector

            SaveDouble distanceToRadiusAngle = Math.Acos(result.GetCosinAngle(vectorFromME));

            SaveDouble radiusAngle = _rotation + distanceToRadiusAngle;

            SaveDouble totalAngle = radiusAngle + Util.QUARTER_RADIANT;

            result = result.Rotate(totalAngle);

            //if vector is 90° to radius angle, reverse it (based on testing)
            if (vectorFromME.Rotate(radiusAngle).GetAngle(result) == Util.QUARTER_RADIANT)
                result = result.Reverse();

            return result;
        }

        private Sprite _sprite { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sprite?.Dispose();
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~RenderedObject() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }

        #endregion

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
    }
}
