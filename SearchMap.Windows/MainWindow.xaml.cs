using Fluent;
using SearchMap.Windows.Controls;
using SearchMap.Windows.Dialog;
using SearchMap.Windows.Rendering;
using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SearchMap.Windows {

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow {

        /// <summary>
        /// The scale factor from abstract length to real length.
        /// 1 for computer seems good, higher for phones, etc.
        /// </summary>
        public const double SCALE_FACTOR = 1;

        /// <summary>
        /// Stores the only instance of MainWindow.
        /// </summary>
        public static MainWindow Window;

        internal static GraphRenderer Renderer;

        /// <summary>
        /// Indicates which control is currently selected.
        /// </summary>
        internal NodeControl Selected { get; set; }

        /// <summary>
        /// Indicates where to paste node.
        /// </summary>
        internal Point? LastClickedPoint { get; set; }

        // Move by dragging
        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        public MainWindow() {
            InitializeComponent();

            if(Window != null) {
                throw new Exception("MainWindow can only be instanciated once.");
            }

            Window = this;
            Renderer = new GraphRenderer();

            WindowState = WindowState.Maximized;

            ContentRendered += OnWindowLoaded;
            Closed += OnWindowClose;

            RegisterEventHandlers();

            // Hide ScrollView when no project is opened - disabled for testing
            // ScrollView.Visibility = Visibility.Hidden;
            
        }

        void OnWindowLoaded(object sender, EventArgs args) {

            MoveToCenterOfCanvas();

            // RenderTestWebNode();
            // RenderTestConnection();

            Renderer = new GraphRenderer();

            Graph test = SearchMapCore.SearchMapCore.CreateTestGraph();
            test.Render(Renderer);
            SearchMapCore.SearchMapCore.Graph = test;
            // test.Renderer.RenderNode(test.RootNode);
            // test.RootNode.Render();

            OnWindowSizeChanged(null, null);

            MinWidth = Width / 3;
            MinHeight = Height / 3;

            // Register Commands
            RibbonTabHome.RegisterCommands();
            RibbonTabInsert.RegisterCommands();

        }

        void OnWindowClose(object sender, EventArgs e) {
            // Close all subwindows
            if (NewWebNodeDialog.Instance != null) NewWebNodeDialog.Instance.Close();
        }

        /// <summary>
        /// Converts the abstract Location in on-screen Point. 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Point ConvertFromLocation(Location location) {
            return new Point {
                X = location.x * SCALE_FACTOR + GraphCanvas.Width / 2,
                Y = location.y * SCALE_FACTOR + GraphCanvas.Height / 2
            };
        }

        /// <summary>
        /// Converts an on-screen Point to a Core Location.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Location ConvertToLocation(Point pt) {
            return new Location {
                x = (int)((pt.X - GraphCanvas.Width / 2) / SCALE_FACTOR),
                y = (int)((pt.Y - GraphCanvas.Height / 2) / SCALE_FACTOR)
            };
        }

        /// <summary>
        /// Resets the scrollview's position to the center of the canvas.
        /// </summary>
        public void MoveToCenterOfCanvas() {
            // Center scrollview 

            ScrollView.ScrollToVerticalOffset((GraphCanvas.Height - ScrollView.ActualHeight) / 2);
            ScrollView.ScrollToHorizontalOffset((GraphCanvas.Width - ScrollView.ActualWidth) / 2);

        }

        /// <summary>
        /// Returns the graph currently visible in MainWindow.
        /// </summary>
        /// <returns></returns>
        public Graph GetGraph() {
            return SearchMapCore.SearchMapCore.Graph;
        }

        /// <summary>
        /// Deselects the selected objects, if any.
        /// </summary>
        public void DeselectAll() {

            if (Selected != null) {
                Selected.SelectionAnimation.Normal();
                Selected = null;
            }

        }

        // EVENT HANDLING
        // See MainWindow_Events.cs

        // TESTS ------------------------------------------------------------------------------------------------------------------------------

        void RenderTestWebNode() {

            Graph graph = new Graph();

            WebNode node = new WebNode(graph, new Uri("https://www.wikipedia.org"), "") {
                Title = "Wikipedia",
                Color = new SearchMapCore.Rendering.Color(255, 255, 255, 255),
                Comment = "A great website!",

            };

            node.MoveTo(new Location(0, 0));
            graph.RootNode = node;

            node.Resize(400, 200);

            WebNodeControl comp = new WebNodeControl(node) {
                Width = 500,
                Height = 250
            };

            // Place on canvas. The pt represents center coordinates, translating is needed.
            Point pt = ConvertFromLocation(node.Location);
            Canvas.SetLeft(comp, pt.X - node.Width / 2);
            Canvas.SetTop(comp, pt.Y - node.Height / 2);

            GraphCanvas.Children.Add(comp);

        }

        void RenderTestConnection() {

            Connection connection = new Connection(null);
            connection.Points.Add(new Location(0, 0));
            connection.Points.Add(new Location(100, 100));
            connection.Points.Add(new Location(100, 0));
            connection.Points.Add(new Location(50, -100));
            connection.Points.Add(new Location(0, -100));
            connection.Points.Add(new Location(-50, 0));

            ConnectionControl control = new ConnectionControl(connection);

            //control.Width = control.WidthToSet;
            //control.Height = control.HeightToSet;

            Point loc = control.PositionOnCanvas;
            Canvas.SetTop(control, loc.Y);
            Canvas.SetLeft(control, loc.X);

            GraphCanvas.Children.Add(control);

        }

    }

}
