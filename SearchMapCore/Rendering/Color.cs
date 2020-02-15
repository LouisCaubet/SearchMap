namespace SearchMapCore.Rendering {

    /// <summary>
    /// Represents an ARGB color in a cross-platform implementation
    /// </summary>
    public class Color {

        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        /// <summary>
        /// Creates a color from ARGB values
        /// </summary>
        public Color(byte a, byte r, byte g, byte b) {
            Alpha = a;
            Red = r;
            Green = g;
            Blue = b;
        }

        /// <summary>
        /// Creates a Color from RGB values
        /// </summary>
        public Color(byte r, byte g, byte b) {
            Alpha = 255;
            Red = r;
            Green = g;
            Blue = b;
        }

        /// <summary>
        /// Returns the ARGB-tuple representing this Color.
        /// </summary>
        /// <returns></returns>
        public (byte, byte, byte, byte) ToARGB() {
            return (Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Returns the RGB values of this Color.
        /// </summary>
        /// <returns></returns>
        public (byte, byte, byte) ToRGB() {
            return (Red, Green, Blue);
        }

    }
}
