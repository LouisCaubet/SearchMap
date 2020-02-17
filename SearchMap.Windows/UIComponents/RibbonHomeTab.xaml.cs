using Fluent;
using SearchMap.Windows.Controls;
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
    /// Logique d'interaction pour RibbonHomeTab.xaml
    /// </summary>
    public partial class RibbonHomeTab : RibbonTabItem {

        protected ICommand Paste { get; private set; }
        protected ICommand Copy { get; private set; }
        protected ICommand Cut { get; private set; }

        public RibbonHomeTab() {
            InitializeComponent();
        }

        public void RegisterCommands() {

            Paste = new RoutedCommand("HomeTabCommands.Paste", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Paste, Paste_Execute, Paste_CanExecute));
            PasteButton.Command = Paste;

            Copy = new RoutedCommand("HomeTabCommands.Copy", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Copy, Copy_Execute, Copy_CanExecute));
            CopyButton.Command = Copy;

            Cut = new RoutedCommand("HomeTabCommands.Cut", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Cut, Cut_Execute, Copy_CanExecute));
            CutButton.Command = Cut;

        }

        // Paste Button
        #region Paste

        void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = ClipboardManager.ClipboardContainsNode() || ApplicationCommands.Paste.CanExecute(e.Parameter, Keyboard.FocusedElement);
        }

        void Paste_Execute(object sender, ExecutedRoutedEventArgs e) {

            if(Keyboard.FocusedElement.GetType() == typeof(System.Windows.Controls.TextBox)) {
                ApplicationCommands.Paste.Execute(e.Parameter, Keyboard.FocusedElement);
            }
            else {
                ClipboardManager.Paste(MainWindow.Window.LastClickedPoint);
            }
        }

        #endregion Paste

        #region Copy

        void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainWindow.Window.Selected != null || ApplicationCommands.Copy.CanExecute(e.Parameter, Keyboard.FocusedElement);
        }

        void Copy_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (Keyboard.FocusedElement.GetType() == typeof(System.Windows.Controls.TextBox)
                && ((System.Windows.Controls.TextBox)Keyboard.FocusedElement).SelectedText != "") {

                ApplicationCommands.Copy.Execute(e.Parameter, Keyboard.FocusedElement);

            }
            else {
                ClipboardManager.CopyToClipboard(MainWindow.Window.Selected);
            }
        }

        #endregion Copy

        #region Cut

        void Cut_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (Keyboard.FocusedElement.GetType() == typeof(System.Windows.Controls.TextBox)) {
                ApplicationCommands.Cut.Execute(e.Parameter, Keyboard.FocusedElement);
            }
            else {
                ClipboardManager.CopyToClipboard(MainWindow.Window.Selected, true);
            }
        }

        #endregion Cut

    }
}
