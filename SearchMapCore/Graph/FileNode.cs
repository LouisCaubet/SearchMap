using System;
using System.Collections.Generic;
using System.Text;

namespace SearchMapCore.Graph {

    public class FileNode : Node {

        public string File { get; private set; }

        public FileNode(Graph graph, string file) : base(graph) {
            // Do something with the file

            File = file;

        }

        public override void OnClick() {
            OpenFile();
        }

        public bool OpenFile() {

            /* - Unzip file from smp archive to tmp
             * - Open it in appropriate software
             * - Place watchdog to save changes to smp archive
             */

            return true;
        }

    }

}
