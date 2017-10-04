using RandomUtilities.Extensions;
using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    class UnrenderedFrame : IDisposable
    {
        /// <summary>
        /// if fps are capped, this property represents the amount of
        /// Ticks which a frame is visible at max fps
        /// </summary>
        public int FrameTickLength { get; set; }

        /// <summary>
        /// This list should contain any RenderedObject that 
        /// will be rendered in the correct order
        /// </summary>
        public List<RenderedObject> RenderOrder { get; set; }
        
        public Bitmap RenderCameraViewPointInternal(Engine en)
        {
            Bitmap b = new Bitmap((int)en.ActiveCamera.FieldOfView.X, (int)en.ActiveCamera.FieldOfView.Y);

            Graphics g = Graphics.FromImage(b);

            g.FillRectangle(new SolidBrush(en.DeafultBackColor), 0, 0, (float)en.ActiveCamera.FieldOfView.X, (float)en.ActiveCamera.FieldOfView.Y);

            foreach (RenderedObject ro in RenderOrder)
            {

                ro.Render(g, en.CoordSystemOffsetToScreen, en.ActiveCamera.Zoom);
            }

            if (en.ShowFPS)
            {
                string fpsString = "FPS: " + en.CurrentFPS;

                SizeF stringSize = g.MeasureString(fpsString, en.EngineFont);



                g.DrawString(fpsString, en.EngineFont, en.EngineFontBrush, new PointF(1, (float)en.ActiveCamera.FieldOfView.Y - stringSize.Height - 1));
            }

            g.Dispose();

            return b;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                FrameTickLength = 0;
                RenderOrder.Clear();
                RenderOrder = null;

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~UnrenderedFrame() {
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
    }
}
