using SearchMapCore.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using SearchMapCore.Undoing;
using Newtonsoft.Json;

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
        public static UndoRedo UndoRedoSystem { get; set; }

        /// <summary>
        /// Logging to console.
        /// </summary>
        public static ILogging Logger { get; set; }

        /// <summary>
        /// Core startup
        /// </summary>
        public static void InitCore(IGraphRenderer renderer) {
            Renderer = renderer;
            UndoRedoSystem = new UndoRedo();
        }

        // TESTING ---
        public static Graph.Graph CreateTestGraph() {

            var graph = new Graph.Graph();

            var node1 = new WebNode(graph, (new UriBuilder("www.ourwebsite.net")).Uri, "") {
                Title = "Welcome to SearchMap",
                Comment = Encoding.UTF8.GetBytes("Learn more - click to visit the website!"),
                Color = new Color(255, 0, 0),
                BorderColor = new Color(255, 255, 255),
                FrontTitleFont = TextFont.DefaultFrontTitleFont(),
                BackTitleFont = TextFont.DefaultBackTitleFont()
            };

            node1.MoveTo(new Location(0, 0));
            node1.FrontTitleFont.Color = TextFont.GetDefaultColorOnBackground(node1.Color);
            node1.BackTitleFont.Color = TextFont.GetDefaultColorOnBackground(node1.Color);
            graph.RootNode = node1;

            var node2 = new WebNode(graph, new Uri("http://www.wikipedia.org"), "") {
                Title = "Wikipedia",
                Comment = Encoding.UTF8.GetBytes("Want to know something? Click here to visit wikipedia"),
                Color = new Color(255, 255, 255),
                FrontTitleFont = TextFont.DefaultFrontTitleFont(),
                BackTitleFont = TextFont.DefaultBackTitleFont()
            };

            node2.SetParent(node1);
            node2.MoveTo(new Location(600, 100));
            node2.FrontTitleFont.Color = TextFont.GetDefaultColorOnBackground(node2.Color);
            node2.BackTitleFont.Color = TextFont.GetDefaultColorOnBackground(node2.Color);

            var node3 = new WebNode(graph, new Uri("http://netflix.com"), "") {
                Title = "Netflix",
                Comment = Encoding.UTF8.GetBytes("Need to relax ? Find awesome video content on Netflix."),
                Color = new Color(0, 0, 0),
                BorderColor = new Color(255, 0, 0),
                FrontTitleFont = TextFont.DefaultFrontTitleFont(),
                BackTitleFont = TextFont.DefaultBackTitleFont()
            };

            node3.SetParent(node1);
            node3.MoveTo(new Location(-600, -600));
            node3.FrontTitleFont.Color = TextFont.GetDefaultColorOnBackground(node3.Color);
            node3.BackTitleFont.Color = TextFont.GetDefaultColorOnBackground(node3.Color);

            var node4 = new WebNode(graph, new Uri("http://youtube.com"), "") {
                Title = "YouTube",
                Comment = Encoding.UTF8.GetBytes("Awesome video content for free."),
                Color = new Color(255, 255, 255),
                BorderColor = new Color(255, 0, 0),
                FrontTitleFont = TextFont.DefaultFrontTitleFont(),
                BackTitleFont = TextFont.DefaultBackTitleFont()
            };

            node4.SetParent(node3);
            node4.MoveTo(NodePlacement.PlaceNode(graph, node4));
            node4.FrontTitleFont.Color = TextFont.GetDefaultColorOnBackground(node4.Color);
            node4.BackTitleFont.Color = TextFont.GetDefaultColorOnBackground(node4.Color);

            node2.AddSibling(node3);

            UndoRedoSystem.ClearCtrlZStack();

            return graph;

        }

    }


}
