using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
using System;
using System.Windows;
using System.Windows.Controls;
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
        bool CommentBoxModified;
        bool IconModified;

        public NewWebNodeDialog() {

            // Only one of such windows open at a time
            if (Instance != null) this.Close();
            Instance = this;

            InitializeComponent();

            TitleBoxModified = false;
            UriBoxModified = false;
            CommentBoxModified = false;
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
            string comment = CommentBox.Text;
            ImageSource icon = NodeIcon.Source;

            if (!CommentBoxModified) {
                comment = "";
            }

            WebNode createdNode = new WebNode(MainWindow.Window.GetGraph(), uri, "") {
                Title = title,
                Comment = comment
            };


            // Determine parent.
            Node parent = null;
            try {
                parent = CoreToWPFUtils.GetNodeFromNodeControl(MainWindow.Window.Selected);
            }
            catch (ArgumentException) { }

            createdNode.SetParent(parent);

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

        private void CommentBox_GotFocus(object sender, RoutedEventArgs e) {

            if (!CommentBoxModified) {
                CommentBox.Text = "";
                CommentBox.FontStyle = FontStyles.Normal;
            }

        }

        private void CommentBox_LostFocus(object sender, RoutedEventArgs e) {

            if (!CommentBoxModified) {
                IgnoreNextTextChange = true;
                CommentBox.Text = "Insert comments here.";
                CommentBox.FontStyle = FontStyles.Italic;
            }

        }

        private void CommentBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (CommentBox.Text != "" && !IgnoreNextTextChange) CommentBoxModified = true;
            if (IgnoreNextTextChange) IgnoreNextTextChange = false;
        }

        #endregion NodeEditionEvents


    }

}
