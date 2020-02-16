using Newtonsoft.Json;
using SearchMapCore.Graph;
using System;

namespace SearchMapCore.Serialization {

    public class SerializableWebNode : SerializableNode {

        public Uri Uri { get; set; }

        public SerializableWebNode(WebNode node) : base(node) {
            Uri = node.Uri;
        }

        [JsonConstructor]
        public SerializableWebNode() : base() { }

    }

}
