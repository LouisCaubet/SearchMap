namespace SearchMapCore.Rendering {

    /// <summary>
    /// Stores font info for a text.
    /// </summary>
    public class TextFont {

        public string FontName { get; set; }
        public double FontSize { get; set; }

        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderlined { get; set; }
        public bool IsStrikedthrough { get; set; }

        public Color Color { get; set; }
        public Color HighlightColor { get; set; }


        public static TextFont DefaultFrontTitleFont() {
            return new TextFont() {
                FontName = "Segoe UI",
                FontSize = 40,
                IsBold = true,
                IsItalic = false,
                IsUnderlined = false,
                IsStrikedthrough = false,
                HighlightColor = new Color(0, 0, 0, 0)
            };
        }

        public static TextFont DefaultBackTitleFont (){
            return new TextFont() {
                FontName = "Segoe UI",
                FontSize = 30,
                IsBold = true,
                IsItalic = false,
                IsUnderlined = false,
                IsStrikedthrough = false,
                HighlightColor = new Color(0, 0, 0, 0)
            };
        }

        // Source : https://stackoverflow.com/questions/50540301/c-sharp-get-good-color-for-label
        public static Color GetDefaultColorOnBackground(Color background) {
            float brightness = (background.Red * 0.299f + background.Green * 0.587f + background.Blue * 0.114f) / 256f;
            return brightness < 0.55 ? new Color(255, 255, 255) : new Color(0, 0, 0);
        }

    }

}
