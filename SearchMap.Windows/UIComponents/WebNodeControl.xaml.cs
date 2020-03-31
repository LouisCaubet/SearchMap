using Newtonsoft.Json;
using SearchMap.Windows.Controls;
using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using SearchMapCore.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour NodeControl.xaml
    /// </summary>
    public partial class WebNodeControl : NodeControl {

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

        public override void Refresh() {

            Border.Background = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.Color));
            Border.BorderBrush = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.BorderColor));

            TitleBox.Text = Node.Title;
            CommentBox.Text = Node.Comment;
            UriLabel.Text = GetWebNode().Uri.OriginalString;

            Color textColor = GetBrightness(CoreToWPFUtils.CoreColorToWPF(Node.Color)) < 0.55 ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);
            TitleBox.Foreground = new SolidColorBrush(textColor);
            CommentBox.Foreground = new SolidColorBrush(textColor);
            UriLabel.Foreground = new SolidColorBrush(textColor);

            Height = Node.Height;
            Width = Node.Width;

        }

        // EVENT HANDLING ------------------------------------------------------------------------------------------------------------------
        // See WebNodeControl_Events.cs

    }

}
