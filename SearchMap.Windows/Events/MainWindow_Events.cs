using Newtonsoft.Json;
using SearchMap.Windows.Controls;
using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System;
using System.Windows;
using System.Windows.Input;

namespace SearchMap.Windows {

    public partial class MainWindow {

        private const int NUMBER_OF_PERSISTANT_TABS = 7;

        // Event Handling

        /// <summary>
        /// Call in MainWindow constructor to register event handlers.
        /// </summary>
        void RegisterEventHandlers() {

            // Move by drag and drop
            ScrollView.MouseLeftButtonUp += OnMouseLeftButtonUp;
            ScrollView.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            ScrollView.PreviewMouseWheel += OnPreviewMouseWheel;

            ScrollView.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            ScrollView.MouseMove += OnMouseMove;

            // Ribbon layout changes.
            SizeChanged += OnWindowSizeChanged;
            StateChanged += OnWindowStateChanged;

            // Clipboard
            KeyDown += OnKeyPress;

        }

        // Implementation of movement by drag and dropping
        // Original code : https://www.codeproject.com/Articles/97871/WPF-simple-zoom-and-drag-support-in-a-ScrollView

        #region MoveByDragDrop

        void OnMouseMove(object sender, MouseEventArgs e) {
            if (lastDragPoint.HasValue) {
                Point posNow = e.GetPosition(ScrollView);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                ScrollView.ScrollToHorizontalOffset(ScrollView.HorizontalOffset - dX);
                ScrollView.ScrollToVerticalOffset(ScrollView.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var mousePos = e.GetPosition(ScrollView);

            if (GraphCanvas.IsMouseDirectlyOver) {
                
                Selected = null;
                ScrollView.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                LastClickedPoint = e.GetPosition(GraphCanvas);
                Mouse.Capture(ScrollView);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            lastMousePositionOnTarget = Mouse.GetPosition(WindowGrid);

            if (e.Delta > 0) {
                ZoomSlider.Value += 0.03;
            }
            if (e.Delta < 0) {
                ZoomSlider.Value -= 0.03;
            }

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ScrollView.Cursor = Cursors.Arrow;
            ScrollView.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            ScaleTransform.ScaleX = e.NewValue;
            ScaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(ScrollView.ViewportWidth / 2,
                                             ScrollView.ViewportHeight / 2);
            lastCenterPositionOnTarget = ScrollView.TranslatePoint(centerOfViewport, WindowGrid);
        }

        // End of move by drag & drop implementation.
        #endregion MoveByDragDrop


        // Size ribbon tabs correctly on window resize

        void OnWindowSizeChanged(object sender, RoutedEventArgs e) {

            double WidthOfTab = Math.Min(100, Math.Max(50, (ActualWidth / 2) / NUMBER_OF_PERSISTANT_TABS));

            RibbonBackstage.Width = WidthOfTab;
            RibbonTabHome.Width = WidthOfTab;
            RibbonTabInsert.Width = WidthOfTab;
            RibbonTabView.Width = WidthOfTab;
            RibbonTabExport.Width = WidthOfTab;
            RibbonTabAccount.Width = WidthOfTab;
            RibbonTabHelp.Width = WidthOfTab;

        }

        void OnWindowStateChanged(object sender, EventArgs e) {
            OnWindowSizeChanged(sender, null);
        }

        // END of ribbon management

        // Clipboard tasks

        #region Clipboard

        void OnKeyPress(object sender, KeyEventArgs e) {
            
            // Detect Ctrl + ... op
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {

                if(e.Key == Key.C) {
                    if(Selected != null) {
                        ClipboardManager.CopyToClipboard(Selected, false);
                    }
                }
                else if(e.Key == Key.X) {
                    if (Selected != null) {
                        ClipboardManager.CopyToClipboard(Selected, true);
                    }
                }
                else if(e.Key == Key.V) {
                    ClipboardManager.Paste(LastClickedPoint);
                }

            }

        }

        #endregion Clipboard

    }

}