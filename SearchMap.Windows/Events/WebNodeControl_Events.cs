using SearchMap.Windows.Events;
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

        NodeControl_Events NodeEvents;

        void RegisterEventHandlers() {

            NodeEvents = new NodeControl_Events(this, Node);

            // Editing
            TitleBox.TextChanged += OnTitleChanged;
            CommentBox.TextChanged += OnCommentChanged;

            NodeEvents.RegisterEventsOnChild(TitleBox);
            NodeEvents.RegisterEventsOnChild(CommentBox);
            NodeEvents.RegisterEventsOnChild(UriLabel);
            NodeEvents.RegisterEventsOnChild(Icon);

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