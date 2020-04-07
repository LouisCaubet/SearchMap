﻿using SearchMapCore.Controls;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    public abstract class Node {

        private const int DEFAULT_HEIGHT = 250;
        private const int DEFAULT_WIDTH = 500;

        #region Properties

        private Graph graph;

        // These properties need to be accessed internally by the undo/redo system. ----------------------------------------------------------------

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetParent() instead.
        /// </summary>
        internal int ParentId { get; private set; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetChildren() instead.
        /// </summary>
        internal HashSet<int> ChildrenIds { get; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetSiblings() instead.
        /// </summary>
        internal HashSet<int> SiblingsIds { get; }

        /// <summary>
        ///  true if this node handles the rendering of the connection.
        /// </summary>
        private Dictionary<int, bool> RenderSibling { get; }

        /// <summary>
        /// The Id of this node in the graph. Set once when creating the node.
        /// </summary>
        public int Id { get; }

        protected bool rendered;

        // Characteristics -------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The theme color of this node
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The border color of this node
        /// </summary>
        public Color BorderColor { get; set; }

        /// <summary>
        /// Title of file/website/section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Comment added by the user when adding the node through the browser, or added later
        /// through SearchMap.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Represents the icon file in smp archive. Is the icon of the website if WebNode, the icon of that filetype if File. 
        /// No icon if section node.
        /// </summary>
        public string Icon { get; set; }

        // Replace string with appropriate type
        /// <summary>
        /// This represents the file included in the .smp archive associated with this node
        /// </summary>
        public string AssociatedFile { get; set; }

        // Location : represents abstract location on drawing zone, 
        // platform-dependent code must convert those to actual location on screen.

        /// <summary>
        /// Height of the node. This is NOT the height on-screen, but an abstract mesure based on
        /// drawing zone size.
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Width of the node. This is NOT the width on-screen, but an abstract mesure based on
        /// drawing zone size.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The location of the node. This is NOT the on-screen location, but the abstract location 
        /// relative to the drawing zone.
        /// </summary>
        public Location Location { get; protected set; }

        #endregion

        /// <summary>
        /// Creates a new node, adding it to the graph passed as argument.
        /// </summary>
        /// <param name="graph">The graph to add the node to.</param>
        public Node(Graph graph) {
            this.graph = graph;
            if (graph == null) throw new InvalidOperationException("Tried to associate a Node with graph null.");

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

        #endregion

        #region Edit Operations

        /// <summary>
        /// Reparents the node, removing it from is previous parent's children, and adding it to the new parent's children.
        /// </summary>
        /// <param name="parent">The node to set as parent</param>
        public void SetParent(Node parent) {

            // Save existing connection between this and parent, if existing.
            Connection conn = null;

            // Take snapshot from graph to revert
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

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
        public void AddSibling(Node sibling) {

            // Argument checks
            if (sibling == null) {
                SearchMapCore.Logger.Error("Tried to add null as sibling of node " + Id + " (Title: " + Title + "). ");
                return;
            }

            if (ParentId == sibling.Id || SiblingsIds.Contains(sibling.Id) || ChildrenIds.Contains(sibling.Id)) {
                throw new ArgumentException("These nodes are already connected!");
            }

            // Undo system snapshot
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            // This node will handle the rendering
            RenderSibling.Add(sibling.Id, true);

            SiblingsIds.Add(sibling.Id);
            sibling.SiblingsIds.Add(Id);

            // The other node does not handle the rendering
            sibling.RenderSibling.Add(Id, false);

            // Render changes
            Refresh();

        }

        /// <summary>
        /// Removes a sibling from this node. This also removes this node from the siblings' siblings. (No need to call it twice).
        /// </summary>
        /// <param name="siblingId">The id of the sibling to remove</param>
        public void RemoveSibling(int siblingId) {

            if (SiblingsIds.Contains(siblingId)) {

                Node sibling = graph.Nodes[siblingId];

                Snapshot snapshot = new Snapshot(graph);
                sibling.SiblingsIds.Remove(Id);
                SiblingsIds.Remove(siblingId);
                SearchMapCore.Clipboard.CtrlZ.Push(snapshot);

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
                SearchMapCore.Logger.Error("Node with id " + siblingId + " does not exist or is not a sibling of node " + Id);
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
            var s_ids = SiblingsIds;
            var c_ids = ChildrenIds;

            // Remove from parent
            SetParent(null);

            // Connect childs to parent
            foreach (int child_id in c_ids) {
                if (p_id == -1) graph.Nodes[child_id].SetParent(null);
                else graph.Nodes[child_id].SetParent(graph.Nodes[p_id]);
            }

            // Disconnect siblings
            foreach (int s_id in s_ids) {
                RemoveSibling(s_id);
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

        #endregion

        #region Graphics Data

        /// <summary>
        /// The ID given by the rendering system to this node.
        /// </summary>
        public int RenderId { get; set; }

        /// <summary>
        /// The ID of the connection line between this and its parent
        /// </summary>
        public Connection ConnectionToParent { get; set; }

        /// <summary>
        /// Dictionnary associating the Id of the connection between this and a sibling to the id of the sibling.
        /// </summary>
        protected Dictionary<int, Connection> ConnectionsToSiblings { get; set; }

        /// <summary>
        /// Returns the ID of the connection between this and a given sibling. Throws ArgumentException if the node is not a sibling.
        /// </summary>
        /// <param name="sibling"></param>
        /// <returns></returns>
        public int GetConnectionToSiblingId(Node sibling) {
            try {
                return ConnectionsToSiblings[sibling.Id].RenderId;
            }
            catch (Exception) {
                throw new ArgumentException("The node given in argument (id = " + sibling.Id + ") is not a sibling of node " + Id);
            }
        }

        /// <summary>
        /// Replaces the connection between this and the given sibling with the given connection <para/>
        /// The given connection must be a connection between this node and sibling.
        /// </summary>
        /// <param name="sibling"></param>
        /// <param name="connection"></param>
        public void SetConnectionToSibling(Node sibling, Connection connection) {
            if (ConnectionsToSiblings.ContainsKey(sibling.Id)) {
                if ((connection.GetDepartureNode() == this && connection.GetArrivalNode() == sibling) ||
                    connection.GetDepartureNode() == sibling && connection.GetArrivalNode() == this) {

                    // Delete the current connection
                    graph.Renderer.DeleteObject(ConnectionsToSiblings[sibling.Id].RenderId);

                    // connection is a leftover connection from previous operations
                    // it has probably been removed from the renderer already.
                    connection.RenderId = graph.Renderer.RenderCurvedLine(connection);

                    connection.IsBoldStyle = false;

                    // Set new connection
                    ConnectionsToSiblings[sibling.Id] = connection;
                    sibling.ConnectionsToSiblings[Id] = connection;
                    RefreshConnectionWithSibling(sibling.Id);

                }
                else {
                    throw new ArgumentException("The given connection must be a connection between {this} and {sibling}.");
                }
            }
            else {
                throw new ArgumentException("The node given in argument (id = " + sibling.Id + ") is not a sibling of node " + Id);
            }
        }

        #endregion

        #region Move & Resize

        /// <summary>
        /// Change abstract size of Node.
        /// Refreshes rendering of node.
        /// </summary>
        /// <param name="width">New width of node</param>
        /// <param name="height">New height of node</param>
        public void Resize(int width, int height) {

            // Snapshot
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            Height = height;
            Width = width;
            Refresh();

        }

        /// <summary>
        /// Moves the node to an absolute location. (Abstract location, not the on-screen location)
        /// </summary>
        public void MoveTo(Location newLocation, bool renderMove = false) {

            if (newLocation == null) throw new InvalidOperationException("Tried to move node " + Id + " to Location null");

            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            Location = newLocation;
            graph.IncreaseSizeIfLocationNotAvailable(this);
            Refresh(renderMove);

        }

        /// <summary>
        /// Moves the node by a specific vector.
        /// </summary>
        public void Move(Vector v, bool renderMove = false) {

            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            Location.Translation(v);
            graph.IncreaseSizeIfLocationNotAvailable(this);
            Refresh(renderMove);

        }

        #endregion

        #region Rendering

        /// <summary>
        /// Called on first render of the node.
        /// </summary>
        private void Render(Dictionary<int, bool> flag) {
            if (graph.IsDisplayed) {

                rendered = true;

                RenderId = graph.Renderer.RenderNode(this);

                if (ParentId != -1) {
                    ConnectionToParent = ConnectionPlacement.CreateConnectionBetween(graph, this, GetParent());
                    ConnectionToParent.IsBoldStyle = true;
                    ConnectionToParent.RenderId = graph.Renderer.RenderCurvedLine(ConnectionToParent);
                }

                foreach (int id in SiblingsIds) {

                    // If this node must handle the rendering.
                    if (RenderSibling[id]) {
                        var conn = ConnectionPlacement.CreateConnectionBetween(graph, this, graph.Nodes[id]);
                        conn.ShadowColor = new Color(100, 100, 100);
                        conn.RenderId = graph.Renderer.RenderCurvedLine(conn);
                        ConnectionsToSiblings.Add(id, conn);
                        graph.Nodes[id].ConnectionsToSiblings.Add(Id, conn);
                    }

                }

                foreach (int child in ChildrenIds) {
                    graph.Nodes[child].Refresh(false, flag);
                }

            }
        }

        /// <summary>
        /// Refreshes the render of the node.
        /// </summary>
        public virtual void Refresh(bool renderMove = false, Dictionary<int, bool> flag = null) {
            if (graph.IsDisplayed) {

                if(flag == null) flag = new Dictionary<int, bool>();

                // This node has already been visited.
                if (flag.ContainsKey(Id) && flag[Id]) return;

                if (!flag.ContainsKey(Id)) flag.Add(Id, true);
                else flag[Id] = true;    
                

                if (!rendered) {
                    Render(flag);
                    return;
                }

                if(renderMove) graph.Renderer.MoveObjectSmoothly(RenderId, Location);

                RefreshContentOnly();

                // Re-render line to parent.
                if(ParentId != -1) {

                    if(ConnectionToParent == null) {
                        ConnectionToParent = ConnectionPlacement.CreateConnectionBetween(graph, this, GetParent());
                        ConnectionToParent.IsBoldStyle = true;
                        ConnectionToParent.RenderId = graph.Renderer.RenderCurvedLine(ConnectionToParent);
                    }
                    else {
                        ConnectionPlacement.RefreshConnection(graph, ConnectionToParent);
                        graph.Renderer.RefreshCurvedLine(ConnectionToParent.RenderId);   
                    }

                                     
                }


                // Re-render lines to siblings.
                foreach (int id in SiblingsIds) {

                    if (!RenderSibling[id]) {
                        // To refresh the connection, we need to refresh the node controlling the render.
                        graph.Nodes[id].RefreshConnectionWithSibling(Id);
                        continue;
                    }

                    RefreshConnectionWithSibling(id);

                }               


                foreach (int child in ChildrenIds) {
                    graph.Nodes[child].Refresh(renderMove, flag);
                }

            }
        }

        private void RefreshConnectionWithSibling(int id) {

            if (ConnectionsToSiblings.ContainsKey(id)) {
                ConnectionPlacement.RefreshConnection(graph, ConnectionsToSiblings[id]);
                graph.Renderer.RefreshCurvedLine(ConnectionsToSiblings[id].RenderId);
            }
            else {
                var conn = ConnectionPlacement.CreateConnectionBetween(graph, this, graph.Nodes[id]);
                conn.RenderId = graph.Renderer.RenderCurvedLine(conn);
                ConnectionsToSiblings.Add(id, conn);
                graph.Nodes[id].ConnectionsToSiblings.Add(Id, conn);
            }

        }

        /// <summary>
        /// Refreshes the node but not its position nor connections to other nodes.
        /// </summary>
        public virtual void RefreshContentOnly() {
            if (graph.IsDisplayed) {
                graph.Renderer.RefreshNode(RenderId);
            }
        }

        #endregion

        // Event Handling

        /// <summary>
        /// Called when user left-clicks the node.
        /// </summary>
        public abstract void OnClick(/*MouseEvent e*/);


    }

}
