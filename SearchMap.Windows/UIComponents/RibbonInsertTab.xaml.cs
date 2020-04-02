using Fluent;
using SearchMap.Windows.Dialog;
using SearchMapCore.Graph;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SearchMapCore.Rendering;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonInsertTab.xaml
    /// </summary>
    public partial class RibbonInsertTab : RibbonTabItem {

        internal ICommand NewWebNode { get; private set; }

        internal ICommand NewConnection { get; private set; }
        internal ICommand NewGrayConn { get; private set; }
        internal ICommand NewRedConn { get; private set; }
        internal ICommand NewGreenConn { get; private set; }

        public RibbonInsertTab() {
            InitializeComponent();
        }

        public void RegisterCommands() {

            NewWebNode = new RoutedCommand("InsertTabCommands.NewWebNode", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewWebNode, NewWebNode_Execute, NewWebNode_CanExecute));
            NewWebNodeButton.Command = NewWebNode;

            NewConnection = new RoutedCommand("InsertTabCommands.NewConnection", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewConnection, NewConnection_Execute, NewConnection_CanExecute));
            NewConnectionButton.Command = NewConnection;

            NewGrayConn = new RoutedCommand("InsertTabCommands.NewGrayConn", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewGrayConn, NewGrayConn_Execute, NewConnection_CanExecute));
            NewGrayConnButton.Command = NewGrayConn;

            NewRedConn = new RoutedCommand("InsertTabCommands.NewRedConn", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewRedConn, NewRedConn_Execute, NewConnection_CanExecute));
            NewRedConnButton.Command = NewRedConn;

            NewGreenConn = new RoutedCommand("InsertTabCommands.NewGreenConn", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewGreenConn, NewGreenConn_Execute, NewConnection_CanExecute));
            NewGreenConnButton.Command = NewGreenConn;

            // Insertion commands from Home tab
            MainWindow.Window.RibbonTabHome.RegisterInsertionCommands();

        }

        #region New Web Node Command

        void NewWebNode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void NewWebNode_Execute(object sender, ExecutedRoutedEventArgs e) {
            var dialog = new NewWebNodeDialog();
            dialog.ShowDialog();
        }

        #endregion

        #region New Connections

        // Colors
        private static readonly Color GRAY_CONN = new Color(100, 100, 100);
        private static readonly Color GREEN_CONN = new Color(0, 255, 0);
        private static readonly Color RED_CONN = new Color(255, 0, 0);

        Color LastUsedColor = RED_CONN;
        Task Running;
        CancellationTokenSource CancelRunning = new CancellationTokenSource();

        /// <summary>
        /// Lets user select two nodes and creates a sibling connection between these nodes.
        /// </summary>
        /// <param name="color">The color of the connection to create.</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns></returns>
        async Task CreateConnection(Color color, CancellationToken ct) {

            MainWindow.Window.DeselectAll();
            MainWindow.Window.StatusBarInstructionField.Value = "Please select the first node...";

            // Waits for user to select a node and returns the selected node.
            Node WaitForUserToSelectNode() {
                while (MainWindow.Window.Selected == null) {
                    // Continue to wait
                    Thread.Sleep(10);
                }
                Node node = MainWindow.Window.Selected.Node;

                MainWindow.Window.Dispatcher.Invoke(delegate {
                    MainWindow.Window.DeselectAll();
                });

                return node;
            }

            Node node1 = await Task.Run(WaitForUserToSelectNode, ct);

            MainWindow.Window.StatusBarInstructionField.Value = "Please select the second node...";

            Node node2 = await Task.Run(WaitForUserToSelectNode, ct);

            try {
                node1.AddSibling(node2);
            }
            catch(ArgumentException e) {

                MainWindow.Window.StatusBarInstructionField.Value = e.Message;
                new Timer(delegate {
                    MainWindow.Window.Dispatcher.Invoke(delegate {
                        MainWindow.Window.StatusBarInstructionField.Value = "";
                    });
                }, null, TimeSpan.FromMilliseconds(5000), TimeSpan.FromMilliseconds(-1));

                return;

            }

            // Set requested color.
            int id = node1.GetConnectionToSiblingId(node2);
            ConnectionControl control = (ConnectionControl) MainWindow.Renderer.RenderedObjects[id];
            control.Connection.ShadowColor = color;
            control.Refresh();

            MainWindow.Window.StatusBarInstructionField.Value = "";

        }


        void NewConnection_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void NewConnection_Execute(object sender, ExecutedRoutedEventArgs e) {

            CancelAllTasks();
            CancelRunning = new CancellationTokenSource();
            Running = CreateConnection(LastUsedColor, CancelRunning.Token);

        }


        void NewGrayConn_Execute(object sender, ExecutedRoutedEventArgs e) {

            LastUsedColor = GRAY_CONN;
            NewConnectionButton.LargeIcon = "../Resources/Connection_Gray.png";
            MainWindow.Window.RibbonTabHome.NewConnectionButton.LargeIcon = "../Resources/Connection_Gray.png";
            NewConnection_Execute(sender, e);

        }


        void NewRedConn_Execute(object sender, ExecutedRoutedEventArgs e) {

            LastUsedColor = RED_CONN;
            NewConnectionButton.LargeIcon = "../Resources/Connection_Red.png";
            MainWindow.Window.RibbonTabHome.NewConnectionButton.LargeIcon = "../Resources/Connection_Red.png";
            NewConnection_Execute(sender, e);

        }


        void NewGreenConn_Execute(object sender, ExecutedRoutedEventArgs e) {

            LastUsedColor = GREEN_CONN;
            NewConnectionButton.LargeIcon = "../Resources/Connection_Green.png";
            MainWindow.Window.RibbonTabHome.NewConnectionButton.LargeIcon = "../Resources/Connection_Green.png";
            NewConnection_Execute(sender, e);

        }

        /// <summary>
        /// Cancels all async tasks linked to commands from this ribbon tab.
        /// </summary>
        public void CancelAllTasks() {
            if(Running != null) {
                CancelRunning.Cancel();
                MainWindow.Window.StatusBarInstructionField.Value = "";
                Running = null;
            }
        }

        #endregion

    }

}
