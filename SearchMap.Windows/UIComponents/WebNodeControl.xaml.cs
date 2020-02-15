using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour NodeControl.xaml
    /// </summary>
    public partial class WebNodeControl : UserControl {

        /// <summary>
        /// The node rendered by this control.
        /// </summary>
        public WebNode Node { get; }

        // For drag and drop
        Point? lastDragPoint;

        public WebNodeControl(WebNode node) {
            InitializeComponent();

            Node = node;

            // Put this in front of connectors.
            Panel.SetZIndex(this, 10);

            // Event handlers
            RegisterEventHandlers();

            Refresh();

        }

        /// <summary>
        /// Updates the rendered values and colors. for the node.
        /// </summary>
        public void Refresh() {

            Border.Background = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.Color));
            Border.BorderBrush = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.BorderColor));

            TitleBox.Text = Node.Title;
            CommentBox.Text = Node.Comment;
            UriLabel.Text = Node.Uri.OriginalString;

            Color textColor = GetBrightness(CoreToWPFUtils.CoreColorToWPF(Node.Color)) < 0.55 ? Color.FromRgb(255, 255, 255) : Color.FromRgb(0, 0, 0);
            TitleBox.Foreground = new SolidColorBrush(textColor);
            CommentBox.Foreground = new SolidColorBrush(textColor);
            UriLabel.Foreground = new SolidColorBrush(textColor);

        }

        /// <summary>
        /// Source : https://stackoverflow.com/questions/50540301/c-sharp-get-good-color-for-label
        /// </summary>
        float GetBrightness(Color c) { 
            return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; 
        }

        // EVENT HANDLING ------------------------------------------------------------------------------------------------------------------
        // See WebNodeControl_Events.cs



    }

}
