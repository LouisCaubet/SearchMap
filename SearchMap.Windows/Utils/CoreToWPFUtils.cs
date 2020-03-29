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
        /// Returns the associated Node if the given UserControl represents a node. <para/>
        /// Throws ArgumentException elsewise.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static Node GetNodeFromNodeControl(UserControl control) {

            if(control == null) {
                throw new ArgumentNullException();
            }

            if(control.GetType() == typeof(WebNodeControl)) {
                return ((WebNodeControl) control).Node;
            }

            throw new ArgumentException("The given UserControl does not represent a Node");

        }

    }

}
