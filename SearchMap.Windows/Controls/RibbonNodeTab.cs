using Fluent;
using SearchMap.Windows.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SearchMap.Windows.Controls {

    public abstract class RibbonNodeTab : RibbonTabItem {

        private ICollection<FontFamily> AvailableFonts { get; set; }

        private bool CommandsRegistered = false;

        internal static ICommand ToggleBold { get; private set; }
        internal static ICommand ToggleItalic { get; private set; }
        internal static ICommand ToggleUnderline { get; private set; }
        internal static ICommand ToggleStriketrough { get; private set; }
        internal static ICommand RemoveFormatting { get; private set; }
        internal static ICommand IncreaseFontSize { get; private set; }
        internal static ICommand DecreaseFontSize { get; private set; }

        internal static ICommand ZoomOnNode { get; private set; }
        internal static ICommand DeleteNode { get; private set; }


        // Access fields of subclass

        protected abstract Fluent.ComboBox GetFontComboBox();
        protected abstract Fluent.ComboBox GetFontSizeComboBox();
        protected abstract Fluent.DropDownButton GetTextHighlightButton();
        protected abstract Fluent.DropDownButton GetFontColorButton();

        protected abstract Fluent.ToggleButton GetBoldButton();
        protected abstract Fluent.ToggleButton GetItalicButton();
        protected abstract Fluent.ToggleButton GetUnderlineButton();
        protected abstract Fluent.ToggleButton GetStrikethroughButton();

        protected abstract Fluent.Button GetClearFormattingButton();
        protected abstract Fluent.Button GetGrowFontButton();
        protected abstract Fluent.Button GetShrinkFontButton();

        protected abstract Fluent.Button GetDeleteNodeButton();
        protected abstract Fluent.Button GetZoomOnNodeButton();

        protected abstract Fluent.ColorGallery GetHighlightColorSelector();
        protected abstract Fluent.ColorGallery GetFontColorSelector();

        protected abstract Fluent.ColorGallery GetBorderColorSelector();
        protected abstract Fluent.ColorGallery GetBackgroundColorSelector();


        public void InitTabBase() {

            AvailableFonts = Fonts.SystemFontFamilies;

            foreach (FontFamily font in AvailableFonts) {
                GetFontComboBox().Items.Add(new TextBlock() {
                    Text = font.Source,
                    Tag = "All Fonts"
                });
            }

            GetFontComboBox().SelectionChanged += OnFontSelectionChanged;
            GetFontSizeComboBox().SelectionChanged += OnFontSizeChanged;
            GetFontSizeComboBox().TextInput += OnFontSizeTyped;

            GetFontColorSelector().SelectedColorChanged += OnFontColorSelected;
            GetHighlightColorSelector().SelectedColorChanged += OnHighlightColorSelected;

            // Events
            GetBackgroundColorSelector().SelectedColorChanged += SelectedBackgroundColorChanged;
            GetBorderColorSelector().SelectedColorChanged += SelectedBorderColorChanged;

        }


        /// <summary>
        /// Registers commands binded with buttons from the Node Tab.
        /// </summary>
        internal void RegisterCommands() {

            if (!CommandsRegistered) {

                ToggleBold = new RoutedCommand("WebNodeTab.ToggleBold", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleBold, ToggleBold_Execute, FontCommands_CanExecute));

                ToggleItalic = new RoutedCommand("WebNodeTab.ToggleItalic", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleItalic, ToggleItalic_Execute, FontCommands_CanExecute));

                ToggleUnderline = new RoutedCommand("WebNodeTab.ToggleUnderline", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleUnderline, ToggleUnderline_Execute, FontCommands_CanExecute));

                ToggleStriketrough = new RoutedCommand("WebNodeTab.ToggleStriketrough", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(ToggleStriketrough, ToggleStriketrough_Execute, FontCommands_CanExecute));

                RemoveFormatting = new RoutedCommand("WebNodeTab.RemoveFormatting", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(RemoveFormatting, RemoveFormatting_Execute, FontCommands_CanExecute));

                IncreaseFontSize = new RoutedCommand("WebNodeTab.IncreaseFontSize", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(IncreaseFontSize, IncreaseFontSize_Execute, FontCommands_CanExecute));

                DecreaseFontSize = new RoutedCommand("WebNodeTab.DecreaseFontSize", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(DecreaseFontSize, DecreaseFontSize_Execute, FontCommands_CanExecute));

                DeleteNode = new RoutedCommand("WebNodeTab.DeleteNode", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(DeleteNode, Delete_Execute, Actions_CanExecute));

                ZoomOnNode = new RoutedCommand("WebNodeTab.ZoomOnNode", GetType());
                MainWindow.Window.CommandBindings.Add(new CommandBinding(ZoomOnNode, ZoomOnNode_Execute, Actions_CanExecute));

                CommandsRegistered = true;

            }

            
            GetBoldButton().Command = ToggleBold;
            GetItalicButton().Command = ToggleItalic;
            GetUnderlineButton().Command = ToggleUnderline;
            GetStrikethroughButton().Command = ToggleStriketrough;
            GetClearFormattingButton().Command = RemoveFormatting;
            GetGrowFontButton().Command = IncreaseFontSize;
            GetShrinkFontButton().Command = DecreaseFontSize;
            GetDeleteNodeButton().Command = DeleteNode;
            GetZoomOnNodeButton().Command = ZoomOnNode;

        }

        internal void OnKeyPress(object sender, KeyEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {

                if (e.Key == Key.B && GetBoldButton().IsEnabled) {
                    ToggleBold_Execute(null, null);
                }
                else if (e.Key == Key.I && GetItalicButton().IsEnabled) {
                    ToggleItalic_Execute(null, null);
                }
                else if (e.Key == Key.U && GetUnderlineButton().IsEnabled) {
                    ToggleUnderline_Execute(null, null);
                }

            }
        }

        #region Color Edition

        private void SelectedBorderColorChanged(object sender, RoutedEventArgs e) {
            if (GetBorderColorSelector().SelectedColor.HasValue && MainWindow.Window.Selected != null) {

                MainWindow.Window.Selected.Node.TakeSnapshot();

                MainWindow.Window.Selected.Node.BorderColor = CoreToWPFUtils.WPFColorToCore(GetBorderColorSelector().SelectedColor.Value);
                MainWindow.Window.Selected.Refresh();
            }
        }

        private void SelectedBackgroundColorChanged(object sender, RoutedEventArgs e) {
            if (GetBackgroundColorSelector().SelectedColor.HasValue && MainWindow.Window.Selected != null) {

                MainWindow.Window.Selected.Node.TakeSnapshot();

                MainWindow.Window.Selected.Node.Color = CoreToWPFUtils.WPFColorToCore(GetBackgroundColorSelector().SelectedColor.Value);
                MainWindow.Window.Selected.Refresh();
            }
        }

        #endregion

        #region Font Group Commands

        public void SetStateOfButtons() {

            if (GetFontComboBox().IsKeyboardFocusWithin || GetFontSizeComboBox().IsKeyboardFocusWithin) return;

            var selected = MainWindow.Window.Selected;
            bool notnull = selected != null;

            if (notnull && selected.GetSelectionFontSize() == -1) {
                GetFontSizeComboBox().IsEnabled = false;
                GetFontComboBox().IsEnabled = false;
                GetTextHighlightButton().IsEnabled = false;
                GetFontColorButton().IsEnabled = false;
                return;
            }

            GetFontSizeComboBox().IsEnabled = true;
            GetFontComboBox().IsEnabled = true;
            GetTextHighlightButton().IsEnabled = true;
            GetFontColorButton().IsEnabled = true;

            GetBoldButton().IsChecked = notnull && selected.IsSelectionBold();
            GetItalicButton().IsChecked = notnull && selected.IsSelectionItalic();
            GetUnderlineButton().IsChecked = notnull && selected.IsSelectionUnderlined();
            GetStrikethroughButton().IsChecked = notnull && selected.IsSelectionStrikedtrough();

            if (notnull) {
                GetFontSizeComboBox().SelectedValue = selected.GetSelectionFontSize();
                GetFontSizeComboBox().Text = selected.GetSelectionFontSize() + "";
                GetFontComboBox().SelectedValue = selected.GetSelectionFont();
                GetFontComboBox().Text = selected.GetSelectionFont();
            }

        }

        void FontCommands_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainWindow.Window.Selected != null
                && (MainWindow.Window.Selected.GetSelectionFontSize() != -1 || IsMouseOver);
            SetStateOfButtons();
        }

        static void ToggleBold_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionBold();
        }

        static void ToggleItalic_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionItalic();
        }

        static void ToggleUnderline_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionUnderline();
        }

        static void ToggleStriketrough_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.ToggleSelectionStriketrough();
        }

        static void RemoveFormatting_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.RemoveFormattingOnSelection();
        }

        static void IncreaseFontSize_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.Selected.SetSelectionFontSize(MainWindow.Window.Selected.GetSelectionFontSize() + 2);
        }

        static void DecreaseFontSize_Execute(object sender, ExecutedRoutedEventArgs e) {
            double current = MainWindow.Window.Selected.GetSelectionFontSize();
            if (current > 2) MainWindow.Window.Selected.SetSelectionFontSize(current - 2);
        }

        #endregion

        #region Font Color Edition

        private void OnHighlightColorSelected(object sender, RoutedEventArgs e) {

            if (GetHighlightColorSelector().SelectedColor.HasValue) {
                Color color = GetHighlightColorSelector().SelectedColor.Value;
                MainWindow.Window.Selected.SetSelectionHighlight(color);
            }

        }

        private void OnFontColorSelected(object sender, RoutedEventArgs e) {

            if (GetFontColorSelector().SelectedColor.HasValue) {
                Color color = GetFontColorSelector().SelectedColor.Value;
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
                GetFontSizeComboBox().Text = previous;
                e.Handled = true;
            }
        }

        private void OnFontSizeChanged(object sender, SelectionChangedEventArgs e) {

            if (GetFontSizeComboBox().SelectedValue != null) {
                double value = Double.Parse(((TextBlock) GetFontSizeComboBox().SelectedValue).Text);
                if (MainWindow.Window.Selected != null) {
                    MainWindow.Window.Selected.SetSelectionFontSize(value);
                }
            }

        }

        private void OnFontSelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (GetFontComboBox().SelectedValue != null) {
                string selected = ((TextBlock) GetFontComboBox().SelectedValue).Text;
                if (MainWindow.Window.Selected != null && selected != null) {
                    MainWindow.Window.Selected.SetSelectionFont(selected);
                }
            }

        }

        #endregion

        #region Action Commands

        private const double ZOOM_ON_NODE_VALUE = 1.9;

        static void Actions_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MainWindow.Window.Selected != null;
        }

        static void ZoomOnNode_Execute(object sender, ExecutedRoutedEventArgs e) {

            var node = MainWindow.Window.Selected.Node;
            Point toCenter = MainWindow.Window.ConvertFromLocation(node.Location);

            MainWindow.Window.ZoomSlider.Value = ZOOM_ON_NODE_VALUE;

            MainWindow.Window.ScrollView.ScrollToVerticalOffset(toCenter.Y * ZOOM_ON_NODE_VALUE * MainWindow.DEFAULT_ZOOM - MainWindow.Window.ScrollView.ActualHeight / 2);
            MainWindow.Window.ScrollView.ScrollToHorizontalOffset(toCenter.X * ZOOM_ON_NODE_VALUE * MainWindow.DEFAULT_ZOOM - MainWindow.Window.ScrollView.ActualWidth / 2);

        }

        static void Delete_Execute(object sender, ExecutedRoutedEventArgs e) {
            MainWindow.Window.GetGraph().DeleteNode(MainWindow.Window.Selected.Node.Id);
        }

        #endregion

    }

}
