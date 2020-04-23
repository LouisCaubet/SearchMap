using Newtonsoft.Json;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SearchMapCore.Undoing {

    class NodeState : IRevertable {

        public Graph.Graph Graph { get; set; }

        private string SerializedNode { get; }
        private Type NodeType { get; }

        public NodeState(Graph.Graph graph, Node node) {

            Graph = graph;

            SerializedNode = JsonConvert.SerializeObject(node, Formatting.Indented,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            NodeType = node.GetType();

            SearchMapCore.Logger.Debug("Took Snapshot of node " + node.Id);

        }

        public void Revert() {

            // DeserializeObject of type determined at runtime
            // For details see Snapshot.cs

            Node node = (Node) Snapshot.JsonConvertGenericDeserializeObjectMethod.MakeGenericMethod(NodeType).Invoke(this,
                new object[] { SerializedNode, new JsonSerializerSettings { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor } });


            node.Internal_SetGraph(Graph);
            if(node.ConnectionToParent != null) node.ConnectionToParent.Internal_SetGraph(Graph);

            var conns = typeof(Node).GetProperty("ConnectionsToSiblings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(node) as Dictionary<int, Connection>;

            foreach(var conn in conns.Values) {
                conn.Internal_SetGraph(Graph);
            }

            // Delete all connections with this node, they will be recreated in Refresh.
            Node replace = Graph.Nodes[node.Id];

            if(replace.ConnectionToParent != null) Graph.Renderer.DeleteObject(replace.ConnectionToParent.RenderId);
            replace.ConnectionToParent = null;

            // Access connections to siblings through reflection
            var connToSiblings = typeof(Node).GetProperty("ConnectionsToSiblings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(replace) as Dictionary<int, Connection>;

            foreach(Connection conn in connToSiblings.Values) {
                Graph.Renderer.DeleteObject(conn.RenderId);
            }

            // Delete the previously existing node.
            replace.rendered = false;
            Graph.Renderer.DeleteObject(replace.RenderId);

            Graph.Nodes[node.Id] = node;

            // Renderer must redraw node.
            node.RenderId = Graph.Renderer.RenderNode(node);

            node.Refresh();

        }

    }

}
