﻿using Newtonsoft.Json;
using SearchMap.Windows.Controls;
using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows {

    public partial class MainWindow {

        internal const int NUMBER_OF_PERSISTANT_TABS = 6;

        internal const double DEFAULT_ZOOM = 0.4;

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

            // Ribbon
            Ribbon.SelectedTabChanged += OnSelectedTabChanged;

            // Clipboard
            KeyDown += OnKeyPress;

        }

        // Implementation of movement by drag and dropping
        // Source : https://www.codeproject.com/Articles/97871/WPF-simple-zoom-and-drag-support-in-a-ScrollView

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

                DeselectAll();

                Selected = null;

                ScrollView.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                LastClickedPoint = e.GetPosition(GraphCanvas);
                Mouse.Capture(ScrollView);

            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {

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
            ScaleTransform.ScaleX = e.NewValue * DEFAULT_ZOOM;
            ScaleTransform.ScaleY = e.NewValue * DEFAULT_ZOOM;

            var centerOfViewport = new Point(ScrollView.ViewportWidth / 2,
                                             ScrollView.ViewportHeight / 2);
        }

        // End of move by drag & drop implementation.
        #endregion MoveByDragDrop


        #region Ribbon

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

        void OnSelectedTabChanged(object sender, SelectionChangedEventArgs e) {
            if(Ribbon.SelectedTabIndex < NUMBER_OF_PERSISTANT_TABS) {
                RibbonTabIndex = Ribbon.SelectedTabIndex;
            }
        }

        #endregion


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
                else if(e.Key == Key.Z) {
                    SearchMapCore.SearchMapCore.UndoRedoSystem.Undo();
                }
                else if(e.Key == Key.Y) {
                    SearchMapCore.SearchMapCore.UndoRedoSystem.Redo();
                }

            }

            if(e.Key == Key.Escape) {
                RibbonTabInsert.CancelAllTasks();
                RibbonTabHome.SetNormalEditMode();
            }

            // Propagate event to selected control (for some reason the event is not fired there)
            if (ConnectionControl.Selected != null) ConnectionControl.Selected.OnKeyDown(sender, e);
            RibbonTabWebNode.OnKeyPress(sender, e);

        }

        #endregion Clipboard

    }

}