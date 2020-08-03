using Newtonsoft.Json;
using SearchMapCore.Undoing;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Represents a SearchMap Graph.
    /// </summary>
    public class Graph {

        // Drawing area size ------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The Height of the drawing zone of the graph.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The Width of the drawing zone of the graph.
        /// </summary>
        public int Width { get; private set; }

        // Graph definition --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The name of the project this graph represents.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Nodes of the graph, accessed by id.
        /// </summary>
        public Dictionary<int, Node> Nodes { get; private set; }

        /// <summary>
        /// The root node of the graph.
        /// </summary>
        public Node RootNode { get; set; }

        /// <summary>
        /// Checks if the graph is currently on user screen.
        /// The renderer may be null if IsDisplayed is false.
        /// </summary>
        public bool IsDisplayed { get; set; }

        /// <summary>
        /// Describes the UI rendering of the graph.
        /// </summary>
        [JsonIgnore]
        public IGraphRenderer Renderer { get; set; }

        /// <summary>
        /// INTERNAL USE ONLY. <para />
        /// The id of the last registered node.
        /// </summary>
        internal int LastRegisteredId { get; private set; }

        // Graph edit operations ---------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructs a new Graph with no nodes.
        /// </summary>
        public Graph() {
            Height = 1000;
            Width = 3000;
            Nodes = new Dictionary<int, Node>();
            RootNode = null;
            LastRegisteredId = 0;
        }

        /// <summary>
        /// FOR INTERNAL USE ONLY. To add a node to the graph, create it using the Node(graph) constructor.
        /// THIS DOES NOT RENDER A NEW NODE.
        /// </summary>
        /// <param name="node">The node to add to the graph</param>
        /// <returns>The id given to the new node</returns>
        internal int Internal_AddNewNode(Node node) {

            TakeSnapshot();

            LastRegisteredId++;

            try {
                Nodes.Add(LastRegisteredId, node);
            }
            catch (ArgumentException) {
                LastRegisteredId = RecomputeLastRegisteredId() + 1;
                Nodes.Add(LastRegisteredId, node);
            }

            return LastRegisteredId;
        }

        /// <summary>
        /// Deletes the node with a given id from the graph.
        /// </summary>
        /// <param name="id">The id of the node to delete.</param>
        public void DeleteNode(int id) {

            TakeSnapshot();

            if (id == RootNode.Id) {
                var children = RootNode.GetChildren();
                if (children.Length > 0) {
                    RootNode = children[0];
                }
                else {
                    RootNode = null;
                }
            }

            // Call deleting code of node
            Nodes[id].Internal_DeleteNode();

            // Remove node
            Nodes.Remove(id);

            // Rendering
            Refresh();

        }

        /// <summary>
        /// Helper method to recompute the last registered id if there was an error reading it.
        /// </summary>
        /// <returns></returns>
        private int RecomputeLastRegisteredId(){
            int max = 0;
            foreach(int key in Nodes.Keys){
                if(key > max){
                    max = key;
                }
            }
            LastRegisteredId = max;
            return max;
        }

        /// <summary>
        /// Takes a snapshot of the graph to be able to revert to this point.
        /// </summary>
        internal void TakeSnapshot() {
            Snapshot snap = new Snapshot(this);
            SearchMapCore.UndoRedoSystem.AddToUndoStack(snap);
        }

        /// <summary>
        /// FOR INTERNAL USE BY THE UNDO/REDO SYSTEM ONLY. To revert to a snapshot, use Snapshot.Revert(). <para/>
        /// Reverts the graph's state to a given snapshot.
        /// </summary>
        /// <param name="snapshot"></param>
        internal void RevertToSnapshot(int lastRegisteredId, Dictionary<int, Node> nodes, int rootNodeId) {

            Nodes = nodes;
            this.LastRegisteredId = lastRegisteredId;
            RootNode = Nodes[rootNodeId];

            foreach(Node node in Nodes.Values) {

                node.rendered = false;
                node.Internal_SetGraph(this);
                node.Internal_RemoveInvalidConnections();

                if(node.ConnectionToParent != null) node.ConnectionToParent.Internal_SetGraph(this);

                var conns = typeof(Node).GetProperty("ConnectionsToSiblings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(node) as Dictionary<int, Connection>;

                foreach (var conn in conns.Values) {
                    conn.Internal_SetGraph(this);
                }

            }

            // Should height and width be reverted ? Not required, as only increasing. 

            Renderer.DeleteAll();

            // Render with the same GraphRenderer.
            Render(Renderer);

        }

        /// <summary>
        /// Returns the last registered node id.
        /// </summary>
        /// <returns></returns>
        public int GetMaxId() {
            return LastRegisteredId;
        }


        // Graphics operations -------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Renders the graph using the given IGraphRenderer
        /// </summary>
        public void Render(IGraphRenderer renderer) {

            Renderer = renderer;
            IsDisplayed = true;
            Renderer.SetDrawingZoneSize(Width, Height);

            Refresh();

        }

        /// <summary>
        /// Checks if the given location is available in the graph, and increase its size if not.
        /// </summary>
        /// <param name="loc"></param>
        public void IncreaseSizeIfLocationNotAvailable(Node node) {

            // Loops are usually only called 0 or 1 times, as the location wont be far from the current drawing zone
            // To make sure loc is available after this method, we use while instead of if.

            while (Math.Abs(node.Location.x) + node.Width / 2 >= Width / 2) {
                Width *= 2;
                if(IsDisplayed) Renderer.SetDrawingZoneSize(Width, Height);
            }

            while (Math.Abs(node.Location.y) + node.Height / 2 >= Height / 2) {
                Height *= 2;
                if(IsDisplayed) Renderer.SetDrawingZoneSize(Width, Height);
            }

        }

        /// <summary>
        /// Call to re-render the graph.
        /// </summary>
        public void Refresh() {
            foreach(Node node in Nodes.Values) {
                node.Refresh();
            }
        }

        // Called when graph is loaded from file
        public void OnReload(){
            

        }

    }

}
