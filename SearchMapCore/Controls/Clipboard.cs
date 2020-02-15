using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SearchMapCore.Controls {

    /// <summary>
    /// Manages the SearchMap Clipboard and Undo/Redo Operations.
    /// </summary>
    public class Clipboard {

        // TODO replace generic stacks with stacks that delete old elements.

        /// <summary>
        /// Stack containing the snapshots to revert to.
        /// </summary>
        public Stack<Snapshot> CtrlZ { get; }

        /// <summary>
        /// Stack containing the snapshots to redo.
        /// </summary>
        public Stack<Snapshot> CtrlY { get; }

        public Clipboard() {
            CtrlZ = new Stack<Snapshot>();
            CtrlY = new Stack<Snapshot>();
        }

        /// <summary>
        /// Reverts to the last snapshot.
        /// </summary>
        public void Undo() {

            if(CtrlZ.Count == 0) {
                SearchMapCore.Logger.Info("Nothing to undo!");
                return;
            }

            var snapshot = CtrlZ.Pop();

            // Take snapshot of current state and add it to CtrlY for redo.
            CtrlY.Push(new Snapshot(snapshot.Graph));

            snapshot.Revert();
        }

        /// <summary>
        /// Redoes the previous undo.
        /// </summary>
        public void Redo() {

            if (CtrlY.Count == 0) {
                SearchMapCore.Logger.Info("Nothing to redo!");
                return;
            }

            var snapshot = CtrlY.Pop();

            CtrlZ.Push(new Snapshot(snapshot.Graph));

            snapshot.Revert();
        }

    }

    /// <summary>
    /// Represents a snapshot of a graph, used to revert to it.
    /// </summary>
    public class Snapshot {

        /// <summary>
        /// The serialized graph at the moment of the snapshot.
        /// </summary>
        private readonly string SerializedSnapshot;

        /// <summary>
        /// Reference to the graph this snapshot was made from.
        /// </summary>
        internal Graph.Graph Graph;

        /// <summary>
        /// Takes a snapshot from the given graph.
        /// </summary>
        /// <param name="graph">The graph to snapshot.</param>
        public Snapshot(Graph.Graph graph) {

            Graph = graph;

            // Specific ReferenceLoopHandling options are required because each node is 
            // pointing back to the graph.

            try {
                SerializedSnapshot = JsonConvert.SerializeObject(graph, Formatting.Indented, 
                                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
            }

        }

        /// <summary>
        /// Reverts the graph to the state stored in this snapshot.
        /// </summary>
        public void Revert() {

            try {
                Graph.RevertToSnapshot((Graph.Graph)JsonConvert.DeserializeObject(SerializedSnapshot));

                // Because of the reference loop handling, graph will be null for each node.
                // We must re-add the pointer from there.
                foreach(int key in Graph.Nodes.Keys) {
                    Graph.Nodes[key].Internal_SetGraph(Graph);
                }

            }
            catch(Exception e) {
                SearchMapCore.Logger.Error("Could not revert to snapshot.");
                SearchMapCore.Logger.Error(e.Message);
                SearchMapCore.Logger.Error(e.StackTrace);
            }

        }

    }

}
