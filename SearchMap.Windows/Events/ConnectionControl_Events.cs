using SearchMapCore.Graph;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    public partial class ConnectionControl {

        private Connection.Action? CurrentAction;

        // EVENT HANDLING

        void RegisterEventHandlers() {

            PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;

        }

        /// <summary>
        /// Only called by MainWindow to propagate a KeyDown event.
        /// </summary>
        internal void OnKeyDown(object sender, KeyEventArgs e) {
            
            if(e.Key == Key.Delete) {
                DeleteConnection();
            }

        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            if (IsMouseOver) {

                var position = e.GetPosition(MainWindow.Window.GraphCanvas);
                var core_location = MainWindow.Window.ConvertToLocation(position);

                CurrentAction = Connection.GetActionAtLocation(core_location);

                Cursor = Cursors.Hand;
                Mouse.Capture(this);

                MainWindow.Window.DeselectAll();
                SetSelected();

            }

        }

        void OnMouseMove(object sender, MouseEventArgs e) {
            if (CurrentAction.HasValue) {
                Point posNow = e.GetPosition(MainWindow.Window.GraphCanvas);

                switch (CurrentAction) {

                    case Connection.Action.EDIT_CONNECTOR1:
                        Connection.MoveConnectorPointOf(Connection.GetDepartureNode(), MainWindow.Window.ConvertToLocation(posNow), true);
                        break;

                    case Connection.Action.EDIT_CONNECTOR2:
                        Connection.MoveConnectorPointOf(Connection.GetArrivalNode(), MainWindow.Window.ConvertToLocation(posNow), true);
                        break;

                    case Connection.Action.EDIT_CONTROLPOINT:
                        Connection.SetControlPoint(MainWindow.Window.ConvertToLocation(posNow));
                        break;

                }

                Refresh();

                Point loc = PositionOnCanvas;
                Canvas.SetTop(this, loc.Y);
                Canvas.SetLeft(this, loc.X);

            }
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

            this.Cursor = Cursors.Arrow;
            this.ReleaseMouseCapture();
            CurrentAction = null;

        }

    }

}