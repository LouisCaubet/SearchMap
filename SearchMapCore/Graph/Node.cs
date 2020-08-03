using Newtonsoft.Json;
using SearchMapCore.Undoing;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    public abstract partial class Node {

        private const int DEFAULT_HEIGHT = 250;
        private const int DEFAULT_WIDTH = 500;

        #region Properties

        [JsonIgnore]
        private Graph graph;

        // These properties need to be accessed internally by the undo/redo system. ----------------------------------------------------------------
        // For Json.NET to be able to deserialize those properties a private setter is required, but it should not be used.

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetParent() instead.
        /// </summary>
        [JsonProperty]
        internal int ParentId { get; private set; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetChildren() instead.
        /// </summary>
        [JsonProperty]
        internal HashSet<int> ChildrenIds { get; private set; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetSiblings() instead.
        /// </summary>
        [JsonProperty]
        internal HashSet<int> SiblingsIds { get; private set; }

        /// <summary>
        ///  true if this node handles the rendering of the connection.
        /// </summary>
        [JsonProperty]
        internal Dictionary<int, bool> RenderSibling { get; set; }

        /// <summary>
        /// The Id of this node in the graph. Set once when creating the node.
        /// </summary>
        [JsonProperty]
        public int Id { get; private set; }

        [JsonProperty]
        protected internal bool rendered;

        // Characteristics -------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The theme color of this node
        /// </summary>
        [JsonProperty]
        public Color Color { get; set; }

        /// <summary>
        /// The border color of this node
        /// </summary>
        [JsonProperty]
        public Color BorderColor { get; set; }

        /// <summary>
        /// Title of file/website/section.
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        /// Comment added by the user when adding the node through the browser, or added later
        /// through SearchMap. <para />
        /// Comment is a byte array representing UTF-8 Rich Text.
        /// </summary>
        [JsonProperty]
        public byte[] Comment { get; set; }

        /// <summary>
        /// Represents the icon file in smp archive. Is the icon of the website if WebNode, the icon of that filetype if File. 
        /// No icon if section node.
        /// </summary>
        [JsonProperty]
        public string Icon { get; set; }

        // Replace string with appropriate type
        /// <summary>
        /// This represents the file included in the .smp archive associated with this node
        /// </summary>
        [JsonProperty]
        public string AssociatedFile { get; set; }

        // Location : represents abstract location on drawing zone, 
        // platform-dependent code must convert those to actual location on screen.

        /// <summary>
        /// Height of the node. This is NOT the height on-screen, but an abstract mesure based on
        /// drawing zone size.
        /// </summary>
        [JsonProperty]
        public int Height { get; protected set; }

        /// <summary>
        /// Width of the node. This is NOT the width on-screen, but an abstract mesure based on
        /// drawing zone size.
        /// </summary>
        [JsonProperty]
        public int Width { get; protected set; }

        /// <summary>
        /// The location of the node. This is NOT the on-screen location, but the abstract location 
        /// relative to the drawing zone.
        /// </summary>
        [JsonProperty]
        public Location Location { get; protected set; }

        #endregion

        /// <summary>
        /// Creates a new node, adding it to the graph passed as argument.
        /// </summary>
        /// <param name="graph">The graph to add the node to.</param>
        public Node(Graph graph) {
            this.graph = graph;
            if (graph == null) throw new InvalidOperationException("Tried to associate a Node with graph null.");

            // Create a revert point.
            graph.TakeSnapshot();

            ChildrenIds = new HashSet<int>();
            SiblingsIds = new HashSet<int>();
            RenderSibling = new Dictionary<int, bool>();
            ParentId = -1;

            // Generate Id
            Id = graph.Internal_AddNewNode(this);

            // Do not render the node for now
            rendered = false;

            // Set default width/height values
            Height = DEFAULT_HEIGHT;
            Width = DEFAULT_WIDTH;

            // Default colors
            Color = new Color(255, 255, 255);
            BorderColor = new Color(50, 50, 50);

            // Default location
            Location = new Location(0, 0);

            ConnectionsToSiblings = new Dictionary<int, Connection>();

        }

        /// <summary>
        /// ONLY FOR DESERIALIZATION. 
        /// NO FIELDS WILL BE INITIALIZED. 
        /// USE OUTSIDE OF SERIALIZATION SYSTEM WILL FAIL.
        /// </summary>
        protected Node() { }

        #region Getters

        // Private helper method to convert list of ids to node array.
        private Node[] GenerateArrayFromIdList(HashSet<int> ids) {
            Node[] array = new Node[ids.Count];

            int i = 0;
            foreach (int id in ids) {
                array[i] = graph.Nodes[id];
                i++;
            }

            return array;
        }

        /// <summary>
        /// The parent node is the node preceding this one in the reading of the graph.
        /// Only one parent node. Other related nodes should be placed as siblings.
        /// </summary>
        public Node GetParent() {
            if (ParentId == -1) return null;
            return graph.Nodes[ParentId];
        }

        /// <summary>
        /// Generates and returns the array of children
        /// </summary>
        /// <returns>Array of children of this node</returns>
        public Node[] GetChildren() {
            return GenerateArrayFromIdList(ChildrenIds);
        }

        /// <summary>
        /// Generates and returns the array of siblings
        /// </summary>
        /// <returns></returns>
        public Node[] GetSiblings() {
            return GenerateArrayFromIdList(SiblingsIds);
        }

        /// <summary>
        /// Checks whether the node with the given id is a sibling of this one.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsSiblingWith(int id) {
            return SiblingsIds.Contains(id);
        }

        /// <summary>
        /// Checks whether the given node is a sibling of this one.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsSiblingWith(Node node) {
            return SiblingsIds.Contains(node.Id);
        }

        #endregion

        #region Edit Operations

        /// <summary>
        /// Reparents the node, removing it from is previous parent's children, and adding it to the new parent's children.
        /// </summary>
        /// <param name="parent">The node to set as parent</param>
        public void SetParent(Node parent, bool takeSnapshot = true) {

            // Save existing connection between this and parent, if existing.
            Connection conn = null;

            // Revert whole graph, hope it doesn't have a too big impact on performance.
            if(takeSnapshot) graph.TakeSnapshot();

            // Remove this from parent children - no public function for this, should only be done through SetParent
            if (GetParent() != null) GetParent().ChildrenIds.Remove(this.Id);

            // Argument checks
            if (parent == null) {
                ParentId = -1;
            }
            else {

                // Case where the new parent is previously a sibling.
                if (SiblingsIds.Contains(parent.Id)) {

                    // Keep the same connection, to keep user customizations.
                    conn = ConnectionsToSiblings[parent.Id];

                    RemoveSibling(parent.Id);

                }

                // Add to new parent children
                parent.ChildrenIds.Add(this.Id);

                // IMPORTANT EDGE CASE : when placing one of the children as parent.
                // In this case, we swap the nodes (we set the previous parent of this as the parent of the new parent).
                if (ChildrenIds.Contains(parent.Id)) {
                    ChildrenIds.Remove(parent.Id);
                    parent.SetParent(GetParent());
                }

                // Reparent
                ParentId = parent.Id;

            }

            // Delete previous connection to parent
            if (graph.IsDisplayed && ConnectionToParent != null) {
                graph.Renderer.DeleteObject(ConnectionToParent.RenderId);
                ConnectionToParent = null;
            }

            if (conn != null) {

                ConnectionToParent = conn;
                ConnectionToParent.IsBoldStyle = true;

                // Re-render, as the connection has been deleted when calling RemoveSibling.
                ConnectionToParent.RenderId = graph.Renderer.RenderCurvedLine(ConnectionToParent);

            }

            // Render changes.
            // This method should not render the node if we're not ready yet, hence the check.
            if (rendered) Refresh();

        }

        /// <summary>
        /// Adds a sibling to this node.
        /// </summary>
        /// <param name="sibling"></param>
        public void AddSibling(Node sibling, bool takeSnapshot = true) {

            // Argument checks
            if (sibling == null) {
                SearchMapCore.Logger.Error("Tried to add null as sibling of node " + Id + " (Title: " + Title + "). ");
                return;
            }

            if (ParentId == sibling.Id || SiblingsIds.Contains(sibling.Id) || ChildrenIds.Contains(sibling.Id)) {
                throw new ArgumentException("These nodes are already connected!");
            }

            // Taking snapshot to revert.
            // NodeState node1 = new NodeState(graph, this);
            // NodeState node2 = new NodeState(graph, sibling);
            // SearchMapCore.UndoRedoSystem.AddToUndoStack(new IRevertable[] { node1, node2 });

            /* The previous code, which should improve performance by only reverting the nodes that have been modified by
             * this action, doesn't work: the previous node is not completely deleted...
             * For now, we are therefore reverting the whole graph. In case performance is an issue, reconsider using previous code.
             */

            if(takeSnapshot) graph.TakeSnapshot();

            SiblingsIds.Add(sibling.Id);
            sibling.SiblingsIds.Add(Id);

            // This node will handle the rendering
            RenderSibling.Add(sibling.Id, true);

            // The other node does not handle the rendering
            sibling.RenderSibling.Add(Id, false);

            // Render changes
            Refresh();

        }

        /// <summary>
        /// Removes a sibling from this node. This also removes this node from the siblings' siblings. (No need to call it twice).
        /// </summary>
        /// <param name="siblingId">The id of the sibling to remove</param>
        public void RemoveSibling(int siblingId, bool takeSnapshot = true) {

            if (SiblingsIds.Contains(siblingId)) {

                if(takeSnapshot) graph.TakeSnapshot();

                Node sibling = graph.Nodes[siblingId];

                sibling.SiblingsIds.Remove(Id);
                SiblingsIds.Remove(siblingId);

                if (rendered) {
                    if (RenderSibling[siblingId]) {
                        graph.Renderer.DeleteObject(ConnectionsToSiblings[siblingId].RenderId);
                    }
                    else {
                        graph.Renderer.DeleteObject(sibling.ConnectionsToSiblings[Id].RenderId);
                    }
                }

                RenderSibling.Remove(siblingId);
                sibling.RenderSibling.Remove(Id);

                ConnectionsToSiblings.Remove(siblingId);
                sibling.ConnectionsToSiblings.Remove(Id);

            }
            else {
                SearchMapCore.Logger.Error("Tried to remove sibling with id " + siblingId + ", which does not exist or is not a sibling of node " + Id);
            }

        }

        /// <summary>
        /// FOR INTERNAL USE ONLY. To delete a node, use Graph.DeleteNode(id). <para/>
        /// Properly removes every connection to this node to allow deletion.
        /// Should only be called from Graph.RemoveNode().
        /// </summary>
        internal void Internal_DeleteNode() {

            // Locally save characteristics
            int p_id = ParentId;
            var s_ids = new HashSet<int>(SiblingsIds);
            var c_ids = new HashSet<int>(ChildrenIds);

            // Remove from parent
            SetParent(null, false);

            // Connect childs to parent
            foreach (int child_id in c_ids) {
                if (p_id == -1) graph.Nodes[child_id].SetParent(null, false);
                else graph.Nodes[child_id].SetParent(graph.Nodes[p_id], false);
            }

            // Disconnect siblings
            foreach (int s_id in s_ids) {
                RemoveSibling(s_id, false);
            }

            // Render Changes
            if (graph.IsDisplayed) {

                graph.Renderer.DeleteObject(RenderId);
                if (ConnectionToParent != null) graph.Renderer.DeleteObject(ConnectionToParent.RenderId);

                foreach (var conn in ConnectionsToSiblings.Values) {
                    graph.Renderer.DeleteObject(conn.RenderId);
                }

            }

        }

        #endregion

        #region Reverting from JSON save.

        /// <summary>
        /// Adds a revert point to the current state of this node. Ctrl+Z will then revert to the current state.
        /// </summary>
        public void TakeSnapshot() {

            NodeState state = new NodeState(graph, this);
            SearchMapCore.UndoRedoSystem.AddToUndoStack(state);

        }

        /// <summary>
        /// Checks if this node was created as node from graph g
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        internal bool IsNodeOfGraph(Graph g) {
            return graph == g;
        }

        /// <summary>
        /// FOR INTERNAL USE BY THE SNAPSHOT SYSTEM ONLY. <para />
        /// The graph of a node should never be set like this, only given when constructing the node.
        /// </summary>
        internal void Internal_SetGraph(Graph g) {
            if (graph != null) {
                throw new ArgumentException("Can't set graph because it is already set");
            }
            graph = g;
        }

        /// <summary>
        /// INTERNAL USE ONLY.
        /// </summary>
        internal void Internal_RemoveInvalidConnections() {

            if (!graph.Nodes.ContainsKey(ParentId)) {
                ParentId = -1;
                ConnectionToParent = null;
            }

            foreach(int id in SiblingsIds) {

                if (!graph.Nodes.ContainsKey(id)) {
                    SiblingsIds.Remove(id);
                    ConnectionsToSiblings.Remove(id);
                }

            }

            foreach(int id in ChildrenIds) {

                if (!graph.Nodes.ContainsKey(id)) {
                    ChildrenIds.Remove(id);
                }

            }

        }

        #endregion

        #region Move & Resize

        /// <summary>
        /// Changes size of Node.
        /// Refreshes rendering of node.
        /// </summary>
        /// <param name="width">New width of node</param>
        /// <param name="height">New height of node</param>
        public void Resize(int width, int height) {

            Height = height;
            Width = width;
            Refresh();

        }

        /// <summary>
        /// Moves the node to an absolute location. (Abstract location, not the on-screen location)
        /// </summary>
        public void MoveTo(Location newLocation, bool renderMove = false) {

            if (newLocation == null) throw new InvalidOperationException("Tried to move node " + Id + " to Location null");

            Location = newLocation;
            graph.IncreaseSizeIfLocationNotAvailable(this);
            Refresh(renderMove);

        }

        /// <summary>
        /// Moves the node by a specific vector.
        /// </summary>
        public void Move(Vector v, bool renderMove = false) {

            Location.Translation(v);
            graph.IncreaseSizeIfLocationNotAvailable(this);
            Refresh(renderMove);

        }

        #endregion

        // Event Handling

        /// <summary>
        /// Called when user left-clicks the node.
        /// </summary>
        public abstract void OnClick(/*MouseEvent e*/);

    }

}
