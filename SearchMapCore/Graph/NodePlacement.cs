using System;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Static class containing tools to determine a good location and place nodes on a graph.
    /// </summary>
    public static class NodePlacement {

        // CONSTANTS
        /// <summary>
        /// How much free space should there be around a node.
        /// </summary>
        private const int NODE_BORDER = 10;

        /// <summary>
        /// Sets all coordinates in area to false between minX, minY and maxX, maxY.
        /// </summary>
        /// <param name="area">The area to edit</param>
        /// <param name="minX">Left X of the area to set unavailable</param>
        /// <param name="maxX">Right X of the area to set unavailable</param>
        /// <param name="minY">Bottom Y of the area to set unavailable</param>
        /// <param name="maxY">Top Y of the area to set unavailable</param>
        /// <returns></returns>
        private static bool[,] SetUnavailable(bool[,] area, int minX, int maxX, int minY, int maxY) {

            int length = area.GetLength(0);

            // Input check
            if (minX < 0) minX = 0;
            if (minY < 0) minY = 0;

            if (maxX > length) maxX = length;
            if (maxY > length) maxY = length;

            // Fill array.
            for(int x=minX; x<maxX; x++) {
                for(int y=minY; y<maxY; y++) {
                    area[x, y] = false;
                }
            }

            return area;
        }

        /// <summary>
        /// Checks if the rectangle from startX, startY (bottom left) and size sizeX, sizeY is available in area.
        /// </summary>
        /// <param name="area">The area to check</param>
        /// <param name="startX">Left pos of the rectangle</param>
        /// <param name="startY">Bottom pos of the rectangle</param>
        /// <param name="sizeX">Width of the rectangle</param>
        /// <param name="sizeY">Height of the rectangle</param>
        /// <returns></returns>
        private static bool IsSpaceAvailableFromHere(bool[,] area, int startX, int startY, int sizeX, int sizeY) {
            
            if (startX + sizeX >= area.GetLength(0)) return false;
            if (startY + sizeY >= area.GetLength(1)) return false;

            for(int x=Math.Max(startX,0); x<startX + sizeX; x++) {
                for(int y=Math.Max(startY,0); y<startY + sizeY; y++) {
                    if (!area[x, y]) return false;
                }
            }

            return true;

        }
        
        /// <summary>
        /// Finds a space of given size available in area.
        /// Throws NotFoundException if not found.
        /// </summary>
        /// <param name="area">The area to search</param>
        /// <param name="sizeX">Width of the rectangle to find.</param>
        /// <param name="sizeY">Height of the rectangle to find.</param>
        /// <returns>The bottom-left coordinates of an available rectangle of that size.</returns>
        private static (int,int) FindSpaceInArea(bool[,] area, int sizeX, int sizeY) {

            // Search done in half-squares starting from center and expanding

            for (int p = 0; p < area.GetLength(0); p++) {

                // start with line at y=p, x=p..0
                for (int x = p; x <= 0; x--) {
                    if(IsSpaceAvailableFromHere(area, x, p, sizeX, sizeY)) {
                        return (x, p);
                    }
                }

                // next line at x=p, y=p..0
                for (int y = p; y >= 0; y--) {
                    if(IsSpaceAvailableFromHere(area, p, y, sizeX, sizeY)) {
                        return (p, y);
                    }
                }

            }

            throw new NotFoundException();

        }

        /// <summary>
        /// Checks if 2 areas are overlapping.
        /// </summary>
        /// <returns></returns>
        private static bool IsOverlapping(int min_x1, int max_x1, int min_x2, int max_x2, int min_y1, int max_y1, int min_y2, int max_y2) {

            // Check for overlapping along x-axis : minX .. n_minX .. maxX .. n_maxX
            if (min_x1 < max_x2 && max_x1 > min_x2) {

                // Check for overlapping along y-axis
                if (min_y1 < max_y2 && max_y1 > min_y2) {
                    return true;
                }

            }

            return false;

        }

        /// <summary>
        /// Searches for a location in the graph to place the node.
        /// </summary>
        /// <param name="graph">The graph to add the node to.</param>
        /// <param name="node">The node to place.</param>
        /// <returns>Location to place the node to.</returns>
        public static Location PlaceNode(Graph graph, Node node) {

            var parent = node.GetParent();
            var parentLocation = parent.Location;

            // We place this node further away from (0,0) than the parent.

            // We consider the circle centered on the parent and of radius radius
            // Compute the max and mean distance between parent and children.
            float r = 300;
            float mean = 0;
            foreach(Node child in parent.GetChildren()) {
                if (child.Id != node.Id) {
                    var distance = parentLocation.Distance(child.Location);
                    if (r < distance) {
                        r = (float)distance;
                        mean += (float)distance;
                    }
                }
            }

            int radius = (int)Math.Ceiling(r);
            mean /= parent.GetChildren().Length;

            // Default value if no children
            if (mean == 0) mean = 300;

            // Stores available positions in circle.
            bool[,] available = new bool[2 * radius,2 * radius];

            // Inner square unavailable as we want to place the child further away than the parent
            available = SetUnavailable(available, 0, radius, 0, radius);

            // Positions of nodes unavailable + lines to these nodes
            foreach(int key in graph.Nodes.Keys) {

                var n = graph.Nodes[key];

                int relativeX = n.Location.x - parentLocation.x;
                int relativeY = n.Location.y - parentLocation.y;

                // Check if the node is in the search area
                // TODO take into account nodes that are partially in area, using IsOverlapping
                if (relativeX > - radius && relativeX < radius) {
                    if(relativeY > - radius && relativeY < radius) {

                        // Mark box as unavailable pixels.
                        available = SetUnavailable(available, relativeX + radius - NODE_BORDER, relativeX + n.Width + radius + NODE_BORDER, 
                            relativeY + radius - NODE_BORDER, relativeY + n.Height + radius + NODE_BORDER);
                    }
                }

                // TODO mark path to node unavailable

            }

            int requiredX = node.Width + 2 * NODE_BORDER;
            int requiredY = node.Height + 2 * NODE_BORDER;

            // Search for a requiredX*requiredY space in available.
            try {
                (int x, int y) = FindSpaceInArea(available, requiredX, requiredY);
                return new Location {
                    x = parentLocation.x - radius + x + NODE_BORDER,
                    y = parentLocation.y - radius + y + NODE_BORDER
                };
            }
            catch (NotFoundException) {
                // Use FreeAreaForNode to place.
                // We choose to place the node at the mean distance of children, in the 45° direction away from center
                int signX = Math.Sign(node.Location.x);
                int signY = Math.Sign(node.Location.y);
                double sqrt2 = Math.Sqrt(2);
                var translation = new Vector((int) Math.Ceiling(mean / sqrt2) * signX, (int)Math.Ceiling(mean / sqrt2) * signY) ;
                Location newLocation = parentLocation;
                newLocation.Translation(translation);

                FreeAreaForNode(graph, newLocation, node);
                return newLocation;

            }
            
        }

        // EXPERIMENTAL : The following method is experimental. We expect it to move nodes out of the way in a natural way.
        // Complexity (Mean) : On average, only one node should overlap with a given location.
        // Thus, only one recursive call.
        // Each call is in O((number of nodes))
        // Total call in O((number of nodes)^2)

        /// <summary>
        /// Moves overlapping nodes to allow placement of node at loc.
        /// </summary>
        /// <param name="graph">The graph to edit</param>
        /// <param name="loc">The location to move node to</param>
        /// <param name="node">The node to move to loc.</param>
        public static void FreeAreaForNode(Graph graph, Location loc, Node node) {

            // Area to free
            int minX = loc.x - NODE_BORDER;
            int maxX = loc.x + node.Width + NODE_BORDER;
            int minY = loc.y - NODE_BORDER;
            int maxY = loc.y + node.Height + NODE_BORDER;

            // For each node overlapping with the area, recursively move it out
            foreach(int key in graph.Nodes.Keys) {
                var n = graph.Nodes[key];

                int n_minX = n.Location.x;
                int n_maxX = n.Location.x + n.Width;
                int n_minY = n.Location.y;
                int n_maxY = n.Location.y + n.Height;

                if(IsOverlapping(minX, maxX, n_minX, n_maxX, minY, maxY, n_minY, n_maxY)) {

                    // Construct direction of movement vector

                    Location center = new Location() {
                        x = node.Location.x + node.Width / 2,
                        y = node.Location.y + node.Height / 2
                    };

                    Location n_center = new Location() {
                        x = n.Location.x + n.Width / 2,
                        y = n.Location.y + n.Height / 2
                    };

                    Vector movement = new Vector(center, n_center);

                    // If movement = 0, both are exactly overlapping. Choosing to move along 1-|x| axis towards x = 0
                    if(movement.x == 0 && movement.y == 0) {
                        if(n.Location.x < 0) {
                            movement.x = 1;
                            movement.y = 1;
                        }
                        else {
                            movement.x = -1;
                            movement.y = 1;
                        }
                    }

                    // Set norm of vector to free area

                    int moveX, moveY;

                    if(movement.x < 0) {
                        moveX = n_maxX - minX;
                    }
                    else {
                        moveX = maxX - n_minX;
                    }

                    if(movement.y < 0) {
                        moveY = n_maxY - minY;
                    }
                    else {
                        moveY = maxY - n_minY;
                    }

                    int scaleX = (int)Math.Ceiling((double) moveX / movement.x);
                    int scaleY = (int)Math.Ceiling((double) moveY / movement.y);

                    movement.Scale(Math.Max(scaleX, scaleY));

                    // Determine new location of node and recursively move it there.
                    Location newLocation = n.Location;
                    newLocation.Translation(movement);

                    FreeAreaForNode(graph, newLocation, n);

                }

                // No overlapping !

            }

            // Move node
            node.MoveTo(loc, true);

        }

    }

    /// <summary>
    /// Represents a failure to find a specific value in an array/list.
    /// </summary>
    public class NotFoundException : Exception { }

}
