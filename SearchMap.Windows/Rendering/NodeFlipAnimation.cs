using SearchMap.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SearchMap.Windows.Rendering {

    /// <summary>
    /// Class implementing the node flip animation. <para />
    /// This class cannot be inherited.
    /// </summary>
    sealed class NodeFlipAnimation {

        private const int ANIM_STEP_MILLIS = 1;
        private const int NUMBER_OF_STEPS = 10;

        /// <summary>
        /// The control on which this animation will play when triggered.
        /// </summary>
        public NodeControl Control { get; }

        /// <summary>
        /// Animation representing the flipping of a node to switch between front and back.
        /// </summary>
        /// <param name="control"></param>
        public NodeFlipAnimation(NodeControl control) {
            Control = control;
        }

        /// <summary>
        /// Plays the animation.
        /// </summary>
        public void Flip() {

            // Store sizes and positions at each frame
            List<double> widths = new List<double>();
            List<double> leftPos = new List<double>();

            var width = Control.ActualWidth;

            // Center should not move during this animation
            double centerX = Canvas.GetLeft(Control) + width / 2;
            double step = width / NUMBER_OF_STEPS;
            
            for(int i=0; i< NUMBER_OF_STEPS; i++) {
                var newWidth = width - i * step;
                widths.Add(newWidth);
                leftPos.Add(centerX - newWidth / 2);
            }

            for(int i= NUMBER_OF_STEPS-1; i>=0; i--) {
                widths.Add(widths[i]);
                leftPos.Add(leftPos[i]);
            }

            // Play animation

            int j = 0;

            Timer timer = null;

            timer = new Timer(delegate {

                if (j == 2*NUMBER_OF_STEPS) {
                    timer.Dispose();
                    return;
                }

                Control.Dispatcher.Invoke(delegate {

                    Control.Width = widths[j];
                    Canvas.SetLeft(Control, leftPos[j]);

                    if (j == NUMBER_OF_STEPS-1) {
                        // Flip

                        if (Control.GetFront().Visibility == Visibility.Visible) {
                            Control.GetFront().Visibility = Visibility.Collapsed;
                            Control.GetBack().Visibility = Visibility.Visible;
                        }
                        else {
                            Control.GetFront().Visibility = Visibility.Visible;
                            Control.GetBack().Visibility = Visibility.Collapsed;
                        }

                    }

                });

                j++;

            }, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(ANIM_STEP_MILLIS));


        }

    }

}
