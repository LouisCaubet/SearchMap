using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.Controls {

    /// <summary>
    /// Registers event handling to enable resizing through usual mouse gestures. <para />
    /// This class cannot be inherited.
    /// </summary>
    sealed class ResizableNodeControl {

        /// <summary>
        /// The control made resizable by this
        /// </summary>
        UserControl Control { get; }

        /// <summary>
        /// The node represented by Control
        /// </summary>
        Node Node { get; }

        /// <summary>
        /// Makes the given control, representing the given node, resizable
        /// </summary>
        public ResizableNodeControl(UserControl control, Node node) {

            Control = control;
            Node = node;

            // Event handlers
            Control.MouseLeftButtonDown += OnMouseLeftDown;
            Control.MouseLeftButtonUp += OnMouseLeftUp;
            Control.MouseMove += OnMouseMove;

        }

        // Source : http://csharphelper.com/blog/2014/12/let-user-move-resize-rectangle-wpf-c/
        // (Heavily modified)

        /// <summary>
        /// Types of interaction with the border of the control
        /// </summary>
        enum HitType {
            None, UL, UR, LR, LL, L, R, T, B
        };


        HitType MouseHitType = HitType.None;
        bool DragInProgress = false;
        Point LastPoint;


        /// <summary>
        /// Return a HitType value to indicate what is at the point.
        /// Point is position relative to the Control.
        /// </summary>
        /// <param name="point">Position relative to Control</param>
        /// <returns></returns>
        HitType SetHitType(Point point) {

            // The width of the border on which interactions allow resizing
            const double GAP = 10;

            if(point.X < GAP) {

                if (point.Y < GAP) return HitType.UL;
                else if (point.Y > Control.ActualHeight - GAP) return HitType.LL;

                return HitType.L;
            }
            else if(point.X > Control.ActualWidth - GAP) {

                if (point.Y < GAP) return HitType.UR;
                else if (point.Y > Control.ActualHeight - GAP) return HitType.LR;

                return HitType.R;

            }
            else if(point.Y < GAP) {
                return HitType.T;
            }
            else if(point.Y > Control.ActualHeight - GAP) {
                return HitType.B;
            }

            return HitType.None;

        }

        /// <summary>
        /// Set a mouse cursor appropriate for the current hit type.
        /// </summary>
        void SetMouseCursor() {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType) {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }

            // Display the desired cursor.
            if (Control.Cursor != desired_cursor) Control.Cursor = desired_cursor;
        }

        /// <summary>
        /// Event Handler for MouseLeftButtonDown. <para />
        /// Starts resizing if the mouse is on the border.
        /// </summary>
        void OnMouseLeftDown(object sender, MouseButtonEventArgs e) {
            
            // Disabled if Mode is not normal
            if(MainWindow.Window.CurrentEditMode != MainWindow.EditMode.NORMAL) {
                return;
            }

            MouseHitType = SetHitType(e.GetPosition(Control));
            SetMouseCursor();

            if (MouseHitType == HitType.None) return;

            // Place revert point
            Node.TakeSnapshot();

            MainWindow.Window.DeselectAll();

            LastPoint = e.GetPosition(MainWindow.Window.GraphCanvas);
            DragInProgress = true;
            Mouse.Capture(Control);

        }

        /// <summary>
        /// Event Handler for MouseMove. <para />
        /// If a resizing is in progress, continue.
        /// Otherwise display the correct cursor.
        /// </summary>
        void OnMouseMove(object sender, MouseEventArgs e) {

            // Disabled if Mode is not normal
            if (MainWindow.Window.CurrentEditMode != MainWindow.EditMode.NORMAL) {
                return;
            }

            if (DragInProgress) {

                // See how much the mouse has moved.
                Point point = e.GetPosition(MainWindow.Window.GraphCanvas);
                double dX = point.X - LastPoint.X;
                double dY = point.Y - LastPoint.Y;

                // Get the Controls's current position.
                double new_x = Canvas.GetLeft(Control);
                double new_y = Canvas.GetTop(Control);
                double new_width = Control.ActualWidth;
                double new_height = Control.ActualHeight;

                // Update the Control.
                switch (MouseHitType) {
                    case HitType.UL:
                        new_x += dX;
                        new_y += dY;
                        new_width -= dX;
                        new_height -= dY;
                        break;
                    case HitType.UR:
                        new_y += dY;
                        new_width += dX;
                        new_height -= dY;
                        break;
                    case HitType.LR:
                        new_width += dX;
                        new_height += dY;
                        break;
                    case HitType.LL:
                        new_x += dX;
                        new_width -= dX;
                        new_height += dY;
                        break;
                    case HitType.L:
                        new_x += dX;
                        new_width -= dX;
                        break;
                    case HitType.R:
                        new_width += dX;
                        break;
                    case HitType.B:
                        new_height += dY;
                        break;
                    case HitType.T:
                        new_y += dY;
                        new_height -= dY;
                        break;
                }

                // Don't use negative width or height.
                if ((new_width > 0) && (new_height > 0)) {

                    // Update Node
                    Point center = new Point(new_x + new_width / 2, new_y + new_height / 2);
                    Node.MoveTo(MainWindow.Window.ConvertToLocation(center));
                    Node.Resize((int) new_width, (int) new_height);

                    // Update the control.
                    Canvas.SetLeft(Control, new_x);
                    Canvas.SetTop(Control, new_y);
                    Control.Width = new_width;
                    Control.Height = new_height;

                    // Save the mouse's new location.
                    LastPoint = point;

                }
            }
            else {
                MouseHitType = SetHitType(e.GetPosition(Control));
                SetMouseCursor();
            }
        }

        /// <summary>
        /// Event Handler for MouseLeftButtonUp. <para />
        /// Stops resizing
        /// </summary>
        void OnMouseLeftUp(object sender, MouseButtonEventArgs e) {
            DragInProgress = false;
            Control.ReleaseMouseCapture();
        }

    }

}
