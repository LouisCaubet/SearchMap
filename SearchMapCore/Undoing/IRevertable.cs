using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Undoing {

    /// <summary>
    /// Represents a state which can be reverted to by the Undo/Redo system.
    /// </summary>
    interface IRevertable {

        /// <summary>
        /// Reference to the graph this state was taken from.
        /// </summary>
        Graph.Graph Graph { get; set; }

        /// <summary>
        /// Revert Graph to this state.
        /// </summary>
        void Revert();

    }

}
