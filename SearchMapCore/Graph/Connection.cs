using Newtonsoft.Json;
using SearchMapCore.Rendering;
using SearchMapCore.Serialization;
using SearchMapCore.Undoing;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Represents a connection between two nodes.
    /// </summary>
    public class Connection {

        /// <summary>
        /// When the click is closer to one node than this distance, it will change the position of the connector 
        /// to this node instead of the control point.
        /// </summary>
        public const int CONNECTOR_EDIT_DISTANCE = 200;

        [JsonIgnore]
        private Graph Graph { get; set; }

        /// <summary>
        /// The Id given to this connection by the rendering system.
        /// </summary>
        [JsonProperty]
        public int RenderId { get; set; }

        /// <summary>
        /// The points interpolated by this connection. 
        /// 4 points required. To interpolate using only one control point, put the control point twice (pos 1 and 2)
        /// </summary>
        [JsonProperty]
        public List<Location> Points { get; set; }

        /// <summary>
        /// The points than have been imposed by user interaction.
        /// </summary>
        [JsonProperty]
        public List<Location> UserImposedPoints { get; set; }

        /// <summary>
        /// The inner color of the Connection
        /// </summary>
        [JsonProperty]
        public Color Color { get; set; }

        /// <summary>
        /// The connection of the shadow of the connection
        /// </summary>
        [JsonProperty]
        public Color ShadowColor { get; set; }

        /// <summary>
        /// true for connections between father-child, false for nodes between siblings.
        /// </summary>
        [JsonProperty]
        public bool IsBoldStyle { get; internal set; }
        
        // Used to refresh user customized connections
        [JsonProperty]
        internal int NodeFromId { get; set; }
        [JsonProperty]
        internal int NodeToId { get; set; }

        [JsonProperty]
        internal Location LocationNode1 { get; set; }
        [JsonProperty]
        internal Location LocationNode2 { get; set; }

        /// <summary>
        /// Indicates if this node was customized by user.
        /// </summary>
        [JsonProperty]
        public bool IsCustomizedByUser { get; set; }

        /// <summary>
        /// Constructs a new Connection object, with default values.
        /// </summary>
        public Connection(Graph graph) {
            Graph = graph;
            Points = new List<Location>();
            UserImposedPoints = new List<Location>(4) { null, null, null, null };
            Color = null;
            ShadowColor = null;
            IsCustomizedByUser = false;
            IsBoldStyle = false;
        }

        /// <summary>
        /// For deserialization only.
        /// </summary>
        private Connection() { }

        public Node GetDepartureNode() {
            return Graph.Nodes[NodeFromId];
        }

        public Node GetArrivalNode() {
            return Graph.Nodes[NodeToId];
        }

        /// <summary>
        /// Sets the control point of this connection to the given location.
        /// </summary>
        /// <param name="controlPt"></param>
        public void SetControlPoint(Location controlPt) {

            Points[1] = controlPt;
            Points[2] = controlPt;

            UserImposedPoints[1] = controlPt;
            UserImposedPoints[2] = controlPt;

            IsCustomizedByUser = true;

        }

        /// <summary>
        /// Sets the connecting point of this connection to the given node to the
        /// point of the border of the node on the line from the center of the node to the given location. <para/>
        /// Throws ArgumentException if the node is not connected to this connection.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="toIntersect"></param>
        public void MoveConnectorPointOf(Node node, Location toIntersect, bool userImposed = false) {

            int x = toIntersect.x - node.Location.x;
            int y = toIntersect.y - node.Location.y;

            Location intersect;

            try {
                (intersect, _) = ConnectionPlacement.FindIntersect(node, x, y);
            }
            catch (ArgumentException) {
                return;
            }
            

            if (node.Id == NodeFromId) {
                Points[0] = intersect;
                if (userImposed) UserImposedPoints[0] = intersect;
            }
            else if(node.Id == NodeToId) {
                Points[3] = intersect;
                if (userImposed) UserImposedPoints[3] = intersect;
            }
            else {
                throw new ArgumentException("The given node is not an extremity of this connection");
            }

            IsCustomizedByUser = true;

        }

        /// <summary>
        /// Returns the action to be performed when a particular location is selected.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public Action GetActionAtLocation(Location loc) {

            var length = Points[0].Distance(Points[3]);

            var connector_edit = Math.Min(CONNECTOR_EDIT_DISTANCE, length / 3);

            if(loc.Distance(Points[0]) <= connector_edit) {
                return Action.EDIT_CONNECTOR1;
            }
            else if(loc.Distance(Points[3]) <= connector_edit) {
                return Action.EDIT_CONNECTOR2;
            }
            else {
                return Action.EDIT_CONTROLPOINT;
            }

        }

        /// <summary>
        /// Refreshes the connection if already rendered, renders it otherwise.
        /// </summary>
        public void RenderOrRefresh() {

            if (Graph.Renderer.ContainsObjectWithId(RenderId)) {
                Graph.Renderer.RefreshCurvedLine(RenderId);
            }
            else {
                RenderId = Graph.Renderer.RenderCurvedLine(this);
            }

        }

        /// <summary>
        /// Actions that can be performed on the connector.
        /// </summary>
        public enum Action {
            EDIT_CONNECTOR1,
            EDIT_CONTROLPOINT,
            EDIT_CONNECTOR2
        }

        /// <summary>
        /// Creates a revert point reverting the connection to its current state.
        /// </summary>
        public void TakeSnapshot() {
            ConnectionState serial = new ConnectionState(Graph, this);
            SearchMapCore.UndoRedoSystem.AddToUndoStack(serial);
        }

        public override string ToString() {
            string text = "Connection interpolating following points: ";
            foreach (Location pt in Points) {
                text += "Point x=" + pt.x + "; y=" + pt.y + "        ";
            }
            return text;
        }

        /// <summary>
        /// For internal use by deserialization system only.
        /// </summary>
        /// <param name="g"></param>
        internal void Internal_SetGraph(Graph g) {
            Graph = g;
        }

    }

}
