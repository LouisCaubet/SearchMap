using SearchMapCore.Graph;
using SearchMapCore.Rendering;
using System.Collections.Generic;

namespace SearchMapCore.Serialization {

    public class SerializableConnection {

        public List<Location> Points { get; }
        public List<Location> UserImposedPoints { get; }
        public Color Color { get; }
        public Color ShadowColor { get; }
        public bool IsBoldStyle { get; }

        public int NodeFromId { get; }
        public int NodeToId { get; }

        public bool IsCustomizedByUser { get; }

        public SerializableConnection(Connection conn) {

            Points = conn.Points;
            UserImposedPoints = conn.UserImposedPoints;
            Color = conn.Color;
            ShadowColor = conn.ShadowColor;
            IsBoldStyle = conn.IsBoldStyle;

            NodeFromId = conn.NodeFromId;
            NodeToId = conn.NodeToId;

            IsCustomizedByUser = conn.IsCustomizedByUser;

        }

        public void RevertConnectionToThis(Connection conn) {

            conn.Points = Points;
            conn.UserImposedPoints = UserImposedPoints;
            conn.Color = Color;
            conn.ShadowColor = conn.ShadowColor;
            conn.IsBoldStyle = IsBoldStyle;
            conn.IsCustomizedByUser = IsCustomizedByUser;

        }

    }

}
