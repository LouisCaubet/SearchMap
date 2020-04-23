using Newtonsoft.Json;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Graph {

    partial class Node {

        #region Graphics Data

        /// <summary>
        /// The ID given by the rendering system to this node.
        /// </summary>
        [JsonProperty]
        public int RenderId { get; set; }

        /// <summary>
        /// The ID of the connection line between this and its parent
        /// </summary>
        [JsonProperty]
        public Connection ConnectionToParent { get; set; }

        /// <summary>
        /// Dictionnary associating the Id of the connection between this and a sibling to the id of the sibling.
        /// </summary>
        [JsonProperty]
        protected internal Dictionary<int, Connection> ConnectionsToSiblings { get; set; }

        /// <summary>
        /// Returns the ID of the connection between this and a given sibling. Throws ArgumentException if the node is not a sibling.
        /// </summary>
        /// <param name="sibling"></param>
        /// <returns></returns>
        public int GetConnectionToSiblingId(Node sibling) {

            if (ConnectionsToSiblings.ContainsKey(sibling.Id)) {
                return ConnectionsToSiblings[sibling.Id].RenderId;
            }
            else {
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

        #region Rendering

        /// <summary>
        /// Called on first render of the node.
        /// </summary>
        private void Render(Dictionary<int, bool> flag) {
            if (graph.IsDisplayed) {

                rendered = true;

                RenderId = graph.Renderer.RenderNode(this);

                if (ParentId != -1) {

                    // ConnectionToParent may already have been initialized if we're deserializing from json.

                    if (ConnectionToParent == null) {
                        ConnectionToParent = ConnectionPlacement.CreateConnectionBetween(graph, this, GetParent());
                        ConnectionToParent.IsBoldStyle = true;
                        ConnectionToParent.RenderOrRefresh();
                    }
                    else {
                        ConnectionPlacement.RefreshConnection(graph, ConnectionToParent);
                        ConnectionToParent.RenderOrRefresh();
                    }

                }

                foreach (int id in SiblingsIds) {


                    if (!RenderSibling[id]) {
                        // To refresh the connection, we need to refresh the node controlling the render.
                        graph.Nodes[id].RefreshConnectionWithSibling(Id);
                        continue;
                    }

                    RefreshConnectionWithSibling(id);

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

                if (flag == null) flag = new Dictionary<int, bool>();

                // This node has already been visited.
                if (flag.ContainsKey(Id) && flag[Id]) return;

                if (!flag.ContainsKey(Id)) flag.Add(Id, true);
                else flag[Id] = true;


                if (!rendered) {
                    Render(flag);
                    return;
                }

                if (renderMove) graph.Renderer.MoveObjectSmoothly(RenderId, Location);

                RefreshContentOnly();


                // Re-render line to parent.
                if (ParentId != -1) {

                    if (ConnectionToParent == null) {
                        ConnectionToParent = ConnectionPlacement.CreateConnectionBetween(graph, this, GetParent());
                        ConnectionToParent.IsBoldStyle = true;
                        ConnectionToParent.RenderOrRefresh();
                    }
                    else {
                        ConnectionPlacement.RefreshConnection(graph, ConnectionToParent);
                        ConnectionToParent.RenderOrRefresh();
                    }


                }


                // Re-render lines to siblings.
                foreach (int id in SiblingsIds) {

                    if (!RenderSibling[id]) {
                        // To refresh the connection, we need to refresh the node controlling the render.
                        graph.Nodes[id].RefreshConnectionWithSibling(Id);
                    }
                    else RefreshConnectionWithSibling(id);

                }


                foreach (int child in ChildrenIds) {
                    graph.Nodes[child].Refresh(renderMove, flag);
                }

            }
        }

        private void RefreshConnectionWithSibling(int id) {

            if (ConnectionsToSiblings.ContainsKey(id)) {
                ConnectionPlacement.RefreshConnection(graph, ConnectionsToSiblings[id]);
                ConnectionsToSiblings[id].RenderOrRefresh();
            }
            else {
                var conn = ConnectionPlacement.CreateConnectionBetween(graph, this, graph.Nodes[id]);
                conn.ShadowColor = new Color(100, 100, 100);
                ConnectionsToSiblings.Add(id, conn);
                ConnectionsToSiblings[id].RenderOrRefresh();
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

    }

}
