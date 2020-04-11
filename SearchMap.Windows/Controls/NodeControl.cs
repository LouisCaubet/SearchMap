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

        NodeSelectionAnimation SelectionAnimation { get; set; }

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
        /// Saves the changes made to the object to the SearchMapCore Node object.
        /// </summary>
        public abstract void Save();


        /// <summary>
        /// Sets the associated ribbon tab visible and, if setSelected, selects it.
        /// </summary>
        /// <param name="setSelected"></param>
        public abstract void ShowAssociatedRibbonTab(bool setSelected);

        /// <summary>
        /// Collapses the associated ribbon tab, and selects the last used ribbon tab still visible.
        /// </summary>
        public abstract void CollapseAssociatedRibbonTab();


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

        /// <summary>
        /// Selects this node in the given color.
        /// </summary>
        /// <param name="color"></param>
        public void SetSelected(Color color, bool openRibbonTab) {

            // Unhighlight previously selected node if it wasn't this one.
            if (MainWindow.Window.Selected != this) {
                MainWindow.Window.DeselectAll();
            }

            MainWindow.Window.Selected = this;

            // Selection Animation - Init here to be sure every required parameter is set.
            SelectionAnimation = new NodeSelectionAnimation(this, 1);
            SelectionAnimation.Highlight(color);

            ShowAssociatedRibbonTab(openRibbonTab);

        }

        /// <summary>
        /// Select with default color (orange).
        /// </summary>
        public void SetSelected() {
            SetSelected(Color.FromRgb(255, 140, 0), true);
        }

        /// <summary>
        /// Deselects this node.
        /// </summary>
        public void SetUnselected() {

            if (MainWindow.Window.Selected != this) return;

            SelectionAnimation.Normal();
            MainWindow.Window.Selected = null;

            CollapseAssociatedRibbonTab();

            Save();

        }

    }

}
