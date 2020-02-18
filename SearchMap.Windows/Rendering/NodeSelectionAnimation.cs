using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.Rendering {

    class NodeSelectionAnimation {

        Node Node { get; set; }
        UserControl Control { get; set; }

        Point NormalPosition { get; set; }
        Point HighlightedPosition { get; set; }

        int NormalWidth { get; set; }
        double HighlightedWidth { get; set; }

        int NormalHeight { get; set; }
        double HighlightedHeight { get; set; }

        public NodeSelectionAnimation(UserControl control, double factor = 1.1) {

            Control = control;

            if (control.GetType() == typeof(WebNodeControl)) {

                WebNodeControl nodeControl = (WebNodeControl)control;
                Node = nodeControl.Node;

            }
            else {
                throw new ArgumentException("This animation can only be applied to controls representing nodes.");
            }

            NormalPosition = new Point(Canvas.GetLeft(control), Canvas.GetTop(control));
            NormalWidth = Node.Width;
            NormalHeight = Node.Height;

            HighlightedWidth = NormalWidth * factor;
            HighlightedHeight = NormalHeight * factor;

            Point center = MainWindow.Window.ConvertFromLocation(Node.Location);
            HighlightedPosition = new Point(center.X - HighlightedWidth / 2, center.Y - HighlightedHeight / 2);

        }

        public void Highlight() {

            if (Control.GetType() == typeof(WebNodeControl)) {
                WebNodeControl nodeControl = (WebNodeControl)Control;
                nodeControl.Shadow.Color = Color.FromRgb(255, 0, 0);
            }

            Control.Width = HighlightedWidth;
            Control.Height = HighlightedHeight;

            Canvas.SetLeft(Control, HighlightedPosition.X);
            Canvas.SetTop(Control, HighlightedPosition.Y);

        }

        public void Normal() {

            if (Control.GetType() == typeof(WebNodeControl)) {
                WebNodeControl nodeControl = (WebNodeControl)Control;
                nodeControl.Shadow.Color = Color.FromRgb(0, 0, 0);
            }

            Control.Width = NormalWidth;
            Control.Height = NormalHeight;

            Canvas.SetLeft(Control, NormalPosition.X);
            Canvas.SetTop(Control, NormalPosition.Y);

        }

    }

}
