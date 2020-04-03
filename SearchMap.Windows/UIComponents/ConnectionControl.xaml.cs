﻿using SearchMap.Windows.Utils;
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

        public ConnectionControl(Connection connection) {
            InitializeComponent();

            Connection = connection;

            Refresh();

            // EVENT HANDLING
            // See ConnectionControl_Events.cs
            RegisterEventHandlers();

        }

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


            // Colors :
            if (Connection.Color != null) {
                Path.Stroke = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Connection.Color));
            }

            if (Connection.ShadowColor != null) {
                ShadowEffect.Color = CoreToWPFUtils.CoreColorToWPF(Connection.ShadowColor);
            }

            // (Colors default to white with blue shadow - see XAML)

        }

        public void SetSelected() {
            Selected = this;
            Path.StrokeThickness = 10;
            MainWindow.Window.connection_tools.Visibility = Visibility.Visible;
            MainWindow.Window.Ribbon.SelectedTabIndex = RibbonCustomizeConnTab.TAB_INDEX;
        }

        public void SetUnselected() {
            Selected = null;
            Path.StrokeThickness = 5;
            MainWindow.Window.Ribbon.SelectedTabIndex = MainWindow.Window.RibbonTabIndex;
            MainWindow.Window.connection_tools.Visibility = Visibility.Collapsed;
        }


    }
}
