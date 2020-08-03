using SearchMapCore.Graph;
using SearchMapCore.Rendering;
using SearchMapCore.Undoing;
using System;
using System.Collections.Generic;

namespace SearchMapCore.Serialization {

    /// <summary>
    /// Obsolete - Use SearchMapCore.Undoing.ConnectionState instead.
    /// </summary>
    [Obsolete]
    public class SerializableConnection : IRevertable {

        public Graph.Graph Graph { get; set; }

        public Connection Connection { get; }

        public List<Location> Points { get; }
        public List<Location> UserImposedPoints { get; }
        public Color Color { get; }
        public Color ShadowColor { get; }
        public bool IsBoldStyle { get; }

        public int NodeFromId { get; }
        public int NodeToId { get; }

        public bool IsCustomizedByUser { get; }

        public SerializableConnection(Graph.Graph graph, Connection conn) {

            Connection = conn;
            Graph = graph;

            Points = conn.Points;
            UserImposedPoints = conn.UserImposedPoints;
            Color = conn.Color;
            ShadowColor = conn.ShadowColor;
            IsBoldStyle = conn.IsBoldStyle;

            NodeFromId = conn.NodeFromId;
            NodeToId = conn.NodeToId;

            IsCustomizedByUser = conn.IsCustomizedByUser;

        }

        public void Revert() {

            Console.WriteLine("Current connection:");
            Console.WriteLine(Connection.ToString());
            Console.WriteLine("Reverting to:");
            

            Connection.Points = Points;
            Connection.UserImposedPoints = UserImposedPoints;
            Connection.Color = Color;
            Connection.ShadowColor = ShadowColor;
            Connection.IsBoldStyle = IsBoldStyle;
            Connection.IsCustomizedByUser = IsCustomizedByUser;

            Connection.RenderOrRefresh();

            Console.WriteLine(Connection.ToString());
            Console.WriteLine("Connection reverted.");

        }

    }

}
