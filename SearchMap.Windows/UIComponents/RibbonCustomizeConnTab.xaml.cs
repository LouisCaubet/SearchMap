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
        public const int TAB_INDEX = 7;

        internal ICommand RevertShape { get; private set; }
        internal ICommand DeleteConnection { get; private set; }
        internal ICommand MakeSecondary { get; private set; }
        internal ICommand MakePrimary { get; private set; }

        public RibbonCustomizeConnTab() {
            InitializeComponent();

            ColorSelector.SelectedColorChanged += SelectedColorChanged;
        }

        private void SelectedColorChanged(object sender, RoutedEventArgs e) {
            
            if(ConnectionControl.Selected == null) {
                return;
            }

            var conn = ConnectionControl.Selected.Connection;

            if (ColorSelector.SelectedColor.HasValue) {
                var color = CoreToWPFUtils.WPFColorToCore(ColorSelector.SelectedColor.Value);
                conn.ShadowColor = color;
                ConnectionControl.Selected.Refresh();
            }

        }

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

            var conn = ConnectionControl.Selected.Connection;

            if (conn.IsBoldStyle) {
                // Must attribute new parent before deleting.
                // If the node doesnt have any siblings, parent is null.
                Node[] siblings = conn.GetDepartureNode().GetSiblings();
                if(siblings.Length == 0) {
                    conn.GetDepartureNode().SetParent(null);
                }
                else {
                    conn.GetDepartureNode().RemoveSibling(siblings[0].Id);
                    conn.GetDepartureNode().SetParent(siblings[0]);
                }
            }
            else {
                // Just delete it.
                conn.GetDepartureNode().RemoveSibling(conn.GetArrivalNode().Id);
            }

        }

        #endregion

        #region Make Secondary Command

        void MakeSecondary_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            if(ConnectionControl.Selected != null) {

                var conn = ConnectionControl.Selected.Connection;
                e.CanExecute = conn.IsBoldStyle;

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

            child.SetParent(null);

            child.AddSibling(parent);
            child.SetConnectionToSibling(parent, connToNewSibling);

        }

        #endregion

        #region Make Primary Command

        void MakePrimary_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            if (ConnectionControl.Selected != null) {

                var conn = ConnectionControl.Selected.Connection;
                // Can only be executed if one of the nodes doesnt have a parent yet. Else, user should use the reparent feature.
                e.CanExecute = !conn.IsBoldStyle && (conn.GetDepartureNode().GetParent() == null || conn.GetArrivalNode().GetParent() == null);

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
