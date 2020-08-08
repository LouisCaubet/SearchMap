using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Graph {

    /// <summary>
    /// Represents a title node, with a big title and a smaller subtitle.
    /// </summary>
    public class TitleNode : Node {

        /// <summary>
        /// The subtitle of this TitleNode.
        /// </summary>
        public string Subtitle { get; set; }

        public TextFont TitleFont { get; set; }
        public TextFont SubtitleFont { get; set; }

        public TitleNode(Graph graph) : base(graph) { }

        public override void OnClick() {
            // nothing to do
            return;
        }

    }

    /// <summary>
    /// Represents a node with only text.
    /// </summary>
    public class TextNode : Node {

        public TextNode(Graph graph) : base(graph) { }

        public override void OnClick() {
            return;
        }
    }

}
