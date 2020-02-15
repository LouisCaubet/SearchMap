using SearchMapCore.Graph;

namespace SearchMapCore.Rendering {

    /// <summary>
    /// Abstract representation of a graph renderer
    /// </summary>
    public interface IGraphRenderer {

        /// <summary>
        /// Should be called before any drawing to set the size of the drawing zone.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void SetDrawingZoneSize(int x, int y);

        /// <summary>
        /// Renders the given node at its location (abstract location). 
        /// Returns an unique ID to identify the rendered object
        /// </summary>
        /// <param name="toRender">The node to render</param>
        int RenderNode(Node toRender);

        /// <summary>
        /// Updates the render of a node's title, icon, etc. 
        /// </summary>
        /// <param name="renderId">The node to update.</param>
        /// <returns>false if the id does not exist, true else.</returns>
        void RefreshNode(int renderId);

        /// <summary>
        /// Deletes the rendered object with a given id.
        /// Returns false if the id doesn't exit, true else.
        /// </summary>
        /// <param name="id">The id of the object to delete</param>
        /// <returns></returns>
        bool DeleteObject(int id);

        /// <summary>
        /// Draws a curved line representing the given connection. 
        /// Returns the id of the rendered object. <para/>
        /// </summary>
        /// <returns></returns>
        int RenderCurvedLine(Connection connection);

        /// <summary>
        /// Refreshes the render of the given curved line to match changes in Connection object.
        /// </summary>
        /// <param name="renderId"></param>
        void RefreshCurvedLine(int renderId);

        /// <summary>
        /// Moves the render with given id smoothly from its current location to the given destination.
        /// </summary>
        /// <param name="id">The object to move.</param>
        /// <param name="destination">Where to move the object to.</param>
        void MoveObjectSmoothly(int id, Location destination);

        /// <summary>
        /// Returns percentage zoom level (0.1 - 5)
        /// </summary>
        /// <returns></returns>
        double GetZoomLevel();

    }

}
