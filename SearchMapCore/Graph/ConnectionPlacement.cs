using System;
using System.Collections.Generic;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Class containing tools to build connections between nodes.
    /// </summary>
    public static class ConnectionPlacement {

        /// <summary>
        /// The distance between the first point and the next point, used to impose the direction of line exiting node.
        /// </summary>
        private const int DERIV_DIST = 100;

        private const int SMOOTHNESS = 10;

        /// <summary>
        /// Finds the intersect between the border of node and the vector x,y starting from the center of node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal static (Location, Side) FindIntersect(Node node, int x, int y) {

            if(x != 0) {

                // Thales :
                int y_intersect = (int)Math.Round((double)y * (node.Width / 2) / x);

                // Are we intersecting a x=cte side of the node ?
                if (Math.Abs(y_intersect) <= node.Height / 2) {

                    var loc = new Location(node.Location.x + Math.Sign(x) * node.Width / 2, node.Location.y + Math.Sign(x) * y_intersect);

                    if (x > 0) {
                        // right side
                        return (loc, Side.RIGHT);
                    }

                    else return (loc, Side.LEFT);

                }

            }

            // y cant be 0 if x is 0
            if (y == 0) throw new ArgumentException("Both nodes are at the same location; can't compute connection.");
            
            // Thales :
            int x_intersect = (int)Math.Round((double)x * (node.Height / 2) / y);

            if(Math.Abs(x_intersect) <= node.Width / 2) {

                var loc = new Location(node.Location.x + Math.Sign(y) * x_intersect, node.Location.y + Math.Sign(y) * node.Height / 2);

                if (y > 0) {
                    return (loc, Side.TOP);
                }

                else return (loc, Side.BOTTOM);

            }

            // This should never happen, and shows an error in the math above.
            throw new InvalidOperationException("INTERNAL ERROR : Could not find an intersection between node and given line.");

        }

        /// <summary>
        /// Creates and returns a smooth connection between nodes node1 and node2
        /// </summary>
        /// <param name="node1">The first node of the connection</param>
        /// <param name="node2">The second node in the connection</param>
        /// <returns>A Connection between node1, node2. Used by frontend to draw the polyline. </returns>
        public static Connection CreateConnectionBetween(Graph graph, Node node1, Node node2) {

            // Compute straight line between nodes.
            int x = node1.Location.x - node2.Location.x;
            int y = node1.Location.y - node2.Location.y;

            // Intersection of this line with node borders.
            (Location intersect1, Side side1) = FindIntersect(node1, -x, -y);
            (Location intersect2, Side side2) = FindIntersect(node2, x, y);

            // Compute last point
            var dir1 = GetDirection(side1);
            var dir2 = GetDirection(side2);

            var dir = new Vector(dir1.x + dir2.x, dir1.y + dir2.y);

            // Determine where to apply dir.
            Location pt3 = new Location();

            // Middle
            pt3.y = (node1.Location.y + node2.Location.y) / 2;
            pt3.x = (node1.Location.x + node2.Location.x) / 2;

            dir.Scale((new Vector(x, y)).GetNorm() / SMOOTHNESS / dir.GetNorm());

            pt3.Translation(dir);

            return new Connection(graph) {
                Points = new List<Location>(4) { intersect1, pt3, pt3, intersect2 },
                LocationNode1 = new Location(node1.Location),
                LocationNode2 = new Location(node2.Location),
                NodeFromId = node1.Id,
                NodeToId = node2.Id,
            };

        }

        /// <summary>
        /// Refreshes the connection to take changes in position for node1/2 into account.
        /// </summary>
        public static void RefreshConnection(Graph graph, Connection connection) {

            var node1 = graph.Nodes[connection.NodeFromId];
            var node2 = graph.Nodes[connection.NodeToId];

            Connection default_conn = CreateConnectionBetween(graph, node1, node2);

            if (!connection.IsCustomizedByUser) {
                connection.Points = new List<Location>(default_conn.Points);
            }
            else {

                // Determine which node moved
                if(node1.Location.x != connection.LocationNode1.x || node1.Location.y != connection.LocationNode1.y) {
                    // Node 1 moved

                    var x = node1.Location.x - connection.LocationNode1.x;
                    var y = node1.Location.y - connection.LocationNode1.y;

                    var intersect = new Location(connection.Points[0].x + x, connection.Points[0].y + y);

                    if(connection.UserImposedPoints[0] != null) {
                        connection.MoveConnectorPointOf(node1, connection.UserImposedPoints[0]);
                    }
                    else {
                        connection.Points[0] = default_conn.Points[0];
                    }

                    if(connection.UserImposedPoints[3] == null) {
                        connection.Points[3] = default_conn.Points[3];
                    }

                }
                else {
                    // Node 2 moved

                    var x = node2.Location.x - connection.LocationNode2.x;
                    var y = node2.Location.y - connection.LocationNode2.y;

                    var intersect = new Location(connection.Points[3].x + x, connection.Points[3].y + y);

                    if(connection.UserImposedPoints[3] != null) {
                        connection.MoveConnectorPointOf(node2, connection.UserImposedPoints[3]);
                    }
                    else {
                        connection.Points[3] = default_conn.Points[3];
                    }

                    if (connection.UserImposedPoints[0] == null) {
                        connection.Points[0] = default_conn.Points[0];
                    }

                }

                //// Move the control point proportionnaly to the distance the node was moved.
                // Proportionnality factor depends on position of the control pt on the connection
                Location controlPt = new Location(connection.Points[1]);

                // The percentage of the way where the control point is.
                // Do not change the control point in the (rare) case where the distance is zero on one axis.

                double fracX, fracY;

                try {
                    fracX = (controlPt.x - node1.Location.x) / Math.Abs(node1.Location.x - node2.Location.x);
                }
                catch (DivideByZeroException) {
                    fracX = 0;
                }

                try {
                    fracY = (controlPt.y - node1.Location.y) / Math.Abs(node1.Location.y - node2.Location.y);
                }
                catch (DivideByZeroException) {
                    fracY = 0;
                }

                // By how much both nodes moved
                var Node1Move_x = node1.Location.x - connection.LocationNode1.x;
                var Node1Move_y = node1.Location.y - connection.LocationNode1.y;
                var Node2Move_x = node2.Location.x - connection.LocationNode2.x;
                var Node2Move_y = node2.Location.y - connection.LocationNode2.y;

                if(Math.Abs(Node1Move_x) >= Math.Abs(Node2Move_x)) {
                    // Node 1 moved along x, we suppose node2 didnt move.
                    controlPt.x = (int) (controlPt.x + Node1Move_x * (1 - fracX));
                }
                else {
                    controlPt.x = (int)(controlPt.x + Node2Move_x * fracX);
                }

                if (Math.Abs(Node1Move_y) >= Math.Abs(Node2Move_y)) {
                    // Node 1 moved along y
                    controlPt.y = (int)(controlPt.y + Node1Move_y * (1 - fracY));
                }
                else {
                    controlPt.y = (int)(controlPt.y + Node2Move_y * fracY);
                }

                // The current implementation gives weird results in some case, we will thus keep the control point as is.
                // TODO fix this.
                // connection.SetControlPoint(controlPt);

                // Update connection
                connection.LocationNode1 = new Location(node1.Location);
                connection.LocationNode2 = new Location(node2.Location);

            }

        }

        /// <summary>
        /// Turns the location around the node of distance move, and returns the resulting location.
        /// </summary>
        /// <returns></returns>
        private static (Location, Side) MoveAlongNodeBorder(Node node, Location current, int move) {

            // THIS DOESNT WORK

            // newLoc is location relative to center of node
            Location newLoc = new Location(current.x - node.Location.x, current.y - node.Location.y);
            Side currentSide = Side.LEFT;

            bool IsPositive = move > 0;

            // Turn around node while move != 0
            while(move != 0) {

                switch (currentSide) {

                    case Side.LEFT:
                        if (move > 0) {

                            var newy = Math.Min(node.Height / 2, newLoc.y + move);
                            move -= node.Height / 2 - newLoc.y;
                            if (move < 0) move = 0;

                            newLoc.y = newy;
                            currentSide = Side.TOP;

                        }
                        else {

                            var newy = Math.Max(-node.Height / 2, newLoc.y + move);
                            move += node.Height / 2 + newLoc.y;
                            if (move > 0) move = 0;

                            newLoc.y = newy;
                            currentSide = Side.BOTTOM;

                        }
                        break;

                    case Side.TOP:
                        if(move > 0) {

                            var newx = Math.Min(node.Width / 2, newLoc.x + move);
                            move -= node.Width / 2 - newLoc.x;
                            if (move < 0) move = 0;

                            newLoc.x = newx;
                            currentSide = Side.RIGHT;

                        }
                        else {

                            var newx = Math.Max(-node.Width / 2, newLoc.x + move);
                            move += node.Width / 2 + newLoc.x;
                            if (move > 0) move = 0;

                            newLoc.x = newx;
                            currentSide = Side.LEFT;

                        }
                        break;

                    case Side.RIGHT:
                        if (move < 0) {

                            var newy = Math.Min(node.Height / 2, newLoc.y - move);
                            move += node.Height / 2 - newLoc.y;
                            if (move > 0) move = 0;

                            newLoc.y = newy;
                            currentSide = Side.TOP;

                        }
                        else {

                            var newy = Math.Max(-node.Height / 2, newLoc.y - move);
                            move -= node.Height / 2 + newLoc.y;
                            if (move < 0) move = 0;

                            newLoc.y = newy;
                            currentSide = Side.BOTTOM;

                        }
                        break;

                    case Side.BOTTOM:
                        if (move < 0) {

                            var newx = Math.Min(node.Width / 2, newLoc.x - move);
                            move += node.Width / 2 - newLoc.x;
                            if (move > 0) move = 0;

                            newLoc.x = newx;
                            currentSide = Side.RIGHT;

                        }
                        else {

                            var newx = Math.Max(-node.Width / 2, newLoc.x - move);
                            move -= node.Width / 2 + newLoc.x;
                            if (move < 0) move = 0;

                            newLoc.x = newx;
                            currentSide = Side.LEFT;

                        }
                        break;

                }

                // Safety check
                if(IsPositive && move < 0) {
                    throw new Exception("INTERNAL ERROR in ConnectionPlacement.MoveAlongNodeBorder");
                }
                else if(!IsPositive && move > 0) {
                    throw new Exception("INTERNAL ERROR in ConnectionPlacement.MoveAlongNodeBorder");
                }

            }

            return (newLoc, currentSide);

        }

        /// <summary>
        /// Represents the 4 sides of a node
        /// </summary>
        internal enum Side {
            LEFT,
            RIGHT,
            TOP,
            BOTTOM,
        }

        /// <summary>
        /// Returns a tangent vector to the given side, used to compute a third point on the connection
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private static Vector GetDirection(Side side) {
            switch (side) {
                case Side.LEFT: return new Vector(0, -DERIV_DIST);
                case Side.RIGHT: return new Vector(0, -DERIV_DIST);
                case Side.TOP: return new Vector(-DERIV_DIST, 0);
                case Side.BOTTOM: return new Vector(-DERIV_DIST, 0);
                default: throw new ArgumentException("Invalid side");
            }
        }

        

    }

}
