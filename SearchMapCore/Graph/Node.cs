using SearchMapCore.Controls;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    public abstract class Node {

        private const int DEFAULT_HEIGHT = 250;
        private const int DEFAULT_WIDTH = 500;

        private Graph graph;

        // These properties need to be accessed internally by the undo/redo system. ----------------------------------------------------------------

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetParent() instead.
        /// </summary>
        internal int ParentId { get; private set; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetChildren() instead.
        /// </summary>
        internal HashSet<int> ChildrenIds { get; private set; }

        /// <summary>
        /// DIRECT ACCESS FOR INTERNAL USE ONLY. Use GetSiblings() instead.
        /// </summary>
        internal HashSet<int> SiblingsIds { get; private set; }

        protected bool rendered;

        /// <summary>
        /// Creates a new node, adding it to the graph passed as argument.
        /// </summary>
        /// <param name="graph">The graph to add the node to.</param>
        public Node(Graph graph) {
            this.graph = graph;

            ChildrenIds = new HashSet<int>();
            SiblingsIds = new HashSet<int>();
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

        }

        /// <summary>
        /// The Id of this node in the graph. Set once when creating the node.
        /// </summary>
        public int Id { get; }

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


        // -- Getters --------------------------------------------------------------------------------------------------------------------------

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


        // Edit operations -----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reparents the node, removing it from is previous parent's children, and adding it to the new parent's children.
        /// </summary>
        /// <param name="parent">The node to set as parent</param>
        public void SetParent(Node parent) {

            // Take snapshot from graph to revert
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            // Argument checks
            if (parent == null) {
                ParentId = -1;
                return;
            }

            // Remove this from parent children - no public function for this, should only be done through SetParent
            if (parent != null) parent.ChildrenIds.Remove(this.Id);

            // Reparent
            ParentId = parent.Id;

            // Add to new parent children
            parent.ChildrenIds.Add(this.Id);

            // Render changes.
            Refresh();

        }

        /// <summary>
        /// Adds a sibling to this node.
        /// </summary>
        /// <param name="sibling"></param>
        public void AddSibling(Node sibling) {

            // Argument checks
            if (sibling == null) {
                SearchMapCore.Logger.Debug("Tried to add null as sibling of node " + Id + " (Title: " + Title + "). ");
                return;
            }

            // Undo system snapshot
            SearchMapCore.Clipboard.CtrlZ.Push(new Snapshot(graph));

            SiblingsIds.Add(sibling.Id);
            sibling.SiblingsIds.Add(Id);

            // Render changes
            Refresh();

        }

        /// <summary>
        /// Removes a sibling from this node. This also removes this node from the siblings' siblings. (No need to call it twice).
        /// </summary>
        /// <param name="siblingId">The id of the sibling to remove</param>
        public void RemoveSibling(int siblingId) {

            try {
                Snapshot snapshot = new Snapshot(graph);
                graph.Nodes[siblingId].SiblingsIds.Remove(Id);
                SiblingsIds.Remove(siblingId);
                SearchMapCore.Clipboard.CtrlZ.Push(snapshot);
            }
            catch (Exception) {
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
            foreach(int child_id in c_ids) {
                graph.Nodes[child_id].SetParent(graph.Nodes[p_id]);
            }

            // Disconnect siblings
            foreach (int s_id in s_ids) {
                RemoveSibling(s_id);
            }

            // Rendering of changes done in Graph.

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


        // Graphics Data ---------------------------------------------------------------------------------------------------------------------------

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
            catch(Exception) {
                throw new ArgumentException("The node given in argument (id = " + sibling.Id + ") is not a sibling of node " + Id);
            }
        }

        // Graphics operations ----------------------------------------------------------------------------------------------------------------------

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


        // Rendering

        /// <summary>
        /// Called on first render of the node.
        /// </summary>
        public virtual void Render() {
            if (graph.IsDisplayed) {

                rendered = true;

                RenderId = graph.Renderer.RenderNode(this);

                if(ParentId != -1) {
                    ConnectionToParent = ConnectionPlacement.CreateConnectionBetween(graph, this, GetParent());
                    ConnectionToParent.RenderId = graph.Renderer.RenderCurvedLine(ConnectionToParent);
                }
                
                foreach(int id in SiblingsIds) {
                    var conn = ConnectionPlacement.CreateConnectionBetween(graph, this, graph.Nodes[id]);
                    conn.RenderId = graph.Renderer.RenderCurvedLine(conn);
                    ConnectionsToSiblings.Add(id, conn);
                }

                foreach(int child in ChildrenIds) {
                    graph.Nodes[child].Refresh();
                }

            }
        }

        /// <summary>
        /// Refreshes the render of the node.
        /// </summary>
        public virtual void Refresh(bool renderMove = false) {
            if (graph.IsDisplayed) {

                if (!rendered) {
                    Render();
                    return;
                }

                if(renderMove) graph.Renderer.MoveObjectSmoothly(RenderId, Location);

                RefreshContentOnly();

                // Re-render line to parent.
                if(ParentId != -1) {
                    ConnectionPlacement.RefreshConnection(graph, ConnectionToParent);
                    graph.Renderer.RefreshCurvedLine(ConnectionToParent.RenderId);                    
                }


                // Re-render lines to siblings.
                foreach (int id in SiblingsIds) {
                    ConnectionPlacement.RefreshConnection(graph, ConnectionsToSiblings[id]);
                    graph.Renderer.RefreshCurvedLine(ConnectionsToSiblings[id].RenderId);
                }               


                foreach (int child in ChildrenIds) {
                    graph.Nodes[child].Refresh(renderMove);
                }

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



        // Event Handling

        /// <summary>
        /// Called when user left-clicks the node.
        /// </summary>
        public abstract void OnClick(/*MouseEvent e*/);


    }

}
