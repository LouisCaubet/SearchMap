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
        internal ICommand RemoveFormatting { get; private set; }
        internal ICommand IncreaseFontSize { get; private set; }
        internal ICommand DecreaseFontSize { get; private set; }

        internal ICommand ZoomOnNode { get; private set; }
        internal ICommand DeleteNode { get; private set; }

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

            FontColorSelector.SelectedColorChanged += OnFontColorSelected;
            HighlightColorSelector.SelectedColorChanged += OnHighlightColorSelected;

        }

        /// <summary>
        /// Registers commands binded with buttons from the Node Tab.
        /// </summary>
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

            RemoveFormatting = new RoutedCommand("WebNodeTab.RemoveFormatting", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(RemoveFormatting, RemoveFormatting_Execute, FontCommands_CanExecute));
            buttonClearFormatting.Command = RemoveFormatting;

            IncreaseFontSize = new RoutedCommand("WebNodeTab.IncreaseFontSize", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(IncreaseFontSize, IncreaseFontSize_Execute, FontCommands_CanExecute));
            buttonGrowFont.Command = IncreaseFontSize; 
            
            DecreaseFontSize = new RoutedCommand("WebNodeTab.DecreaseFontSize", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(DecreaseFontSize, DecreaseFontSize_Execute, FontCommands_CanExecute));
            buttonShrinkFont.Command = DecreaseFontSize;

            DeleteNode = new RoutedCommand("WebNodeTab.DeleteNode", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(DeleteNode, Delete_Execute, Actions_CanExecute));
            DeleteNodeButton.Command = DeleteNode;

            ZoomOnNode = new RoutedCommand("WebNodeTab.ZoomOnNode", GetType());
            MainWindow.Window.CommandBindings.Add(new CommandBinding(ZoomOnNode, ZoomOnNode_Execute, Actions_CanExecute));
            ZoomOnNodeButton.Command = ZoomOnNode;

        }

        internal void OnKeyPress(object sender, KeyEventArgs e) {
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {

                if(e.Key == Key.B && buttonBold.IsEnabled) {
                    ToggleBold_Execute(null, null);
                }
                else if(e.Key == Key.I && buttonItalic.IsEnabled) {
                    ToggleItalic_Execute(null, null);
                }
                else if(e.Key == Key.U && buttonUnderline.IsEnabled) {
                    ToggleUnderline_Execute(null, null);
                }

            }
        }

        #region Color Edition

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

        #endregion

        #region Font Group Commands

        public void SetStateOfButtons() {

            if (FontComboBox.IsKeyboardFocusWithin || comboBoxFontSize.IsKeyboardFocusWithin) return;

            var selected = MainWindow.Window.Selected;
            bool notnull = selected != null;

            if(notnull && selected.GetSelectionFontSize() == -1) {
                comboBoxFontSize.IsEnabled = false;
                FontComboBox.IsEnabled = false;
                buttonTextHighlightColor.IsEnabled = false;
                buttonFontColor.IsEnabled = false;
                return;
            }

            comboBoxFontSize.IsEnabled = true;
            FontComboBox.IsEnabled = true;
            buttonTextHighlightColor.IsEnabled = true;
            buttonFontColor.IsEnabled = true;

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
            e.CanExecute = MainWindow.Window.Selected != null 
                && (MainWindow.Window.Selected.GetSelectionFontSize() != -1 || IsMouseOver);
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

        void RemoveFormatting_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.RemoveFormattingOnSelection();
        }

        void IncreaseFontSize_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.SetSelectionFontSize(MainWindow.Window.Selected.GetSelectionFontSize() + 2);
        }

        void DecreaseFontSize_Execute(object sender, ExecutedRoutedEventArgs e) {
            double current = MainWindow.Window.Selected.GetSelectionFontSize();
            if (current > 2) MainWindow.Window.Selected.SetSelectionFontSize(current - 2);
        }

        #endregion

        #region Font Color Edition

        private void OnHighlightColorSelected(object sender, RoutedEventArgs e) {

            if (HighlightColorSelector.SelectedColor.HasValue) {
                Color color = HighlightColorSelector.SelectedColor.Value;
                MainWindow.Window.Selected.SetSelectionHighlight(color);
            }

        }

        private void OnFontColorSelected(object sender, RoutedEventArgs e) {

            if (FontColorSelector.SelectedColor.HasValue) {
                Color color = FontColorSelector.SelectedColor.Value;
                MainWindow.Window.Selected.SetSelectionColor(color);
            }

        }

        #endregion

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

        #region Action Commands

        private const double ZOOM_ON_NODE_VALUE = 1.9;

        void Actions_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainWindow.Window.Selected != null;
        }

        void ZoomOnNode_Execute(object sender, ExecutedRoutedEventArgs e) {

            var node = MainWindow.Window.Selected.Node;
            Point toCenter = MainWindow.Window.ConvertFromLocation(node.Location);

            MainWindow.Window.ZoomSlider.Value = ZOOM_ON_NODE_VALUE;

            MainWindow.Window.ScrollView.ScrollToVerticalOffset(toCenter.Y * ZOOM_ON_NODE_VALUE * MainWindow.DEFAULT_ZOOM - MainWindow.Window.ScrollView.ActualHeight / 2);
            MainWindow.Window.ScrollView.ScrollToHorizontalOffset(toCenter.X * ZOOM_ON_NODE_VALUE * MainWindow.DEFAULT_ZOOM - MainWindow.Window.ScrollView.ActualWidth / 2);

        }

        void Delete_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.GetGraph().DeleteNode(MainWindow.Window.Selected.Node.Id);
        }

        #endregion

    }

}
