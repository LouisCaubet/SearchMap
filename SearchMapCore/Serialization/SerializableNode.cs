using Newtonsoft.Json;
using SearchMapCore.Graph;
using SearchMapCore.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Serialization {

    public abstract class SerializableNode {

        public Color Color { get; set; }
        public Color BorderColor { get; set; }
        public string Title { get; set; }
        public byte[] Comment { get; set; }
        public string Icon { get; set; }
        public string AssociatedFile { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Location Location { get; set; }

        public SerializableNode(Node node) {

            Color = node.Color;
            BorderColor = node.BorderColor;
            Title = node.Title;
            Comment = node.Comment;
            Icon = node.Icon;
            AssociatedFile = node.AssociatedFile;
            Height = node.Height;
            Width = node.Width;
            Location = node.Location;

        }

        public SerializableNode() { }

    }

}
