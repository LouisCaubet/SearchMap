using SearchMapCore.Graph;
using System;
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

            // Editing
            TitleBox.TextChanged += OnTitleChanged;
            CommentBox.TextChanged += OnCommentChanged;

            // Ctrl + Click to open
            MouseLeftButtonDown += OnMouseLeftButtonDown;

            // ContextMenu
            ContextMenuOpening += OnContextMenuOpening;

        }


        // Move by drag and drop
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