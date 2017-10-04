using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    class Layer
    {
        public Layer()
        {
            RenderedObjects = new List<RenderedObject>();
        }

        public string Name { get; set; }

        public List<RenderedObject> RenderedObjects { get; set; }

        public bool GraphicsEnabled { get; set; }

        public bool PhysicsEnabled { get; set; }
    }
}
