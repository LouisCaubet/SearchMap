using SearchMapCore.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using SearchMapCore.Controls;

namespace SearchMapCore {

    public static class SearchMapCore {

        /// <summary>
        /// The instance of Graph currently visible in SearchMap
        /// </summary>
        public static Graph.Graph Graph { get; set; }

        /// <summary>
        /// Interface to link with the platform-dependent rendering code.
        /// </summary>
        public static IGraphRenderer Renderer { get; set; }

        /// <summary>
        /// Manages clipboard and undo/redo ops.
        /// </summary>
        public static Clipboard Clipboard { get; set; }

        /// <summary>
        /// Logging to console.
        /// </summary>
        public static ILogging Logger { get; set; }

        /// <summary>
        /// Core startup
        /// </summary>
        public static void InitCore(IGraphRenderer renderer) {
            Renderer = renderer;
            Clipboard = new Clipboard();
        }

        // TESTING ---
        public static Graph.Graph CreateTestGraph() {

            var graph = new Graph.Graph();

            var node1 = new WebNode(graph, (new UriBuilder("www.ourwebsite.net")).Uri, "") {
                Title = "Welcome to SearchMap",
                Comment = "Learn more - click to visit the website!",
                Color = new Color(255, 0, 0),
                BorderColor = new Color(255, 255, 255),
            };

            node1.MoveTo(new Location(0, 0));
            graph.RootNode = node1;

            var node2 = new WebNode(graph, new Uri("http://www.wikipedia.org"), "") {
                Title = "Wikipedia",
                Comment = "Want to know something? Click here to visit wikipedia",
                Color = new Color(255, 255, 255)
            };

            node2.SetParent(node1);
            node2.MoveTo(new Location(600, 100));

            var node3 = new WebNode(graph, new Uri("http://netflix.com"), "") {
                Title = "Netflix",
                Comment = "Need to relax ? Find awesome video content on Netflix.",
                Color = new Color(0, 0, 0),
                BorderColor = new Color(255, 0, 0)
            };

            node3.SetParent(node1);
            node3.MoveTo(new Location(-600, -600));

            var node4 = new WebNode(graph, new Uri("http://youtube.com"), "") {
                Title = "YouTube",
                Comment = "Awesome video content for free.",
                Color = new Color(255, 255, 255),
                BorderColor = new Color(255, 0, 0)
            };

            node4.SetParent(node3);
            node4.MoveTo(NodePlacement.PlaceNode(graph, node4));

            node2.AddSibling(node3);

            return graph;

        }

    }


}
