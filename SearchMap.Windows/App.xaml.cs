using SearchMap.Windows.Rendering;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SearchMap.Windows {

    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application {

        protected override void OnStartup(StartupEventArgs args) {
            base.OnStartup(args);

            SearchMapCore.SearchMapCore.InitCore(null);
            SearchMapCore.SearchMapCore.Logger = new Logging();

        }

    }
}
