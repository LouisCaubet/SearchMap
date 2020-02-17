using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    public partial class WebNodeControl {

        // EVENT HANDLING

        void RegisterEventHandlers() {

            // Drag and drop
            MouseRightButtonUp += OnMouseRightButtonUp;
            PreviewMouseRightButtonUp += OnMouseRightButtonUp;
            PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            MouseMove += OnMouseMove;

            RegisterScrollManager();

            // Editing
            TitleBox.TextChanged += OnTitleChanged;
            CommentBox.TextChanged += OnCommentChanged;

            // Ctrl + Click to open
            MouseLeftButtonDown += OnMouseLeftButtonDown;

            // ContextMenu
            ContextMenuOpening += OnContextMenuOpening;

        }


        // Move by drag and drop

        // Scrolls the view when the node is dragged out of sight.
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

                    Console.WriteLine("Action invoked");

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
                }

            }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10));

        }

        #endregion ScrollWhenOutOfBounds

        #region MoveByDragDrop

        private bool IsRightClickDown = false;
        
        void OnMouseRightButtonDown(object sender, MouseButtonEventArgs args) {

            ContextMenu.IsOpen = false;

            if (this.IsMouseOver) {

                IsRightClickDown = true;

                var pos = args.GetPosition(MainWindow.Window.GraphCanvas);
                lastDragPoint = pos;
                Mouse.Capture(this);

                // To distiguish between right click and move, check if the mouse is released 0.1 seconds later.
                new Timer(delegate {

                    if(IsRightClickDown) {
                        // User wants to move the node

                        Dispatcher.Invoke(delegate {
                            this.Cursor = Cursors.SizeAll;

                            // Show on top
                            Panel.SetZIndex(this, 20);
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

            this.Cursor = Cursors.Arrow;
            this.ReleaseMouseCapture();
            lastDragPoint = null;

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
                    Console.WriteLine("ScrollAction LEFT added to queue");
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

        // Editing

        void OnTitleChanged(object sender, TextChangedEventArgs e) {
            Node.Title = TitleBox.Text;
        }

        void OnCommentChanged(object sender, TextChangedEventArgs e) {
            Node.Comment = CommentBox.Text;
        }
        
        // Click Handling

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                Node.OnClick();
            }

            MainWindow.Window.Selected = this;
            MainWindow.Window.LastClickedPoint = null;

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