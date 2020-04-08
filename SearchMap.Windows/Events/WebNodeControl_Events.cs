using SearchMap.Windows.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchMap.Windows.UIComponents {

    public partial class WebNodeControl {

        // EVENT HANDLING

        void RegisterEventHandlers() {

            RegisterBaseEvents();

            // Editing
            FrontTitleBox.TextChanged += OnFrontTitleChanged;
            BackTitleBox.TextChanged += OnBackTitleChanged;
            CommentBox.TextChanged += OnCommentChanged;
            Loaded += OnControlLoaded;

            MouseDoubleClick += OnMouseDoubleClick;

            RegisterEventsOnChild(FrontTitleBox);
            RegisterEventsOnChild(BackTitleBox);
            RegisterEventsOnChild(CommentBox);
            RegisterEventsOnChild(Icon);

        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(!FrontTitleBox.IsMouseDirectlyOver && !BackTitleBox.IsMouseDirectlyOver) Flip();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e) {
            Refresh();
        }

        // Editing

        void OnFrontTitleChanged(object sender, TextChangedEventArgs e) {

            Node.Title = FrontTitleBox.Text;
            BackTitleBox.Text = Node.Title;

            if (FrontTitleBox.Text == "") {
                FrontTitleBox.Text = "Untitled";
                BackTitleBox.Text = "Untitled";
                IsUntitled = true;
                FrontTitleBox.FontStyle = FontStyles.Italic;
                BackTitleBox.FontStyle = FontStyles.Italic;
            }
            else {

                // this will throw a new TitleChanged event to update the node title.
                if (IsUntitled) FrontTitleBox.Text = FrontTitleBox.Text.Replace("Untitled", "");
                IsUntitled = false;

                FrontTitleBox.FontStyle = FontStyles.Normal;
                BackTitleBox.FontStyle = FontStyles.Normal;

            }
            
        }

        void OnBackTitleChanged(object sender, TextChangedEventArgs e) {
            Node.Title = BackTitleBox.Text;
            FrontTitleBox.Text = Node.Title;
        }

        void OnCommentChanged(object sender, TextChangedEventArgs e) {
            Node.Comment = CommentBox.Text;
        }
        
    }

}