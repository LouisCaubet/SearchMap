using Fluent;
using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using System.Windows;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonCustomizeConnTab.xaml
    /// </summary>
    public partial class RibbonCustomizeConnTab : RibbonTabItem {

        /// <summary>
        /// The index in the ribbon of this tab
        /// </summary>
        public const int TAB_INDEX = 8;

        internal ICommand RevertShape { get; private set; }
        internal ICommand DeleteConnection { get; private set; }
        internal ICommand MakeSecondary { get; private set; }
        internal ICommand MakePrimary { get; private set; }

        public RibbonCustomizeConnTab() {
            InitializeComponent();

            // Events
            ColorSelector.SelectedColorChanged += SelectedColorChanged;

        }

        private void SelectedColorChanged(object sender, RoutedEventArgs e) {
            
            if(ConnectionControl.Selected == null) {
                return;
            }

            var conn = ConnectionControl.Selected.Connection;

            if (ColorSelector.SelectedColor.HasValue) {
                var color = CoreToWPFUtils.WPFColorToCore(ColorSelector.SelectedColor.Value);

                // Revert point
                conn.TakeSnapshot();

                conn.ShadowColor = color;
                ConnectionControl.Selected.Refresh();
            }

        }

        /// <summary>
        /// Registers commands associated to the buttons in this ribbon tab.
        /// </summary>
        internal void RegisterCommands() {

            RevertShape = new RoutedCommand("RibbonConnectionTab.RevertShape", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(RevertShape, RevertShape_Execute, RevertShape_CanExecute));
            RevertButton.Command = RevertShape;

            DeleteConnection = new RoutedCommand("RibbonConnectionTab.DeleteConnection", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(DeleteConnection, Delete_Execute, Delete_CanExecute));
            DeleteButton.Command = DeleteConnection;

            MakeSecondary = new RoutedCommand("RibbonConnectionTab.MakeSecondary", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(MakeSecondary, MakeSecondary_Execute, MakeSecondary_CanExecute));
            MakeSecondaryButton.Command = MakeSecondary;

            MakePrimary = new RoutedCommand("RibbonConnectionTab.MakePrimary", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(MakePrimary, MakePrimary_Execute, MakePrimary_CanExecute));
            MakePrimaryButton.Command = MakePrimary;

        }

        #region Revert Shape Command

        void RevertShape_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            if(ConnectionControl.Selected == null) {
                e.CanExecute = false;
            }
            else {
                var conn = ConnectionControl.Selected.Connection;
                e.CanExecute = conn.IsCustomizedByUser;
            }

        }

        void RevertShape_Execute(object sender, ExecutedRoutedEventArgs e) {

            if(ConnectionControl.Selected == null) {
                SearchMapCore.SearchMapCore.Logger.Error("Revert Shape executed, but no connection is selected.");
                return;
            }

            var conn = ConnectionControl.Selected.Connection;

            // Revert point.
            conn.TakeSnapshot();

            conn.IsCustomizedByUser = false;
            for(int i=0; i<4; i++) {
                conn.UserImposedPoints[i] = null;
            }

            ConnectionPlacement.RefreshConnection(MainWindow.Window.GetGraph(), conn);
            MainWindow.Renderer.RefreshCurvedLine(conn.RenderId);

        }

        #endregion

        #region Delete Command

        void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = ConnectionControl.Selected != null;
        }

        void Delete_Execute(object sender, ExecutedRoutedEventArgs e) {

            if (ConnectionControl.Selected == null) {
                SearchMapCore.SearchMapCore.Logger.Error("Delete Connection executed, but no connection is selected.");
                return;
            }

            ConnectionControl.Selected.DeleteConnection();

        }

        #endregion

        #region Make Secondary Command

        void MakeSecondary_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            if(ConnectionControl.Selected != null) {
                var conn = ConnectionControl.Selected.Connection;
                e.CanExecute = conn.IsBoldStyle;

                // Tooltip
                if (!conn.IsBoldStyle) {
                    MakeSecondaryScreenTip.Text = "Secondary connections are used to indicate weaker relations between nodes." +
                        "\n\n" + "This feature is disabled, as the selected connection is already secondary.";
                }
                else {
                    MakeSecondaryScreenTip.Text = "Secondary connections are used to indicate weaker relations between nodes.";
                }

            }
            else {
                e.CanExecute = false;
            }

        }

        void MakeSecondary_Execute(object sender, ExecutedRoutedEventArgs e) {

            if(ConnectionControl.Selected == null) {
                SearchMapCore.SearchMapCore.Logger.Error("Make Connection Secondary executed, but no connection is selected.");
                return;
            }

            var conn = ConnectionControl.Selected.Connection;

            Node parent;
            Node child;

            if(conn.GetDepartureNode().GetParent().Id == conn.GetArrivalNode().Id) {
                parent = conn.GetArrivalNode();
                child = conn.GetDepartureNode();
            }
            else {
                parent = conn.GetDepartureNode();
                child = conn.GetArrivalNode();
            }

            var connToNewSibling = child.ConnectionToParent;

            child.SetParent(null, true);

            // Snapshot already taken in SetParent.
            child.AddSibling(parent, false);
            child.SetConnectionToSibling(parent, connToNewSibling);

        }

        #endregion

        #region Make Primary Command

        void MakePrimary_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            if (ConnectionControl.Selected != null) {

                var conn = ConnectionControl.Selected.Connection;
                // Can only be executed if one of the nodes doesnt have a parent yet. Else, user should use the reparent feature.
                e.CanExecute = !conn.IsBoldStyle && (conn.GetDepartureNode().GetParent() == null || conn.GetArrivalNode().GetParent() == null);

                MakePrimaryScreenTip.Text = "Primary connections represent the main relations between nodes. They connect a child node to its only parent.";

                if (!e.CanExecute) {
                    if (conn.IsBoldStyle) {
                        MakePrimaryScreenTip.Text += "\n\nThis feature is disabled, as the selected connection is already primary.";
                    }
                    else if (conn.GetDepartureNode().GetParent() != null && conn.GetArrivalNode().GetParent() != null) {
                        MakePrimaryScreenTip.Text += "\n\nThis feature is disabled, because both nodes connected by this connection already have a parent. " +
                            "To change the parent, drag the child node on another node, which will become the parent.";
                    }
                }

            }
            else {
                e.CanExecute = false;
            }

        }

        void MakePrimary_Execute(object sender, ExecutedRoutedEventArgs e) {

            var conn = ConnectionControl.Selected.Connection;
            Node parent, child;

            // If none of the nodes has a parent yet, use the oldest as parent.
            // This does not make a graphic difference, but makes more sense.
            if(conn.GetDepartureNode().GetParent() == null && conn.GetArrivalNode().GetParent() == null) {
                parent = conn.GetDepartureNode().Id < conn.GetArrivalNode().Id ? conn.GetDepartureNode() : conn.GetArrivalNode();
                child = conn.GetDepartureNode().Id == parent.Id ? conn.GetArrivalNode() : conn.GetDepartureNode();
            }
            else if(conn.GetDepartureNode().GetParent() == null) {
                child = conn.GetDepartureNode();
                parent = conn.GetArrivalNode();
            }
            else {
                parent = conn.GetDepartureNode();
                child = conn.GetArrivalNode();
            }

            child.SetParent(parent);

        }

        #endregion


    }

}
