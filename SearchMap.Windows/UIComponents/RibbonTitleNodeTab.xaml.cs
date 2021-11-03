using Fluent;
using SearchMap.Windows.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SearchMap.Windows.UIComponents {

    /// <summary>
    /// Logique d'interaction pour RibbonTitleNodeTab.xaml
    /// </summary>
    public partial class RibbonTitleNodeTab : RibbonNodeTab {

        public const int TAB_INDEX = 7;

        public RibbonTitleNodeTab() {
            InitializeComponent();

            InitTabBase();
        }

        // Required to access XAML fields from superclass.

        protected override Fluent.ComboBox GetFontComboBox() {
            return FontComboBox;
        }

        protected override Fluent.ComboBox GetFontSizeComboBox() {
            return comboBoxFontSize;
        }

        protected override DropDownButton GetTextHighlightButton() {
            return buttonTextHighlightColor;
        }

        protected override DropDownButton GetFontColorButton() {
            return buttonFontColor;
        }

        protected override ToggleButton GetBoldButton() {
            return buttonBold;
        }

        protected override ToggleButton GetItalicButton() {
            return buttonItalic;
        }

        protected override ToggleButton GetUnderlineButton() {
            return buttonUnderline;
        }

        protected override ToggleButton GetStrikethroughButton() {
            return buttonStrikethrough;
        }

        protected override Fluent.Button GetClearFormattingButton() {
            return buttonClearFormatting;
        }

        protected override Fluent.Button GetGrowFontButton() {
            return buttonGrowFont;
        }

        protected override Fluent.Button GetShrinkFontButton() {
            return buttonShrinkFont;
        }

        protected override Fluent.Button GetDeleteNodeButton() {
            return DeleteNodeButton;
        }

        protected override Fluent.Button GetZoomOnNodeButton() {
            return ZoomOnNodeButton;
        }

        protected override ColorGallery GetHighlightColorSelector() {
            return HighlightColorSelector;
        }

        protected override ColorGallery GetFontColorSelector() {
            return FontColorSelector;
        }

        protected override ColorGallery GetBorderColorSelector() {
            return BorderColorSelector;
        }

        protected override ColorGallery GetBackgroundColorSelector() {
            return BackgroundColorSelector;
        }

    }

}
