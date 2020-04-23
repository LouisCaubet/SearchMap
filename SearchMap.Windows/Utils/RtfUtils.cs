using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SearchMap.Windows.Utils {

    static class RtfUtils {

        public static string ExtractTextFromRtfBox(RichTextBox box) {

            TextRange range = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();

            range.Save(stream, DataFormats.Rtf);

            byte[] bytes = stream.ToArray();

            return Encoding.UTF8.GetString(bytes);

        }

        public static void SetRtfBoxText(RichTextBox box, string text) {



        }

    }

}
