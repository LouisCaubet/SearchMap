using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SearchMap.Windows.Dialog {

    /// <summary>
    /// Logique d'interaction pour NewWebNodeDialog.xaml
    /// </summary>
    public partial class NewWebNodeDialog : Window {

        public NewWebNodeDialog() {
            InitializeComponent();

            MouseLeftButtonDown += OnMouseDown;

        }

        void OnMouseDown(object sender, MouseButtonEventArgs e) {

            if (!Content1.IsMouseOver && !Content2.IsMouseOver) {
                this.DragMove();
            }

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e) {
            // TODO create new webnode with given values.
            this.Close();
        }
    }

}
