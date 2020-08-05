using SearchMapCore.Rendering;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using SearchMapCore.Undoing;
using Newtonsoft.Json;
using SearchMapCore.File;
using System.IO;

namespace SearchMapCore {

    public static class SearchMapCore {

        /// <summary>
        /// The instance of Graph currently visible in SearchMap
        /// </summary>
        public static Graph.Graph Graph { get; set; }

        /// <summary>
        /// The file storing the opened Graph.
        /// </summary>
        public static SearchMapFile File { get; private set; }

        /// <summary>
        /// Indicates whether the current project has already been saved by the user.
        /// </summary>
        public static bool IsCurrentProjectSaved { get; set; }

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

        /// <summary>
        /// Opens a SearchMap Project from the given path.
        /// </summary>
        /// <param name="path"></param>
        public static void OpenProject(string path) {

            File = new SearchMapFile(path);
            Graph = File.Graph;

        }

        /// <summary>
        /// Creates a new project at the given path from the given graph.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="graph"></param>
        public static void NewProject(string path, Graph.Graph graph) {

            Graph = graph;

            // Graph must have been rendered for everything to be initialized correctly.
            Graph.Render(Renderer);

            // Delete file if it already exists (override will have been prompted in UI)
            System.IO.File.Delete(path);

            File = new SearchMapFile(path, graph);

        }

        /// <summary>
        /// Creates a new empty project at the given path.
        /// </summary>
        /// <param name="path"></param>
        public static void NewProject(string path) {
            NewProject(path, new Graph.Graph());
        }

        /// <summary>
        /// Creates new project without saving it.
        /// </summary>
        public static void ShowNewGraph() {
            Graph = new Graph.Graph();
            IsCurrentProjectSaved = false;
            File = null;

            Renderer.DeleteAll();

            Graph.Render(Renderer);
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
