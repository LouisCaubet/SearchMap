using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.Controls {

    /// <summary>
    /// Registers event handling to enable resizing through usual mouse gestures.
    /// </summary>
    class ResizableNodeControl {

        UserControl Control { get; }
        Node Node { get; }

        /// <summary>
        /// Makes the given control, representing the given node, resizable
        /// </summary>
        public ResizableNodeControl(UserControl control, Node node) {

            Control = control;
            Node = node;

            Control.MouseLeftButtonDown += OnMouseLeftDown;
            Control.MouseLeftButtonUp += OnMouseLeftUp;
            Control.MouseMove += OnMouseMove;

        }

        // Source : http://csharphelper.com/blog/2014/12/let-user-move-resize-rectangle-wpf-c/
        // (Heavily modified)

        private enum HitType {
            None, UL, UR, LR, LL, L, R, T, B
        };


        HitType MouseHitType = HitType.None;
        private bool DragInProgress = false;
        private Point LastPoint;


        // Return a HitType value to indicate what is at the point.
        // Point is position relative to the Control.
        private HitType SetHitType(Point point) {

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

        // Set a mouse cursor appropriate for the current hit type.
        private void SetMouseCursor() {
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

        // Start dragging.
        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e) {
            
            MouseHitType = SetHitType(e.GetPosition(Control));
            SetMouseCursor();

            if (MouseHitType == HitType.None) return;

            LastPoint = e.GetPosition(MainWindow.Window.GraphCanvas);
            DragInProgress = true;
            Mouse.Capture(Control);

        }

        // If a drag is in progress, continue the drag.
        // Otherwise display the correct cursor.
        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (DragInProgress) {

                // See how much the mouse has moved.
                Point point = e.GetPosition(MainWindow.Window.GraphCanvas);
                double dX = point.X - LastPoint.X;
                double dY = point.Y - LastPoint.Y;

                // Get the rectangle's current position.
                double new_x = Canvas.GetLeft(Control);
                double new_y = Canvas.GetTop(Control);
                double new_width = Control.ActualWidth;
                double new_height = Control.ActualHeight;

                // Update the rectangle.
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

        // Stop dragging.
        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e) {
            DragInProgress = false;
            Control.ReleaseMouseCapture();
        }

    }

}
