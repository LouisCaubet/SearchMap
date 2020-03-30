using Fluent;
using SearchMap.Windows.Dialog;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonInsertTab.xaml
    /// </summary>
    public partial class RibbonInsertTab : RibbonTabItem {

        internal ICommand NewWebNode { get; private set; }

        public RibbonInsertTab() {
            InitializeComponent();
        }

        public void RegisterCommands() {

            NewWebNode = new RoutedCommand("InsertTabCommands.NewWebNode", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewWebNode, NewWebNode_Execute, NewWebNode_CanExecute));
            NewWebNodeButton.Command = NewWebNode;

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

    }

}
