using System.Collections.Generic;

namespace SearchMap.Windows.Utils {

    /// <summary>
    /// Contains useful math methods.
    /// </summary>
    static class MathUtils {

        /// <summary>
        /// Returns the largest element of a double list.
        /// </summary>
        /// <returns></returns>
        public static double Max(IEnumerable<double> list) {

            double max = double.NegativeInfinity;

            foreach (double d in list) {
                if (d > max) {
                    max = d;
                }
            }

            return max;

        }

        /// <summary>
        /// Returns the smallest element of a double list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double Min(IEnumerable<double> list) {

            double min = double.PositiveInfinity;

            foreach (double d in list) {
                if (d < min) {
                    min = d;
                }
            }

            return min;

        }
    }

}
