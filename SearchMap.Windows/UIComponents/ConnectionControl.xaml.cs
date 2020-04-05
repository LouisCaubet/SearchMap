using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : UserControl {

        private const double WIDTH_SECONDARY = 5;
        private const double WIDTH_PRIMARY = 12;
        private const double SHADOW_SECONDARY = 20;
        private const double SHADOW_PRIMARY = 45;

        /// <summary>
        /// The currently selected connection.
        /// </summary>
        public static ConnectionControl Selected = null;

        /// <summary>
        /// The connection represented by this control
        /// </summary>
        public Connection Connection { get; }

        /// <summary>
        /// Where the connection must be placed on canvas.
        /// </summary>
        internal Point PositionOnCanvas { get; private set; }

        /// <summary>
        /// Creates a new Connection Control representing the given Connection
        /// </summary>
        /// <param name="connection"></param>
        public ConnectionControl(Connection connection) {
            InitializeComponent();

            Connection = connection;

            Refresh();

            // EVENT HANDLING
            // See ConnectionControl_Events.cs
            RegisterEventHandlers();

        }

        /// <summary>
        /// Renders changes of the Connection object to this Control.
        /// </summary>
        public void Refresh() {

            // Convert the abstract connection to real points
            List<Point> points = new List<Point>();

            foreach (Location loc in Connection.Points) {
                points.Add(MainWindow.Window.ConvertFromLocation(loc));
            }

            // Determine TopLeft and BottomRight
            List<double> ylist = new List<double>();
            List<double> xlist = new List<double>();

            foreach (Point pt in points) {
                ylist.Add(pt.Y);
                xlist.Add(pt.X);
            }

            // double maxY = MathUtils.Max(ylist);
            double minY = MathUtils.Min(ylist);
            // double maxX = MathUtils.Max(xlist);
            double minX = MathUtils.Min(xlist);



            // Compute position and size
            PositionOnCanvas = new Point(minX, minY);

            // Positions relative to this Control
            List<Point> draw_points = new List<Point>();

            foreach (Point pt in points) {
                Point draw_pt = new Point(pt.X - minX, pt.Y - minY);
                draw_points.Add(draw_pt);
            }


            // Draw
            Figure.StartPoint = draw_points[0];
            HitboxFigure.StartPoint = draw_points[0];

            // Note: a multiple of 3 points is required.
            PolySegment.Points = new PointCollection(draw_points.GetRange(1, 3));
            HitboxPolySegment.Points = new PointCollection(draw_points.GetRange(1, 3));

            // Thickness
            if (Connection.IsBoldStyle) {
                Path.StrokeThickness = WIDTH_PRIMARY;
                ShadowEffect.BlurRadius = SHADOW_PRIMARY;
            }
            else {
                Path.StrokeThickness = WIDTH_SECONDARY;
                ShadowEffect.BlurRadius = SHADOW_SECONDARY;
            }

            // Colors :
            if (Connection.Color != null) {
                Path.Stroke = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Connection.Color));
            }

            if (Connection.ShadowColor != null) {
                ShadowEffect.Color = CoreToWPFUtils.CoreColorToWPF(Connection.ShadowColor);
            }

            // (Colors default to white with blue shadow - see XAML)

        }

        /// <summary>
        /// Selects this connection, allowing the "Connection" tab to interact with it.
        /// </summary>
        public void SetSelected() {

            Selected = this;
            Path.Stroke = new SolidColorBrush(Color.FromRgb(225, 225, 225));

            MainWindow.Window.connection_tools.Visibility = Visibility.Visible;
            MainWindow.Window.Ribbon.SelectedTabIndex = RibbonCustomizeConnTab.TAB_INDEX;

        }

        /// <summary>
        /// Deselects this connection.
        /// </summary>
        public void SetUnselected() {
            Selected = null;

            if (Connection.Color != null) {
                Path.Stroke = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Connection.Color));
            }
            else Path.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            MainWindow.Window.Ribbon.SelectedTabIndex = MainWindow.Window.RibbonTabIndex;
            MainWindow.Window.connection_tools.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Deletes this control and the associated connection.
        /// </summary>
        public void DeleteConnection() {

            if (Connection.IsBoldStyle) {
                // Must attribute new parent before deleting.
                // If the node doesnt have any siblings, parent is null.
                Node[] siblings = Connection.GetDepartureNode().GetSiblings();
                if (siblings.Length == 0) {
                    Connection.GetDepartureNode().SetParent(null);
                }
                else {
                    Connection.GetDepartureNode().RemoveSibling(siblings[0].Id);
                    Connection.GetDepartureNode().SetParent(siblings[0]);
                }
            }
            else {
                // Just delete it.
                Connection.GetDepartureNode().RemoveSibling(Connection.GetArrivalNode().Id);
            }

            MainWindow.Window.DeselectAll();

        }

    }
}
