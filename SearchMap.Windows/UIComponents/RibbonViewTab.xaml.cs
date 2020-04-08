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

        // TODO move to user preferences
        public bool ShowNodeFlipAnimation { get; internal set; }

        private bool IsGridShown = false;

        internal ICommand ShowGrid { get; private set; }
        internal ICommand ShowNodeFlipAnim { get; private set; }

        public RibbonViewTab() {
            InitializeComponent();

            // Default ShowNodeFlipAnimation = true
            // TODO load from user preferences
            ShowNodeFlipAnimation = true;
            ShowNodeFlipAnimButton.IsChecked = true;

        }

        /// <summary>
        /// Registers commands associated with buttons in the View tab
        /// </summary>
        internal void RegisterCommands() {

            ShowGrid = new RoutedCommand("RibbonViewTab.ShowGrid", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ShowGrid, ShowGrid_Execute, ShowGrid_CanExecute));
            ShowGridButton.Command = ShowGrid;

            ShowNodeFlipAnim = new RoutedCommand("RibbonViewTab.ShowNodeFlipAnim", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ShowNodeFlipAnim, ShowNodeFlipAnim_Execute));
            ShowNodeFlipAnimButton.Command = ShowNodeFlipAnim;

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

        #region Show/Hide Node Flip Animation

        void ShowNodeFlipAnim_Execute(object sender, ExecutedRoutedEventArgs e) {
            ShowNodeFlipAnimation = !ShowNodeFlipAnimation;
        }

        #endregion

    }
}
