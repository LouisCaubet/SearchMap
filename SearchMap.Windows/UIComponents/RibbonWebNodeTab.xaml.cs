using Fluent;
using SearchMap.Windows.Utils;
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
    /// Logique d'interaction pour RibbonWebNodeTab.xaml
    /// </summary>
    public partial class RibbonWebNodeTab : RibbonTabItem {

        public const int TAB_INDEX = 6;

        private ICollection<FontFamily> AvailableFonts { get; set; }

        internal ICommand ToggleBold { get; private set; }
        internal ICommand ToggleItalic { get; private set; }
        internal ICommand ToggleUnderline { get; private set; }
        internal ICommand ToggleStriketrough { get; private set; }

        public RibbonWebNodeTab() {
            InitializeComponent();

            AvailableFonts = Fonts.SystemFontFamilies;

            foreach(FontFamily font in AvailableFonts) {
                FontComboBox.Items.Add(new TextBlock() {
                    Text = font.Source,
                    Tag = "All Fonts"
                });
            }

            // Events
            BackgroundColorSelector.SelectedColorChanged += SelectedBackgroundColorChanged;
            BorderColorSelector.SelectedColorChanged += SelectedBorderColorChanged;

            FontComboBox.SelectionChanged += OnFontSelectionChanged;
            comboBoxFontSize.SelectionChanged += OnFontSizeChanged;
            comboBoxFontSize.TextInput += OnFontSizeTyped;

        }

        internal void RegisterCommands() {

            ToggleBold = new RoutedCommand("WebNodeTab.ToggleBold", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleBold, ToggleBold_Execute, FontCommands_CanExecute));
            buttonBold.Command = ToggleBold;

            ToggleItalic = new RoutedCommand("WebNodeTab.ToggleItalic", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleItalic, ToggleItalic_Execute, FontCommands_CanExecute));
            buttonItalic.Command = ToggleItalic;

            ToggleUnderline = new RoutedCommand("WebNodeTab.ToggleUnderline", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleUnderline, ToggleUnderline_Execute, FontCommands_CanExecute));
            buttonUnderline.Command = ToggleUnderline;

            ToggleStriketrough = new RoutedCommand("WebNodeTab.ToggleStriketrough", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleStriketrough, ToggleStriketrough_Execute, FontCommands_CanExecute));
            buttonStrikethrough.Command = ToggleStriketrough;

        }


        // COLOR EDITION

        private void SelectedBorderColorChanged(object sender, RoutedEventArgs e) {
            if(BorderColorSelector.SelectedColor.HasValue && MainWindow.Window.Selected != null) {
                MainWindow.Window.Selected.Node.BorderColor = CoreToWPFUtils.WPFColorToCore(BorderColorSelector.SelectedColor.Value);
                MainWindow.Window.Selected.Refresh();
            }
        }

        private void SelectedBackgroundColorChanged(object sender, RoutedEventArgs e) {
            if(BackgroundColorSelector.SelectedColor.HasValue && MainWindow.Window.Selected != null) {
                MainWindow.Window.Selected.Node.Color = CoreToWPFUtils.WPFColorToCore(BackgroundColorSelector.SelectedColor.Value);
                MainWindow.Window.Selected.Refresh();
            }
        }


        // FONT GROUP COMMANDS

        public void SetStateOfButtons() {

            if (FontComboBox.IsKeyboardFocusWithin || comboBoxFontSize.IsKeyboardFocusWithin) return;

            var selected = MainWindow.Window.Selected;
            bool notnull = selected != null;

            if(notnull && selected.GetSelectionFontSize() == -1) {
                comboBoxFontSize.IsEnabled = false;
                FontComboBox.IsEnabled = false;
                return;
            }

            comboBoxFontSize.IsEnabled = true;
            FontComboBox.IsEnabled = true;

            buttonBold.IsChecked = notnull && selected.IsSelectionBold();
            buttonItalic.IsChecked = notnull && selected.IsSelectionItalic();
            buttonUnderline.IsChecked = notnull && selected.IsSelectionUnderlined();
            buttonStrikethrough.IsChecked = notnull && selected.IsSelectionStrikedtrough();

            if (notnull) {
                comboBoxFontSize.SelectedValue = selected.GetSelectionFontSize();
                comboBoxFontSize.Text = selected.GetSelectionFontSize() + "";
                FontComboBox.SelectedValue = selected.GetSelectionFont();
                FontComboBox.Text = selected.GetSelectionFont();
            }

        }

        void FontCommands_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainWindow.Window.Selected != null && MainWindow.Window.Selected.GetSelectionFontSize() != -1;
            SetStateOfButtons();
        }

        void ToggleBold_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionBold();
        }

        void ToggleItalic_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionItalic();
        }

        void ToggleUnderline_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionUnderline();
        }

        void ToggleStriketrough_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionStriketrough();
        }


        #region Font Size Combo Boxes

        private void OnFontSizeTyped(object sender, TextCompositionEventArgs e) {
            try {
                double value = Double.Parse(e.Text);
                if (value <= 0) throw new Exception();
                MainWindow.Window.Selected.SetSelectionFontSize(value);
            }
            catch (Exception) {
                string previous = e.Text.Remove(e.Text.Length - 1);
                comboBoxFontSize.Text = previous;
                e.Handled = true;
            }
        }

        private void OnFontSizeChanged(object sender, SelectionChangedEventArgs e) {

            if(comboBoxFontSize.SelectedValue != null) {
                double value = Double.Parse(((TextBlock) comboBoxFontSize.SelectedValue).Text);
                if (MainWindow.Window.Selected != null) {
                    MainWindow.Window.Selected.SetSelectionFontSize(value);
                }
            }

        }

        private void OnFontSelectionChanged(object sender, SelectionChangedEventArgs e) {

            if(FontComboBox.SelectedValue != null) {
                string selected = ((TextBlock) FontComboBox.SelectedValue).Text;
                if (MainWindow.Window.Selected != null && selected != null) {
                    MainWindow.Window.Selected.SetSelectionFont(selected);
                }
            }

        }

         
        #endregion


    }

}
