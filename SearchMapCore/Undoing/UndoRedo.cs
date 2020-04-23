using System;
using System.Collections.Generic;

namespace SearchMapCore.Undoing {

    /// <summary>
    /// Manages the SearchMap Clipboard and Undo/Redo Operations.
    /// </summary>
    public class UndoRedo {

        // TODO replace generic stacks with stacks that delete old elements.

        /// <summary>
        /// Stack containing the snapshots to revert to.
        /// </summary>
        private Stack<IEnumerable<IRevertable>> CtrlZ { get; }

        /// <summary>
        /// Stack containing the snapshots to redo.
        /// </summary>
        private Stack<IEnumerable<IRevertable>> CtrlY { get; }

        public UndoRedo() {
            CtrlZ = new Stack<IEnumerable<IRevertable>>();
            CtrlY = new Stack<IEnumerable<IRevertable>>();
        }


        internal void AddToUndoStack(IRevertable state) {
            CtrlZ.Push(new IRevertable[] { state });
        }

        internal void AddToUndoStack(IEnumerable<IRevertable> states) {
            CtrlZ.Push(states);
        }

        /// <summary>
        /// Removes the last taken snapshot from the revert stack.
        /// </summary>
        public void RemoveLastSnapshot() {
            // Required to take snapshot but remove if no action is taken in a given delay.
            // See SearchMap.Windows.Events.NodeControl_Events.cs for an example.
            CtrlZ.Pop();
        }

        /// <summary>
        /// Call once after loading a graph from a save.
        /// </summary>
        internal void ClearCtrlZStack() {
            CtrlZ.Clear();
        }


        /// <summary>
        /// Reverts to the last snapshot.
        /// </summary>
        public void Undo() {

            SearchMapCore.Logger.Info("Undoing last actions...");

            if(CtrlZ.Count == 0) {
                SearchMapCore.Logger.Info("Nothing to undo!");
                return;
            }

            var snapshot = CtrlZ.Pop();

            // Take snapshot of current state and add it to CtrlY for redo.
            // TODO take snapshot of what will be reverted
            // CtrlY.Push(new Snapshot(snapshot.Graph));

            foreach(var revertable in snapshot) {
                revertable.Revert();
            }

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

            // TODO
            // CtrlZ.Push(new Snapshot(snapshot.Graph));

            foreach (var revertable in snapshot) {
                revertable.Revert();
            }

        }

    }

}
