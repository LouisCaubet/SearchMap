using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SearchMap.Windows.Rendering {

    /// <summary>
    /// Windows WPF implementation of IGraphRenderer.
    /// </summary>
    class GraphRenderer : IGraphRenderer {

        private const double SMOOTH_STEP = 10;

        public Dictionary<int, UserControl> RenderedObjects { get; }
        private int lastRegisteredId;

        // Locks on objects for smooth movements
        private Dictionary<int, bool> LockedObjects { get; }
        private Dictionary<int, List<Location>> QueuedMovements { get; }

        public GraphRenderer() {
            RenderedObjects = new Dictionary<int, UserControl>();
            lastRegisteredId = 0;
            LockedObjects = new Dictionary<int, bool>();
            QueuedMovements = new Dictionary<int, List<Location>>();
        }

        public bool DeleteObject(int id) {
            
            try {
                MainWindow.Window.GraphCanvas.Children.Remove(RenderedObjects[id]);
                RenderedObjects.Remove(id);
                return true;
            }
            catch (Exception) {
                return false;
            }

        }

        public double GetZoomLevel() {
            return MainWindow.Window.ZoomSlider.Value;
        }

        public void MoveObjectSmoothly(int id, Location destination) {

            if (LockedObjects[id]) {
                QueuedMovements[id].Add(destination);
                return;
            }

            LockedObjects[id] = true;

            // Get position on canvas, simplified
            var currentPt = new Point(Canvas.GetLeft(RenderedObjects[id]), Canvas.GetTop(RenderedObjects[id]));

            // Where to move to
            var objectivePt = MainWindow.Window.ConvertFromLocation(destination);

            // Decompose movement in small steps.
            System.Windows.Vector move = new System.Windows.Vector(objectivePt.X - currentPt.X, objectivePt.Y - currentPt.Y);
            var norm = move.Length;

            List<System.Windows.Vector> moves = new List<System.Windows.Vector>();

            while(norm > SMOOTH_STEP) {

                System.Windows.Vector dMove = new System.Windows.Vector(move.X, move.Y);
                var scale = SMOOTH_STEP / move.Length;

                dMove.X *= scale;
                dMove.Y *= scale;

                moves.Add(dMove);

                norm -= SMOOTH_STEP;

            }

            var final_scale = norm / move.Length;
            move.X *= final_scale;
            move.Y *= final_scale;

            moves.Add(move);

            int i = 0;

            // Execute these small moves at 10ms interval.
            Timer timer = null;
                
            timer = new Timer(delegate {

                if (i >= moves.Count) {
                    timer.Dispose();
                    LockedObjects[id] = false;

                    if(QueuedMovements[id].Count >= 1) {
                        var nextmove = QueuedMovements[id][0];
                        QueuedMovements[id].RemoveAt(0);
                        MoveObjectSmoothly(id, nextmove);
                    }

                    return;
                }

                var currentMove = moves[i];
                var newPt = new Point(currentPt.X + currentMove.X, currentPt.Y + currentMove.Y);

                var control = RenderedObjects[id];

                currentPt = newPt;

                // UI changes must be run on UI thread.
                control.Dispatcher.Invoke(delegate {

                    Canvas.SetLeft(control, newPt.X);
                    Canvas.SetTop(control, newPt.Y);

                });

                i++;

            }, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(10));

        }

        public void RefreshNode(int renderId) {

            try {
                var control = RenderedObjects[renderId];

                if(control.GetType() == typeof(WebNodeControl)) {
                    ((WebNodeControl)control).Refresh();
                }
                else {
                    // Go to catch.
                    throw new Exception();
                }

            }
            catch (Exception) {
                throw new ArgumentException("The id " + renderId + " is not the id of a renderer object corresponding to a node.");
            }
            

        }

        public int RenderCurvedLine(Connection connection) {

            lastRegisteredId++;

            var control = new ConnectionControl(connection);

            Point loc = control.PositionOnCanvas;
            Canvas.SetTop(control, loc.Y);
            Canvas.SetLeft(control, loc.X);

            MainWindow.Window.GraphCanvas.Children.Add(control);

            RenderedObjects.Add(lastRegisteredId, control);

            return lastRegisteredId;

        }

        public void RefreshCurvedLine(int renderId) {
            try {
                var control = RenderedObjects[renderId];
                if (control.GetType() == typeof(ConnectionControl)) {
                    ((ConnectionControl)control).Refresh();

                    Point loc = ((ConnectionControl)control).PositionOnCanvas;
                    Canvas.SetTop(control, loc.Y);
                    Canvas.SetLeft(control, loc.X);

                }
                else {
                    throw new ArgumentException("The given id is not the id of a ConnectionControl.");
                }
            }
            catch (KeyNotFoundException) {
                throw new ArgumentException("The given id is invalid.");
            }
           
        }

        public int RenderNode(Node toRender) {

            lastRegisteredId++;

            if(toRender.GetType() == typeof(WebNode)) {

                var control = new WebNodeControl((WebNode)toRender) {
                    Height = toRender.Height * MainWindow.SCALE_FACTOR,
                    Width = toRender.Width * MainWindow.SCALE_FACTOR
                };

                Point pt = MainWindow.Window.ConvertFromLocation(toRender.Location);
                Canvas.SetLeft(control, pt.X - toRender.Width / 2);
                Canvas.SetTop(control, pt.Y - toRender.Height / 2);

                MainWindow.Window.GraphCanvas.Children.Add(control);

                RenderedObjects.Add(lastRegisteredId, control);
                LockedObjects.Add(lastRegisteredId, false);
                QueuedMovements.Add(lastRegisteredId, new List<Location>());

            }
            else if(toRender.GetType() == typeof(FileNode)) {
                // TODO
            }
            else {
                throw new ArgumentException("Unknown type of node");
            }

            return lastRegisteredId;

        }

        public void SetDrawingZoneSize(int x, int y) {
            MainWindow.Window.GraphCanvas.Width = x;
            MainWindow.Window.GraphCanvas.Height = y;

            MainWindow.Window.MoveToCenterOfCanvas();
        }

    }

}
