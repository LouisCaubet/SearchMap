using Newtonsoft.Json;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Undoing {

    /// <summary>
    /// Represents a revert point for a Connection object.
    /// </summary>
    class ConnectionState : IRevertable {

        public Graph.Graph Graph { get; set; }

        string SerializedConnection { get; }

        Connection Connection { get; }

        /// <summary>
        /// Creates a revert point for the given connection, part of the given graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="conn"></param>
        public ConnectionState(Graph.Graph graph, Connection conn) {

            Graph = graph;
            Connection = conn;

            SerializedConnection = JsonConvert.SerializeObject(conn, Formatting.Indented,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        }

        public void Revert() {

            Connection conn = JsonConvert.DeserializeObject<Connection>(SerializedConnection,
                new JsonSerializerSettings { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor });

            Connection.Points = conn.Points;
            Connection.UserImposedPoints = conn.UserImposedPoints;
            Connection.Color = conn.Color;
            Connection.ShadowColor = conn.ShadowColor;
            Connection.IsBoldStyle = conn.IsBoldStyle;
            Connection.IsCustomizedByUser = conn.IsCustomizedByUser;

            Connection.RenderOrRefresh();

            Console.WriteLine("Connection reverted.");

        }

    }

}
