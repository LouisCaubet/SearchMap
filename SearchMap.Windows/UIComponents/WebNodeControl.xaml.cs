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

            // Front
            FrontTitleBox.Text = Node.Title;
            ApplyTextFontToTextBox(FrontTitleBox, GetWebNode().FrontTitleFont);

            // Back
            BackTitleBox.Text = Node.Title;
            ApplyTextFontToTextBox(BackTitleBox, GetWebNode().BackTitleFont);

            // Comment
            TextRange range = new TextRange(CommentBox.Document.ContentStart, CommentBox.Document.ContentEnd);
            range.Load(new MemoryStream(Node.Comment), DataFormats.Rtf);

            // TODO move somewhere else (dont want to remove color edits by user).
            // Move to place where text is added in code (when typed in browser, ...)
            range.ApplyPropertyValue(Inline.ForegroundProperty, 
                new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color))));

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

            // Export
            ExportTitleBox.Text = Node.Title;
            // ExportCommentBox.Text = Node.Comment;
            ExportUriLabel.Text = GetWebNode().Uri.OriginalString;

            // Color
            ExportTitleBox.Foreground = BackTitleBox.Foreground;
            ExportCommentBox.Foreground = BackTitleBox.Foreground;
            ExportUriLabel.Foreground = BackTitleBox.Foreground;

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
            Color textColor = CoreToWPFUtils.CoreColorToWPF(SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color));

            if (FrontTitleBox.Equals(LastObjectWithKeyboardFocus)) {

                GetWebNode().FrontTitleFont = SearchMapCore.Rendering.TextFont.DefaultFrontTitleFont();
                GetWebNode().FrontTitleFont.Color = SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color);

                ApplyTextFontToTextBox(FrontTitleBox, GetWebNode().FrontTitleFont);

            }
            else if (BackTitleBox.Equals(LastObjectWithKeyboardFocus)) {

                GetWebNode().BackTitleFont = SearchMapCore.Rendering.TextFont.DefaultBackTitleFont();
                GetWebNode().BackTitleFont.Color = SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color);

                ApplyTextFontToTextBox(BackTitleBox, GetWebNode().BackTitleFont);

            }
            else if (CommentBox.Equals(LastObjectWithKeyboardFocus)) {
                CommentBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, font);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 16D);
                CommentBox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(textColor));
                CommentBox.Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)));
                CommentBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
                CommentBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }

        }

        public override void Save() {

            Node.TakeSnapshot();

            var node = GetWebNode();
            node.FrontTitleFont = GetTextFontFromTextBox(FrontTitleBox);
            node.BackTitleFont = GetTextFontFromTextBox(BackTitleBox);

            // Save RichTextBox

            // Extract rich text from CommentBox
            TextRange range = new TextRange(CommentBox.Document.ContentStart, CommentBox.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();

            range.Save(stream, DataFormats.Rtf);

            Node.Comment = stream.ToArray();

        }

        #endregion

        // EVENT HANDLING ------------------------------------------------------------------------------------------------------------------
        // See WebNodeControl_Events.cs

    }

}
