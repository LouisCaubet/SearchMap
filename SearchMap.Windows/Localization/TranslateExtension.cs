using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SearchMap.Windows.Localization {

    [MarkupExtensionReturnType(typeof(object))]
    [ContentProperty("Key")]
    public class TranslateExtension : MarkupExtension {

        public string Key { get; set; }

        readonly CultureInfo ci;

        static readonly Lazy<ResourceManager> ResMgr = new Lazy<ResourceManager>(
            () => new ResourceManager("SearchMap.Windows.Localization.Resources", IntrospectionExtensions.GetTypeInfo(typeof(TranslateExtension)).Assembly));

        public TranslateExtension() {
            // TODO get culture.
            ci = new CultureInfo("en-us");
        }

        public TranslateExtension(string key) : this() {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            if (Key == null) return null;
            var translation = ResMgr.Value.GetString(Key, ci);

            if (translation == null) translation = "#" + Key;

            return translation;

        }

    }

}

