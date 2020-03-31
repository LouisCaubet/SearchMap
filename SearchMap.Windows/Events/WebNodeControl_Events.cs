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
            TitleBox.TextChanged += OnTitleChanged;
            CommentBox.TextChanged += OnCommentChanged;

            RegisterEventsOnChild(TitleBox);
            RegisterEventsOnChild(CommentBox);
            RegisterEventsOnChild(UriLabel);
            RegisterEventsOnChild(Icon);

        }

        // Editing

        void OnTitleChanged(object sender, TextChangedEventArgs e) {
            Node.Title = TitleBox.Text;
        }

        void OnCommentChanged(object sender, TextChangedEventArgs e) {
            Node.Comment = CommentBox.Text;
        }
        
    }

}