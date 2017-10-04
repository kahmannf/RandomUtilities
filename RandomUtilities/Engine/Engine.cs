using RandomUtilities.MathUtil.Vectors;
using RandomUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RandomUtilities.Engine
{
    public class Engine
    {
        public bool IsAdvancedBenchmarkEnabled { get; set; }

        private int _advancedBenchmarkAverageCount;
        public int AdvancedBenchmarkAverageCount
        {
            get => _advancedBenchmarkAverageCount;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();

                if (_rendering)
                    throw new InvalidOperationException("Cannot change that value while Rendering");

                _advancedBenchmarkAverageCount = value;
            }
        }

        public double AveragePhysicTimeTimeMS
        {
            get
            {
                int valueCount = _physicTimeArray.Where(x => x != 0).Count();
                if (valueCount > 0)
                    return _physicTimeArray.Aggregate((x, y) => x + y) / valueCount;
                else
                    return -1;
            }
        }

        public double AverageRenderTimeMS
        {
            get
            {
                int valueCount = _renderTimeArray.Where(x => x != 0).Count();
                if (valueCount > 0)
                    return _renderTimeArray.Aggregate((x, y) => x + y) / valueCount;
                else
                    return -1;
            }
        }

        private double[] _physicTimeArray;
        private int _physicTimeArrayIndex;

        private double[] _renderTimeArray;
        private int _renderTimeArrayIndex;

        public Engine()
        {
            _layers = new Dictionary<string, int>();
            _layerPositions = new Dictionary<int, Layer>();
            _cameras = new Dictionary<string, Camera>();
            _allObjects = new List<RenderedObject>();
            IsAdvancedBenchmarkEnabled = false;
            AdvancedBenchmarkAverageCount = 100;
        }

        #region Private Variables

        private Dictionary<string, int> _layers;
        private Dictionary<int, Layer> _layerPositions;
        private Dictionary<string, Camera> _cameras;

        private List<RenderedObject> _allObjects;

        private int lockRenderingCalls = 0;

        private bool _rendering = false;

        private Exception _lastException;
        private int _currentFPS;

        /// <summary>
        /// Grphics object handed in by the user on StartRendering(Graphics g)
        /// </summary>
        private Graphics _finalGraphics;

        #endregion

        #region Properties

        public bool Rendering { get => _rendering; }

        private Size _renderSize;
        public Size RenderSize => _renderSize;
        public string ActiveCameraName { get; set; }

        public Camera ActiveCamera
        {
            get
            {
                if (_cameras.ContainsKey(ActiveCameraName))
                {
                    return _cameras[ActiveCameraName];
                }
                else
                {
                    throw new Exception($"The Camera \"{ActiveCameraName}\" was not found.");
                }
            }
        }

        public Exception LastException { get => _lastException; }

        public bool AreFPSCapped { get; set; }

        public Color DeafultBackColor { get; set; } = Color.Black;

        public int FPSCap { get; set; } = 60;

        public bool ShowFPS { get; set; }

        public int CurrentFPS { get => _currentFPS; }

        public float CurrentFrameTime
        {
            get
            {
                if (!AreFPSCapped || FPSCap < 1)
                    return -1;
                else
                {
                    return (1.0f / FPSCap) * 1000.0f;
                }
            }
        }

        /// <summary>
        /// The Offset of the Cameras TopLeft Corner Based on the Physical location
        /// </summary>
        public Vector2 CameraPhysicalOffset
        {
            get
            {
                if (string.IsNullOrEmpty(ActiveCameraName) || !_cameras.ContainsKey(ActiveCameraName))
                {
                    throw new Exception("\"ActiveCameraName\"-property is not set or the set value cannot be used to find a camera.");
                }

                Camera c = _cameras[ActiveCameraName];
                return c.PhysicalLocation.Subtract(c.FieldOfView.Scale(0.5));
            }
        }

        #endregion

        public Dictionary<string, int> GetLayers()
        {
            return new Dictionary<string, int>(_layers);
        }

        public enum Layerpositions { Front, Back }

        #region LayerMethodes

        public void ShiftLayer(int position, int addNumber)
        {
            ++lockRenderingCalls;
            if (position + addNumber < 0)
            {
                throw new ArgumentOutOfRangeException("A layer cannot be shifted to a position below zero");
            }

            if (_layerPositions.ContainsKey(position + addNumber))
            {
                ShiftLayer(position + addNumber, 1);
            }

            Layer layer = _layerPositions[position];

            _layerPositions.Remove(position);
            _layers.Remove(layer.Name);

            _layerPositions.Add(position + addNumber, layer);
            _layers.Add(layer.Name, position + addNumber);

            lockRenderingCalls--;
        }

        public void ShiftLayer(string name, int addNumber)
        {
            ShiftLayer(_layers[name], addNumber);
        }

        public void RegisterLayer(string name, int position, bool physicsEnabled, bool graphicsEnabled, bool shiftOtherLayersBack)
        {
            lockRenderingCalls++;

            if (_layers.ContainsValue(position))
            {
                if (!shiftOtherLayersBack)
                {
                    throw new InvalidOperationException($"There is already a Layer at this position ({position})");
                }
                else
                {
                    ShiftLayer(position, 1);
                }
            }

            _layers.Add(name, position);

            Layer l = new Layer()
            {
                Name = name,
                PhysicsEnabled = physicsEnabled,
                GraphicsEnabled = graphicsEnabled
            };


            _layerPositions.Add(position, l);

            lockRenderingCalls--;
        }

        public void RegisterLayer(string name, int position, bool shiftOtherLayersBack)
        {
            RegisterLayer(name, position, true, true, shiftOtherLayersBack);
        }

        public void RegisterLayer(string name, Layerpositions position)
        {
            switch (position)
            {
                case Layerpositions.Front:
                    List<int> layers = new List<int>(_layerPositions.Keys.OrderBy(x => x));
                    if (layers.Count == 0)
                    {
                        RegisterLayer(name, 0, false);
                    }
                    else if (layers[0] == 0)
                    {
                        RegisterLayer(name, 0, true);
                    }
                    else
                    {
                        RegisterLayer(name, layers[0] - 1, true);
                    }
                    break;
                case Layerpositions.Back:
                    List<int> layersDesc = new List<int>(_layerPositions.Keys.OrderByDescending(x => x));
                    if (layersDesc.Count == 0)
                    {
                        RegisterLayer(name, 0, false);
                    }
                    else
                    {
                        RegisterLayer(name, layersDesc[0] + 1, true);
                    }
                    break;
            }
        }


        #endregion

        public void RegisterCamera(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (string.IsNullOrEmpty(camera.Name))
            {
                throw new ArgumentException("The provided camera has no name attached.");
            }

            if (_cameras.ContainsKey(camera.Name))
            {
                throw new InvalidOperationException($"Tried to add a the camera \"{camera.Name}\" , but there was already a camera with that name.");
            }

            if (camera.FieldOfView.NullVector)
            {
                camera.SetFieldOfView(this);
            }

            _cameras.Add(camera.Name, camera);
            _allObjects.Add(camera);
        }

        public void RegisterRenderObject(RenderedObject ro, string layer)
        {
            if (ro == null)
            {
                throw new ArgumentNullException("ro");
            }

            if (!_layers.ContainsKey(layer))
            {
                throw new ArgumentException($"There is no such layer registered: \"{layer}\"");
            }

            int position = _layers[layer];

            ro.Layer = layer;

            _layerPositions[position].RenderedObjects.Add(ro);

            _allObjects.Add(ro);
        }

        public void UnregisterRenderedObject(RenderedObject ro)
        {
            if (ro == null)
                throw new ArgumentNullException("ro");

            ++lockRenderingCalls;

            if (_allObjects.Contains(ro))
                _allObjects.Remove(ro);

            if (_layers.ContainsKey(ro.Layer))
            {
                int layerPosition = _layers[ro.Layer];

                if (_layerPositions[layerPosition].RenderedObjects.Contains(ro))
                    _layerPositions[layerPosition].RenderedObjects.Remove(ro);
            }

            lockRenderingCalls--;
        }


        public event EventHandler<Exception> EngineFatalError;
        public event EventHandler FrameRendered;

        public void StartRendering(Graphics graphics, Size renderSize)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (new Vector2(renderSize).NullVector)
                throw new ArgumentNullException("renderSize", "the parameter \"renderSize\" cannot be {0,0}");

            if (!_rendering)
            {
                _renderSize = renderSize;
                _finalGraphics = graphics;
                _rendering = true;

                if (ActiveCamera.FieldOfView.NullVector)
                    ActiveCamera.SetFieldOfView(this);

                _physicTimeArray = new double[AdvancedBenchmarkAverageCount];
                _renderTimeArray = new double[AdvancedBenchmarkAverageCount];

                RunInternal();
            }
        }

        private async void RunInternal()
        {
            await Task.Run(() =>
            {
                try
                {
                    UnrenderedFrame lastFrame = null;
                    long tickCountLastFrame = Environment.TickCount;
                    long tickSecondCount = Environment.TickCount;
                    int framecount = 0;
                    _physicTimeArrayIndex = _renderTimeArrayIndex = -1;
                    Stopwatch stopWatch = new Stopwatch();
                    
                    double nextPhysikTickLength = 0.020; //fix this value for now CAUTION. FIX THIS. THIS IS WRONG AF

                    while (_rendering)
                    {
                        try
                        {
                            while (lockRenderingCalls > 0)
                                System.Threading.Thread.Sleep(1);

                            IEnumerable<RenderedObject> collidingROs =
                            (from ro in _allObjects
                             where !string.IsNullOrEmpty(ro.Layer) &&
                                   _layers.ContainsKey(ro.Layer) &&
                                   _layerPositions[_layers[ro.Layer]].PhysicsEnabled &&
                                   ro.CanCollide
                             select ro);

                            IEnumerable<Collider> colliders = new List<Collider>();
                            if (collidingROs.Count() > 0)
                                colliders = collidingROs.Select(x => x.Colliders).Aggregate((x, y) => x.Union(y)).ToList();


                            #region PhysicsCalculation

                            if (IsAdvancedBenchmarkEnabled)
                            {
                                stopWatch.Reset();
                                stopWatch.Start();
                            }

                            foreach (RenderedObject ro in collidingROs)
                            {
                                if (_layerPositions[_layers[ro.Layer]].PhysicsEnabled && ro.CanCollide)
                                    ro.CalculatePhysics(colliders.Except(ro.Colliders), nextPhysikTickLength);
                            }

                            if (IsAdvancedBenchmarkEnabled)
                            {
                                stopWatch.Stop();

                                _physicTimeArrayIndex++;

                                if (_physicTimeArrayIndex >= AdvancedBenchmarkAverageCount)
                                    _physicTimeArrayIndex = 0;

                                _physicTimeArray[_physicTimeArrayIndex] = stopWatch.Elapsed.TotalMilliseconds;
                            }

                            #endregion

                            if (IsAdvancedBenchmarkEnabled)
                            {
                                stopWatch.Reset();
                                stopWatch.Start();
                            }

                            UnrenderedFrame thisFrame = CreateUnrenderedFrame();

                            Bitmap bitmapFrame = thisFrame.RenderCameraViewPointInternal(this);

                            if (FPSCap < 1)
                                AreFPSCapped = false;

                            if (AreFPSCapped)
                            {
                                if (lastFrame != null)
                                {
                                    while (Environment.TickCount - tickCountLastFrame < CurrentFrameTime)
                                        System.Threading.Thread.Sleep(1);
                                }

                                DrawBitmap(bitmapFrame);
                            }
                            else
                            {
                                DrawBitmap(bitmapFrame);
                            }

                            bitmapFrame.Dispose();
                            bitmapFrame = null;

                            tickCountLastFrame = Environment.TickCount;

                            framecount++;

                            if (Environment.TickCount - tickSecondCount >= 1000)
                            {
                                _currentFPS = framecount;
                                framecount = 0;
                                tickSecondCount = Environment.TickCount;
                            }

                            lastFrame = thisFrame;

                            if (IsAdvancedBenchmarkEnabled)
                            {
                                stopWatch.Stop();

                                _renderTimeArrayIndex++;

                                if (_renderTimeArrayIndex >= AdvancedBenchmarkAverageCount)
                                    _renderTimeArrayIndex = 0;

                                _renderTimeArray[_renderTimeArrayIndex] = stopWatch.Elapsed.TotalMilliseconds;
                            }

                            this.FrameRendered?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                            _lastException = ex;

                            if (lockRenderingCalls == 0)
                                throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _lastException = ex;

                    _rendering = false;

                    EngineFatalError?.Invoke(this, ex);
                }
            });
        }

        private Font _engineFont = new Font(new FontFamily("Arial"), 10);
        public Font EngineFont => _engineFont;

        private Brush _engineFontBrush = new SolidBrush(Color.White);
        public Brush EngineFontBrush => _engineFontBrush;

        public Vector2 CoordSystemOffsetToScreen => ActiveCamera.PhysicalLocation.Add(new Vector2(this.RenderSize).Scale(0.5));

        private void DrawBitmap(Bitmap b)
        {
            _finalGraphics.DrawImage(b, 0.0f, 0.0f, RenderSize.Width, RenderSize.Height);
        }

        private UnrenderedFrame CreateUnrenderedFrame()
        {
            UnrenderedFrame result = new UnrenderedFrame();

            if (AreFPSCapped)
            {
                double frameLength = (1.0 / FPSCap) * 1000.0;

                result.FrameTickLength = (int)frameLength;
            }
            else
            {
                result.FrameTickLength = -1;
            }

            result.RenderOrder = new List<RenderedObject>();

            IEnumerable<int> layerKeys = _layerPositions.Keys.OrderBy(x => x);

            foreach (int layerKey in layerKeys)
            {
                if (_layerPositions[layerKey].GraphicsEnabled)
                    result.RenderOrder.AddRange(_layerPositions[layerKey].RenderedObjects);
            }

            return result;
        }

        public RenderedObject[] SearchObjectByName(string name)
        {
            return _allObjects.Where(x => x.Name.Contains(name)).ToArray();
        }
    }
}
