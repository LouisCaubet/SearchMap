using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using SearchMapCore.Rendering;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SearchMap.Windows.Dialog {

    /// <summary>
    /// Logique d'interaction pour NewWebNodeDialog.xaml
    /// </summary>
    public partial class NewWebNodeDialog : Window {

        private static Point? DefaultLocation = null;
        internal static NewWebNodeDialog Instance;

        bool TitleBoxModified;
        bool UriBoxModified;
        bool IconModified;

        public NewWebNodeDialog() {

            // Only one of such windows open at a time
            if (Instance != null) this.Close();
            Instance = this;

            InitializeComponent();

            TitleBoxModified = false;
            UriBoxModified = false;
            IconModified = false;

            MouseLeftButtonDown += OnMouseDown;

            this.Topmost = true;

            if (DefaultLocation.HasValue) {
                this.Left = DefaultLocation.Value.X;
                this.Top = DefaultLocation.Value.Y;
            }

        }

        void CloseWindow() {
            DefaultLocation = new Point(this.Left, this.Top);
            Instance = null;
            this.Close();
        }

        void OnMouseDown(object sender, MouseButtonEventArgs e) {

            if (!Content1.IsMouseOver && !Content2.IsMouseOver) {
                this.DragMove();
            }

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) {
            CloseWindow();
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e) {

            // create new webnode with given values.
            
            if (!UriBoxModified) {
                // Open error : uri required.
                MessageBox.Show("A Web Node requires a valid URL!", "URL required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Uri uri;
            try {
                uri = new UriBuilder(UriBox.Text).Uri;
            }
            catch (Exception) {
                MessageBox.Show("A Web Node requires a valid URL!", "URL required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
             
            string title = TitleBox.Text;

            // Comment
            TextRange range = new TextRange(CommentBox.Document.ContentStart, CommentBox.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();

            range.Save(stream, DataFormats.Rtf);

            byte[] comment = stream.ToArray();

            ImageSource icon = NodeIcon.Source;

            WebNode createdNode = new WebNode(MainWindow.Window.GetGraph(), uri, "") {
                Title = title,
                Comment = comment
            };

            createdNode.FrontTitleFont.Color = TextFont.GetDefaultColorOnBackground(createdNode.Color);
            createdNode.BackTitleFont.Color = TextFont.GetDefaultColorOnBackground(createdNode.Color);

            // Determine parent.
            Node parent = null;

            if(MainWindow.Window.Selected != null) {
                parent = MainWindow.Window.Selected.Node;
            }

            createdNode.SetParent(parent, false);

            // Place Node
            if (parent != null) {
                Location loc1 = NodePlacement.PlaceNode(MainWindow.Window.GetGraph(), createdNode);
                createdNode.MoveTo(loc1);
                // TODO move scrollview to center on the new node.
            }
            else {

                double x = MainWindow.Window.ScrollView.HorizontalOffset + MainWindow.Window.ScrollView.ActualWidth / 2;
                double y = MainWindow.Window.ScrollView.VerticalOffset + MainWindow.Window.ScrollView.ActualHeight / 2;

                Point centerPt = new Point(x, y);
                Location loc = MainWindow.Window.ConvertToLocation(centerPt);
                createdNode.MoveTo(loc);

            }

            CloseWindow();

        }




        // TEXT EDITION EVENT HANDLING
        #region NodeEditionEvents

        bool IgnoreNextTextChange = false;

        private void TitleBox_GotFocus(object sender, RoutedEventArgs e) {

            if (!TitleBoxModified) {
                TitleBox.Text = "";
                TitleBox.FontStyle = FontStyles.Normal;
            }

        }

        private void TitleBox_LostFocus(object sender, RoutedEventArgs e) {

            if (!TitleBoxModified) {
                IgnoreNextTextChange = true;
                TitleBox.Text = "New Web Node";
                TitleBox.FontStyle = FontStyles.Italic;
            }

        }

        private void TitleBox_TextChanged(object sender, TextChangedEventArgs e) {
            if(TitleBox.Text != "" && !IgnoreNextTextChange) TitleBoxModified = true;
            if (IgnoreNextTextChange) IgnoreNextTextChange = false;
        }

        private void UriBox_GotFocus(object sender, RoutedEventArgs e) {

            if (!UriBoxModified) {
                UriBox.Text = "";
                UriBox.FontStyle = FontStyles.Normal;
            }

        }

        private void UriBox_LostFocus(object sender, RoutedEventArgs e) {

            if (!UriBoxModified) {
                IgnoreNextTextChange = true;
                UriBox.Text = "www.example.com";
                UriBox.FontStyle = FontStyles.Italic;
            }

        }

        private void UriBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (UriBox.Text != "" && !IgnoreNextTextChange) UriBoxModified = true;
            if (IgnoreNextTextChange) IgnoreNextTextChange = false;
        }

        #endregion NodeEditionEvents


    }

}
