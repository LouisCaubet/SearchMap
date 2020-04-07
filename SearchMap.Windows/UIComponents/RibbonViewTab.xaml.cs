using Fluent;
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
    /// Logique d'interaction pour RibbonViewTab.xaml
    /// </summary>
    public partial class RibbonViewTab : RibbonTabItem {

        private bool IsGridShown = false;

        internal ICommand ShowGrid { get; private set; }

        public RibbonViewTab() {
            InitializeComponent();
        }

        internal void RegisterCommands() {

            ShowGrid = new RoutedCommand("RibbonViewTab.ShowGrid", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ShowGrid, ShowGrid_Execute, ShowGrid_CanExecute));
            ShowGridButton.Command = ShowGrid;

        }

        #region Show/Hide Grid Command

        void ShowGrid_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void ShowGrid_Execute(object sendre, ExecutedRoutedEventArgs e) {

            if (IsGridShown) {
                IsGridShown = false;
                MainWindow.Window.GridPen.Thickness = 0;
            }
            else {
                IsGridShown = true;
                MainWindow.Window.GridPen.Thickness = 1;
            }

        }

        #endregion

    }
}
