using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    public class Camera : RenderedObject
    {
        public Camera()
        {
            FieldOfView = new Vector2();
            Zoom = 1;
        }

        public Vector2 FieldOfView { get; private set; }

        internal void SetFieldOfView(Engine engine)
        {
            FieldOfView = new Vector2(engine.RenderSize);
        }

        private float _zoom;

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (value > 0)
                    _zoom = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
