using SearchMapCore.Graph;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.Events {

    class NodeControl_Events {

        UserControl Control { get; }
        Node Node { get; }

        public NodeControl_Events(UserControl control, Node node) {

            Control = control;
            Node = node;

            // Drag and drop
            Control.MouseRightButtonUp += OnMouseRightButtonUp;
            Control.PreviewMouseRightButtonUp += OnMouseRightButtonUp;
            Control.PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            Control.MouseMove += OnMouseMove;

            RegisterScrollManager();

            // Ctrl + Click to open
            Control.MouseLeftButtonDown += OnMouseLeftButtonDown;

            // ContextMenu
            Control.ContextMenuOpening += OnContextMenuOpening;

        }

        public void RegisterEventsOnChild(UIElement child) {
            child.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        #region ScrollWhenOutOfBounds

        private const int STEP = 5;

        private enum ScrollAction {
            TOP,
            BOTTOM,
            RIGHT,
            LEFT
        }

        private ScrollAction? QueuedAction;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Qualité du code", "IDE0052:Supprimer les membres privés non lus",
            Justification = "Timer stored here to prevent destruction by GC.")]
        private Timer CooldownTimer;

        private void RegisterScrollManager() {

            CooldownTimer = new Timer(delegate {

                if (QueuedAction.HasValue) {
                    var action = QueuedAction.Value;

                    if (action == ScrollAction.LEFT) {
                        MainWindow.Window.Dispatcher.Invoke(delegate {
                            MainWindow.Window.ScrollView.ScrollToHorizontalOffset(MainWindow.Window.ScrollView.HorizontalOffset - STEP);
                        });
                    }
                    else if (action == ScrollAction.RIGHT) {
                        MainWindow.Window.Dispatcher.Invoke(delegate {
                            MainWindow.Window.ScrollView.ScrollToHorizontalOffset(MainWindow.Window.ScrollView.HorizontalOffset + STEP);
                        });
                    }
                    else if (action == ScrollAction.TOP) {
                        MainWindow.Window.Dispatcher.Invoke(delegate {
                            MainWindow.Window.ScrollView.ScrollToVerticalOffset(MainWindow.Window.ScrollView.VerticalOffset - STEP);
                        });
                    }
                    else if (action == ScrollAction.BOTTOM) {
                        MainWindow.Window.Dispatcher.Invoke(delegate {
                            MainWindow.Window.ScrollView.ScrollToVerticalOffset(MainWindow.Window.ScrollView.VerticalOffset + STEP);
                        });
                    }

                    QueuedAction = null;

                }

            }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10));

        }

        #endregion 

        #region MoveByDragDrop

        private bool IsRightClickDown = false;
        private Point? lastDragPoint;

        void OnMouseRightButtonDown(object sender, MouseButtonEventArgs args) {

            Control.ContextMenu.IsOpen = false;

            if (Control.IsMouseOver) {

                IsRightClickDown = true;

                var pos = args.GetPosition(MainWindow.Window.GraphCanvas);
                lastDragPoint = pos;
                Mouse.Capture(Control);

                // To distiguish between right click and move, check if the mouse is released 0.1 seconds later.
                new Timer(delegate {

                    if (IsRightClickDown) {
                        // User wants to move the node

                        Control.Dispatcher.Invoke(delegate {
                            Control.Cursor = Cursors.SizeAll;

                            // Show on top
                            Panel.SetZIndex(Control, 20);
                        });

                        ShouldContextMenuBeOpened = false;

                    }
                    else {
                        // It's a right click

                        ShouldContextMenuBeOpened = true;

                        Control.Dispatcher.Invoke(delegate {
                            Control.ContextMenu.IsOpen = true;
                        });

                    }

                }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(-1));

                args.Handled = true;

            }

        }

        void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            IsRightClickDown = false;
            QueuedAction = null;

            Control.Cursor = Cursors.Arrow;
            Control.ReleaseMouseCapture();
            lastDragPoint = null;

            // Put back on normal level
            Panel.SetZIndex(Control, 10);
        }

        void OnMouseMove(object sender, MouseEventArgs e) {
            if (lastDragPoint.HasValue) {
                Point posNow = e.GetPosition(MainWindow.Window.GraphCanvas);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                Canvas.SetLeft(Control, Canvas.GetLeft(Control) + dX);
                Canvas.SetTop(Control, Canvas.GetTop(Control) + dY);

                Location topLeft = new Location(MainWindow.Window.ConvertToLocation(new Point(Canvas.GetLeft(Control), Canvas.GetTop(Control))));

                Location newLoc = new Location(topLeft.x + Node.Width / 2, topLeft.y + Node.Height / 2);

                Node.MoveTo(newLoc, false);

                // Scroll if border is reached
                Point relativeToScrollView = e.GetPosition(MainWindow.Window.ScrollView);

                if (relativeToScrollView.X <= 20) {
                    QueuedAction = ScrollAction.LEFT;
                }
                else if (relativeToScrollView.X >= MainWindow.Window.ScrollView.ActualWidth - 20) {
                    QueuedAction = ScrollAction.RIGHT;
                }

                if (relativeToScrollView.Y <= 20) {
                    QueuedAction = ScrollAction.TOP;
                }
                else if (relativeToScrollView.Y >= MainWindow.Window.ScrollView.ActualHeight - 20) {
                    QueuedAction = ScrollAction.BOTTOM;
                }



            }
        }

        // TODO allow move with left-click when edit mode is move.
        // End of drag-and-drop handling

        #endregion

        // Click Handling

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                Node.OnClick();
            }

            MainWindow.Window.Selected = Control;
            MainWindow.Window.LastClickedPoint = null;

        }

        // Context Menu
        bool ShouldContextMenuBeOpened = false;

        void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {

            e.Handled = true;
            Control.ContextMenu.IsOpen = false;

            if (!ShouldContextMenuBeOpened) {
                Control.ContextMenu.IsOpen = false;
                e.Handled = true;
            }
            else {
                ShouldContextMenuBeOpened = false;
            }

        }

    }

}
