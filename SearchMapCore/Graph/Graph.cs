using Newtonsoft.Json;
using SearchMapCore.Controls;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;

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
        /// The id of the last registered node.
        /// </summary>
        private int lastRegisteredId = 0;

        // Graph edit operations ---------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructs a new Graph with no nodes.
        /// </summary>
        public Graph() {
            Height = 1000;
            Width = 3000;
            Nodes = new Dictionary<int, Node>();
            RootNode = null;
        }

        /// <summary>
        /// FOR INTERNAL USE ONLY. To add a node to the graph, create it using the Node(graph) constructor.
        /// THIS DOES NOT RENDER A NEW NODE.
        /// </summary>
        /// <param name="node">The node to add to the graph</param>
        /// <returns>The id given to the new node</returns>
        internal int Internal_AddNewNode(Node node) {

            lastRegisteredId++;

            try {
                Nodes.Add(lastRegisteredId, node);
            }
            catch (ArgumentException) {
                lastRegisteredId = RecomputeLastRegisteredId() + 1;
                Nodes.Add(lastRegisteredId, node);
            }

            return lastRegisteredId;
        }

        /// <summary>
        /// Deletes the node with a given id from the graph.
        /// </summary>
        /// <param name="id">The id of the node to delete.</param>
        public void DeleteNode(int id) {

            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(this));

            if (id == RootNode.Id) {
                var children = RootNode.GetChildren();
                if(children.Length > 0) {
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
        /// Determines good location for node given its parent, and adds it to the graph. <para />
        /// The node must have been created in this graph. 
        /// </summary>
        /// <param name="node">The node to place.</param>
        [Obsolete]
        public void PlaceNodeAsChild(Node node) {

            // Argument checks
            if (node == null) {
                SearchMapCore.Logger.Error("NullPointerException: Tried to place node null.");
                return;
            }

            if (!node.IsNodeOfGraph(this)) {
                SearchMapCore.Logger.Error("ArgumentException: Attempted to add a node which is not associated with this graph.");
                return;
            }

            
            if(node.Location != null) {
                SearchMapCore.Logger.Warning("Requested automatic placement of node that already contains location information. " +
                    "This information will be lost.");
            }

            // Save current state of graph to revert
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(this));

            // Determine location of node
            try {
                Location loc = NodePlacement.PlaceNode(this, node);
                node.MoveTo(loc);
                node.Refresh();
            }
            catch(Exception e) {
                SearchMapCore.Logger.Error("Requested to place node missing information for placement.");
                SearchMapCore.Logger.Error(e.Message);
                SearchMapCore.Logger.Error(e.StackTrace);
            }
            

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
            lastRegisteredId = max;
            return max;
        }

        /// <summary>
        /// FOR INTERNAL USE BY THE UNDO/REDO SYSTEM ONLY. To revert to a snapshot, use Snapshot.Revert(). <para/>
        /// Reverts the graph's state to a given snapshot.
        /// </summary>
        /// <param name="snapshot"></param>
        internal void RevertToSnapshot(Graph snapshot) {

            Nodes = snapshot.Nodes;
            RootNode = snapshot.RootNode;
            lastRegisteredId = snapshot.lastRegisteredId;

            // Should height and width be reverted ? Not required, as only increasing. 

            // TODO Renderer.DeleteAll()

            // Render with the same GraphRenderer.
            Render(Renderer);

        }

        /// <summary>
        /// Returns the last registered node id.
        /// </summary>
        /// <returns></returns>
        public int GetMaxId() {
            return lastRegisteredId;
        }


        // Graphics operations -------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Renders the graph using the given IGraphRenderer
        /// </summary>
        public void Render(IGraphRenderer renderer) {
            Renderer = renderer;
            IsDisplayed = true;
            Renderer.SetDrawingZoneSize(Width, Height);

            if(RootNode != null) RootNode.Refresh();

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
            if(RootNode != null) RootNode.Refresh();
        }

        // Called when graph is loaded from file
        public void OnReload(){
            

        }

    }

}
