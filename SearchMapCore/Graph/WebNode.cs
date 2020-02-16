using Newtonsoft.Json;
using SearchMapCore.Serialization;
using System;

namespace SearchMapCore.Graph {

    public class WebNode : Node {

        public Uri Uri { get; set; }

        // TODO save highlights / notes

        public WebNode (Graph graph, Uri uri, string html) : base(graph) {

            Uri = uri;

            if(html == null){
                // Retrieve HTML from internet
            }

            // Save HTML to file in SMP archive

        }

        public WebNode(Graph graph, SerializableWebNode toDuplicate) : this(graph, toDuplicate.Uri, "") {

            AssociatedFile = toDuplicate.AssociatedFile;
            BorderColor = toDuplicate.BorderColor;
            Color = toDuplicate.Color;
            Comment = toDuplicate.Comment;
            Icon = toDuplicate.Icon;
            Title = toDuplicate.Title;

            MoveTo(toDuplicate.Location);
            Resize(toDuplicate.Width, toDuplicate.Height);

            graph.Refresh();

        }

        public string GetHtml() {
            // Retrieve Html from archive, return it
            return "Not Implemented Yet";
        }

        public override void OnClick() {
            // TODO open URI
            throw new NotImplementedException();
        }

    }

}
