using Newtonsoft.Json;
using SearchMapCore.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SearchMapCore.Undoing {

    /// <summary>s
    /// Represents a snapshot of a graph, used to revert to it.
    /// </summary>
    internal class Snapshot : IRevertable {

        public static MethodInfo JsonConvertGenericDeserializeObjectMethod { get; private set; }

        // ALL THE FOLLOWING FIELDS SHOULD BE CONSIDERED READONLY
        // THEY NEED TO BE SET-ABLE FOR DESERIALIZATION.

        /// <summary>
        /// Calling revert won't do anything if the snapshot could not be made.
        /// </summary>
        
        [JsonProperty]
        private bool IsValidSnapshot { get; set; }

        [JsonProperty]
        private int Height { get; set; }

        [JsonProperty]
        private int Width { get; set; }

        [JsonProperty]
        private int LastRegisteredId { get; set; }

        [JsonProperty]
        private Dictionary<int, string> SerializedNodes { get; set; }

        [JsonProperty]
        private Dictionary<int, Type> NodeTypes { get; set; }

        [JsonProperty]
        private int RootNodeId { get; set; }

        [JsonIgnore]
        public Graph.Graph Graph { get; set; }

        /// <summary>
        /// Takes a snapshot from the given graph.
        /// </summary>
        /// <param name="graph">The graph to snapshot.</param>
        public Snapshot(Graph.Graph graph) {

            if(JsonConvertGenericDeserializeObjectMethod == null) {

                // Find the MethodInfo corresponding to JsonConvert.DeserializeObject<T>(string, JsonSerializerSettings)

                var methods = typeof(JsonConvert).GetMethods();

                foreach (var method in methods) {
                    if (method.Name == "DeserializeObject" && method.IsGenericMethod && method.GetParameters().Length == 2
                        && method.GetParameters()[0].ParameterType == typeof(string)
                        && method.GetParameters()[1].ParameterType == typeof(JsonSerializerSettings)) {

                        JsonConvertGenericDeserializeObjectMethod = method;
                        break;

                    }
                }

            }

            // Dont backup empty graphs.
            if (graph == null || graph.RootNode == null) {
                IsValidSnapshot = false;
                return;
            }

            Graph = graph;

            SerializedNodes = new Dictionary<int, string>();
            NodeTypes = new Dictionary<int, Type>();

            Height = graph.Height;
            Width = graph.Width;

            // Specific ReferenceLoopHandling options are required because each node is 
            // pointing back to the graph.

            try {

                LastRegisteredId = graph.LastRegisteredId;

                foreach (Node node in graph.Nodes.Values) {
                    SerializedNodes.Add(node.Id, JsonConvert.SerializeObject(node, Formatting.Indented,
                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                    NodeTypes.Add(node.Id, node.GetType());
                }

                RootNodeId = graph.RootNode.Id;

                IsValidSnapshot = true;

            }
            catch (Exception e) {
                IsValidSnapshot = false;
                SearchMapCore.Logger.Warning("Unable to take valid snapshot of graph in its current state.");
                SearchMapCore.Logger.Warning(e.Message);
                SearchMapCore.Logger.Warning(e.StackTrace);
                throw e;
            }

        }

        public void Revert() {

            if (!IsValidSnapshot) return;

            try {

                var nodes = new Dictionary<int, Node>();

                foreach (int id in SerializedNodes.Keys) {

                    // We determine the type of Node at runtime to prevent a switch on all possible Node types.
                    // This also permits plugins to add node types without modifications to this class.
                    // The following code is the runtime equivalent of:
                    // Node node = JsonConvert.DeserializeObject<NodeType>(SerializedNodes[id], ...)

                    Node node = (Node) JsonConvertGenericDeserializeObjectMethod.MakeGenericMethod(NodeTypes[id]).Invoke(this,
                        new object[] { SerializedNodes[id],
                        new JsonSerializerSettings { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor } });

                    nodes.Add(node.Id, node);

                }

                Graph.RevertToSnapshot(LastRegisteredId, nodes, RootNodeId, Height, Width);

            }
            catch (Exception e) {
                SearchMapCore.Logger.Error("Could not revert to snapshot.");
                SearchMapCore.Logger.Error(e.Message);
                SearchMapCore.Logger.Error(e.StackTrace);
            }

        }

    }

}
