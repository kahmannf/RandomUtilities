using RandomUtilities.MathUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomUtilities.Engine
{
    public class Sprite :IDisposable
    {
        public Sprite(List<SpriteFrame> frames)
        {
            _frames = new Dictionary<int, SpriteFrame>();

            double topPoint = 0.0F, 
                  bottomPoint = 0.0F, 
                  leftPoint = 0.0F, 
                  rightPoint = 0.0F;

            for (int i = 0; i < frames.Count; i++)
            {
                SpriteFrame f = frames[i];

                _frames.Add(i, f);

                if (f.Offset.X < leftPoint)
                    leftPoint = f.Offset.X;

                if (f.Offset.Y < topPoint)
                    topPoint = f.Offset.Y;

                if (f.Size.X + f.Offset.X > rightPoint)
                    rightPoint = f.Size.X + f.Offset.X;

                if (f.Size.Y + f.Offset.Y > bottomPoint)
                    bottomPoint = f.Size.Y + f.Offset.Y;
            }

            _topLeftCorner = new Vector2(leftPoint, topPoint);

            _bottomRightCorner = new Vector2(rightPoint, bottomPoint);

            _totalSize = new Vector2(rightPoint - leftPoint, bottomPoint - topPoint);
        }

        #region Physical Properties

        private Vector2 _topLeftCorner;
        private Vector2 _bottomRightCorner;

        private Vector2 _totalSize;

        public Vector2 TopLeftCorner => _topLeftCorner;
        public Vector2 BottomRightCorner => _bottomRightCorner;

        public Vector2 TotalSize => _totalSize;

        #endregion


        private Dictionary<int, SpriteFrame> _frames { get; set; }

        private bool _started = false;

        private long _thisFrameStart = 0;

        private int _currentFrameId = -1;

        public SpriteFrame Current
        {
            get
            {
                if (_frames.Count == 1)
                {
                    return _frames[0];
                }

                if (!_started)
                {
                    _currentFrameId = 0;
                    _started = true;
                    _thisFrameStart = Environment.TickCount;
                }

                if (Environment.TickCount - _thisFrameStart > _frames[_currentFrameId].TickLength)
                {
                    NextFrame();
                }

                return _frames[_currentFrameId];
            }
        }

        private void NextFrame()
        {
            if (_currentFrameId < _frames.Count - 1)
            {
                _currentFrameId++;
            }
            else
            {
                _currentFrameId = 0;
            }

            _thisFrameStart = Environment.TickCount;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (int key in _frames.Keys)
                    {
                        SpriteFrame f = _frames[key];
                        _frames.Remove(key);
                        f.Dispose();
                    }

                    _frames = null;
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~Sprite() {
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
