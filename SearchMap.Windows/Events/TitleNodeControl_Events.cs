using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SearchMap.Windows.UIComponents {

    public partial class TitleNodeControl {

        void RegisterEventHandlers() {

            RegisterBaseEvents();

            RegisterEventsOnChild(TitleBox);
            RegisterEventsOnChild(SubtitleBox);

            TitleBox.TextChanged += OnTitleChanged;

        }

        private void OnTitleChanged(object sender, TextChangedEventArgs e) {

            Node.Title = TitleBox.Text;

            if (TitleBox.Text == "") {
                TitleBox.Text = "Untitled";
                IsUntitled = true;
                TitleBox.FontStyle = FontStyles.Italic;
            }
            else {

                // this will throw a new TitleChanged event to update the node title.
                if (IsUntitled) TitleBox.Text = TitleBox.Text.Replace("Untitled", "");
                IsUntitled = false;

                TitleBox.FontStyle = FontStyles.Normal;

            }

        }

    }

}
