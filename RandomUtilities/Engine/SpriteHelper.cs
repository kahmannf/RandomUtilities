using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    public static class SpriteHelper
    {
        public static RenderedObject CreateRenderedObject(List<SpriteFrame> frames)
        {
            Sprite s = new Sprite(frames);

            RenderedObject ro = new RenderedObject(s);

            return ro;
        }
    }
}
