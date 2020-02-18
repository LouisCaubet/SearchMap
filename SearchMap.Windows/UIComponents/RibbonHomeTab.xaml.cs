using Fluent;
using SearchMap.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonHomeTab.xaml
    /// </summary>
    public partial class RibbonHomeTab : RibbonTabItem {

        internal ICommand Paste { get; private set; }
        internal ICommand Copy { get; private set; }
        internal ICommand Cut { get; private set; }

        public RibbonHomeTab() {
            InitializeComponent();
        }

        /// <summary>
        /// Registers commands associated with the buttons of the Ribbon Home Tab.
        /// Must be called before any interaction with the ribbon.
        /// </summary>
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

        // Command definitions

        #region Paste Command

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

        #region Copy Command

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

        #region Cut Command

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
