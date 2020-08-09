using SearchMap.Windows.Controls;
using SearchMap.Windows.Utils;
using SearchMapCore.Graph;
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
    /// Logique d'interaction pour TitleNodeControl.xaml
    /// </summary>
    public partial class TitleNodeControl : NodeControl {

        private bool IsUntitled { get; set; }

        public TitleNodeControl(TitleNode node): base(node) {
            InitializeComponent();

            // Put this in front of connectors.
            Panel.SetZIndex(this, 10);

            RegisterEventHandlers();

            Refresh();

        }

        public TitleNode GetTitleNode() {
            return (TitleNode) Node;
        }

        public override void CollapseAssociatedRibbonTab() {
            throw new NotImplementedException();
        }

        public override FrameworkElement GetBack() {
            return ContentGrid;
        }

        public override FrameworkElement GetFront() {
            return ContentGrid;
        }

        public override bool IsFlippable() {
            return false;
        }

        public override void Refresh() {

            Border.Background = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.Color));
            Border.BorderBrush = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(Node.BorderColor));

            TitleBox.Text = GetTitleNode().Title;
            SubtitleBox.Text = GetTitleNode().Subtitle;

            ApplyTextFontToTextBox(TitleBox, GetTitleNode().TitleFont);
            ApplyTextFontToTextBox(SubtitleBox, GetTitleNode().SubtitleFont);

            // Prevent empty text boxes.
            if (TitleBox.Text == "") {
                TitleBox.Text = "Untitled";
                IsUntitled = true;
                TitleBox.FontStyle = FontStyles.Italic;
            }
            else {
                IsUntitled = false;
                TitleBox.FontStyle = FontStyles.Normal;
            }

            Height = Node.Height;
            Width = Node.Width;

        }

        public override void RemoveFormattingOnSelection() {

            if (TitleBox.Equals(LastObjectWithKeyboardFocus)) {

                GetTitleNode().TitleFont = SearchMapCore.Rendering.TextFont.DefaultFrontTitleFont();
                GetTitleNode().TitleFont.Color = SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color);
                ApplyTextFontToTextBox(TitleBox, GetTitleNode().TitleFont);

            }
            else if (SubtitleBox.Equals(LastObjectWithKeyboardFocus)) {

                GetTitleNode().SubtitleFont = SearchMapCore.Rendering.TextFont.DefaultSubtitleFont();
                GetTitleNode().SubtitleFont.Color = SearchMapCore.Rendering.TextFont.GetDefaultColorOnBackground(Node.Color);
                ApplyTextFontToTextBox(SubtitleBox, GetTitleNode().SubtitleFont);

            }

        }

        public override void Save() {

            Node.TakeSnapshot();

            GetTitleNode().Title = TitleBox.Text;
            GetTitleNode().Subtitle = SubtitleBox.Text;

            GetTitleNode().TitleFont = GetTextFontFromTextBox(TitleBox);
            GetTitleNode().SubtitleFont = GetTextFontFromTextBox(SubtitleBox);

        }

        public override void ShowAssociatedRibbonTab(bool setSelected) {
            throw new NotImplementedException();
        }

        protected override void SetObjectWithLastKeyboardFocus() {

            if (TitleBox.IsKeyboardFocusWithin) {
                LastNotNullObjectWithKeyboardFocus = TitleBox;
                LastObjectWithKeyboardFocus = TitleBox;
            }
            else if (SubtitleBox.IsKeyboardFocusWithin) {
                LastNotNullObjectWithKeyboardFocus = SubtitleBox;
                LastObjectWithKeyboardFocus = SubtitleBox;
            }
            else {
                LastObjectWithKeyboardFocus = null;
            }

        }
    }
}
