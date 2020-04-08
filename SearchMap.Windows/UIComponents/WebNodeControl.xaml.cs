using Newtonsoft.Json;
using SearchMap.Windows.Controls;
using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System;
using System.Windows;
using System.Windows.Controls;
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

            // TitleColumn.Width = new GridLength(Math.Min(TitleColumn.ActualWidth, 0.8 * Front.ActualWidth));

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
            CommentBox.Text = Node.Comment;
            CommentBox.Foreground = new SolidColorBrush(textColor);

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

        // EVENT HANDLING ------------------------------------------------------------------------------------------------------------------
        // See WebNodeControl_Events.cs

    }

}
