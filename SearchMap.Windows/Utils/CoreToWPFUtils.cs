using System.Windows.Media;

namespace SearchMap.Windows.Utils {

    static class CoreToWPFUtils {

        public static Color CoreColorToWPF(SearchMapCore.Rendering.Color color) {

            (byte a, byte r, byte g, byte b) = color.ToARGB();
            return Color.FromArgb(a, r, g, b);

        }

    }

}
