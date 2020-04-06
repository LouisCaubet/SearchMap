using SearchMap.Windows.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SearchMap.Windows.Controls {

    partial class NodeControl {

        private const int REPARENT_ANIMATION_NB_PULSES = 5;

        internal NodeSelectionAnimation SelectionAnimation { get; private set; }

        /// <summary>
        /// Register events independent of the type of Node.
        /// </summary>
        public void RegisterBaseEvents() {

            // Drag and drop
            this.MouseRightButtonUp += OnMouseRightButtonUp;
            this.PreviewMouseRightButtonUp += OnMouseRightButtonUp;
            this.PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            this.MouseMove += OnMouseMove;

            RegisterScrollManager();

            // Ctrl + Click to open
            this.MouseLeftButtonDown += OnMouseLeftButtonDown;

            // Move mode
            this.MouseLeftButtonUp += OnMouseLeftButtonUp;
            this.MouseMove += MoveMode_OnMouseMove;

            // ContextMenu
            this.ContextMenuOpening += OnContextMenuOpening;

            // Resizing
            new ResizableNodeControl(this, Node);

           
            
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

            ContextMenu.IsOpen = false;

            if (IsMouseOver) {

                IsRightClickDown = true;

                // Deselect selected node
                MainWindow.Window.DeselectAll();

                var pos = args.GetPosition(MainWindow.Window.GraphCanvas);
                lastDragPoint = pos;
                Mouse.Capture(this);

                // To distiguish between right click and move, check if the mouse is released 0.1 seconds later.
                new Timer(delegate {

                    if (IsRightClickDown) {
                        // User wants to move the node

                        Dispatcher.Invoke(delegate {
                            Cursor = Cursors.SizeAll;

                            // Show on top
                            Panel.SetZIndex(this, 20);

                            // Call reparent code
                            OnMoveStarted();

                        });

                        ShouldContextMenuBeOpened = false;
                        

                    }
                    else {
                        // It's a right click

                        ShouldContextMenuBeOpened = true;

                        Dispatcher.Invoke(delegate {
                            ContextMenu.IsOpen = true;
                        });

                    }

                }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(-1));

                args.Handled = true;

            }

        }

        void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            IsRightClickDown = false;
            QueuedAction = null;

            Cursor = Cursors.Arrow;
            this.ReleaseMouseCapture();
            lastDragPoint = null;

            OnMoveStop();

            // Put back on normal level
            Panel.SetZIndex(this, 10);
        }

        void OnMouseMove(object sender, MouseEventArgs e) {
            if (lastDragPoint.HasValue) {
                Point posNow = e.GetPosition(MainWindow.Window.GraphCanvas);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                Canvas.SetLeft(this, Canvas.GetLeft(this) + dX);
                Canvas.SetTop(this, Canvas.GetTop(this) + dY);

                Location topLeft = new Location(MainWindow.Window.ConvertToLocation(new Point(Canvas.GetLeft(this), Canvas.GetTop(this))));

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
            LastLocationOfNodeControl = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
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

                Dispatcher.Invoke(delegate {
                    Animation.Highlight();
                });

                Thread.Sleep(250);

                if (Animation == null) return;

                Dispatcher.Invoke(delegate {
                    Animation.Normal();
                });

                if (pulses == REPARENT_ANIMATION_NB_PULSES) {
                    timer.Dispose();
                    Dispatcher.Invoke(delegate {
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
            Canvas.SetLeft(this, LastLocationOfNodeControl.Value.X);
            Canvas.SetTop(this, LastLocationOfNodeControl.Value.Y);

        }

        #endregion

        // Click Handling

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            if(MainWindow.Window.CurrentEditMode == MainWindow.EditMode.MOVE) {

                var pos = e.GetPosition(MainWindow.Window.GraphCanvas);
                lastDragPoint = pos;
                Mouse.Capture(this);

                // Show on top
                Panel.SetZIndex(this, 20);

                // Call reparent code
                OnMoveStarted();

                return;

            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                Node.OnClick();
            }

            // Unhighlight previously selected node if it wasn't this one.
            if (MainWindow.Window.Selected != this) {
                MainWindow.Window.DeselectAll();
            }

            MainWindow.Window.Selected = this;
            MainWindow.Window.LastClickedPoint = null;

            // Selection Animation - Init here to be sure every required parameter is set.
            SelectionAnimation = new NodeSelectionAnimation(this, 1);
            SelectionAnimation.Highlight(Color.FromRgb(33, 196, 93));

        }

        // Move mode

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

            if (MainWindow.Window.CurrentEditMode == MainWindow.EditMode.MOVE) {
                this.ReleaseMouseCapture();
                lastDragPoint = null;

                OnMoveStop();

                // Put back on normal level
                Panel.SetZIndex(this, 10);
            }

        }

        void MoveMode_OnMouseMove(object sender, MouseEventArgs e) {

            if (MainWindow.Window.CurrentEditMode == MainWindow.EditMode.MOVE) {
                Cursor = Cursors.SizeAll;
                ForceCursor = true;
            }
            else {
                ForceCursor = false;
            }

        }

        // Context Menu
        bool ShouldContextMenuBeOpened = false;

        void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {

            e.Handled = true;
            ContextMenu.IsOpen = false;

            if (!ShouldContextMenuBeOpened) {
                ContextMenu.IsOpen = false;
                e.Handled = true;
            }
            else {
                ShouldContextMenuBeOpened = false;
            }

        }

    }

}
