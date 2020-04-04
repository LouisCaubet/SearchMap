using SearchMap.Windows.UIComponents;
using SearchMapCore.Graph;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace SearchMap.Windows.Utils {

    static class CoreToWPFUtils {

        /// <summary>
        /// Converts a Color (SearchMapCore.Rendering) to a Color (Windows)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color CoreColorToWPF(SearchMapCore.Rendering.Color color) {

            (byte a, byte r, byte g, byte b) = color.ToARGB();
            return Color.FromArgb(a, r, g, b);

        }

        /// <summary>
        /// Converts a Color (Windows) to a Color (SearchMapCore.Rendering)
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static SearchMapCore.Rendering.Color WPFColorToCore(Color color) {
            return new SearchMapCore.Rendering.Color(color.A, color.R, color.G, color.B);
        }

    }

}
