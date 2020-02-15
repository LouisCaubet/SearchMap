using SearchMapCore.Rendering;
using System;

namespace SearchMap.Windows.Rendering {

    class Logging : ILogging {

        public const bool DEBUG = true;

        public void Debug(string log) {
            if (DEBUG) {
                Console.WriteLine("[" + DateTime.Now.ToString() + "][DEBUG] " + log);
            }
        }

        public void Error(string log) {
            Console.WriteLine("[" + DateTime.Now.ToString() + "][ERROR] " + log);
        }

        public void Info(string log) {
            Console.WriteLine("[" + DateTime.Now.ToString() + "][INFO] " + log);
        }

        public void Warning(string log) {
            Console.WriteLine("[" + DateTime.Now.ToString() + "][WARNING] " + log);
        }
    }

}
