using Fluent;
using SearchMap.Windows.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        }

        #region New Web Node Command

        void NewWebNode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void NewWebNode_Execute(object sender, ExecutedRoutedEventArgs e) {
            var dialog = new NewWebNodeDialog();
            dialog.Show();
        }

        #endregion

    }
}
