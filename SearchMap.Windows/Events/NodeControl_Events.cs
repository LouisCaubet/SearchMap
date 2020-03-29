using SearchMap.Windows.Controls;
using SearchMap.Windows.Rendering;
using SearchMapCore.Graph;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.Events {

    /// <summary>
    /// Registers events independent of the type of node on a control representing a node. <para />
    /// This class cannot be inherited.
    /// </summary>
    sealed class NodeControl_Events {

        private const int REPARENT_ANIMATION_NB_PULSES = 5;

        UserControl Control { get; }
        Node Node { get; }

        /// <summary>
        /// Register events independent of the type of Node on the given control.
        /// </summary>
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

            // Resizing
            new ResizableNodeControl(Control, Node);
            
        }

        /// <summary>
        /// Call this on every child of Control to complete initialization of the event handlers
        /// </summary>
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

                            // Call reparent code
                            OnMoveStarted();

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

            OnMoveStop();

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

                // Call reparent check
                OnMove(e.GetPosition(MainWindow.Window.GraphCanvas));

            }
        }

        // TODO allow move with left-click when edit mode is move.
        // End of drag-and-drop handling

        #endregion

        #region Reparent

        Node PotentialNewParent;
        UserControl PotentialNewParentControl;

        Timer timer = null;

        Point? LastLocationOfNodeControl;
        Location LastLocationOfNode;

        NodeSelectionAnimation Animation;

        // Called once when user stars moving the node
        void OnMoveStarted() {
            LastLocationOfNodeControl = new Point(Canvas.GetLeft(Control), Canvas.GetTop(Control));
            LastLocationOfNode = new Location(Node.Location);
        }

        // Called by the MouseMove Event
        void OnMove(Point positionOnCanvas) {
            CheckIfOverOtherNode(positionOnCanvas);
        }

        // Called when the user stops moving the node
        void OnMoveStop() {

            StopAnimation();

            LastLocationOfNode = null;
            LastLocationOfNodeControl = null;

        }

        // Checks if the current node is above another node
        void CheckIfOverOtherNode(Point positionOnCanvas) {

            foreach(var node in SearchMapCore.SearchMapCore.Graph.Nodes.Values) {

                if (node.Id == Node.Id) continue;

                var control = MainWindow.Renderer.RenderedObjects[node.RenderId];

                // Check if mouse is over node
                if (positionOnCanvas.X >= Canvas.GetLeft(control) && positionOnCanvas.X <= Canvas.GetLeft(control) + control.ActualWidth
                    && positionOnCanvas.Y >= Canvas.GetTop(control) && positionOnCanvas.Y <= Canvas.GetTop(control) + control.ActualHeight) {

                    if(PotentialNewParent != null && node.Id != PotentialNewParent.Id) StopAnimation();

                    PotentialNewParent = node;
                    PotentialNewParentControl = control;
                    AnimateNewParent();
                    return;
                }

            }

            // Stop animation if not over a node.
            StopAnimation();

        }

        // Stops the animation if currently played.
        void StopAnimation() {

            if(timer != null) timer.Dispose();

            if (Animation != null) {
                Animation.Normal();
                Animation = null;
            }

            PotentialNewParent = null;
            PotentialNewParentControl = null;
            timer = null;

        }

        // Plays the selection animation on the PotentialParent
        void AnimateNewParent() {

            if (timer != null) return;

            int pulses = 0;
            Animation = new NodeSelectionAnimation(PotentialNewParentControl);

            timer = new Timer(delegate {

                if (Animation == null) return;

                Control.Dispatcher.Invoke(delegate {
                    Animation.Highlight();
                });

                Thread.Sleep(250);

                if (Animation == null) return;

                Control.Dispatcher.Invoke(delegate {
                    Animation.Normal();
                });

                if (pulses == REPARENT_ANIMATION_NB_PULSES) {
                    timer.Dispose();
                    Control.Dispatcher.Invoke(delegate {
                        SetNewParent();
                    });                    
                }

                pulses++;

            }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));

        }

        void SetNewParent() {

            // Operation was cancelled exactly when this method was called, cancel it.
            if(LastLocationOfNode == null) {
                return;
            }

            Node.SetParent(PotentialNewParent);

            // Move node back to its original location
            Node.MoveTo(LastLocationOfNode, false);
            Canvas.SetLeft(Control, LastLocationOfNodeControl.Value.X);
            Canvas.SetTop(Control, LastLocationOfNodeControl.Value.Y);

        }

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
