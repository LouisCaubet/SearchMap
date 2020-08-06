using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace SearchMap.Windows.Controls {

    /// <summary>
    /// Stores logic for buttons of the Quick Access Toolbar.
    /// </summary>
    static class QuickAccessCommands {

        internal static ICommand SaveCommand { get; private set; }
        internal static ICommand SaveAsCommand { get; private set; }
        internal static ICommand OpenCommand { get; private set; }
        internal static ICommand NewCommand { get; private set; }

        /// <summary>
        /// Registers commands for the quick access toolbar buttons.
        /// </summary>
        internal static void RegisterCommands() {

            SaveCommand = new RoutedCommand("QuickAccess.Save", typeof(MainWindow));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(SaveCommand, Save_Execute, Save_CanExecute));
            MainWindow.Window.QuickAccessSave.Command = SaveCommand;

            OpenCommand = new RoutedCommand("QuickAccess.Open", typeof(MainWindow));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(OpenCommand, Open_Execute, Open_CanExecute));
            MainWindow.Window.QuickAccessOpen.Command = OpenCommand;

            NewCommand = new RoutedCommand("QuickAccess.New", typeof(MainWindow));
            MainWindow.Window.CommandBindings.Add(new CommandBinding(NewCommand, New_Execute, New_CanExecute));
            MainWindow.Window.QuickAccessNew.Command = NewCommand;

        }

        #region SaveCommand

        static void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        static void Save_Execute(object sender, ExecutedRoutedEventArgs e) {

            if (SearchMapCore.SearchMapCore.IsCurrentProjectSaved) {
                SearchMapCore.SearchMapCore.File.SaveGraph();
            }
            else {
                // Save As...
                SaveAs_Execute(null, null);
            }

        }

        #endregion

        #region Save As Command

        static void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        static void SaveAs_Execute(object sender, ExecutedRoutedEventArgs e) {
            SaveAsDialog();
        }

        internal static bool SaveAsDialog() {

            SaveFileDialog dialog = new SaveFileDialog() {
                AddExtension = true,
                DefaultExt = "*.smp",
                Filter = "SearchMap Project|*.smp",
                OverwritePrompt = true,
                Title = "Save Project"
            };

            bool? success = dialog.ShowDialog();

            if (success == true) {
                string path = dialog.FileName;
                SearchMapCore.SearchMapCore.NewProject(path, SearchMapCore.SearchMapCore.Graph);
                MainWindow.Window.Title = dialog.SafeFileName + " - SearchMap";
                SearchMapCore.SearchMapCore.IsCurrentProjectSaved = true;
            }

            return success == true;

        }

        #endregion

        #region OpenCommand

        static void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        static void Open_Execute(object sender, ExecutedRoutedEventArgs e) {

            OpenFileDialog openFileDialog = new OpenFileDialog() {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "smp",
                Title = "Open Project",
                Filter = "SearchMap Project|*.smp|All files|*.*"
            };

            if(openFileDialog.ShowDialog() == true) {
                string path = openFileDialog.FileName;
                SearchMapCore.SearchMapCore.OpenProject(path);
                MainWindow.Window.Title = openFileDialog.SafeFileName + " - SearchMap";
                SearchMapCore.SearchMapCore.IsCurrentProjectSaved = true;
            }

        }

        #endregion

        #region NewCommand

        static void New_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        static void New_Execute(object sender, ExecutedRoutedEventArgs e) {

            if (!SearchMapCore.SearchMapCore.IsCurrentProjectSaved) {

                var result = MessageBox.Show("Do you want to save the current project?", "Unsaved project", 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result) {

                    case MessageBoxResult.Yes:
                        if (!SaveAsDialog()) {
                            return;
                        }
                        break;

                    case MessageBoxResult.No:
                        break;

                    case MessageBoxResult.Cancel:
                        return;

                }

            }

            SearchMapCore.SearchMapCore.ShowNewGraph();
            MainWindow.Window.Title = "New Project - SearchMap";

        }

        #endregion


    }

}
