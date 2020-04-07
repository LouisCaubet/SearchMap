using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.Rendering {

    /// <summary>
    /// Represents the pulsation animation when a node is selected.
    /// Increases the size of the node and changes the shadow color.
    /// </summary>
    sealed class NodeSelectionAnimation {

        Node Node { get; set; }
        UserControl Control { get; set; }

        Point HighlightedPosition { get; set; }

        int NormalWidth { get; set; }
        double HighlightedWidth { get; set; }

        int NormalHeight { get; set; }
        double HighlightedHeight { get; set; }

        /// <summary>
        /// Creates a NodeSelectionAnimation for a given control with a size increase of the given factor.
        /// </summary>
        public NodeSelectionAnimation(UserControl control, double factor = 1.1) {

            Control = control;

            if (control.GetType() == typeof(WebNodeControl)) {

                WebNodeControl nodeControl = (WebNodeControl)control;
                Node = nodeControl.Node;

            }
            else {
                throw new ArgumentException("This animation can only be applied to controls representing nodes.");
            }

            NormalWidth = Node.Width;
            NormalHeight = Node.Height;

            HighlightedWidth = NormalWidth * factor;
            HighlightedHeight = NormalHeight * factor;

            Point center = MainWindow.Window.ConvertFromLocation(Node.Location);
            HighlightedPosition = new Point(center.X - HighlightedWidth / 2, center.Y - HighlightedHeight / 2);

        }

        /// <summary>
        /// Switches the animation state to Highlighted, with the given color.
        /// </summary>
        public void Highlight(Color color) {

            if (Control.GetType() == typeof(WebNodeControl)) {
                WebNodeControl nodeControl = (WebNodeControl)Control;
                nodeControl.Shadow.Color = color;
                nodeControl.Shadow.BlurRadius = 70;
            }

            Control.Width = HighlightedWidth;
            Control.Height = HighlightedHeight;

            Canvas.SetLeft(Control, HighlightedPosition.X);
            Canvas.SetTop(Control, HighlightedPosition.Y);

        }

        /// <summary>
        /// Highlights the node in red.
        /// </summary>
        public void Highlight() {
            Highlight(Color.FromRgb(255, 0, 0));
        }

        /// <summary>
        /// Switches the animation state to Normal
        /// </summary>
        public void Normal() {

            if (Control.GetType() == typeof(WebNodeControl)) {
                WebNodeControl nodeControl = (WebNodeControl)Control;
                nodeControl.Shadow.Color = Color.FromRgb(0, 0, 0);
                nodeControl.Shadow.BlurRadius = 30;
            }

            // recompute position every time
            Point pt = MainWindow.Window.ConvertFromLocation(Node.Location);
            Canvas.SetLeft(Control, pt.X - Node.Width / 2);
            Canvas.SetTop(Control, pt.Y - Node.Height / 2);

            Control.Width = NormalWidth;
            Control.Height = NormalHeight;

        }

    }

}
