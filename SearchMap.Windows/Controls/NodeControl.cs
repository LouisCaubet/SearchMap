using SearchMap.Windows.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.Controls {

    public abstract partial class NodeControl : UserControl {

        /// <summary>
        /// The node rendered by this control.
        /// </summary>
        public Node Node { get; }

        NodeFlipAnimation FlipAnim { get; set; }

        /// <summary>
        /// Creates a new NodeControl representing the given Node
        /// </summary>
        /// <param name="node"></param>
        public NodeControl(Node node) {
            Node = node;

            // Flip
            FlipAnim = new NodeFlipAnimation(this);

        }

        /// <summary>
        /// Updates the rendered values and colors for the node.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Returns the WPF element representing the front side of the node.
        /// </summary>
        /// <returns></returns>
        public abstract FrameworkElement GetFront();

        /// <summary>
        /// Returns the WPF element representing the back side of the node.
        /// </summary>
        /// <returns></returns>
        public abstract FrameworkElement GetBack();

        /// <summary>
        /// Source : https://stackoverflow.com/questions/50540301/c-sharp-get-good-color-for-label
        /// </summary>
        protected float GetBrightness(Color c) {
            return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f;
        }

        /// <summary>
        /// Flips the node.
        /// </summary>
        public void Flip() {
            // Flip node

            if (MainWindow.Window.RibbonTabView.ShowNodeFlipAnimation) {
                FlipAnim.Flip();
            }
            else {

                if (GetFront().Visibility == Visibility.Visible) {
                    GetFront().Visibility = Visibility.Collapsed;
                    GetBack().Visibility = Visibility.Visible;
                }
                else {
                    GetFront().Visibility = Visibility.Visible;
                    GetBack().Visibility = Visibility.Collapsed;
                }

            }

        }

    }

}
