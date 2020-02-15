using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Represents a connection between two nodes.
    /// </summary>
    public class Connection {

        public const int CONNECTOR_EDIT_DISTANCE = 75;

        private Graph Graph { get; }

        /// <summary>
        /// The Id given to this connection by the rendering system.
        /// </summary>
        public int RenderId { get; set; }

        /// <summary>
        /// The points interpolated by this connection. 
        /// 4 points required. To interpolate using only one control point, put the control point twice (pos 1 and 2)
        /// </summary>
        public List<Location> Points { get; set; }

        public List<Location> UserImposedPoints { get; set; }

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
        }

        /// <summary>
        /// The inner color of the Connection
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The connection of the shadow of the connection
        /// </summary>
        public Color ShadowColor { get; set; }

        // Used to refresh user customized connections
        internal int NodeFromId { get; set; }
        internal int NodeToId { get; set; }

        internal Location LocationNode1 { get; set; }
        internal Location LocationNode2 { get; set; }

        /// <summary>
        /// Indicates if this node was customized by user.
        /// </summary>
        public bool IsCustomizedByUser { get; set; }

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
        /// Actions that can be performed on the connector.
        /// </summary>
        public enum Action {
            EDIT_CONNECTOR1,
            EDIT_CONTROLPOINT,
            EDIT_CONNECTOR2
        }


        public override string ToString() {
            string text = "";
            foreach (Location pt in Points) {
                text += "Point x=" + pt.x + "; y=" + pt.y + "        ";
            }
            return text;
        }

    }

}
