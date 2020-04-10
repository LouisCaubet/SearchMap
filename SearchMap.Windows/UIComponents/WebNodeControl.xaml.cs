using Newtonsoft.Json;
using SearchMap.Windows.Controls;
using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour NodeControl.xaml
    /// </summary>
    public partial class WebNodeControl : NodeControl {

        private bool IsUntitled { get; set; }

        public WebNodeControl(WebNode node) : base(node) {
            InitializeComponent();

            // Put this in front of connectors.
            Panel.SetZIndex(this, 10);

            // Event handlers
            RegisterEventHandlers();

            Refresh();

        }

        public WebNode GetWebNode() {
            return (WebNode) Node;
        }

        public override FrameworkElement GetFront() {
            return Front;
        }

        public override FrameworkElement GetBack() {
            return Back;
        }

        public override void Refresh() {

            Border.Background = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.Color));
            Border.BorderBrush = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.BorderColor));

            // Determine good color depending on the background
            Color textColor = GetBrightness(CoreToWPFUtils.CoreColorToWPF(Node.Color)) < 0.55 ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);

            // Front
            FrontTitleBox.Text = Node.Title;
            FrontTitleBox.Foreground = new SolidColorBrush(textColor);

            // Back
            BackTitleBox.Text = Node.Title;
            BackTitleBox.Foreground = new SolidColorBrush(textColor);

            // maybe we should store comment as byteArray ?
            byte[] byteArray = Encoding.UTF8.GetBytes(Node.Comment);

            TextRange range = new TextRange(CommentBox.Document.ContentStart, CommentBox.Document.ContentEnd);
            range.Load(new MemoryStream(byteArray), DataFormats.Rtf);

            // TODO move somewhere else (dont want to remove color edits by user).
            range.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(textColor));

            // Tooltip
            UriToolTip.Text = GetWebNode().Uri.AbsoluteUri;

            // Prevent empty text boxes.
            if (FrontTitleBox.Text == "") {
                FrontTitleBox.Text = "Untitled";
                BackTitleBox.Text = "Untitled";
                IsUntitled = true;
                FrontTitleBox.FontStyle = FontStyles.Italic;
                BackTitleBox.FontStyle = FontStyles.Italic;
            }
            else {
                IsUntitled = false;
                FrontTitleBox.FontStyle = FontStyles.Normal;
                BackTitleBox.FontStyle = FontStyles.Normal;
            }

            Height = Node.Height;
            Width = Node.Width;

        }

        public void PrepareForExport() {

            // Determine good color depending on the background
            Color textColor = GetBrightness(CoreToWPFUtils.CoreColorToWPF(Node.Color)) < 0.55 ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);

            // Export
            ExportTitleBox.Text = Node.Title;
            ExportCommentBox.Text = Node.Comment;
            ExportUriLabel.Text = GetWebNode().Uri.OriginalString;

            // Color
            ExportTitleBox.Foreground = new SolidColorBrush(textColor);
            ExportCommentBox.Foreground = new SolidColorBrush(textColor);
            ExportUriLabel.Foreground = new SolidColorBrush(textColor);

        }

        public override void ShowAssociatedRibbonTab(bool setSelected) {
            MainWindow.Window.NodeContextualGroup.Visibility = Visibility.Visible;
            MainWindow.Window.RibbonTabWebNode.Visibility = Visibility.Visible;
            if (setSelected) MainWindow.Window.Ribbon.SelectedTabIndex = RibbonWebNodeTab.TAB_INDEX;
        }

        public override void CollapseAssociatedRibbonTab() {
            MainWindow.Window.Ribbon.SelectedTabIndex = MainWindow.Window.RibbonTabIndex;
            MainWindow.Window.RibbonTabWebNode.Visibility = Visibility.Collapsed;
        }


        #region Text Edition

        protected override void SetObjectWithLastKeyboardFocus() {
            if (FrontTitleBox.IsKeyboardFocusWithin) {
                LastObjectWithKeyboardFocus = FrontTitleBox;
                LastNotNullObjectWithKeyboardFocus = FrontTitleBox;
            }
            else if (BackTitleBox.IsKeyboardFocusWithin) {
                LastObjectWithKeyboardFocus = BackTitleBox;
                LastNotNullObjectWithKeyboardFocus = BackTitleBox;
            }
            else if (CommentBox.IsKeyboardFocusWithin) {
                LastObjectWithKeyboardFocus = CommentBox;
                LastNotNullObjectWithKeyboardFocus = CommentBox;
            }
            else {
                LastObjectWithKeyboardFocus = null;
            }
        }

        public override void RemoveFormattingOnSelection() {

            FontFamily font = new FontFamily("Segoe UI");
            Color textColor = GetBrightness(CoreToWPFUtils.CoreColorToWPF(Node.Color)) < 0.55 ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);

            if (FrontTitleBox.Equals(LastObjectWithKeyboardFocus)) {
                FrontTitleBox.FontFamily = font;
                FrontTitleBox.FontSize = 40;
                FrontTitleBox.Foreground = new SolidColorBrush(textColor);
                FrontTitleBox.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                FrontTitleBox.TextDecorations = null;
                FrontTitleBox.FontWeight = FontWeights.Black;
                FrontTitleBox.FontStyle = FontStyles.Normal;
            }
            else if (BackTitleBox.Equals(LastObjectWithKeyboardFocus)) {
                BackTitleBox.FontFamily = font;
                BackTitleBox.FontSize = 30;
                BackTitleBox.Foreground = new SolidColorBrush(textColor);
                BackTitleBox.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                BackTitleBox.TextDecorations = null;
                BackTitleBox.FontWeight = FontWeights.Black;
                BackTitleBox.FontStyle = FontStyles.Normal;
            }
            else if (CommentBox.Equals(LastObjectWithKeyboardFocus)) {
                CommentBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, font);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 16);
                CommentBox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(textColor));
                CommentBox.Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)));
                CommentBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }

        }

        #endregion

        // EVENT HANDLING ------------------------------------------------------------------------------------------------------------------
        // See WebNodeControl_Events.cs

    }

}
