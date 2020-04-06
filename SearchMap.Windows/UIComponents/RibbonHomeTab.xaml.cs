using Fluent;
using SearchMap.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonHomeTab.xaml
    /// </summary>
    public partial class RibbonHomeTab : RibbonTabItem {

        internal ICommand Paste { get; private set; }
        internal ICommand Copy { get; private set; }
        internal ICommand Cut { get; private set; }

        internal ICommand NormalEditMode { get; private set; }
        internal ICommand MoveEditMode { get; private set; }
        internal ICommand ReparentEditMode { get; private set; }

        public RibbonHomeTab() {
            InitializeComponent();

            NormalEditModeButton.Click += NormalEditModeButton_Click;
            MoveEditModeButton.Click += MoveEditModeButton_Click;
            ReparentEditModeButton.Click += ReparentEditModeButton_Click;
        }

        

        /// <summary>
        /// Registers commands associated with the buttons of the Ribbon Home Tab.
        /// Must be called before any interaction with the ribbon.
        /// </summary>
        internal void RegisterCommands() {

            Paste = new RoutedCommand("HomeTabCommands.Paste", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Paste, Paste_Execute, Paste_CanExecute));
            PasteButton.Command = Paste;

            Copy = new RoutedCommand("HomeTabCommands.Copy", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Copy, Copy_Execute, Copy_CanExecute));
            CopyButton.Command = Copy;

            Cut = new RoutedCommand("HomeTabCommands.Cut", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(Cut, Cut_Execute, Copy_CanExecute));
            CutButton.Command = Cut;

            NormalEditMode = new RoutedCommand("HomeTabCommands.NormalEditMode", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NormalEditMode, NormalEditMode_Execute, EditMode_CanExecute));
            NormalEditModeButton.Command = NormalEditMode;

            MoveEditMode = new RoutedCommand("HomeTabCommands.MoveEditMode", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(MoveEditMode, MoveEditMode_Execute, EditMode_CanExecute));
            MoveEditModeButton.Command = MoveEditMode;

            ReparentEditMode = new RoutedCommand("HomeTabCommands.ReparentEditMode", typeof(RibbonHomeTab));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ReparentEditMode, ReparentEditMode_Execute, EditMode_CanExecute));
            ReparentEditModeButton.Command = ReparentEditMode;

        }

        /// <summary>
        /// Registers commands defined in the Insert ribbon tab. Must be called once those commands have been initialized.
        /// </summary>
        internal void RegisterInsertionCommands() {
            NewWebNodeButton.Command = MainWindow.Window.RibbonTabInsert.NewWebNode;

            NewConnectionButton.Command = MainWindow.Window.RibbonTabInsert.NewConnection;
            NewGrayConnButton.Command = MainWindow.Window.RibbonTabInsert.NewGrayConn;
            NewRedConnButton.Command = MainWindow.Window.RibbonTabInsert.NewRedConn;
            NewGreenConnButton.Command = MainWindow.Window.RibbonTabInsert.NewGreenConn;

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

        #region Edit Modes Commands

        void EditMode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        void NormalEditMode_Execute(object sender, ExecutedRoutedEventArgs e) {

            MainWindow.Window.CurrentEditMode = MainWindow.EditMode.NORMAL;
            MoveEditModeButton.IsChecked = false;
            ReparentEditModeButton.IsChecked = false;

            MainWindow.Window.GraphCanvas.Cursor = Cursors.Arrow;

        }

        void MoveEditMode_Execute(object sender, ExecutedRoutedEventArgs e) {

            MainWindow.Window.CurrentEditMode = MainWindow.EditMode.MOVE;
            NormalEditModeButton.IsChecked = false;
            ReparentEditModeButton.IsChecked = false;

            MainWindow.Window.GraphCanvas.Cursor = Cursors.SizeAll;

        }

        void ReparentEditMode_Execute(object sender, ExecutedRoutedEventArgs e) {

            MainWindow.Window.CurrentEditMode = MainWindow.EditMode.REPARENT;
            NormalEditModeButton.IsChecked = false;
            MoveEditModeButton.IsChecked = false;

            MainWindow.Window.GraphCanvas.Cursor = Cursors.Arrow;
        }

        #endregion

        #region Edit Mode Events

        private void ReparentEditModeButton_Click(object sender, RoutedEventArgs e) {
            if (MainWindow.Window.CurrentEditMode == MainWindow.EditMode.REPARENT) {
                e.Handled = true;
                ReparentEditModeButton.IsChecked = true;
            }
        }

        private void MoveEditModeButton_Click(object sender, RoutedEventArgs e) {
            if (MainWindow.Window.CurrentEditMode == MainWindow.EditMode.MOVE) {
                e.Handled = true;
                MoveEditModeButton.IsChecked = true;
            }
        }

        private void NormalEditModeButton_Click(object sender, RoutedEventArgs e) {

            if (MainWindow.Window.CurrentEditMode == MainWindow.EditMode.NORMAL) {
                e.Handled = true;
                NormalEditModeButton.IsChecked = true;
            }

        }

        #endregion

    }
}
