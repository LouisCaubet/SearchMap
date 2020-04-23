using SearchMap.Windows.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SearchMap.Windows.Controls {

    partial class NodeControl {

        /// <summary>
        /// Saves the last object of this node which had keyboard focus.
        /// </summary>
        protected FrameworkElement LastObjectWithKeyboardFocus { get; set; }

        protected FrameworkElement LastNotNullObjectWithKeyboardFocus { get; set; }

        /// <summary>
        /// Sets the value of LastObjectWithKeyboardFocus. Should only be called from the NodeControl base class.
        /// </summary>
        protected abstract void SetObjectWithLastKeyboardFocus();

        /// <summary>
        /// Indicates whether the currently selected text in this node is in bold font.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSelectionBold() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return false;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.FontWeight == FontWeights.Black;
            }
            else if(LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return rtb.Selection.GetPropertyValue(Inline.FontWeightProperty).Equals(FontWeights.Bold);
            }
            else {
                return false;
            }

        }

        /// <summary>
        /// Indicates whether the currently selected text in this node is in italic font.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSelectionItalic() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return false;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.FontStyle == FontStyles.Italic;
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return rtb.Selection.GetPropertyValue(Inline.FontStyleProperty).Equals(FontStyles.Italic);
            }
            else {
                return false;
            }

        }

        /// <summary>
        /// Indicates whether the currently selected text in this node is underlined.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSelectionUnderlined() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return false;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.TextDecorations == TextDecorations.Underline;
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty).Equals(TextDecorations.Underline);
            }
            else {
                return false;
            }

        }

        /// <summary>
        /// Indicates whether the currently selected text in this node is striked through.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSelectionStrikedtrough() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return false;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.TextDecorations == TextDecorations.Strikethrough;
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty).Equals(TextDecorations.Strikethrough);
            }
            else {
                return false;
            }

        }

        /// <summary>
        /// Returns the Source of the FontFamily of the current selection in this node.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSelectionFont() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return "";

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.FontFamily.Source;
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return ((FontFamily) rtb.Selection.GetPropertyValue(Inline.FontFamilyProperty)).Source;
            }
            else {
                return "";
            }

        }

        /// <summary>
        /// Returns the font size of the currently selected text in this node.
        /// </summary>
        /// <returns></returns>
        public virtual double GetSelectionFontSize() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return -1;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                return textbox.FontSize;
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                return (double) rtb.Selection.GetPropertyValue(Inline.FontSizeProperty);
            }
            else {
                return -1;
            }

        }

        /// <summary>
        /// Toggles bold/normal font on the selected text in this node.
        /// </summary>
        public virtual void ToggleSelectionBold() {

            if (LastObjectWithKeyboardFocus == null) return;
          
            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = LastObjectWithKeyboardFocus as TextBox;
                
                if (textbox.FontWeight == FontWeights.Black) {
                    textbox.FontWeight = FontWeights.Normal;
                }
                else {
                    textbox.FontWeight = FontWeights.Black;
                }
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                if (rtb.Selection.GetPropertyValue(Inline.FontWeightProperty).Equals(FontWeights.Normal)) {
                    rtb.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
                }
                else {
                    rtb.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
                }
            }

        }

        /// <summary>
        /// Toggles italic/normal on the selected text in this node.
        /// </summary>
        public virtual void ToggleSelectionItalic() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                if (textbox.FontStyle == FontStyles.Italic) {
                    textbox.FontStyle = FontStyles.Normal;
                }
                else {
                    textbox.FontStyle = FontStyles.Italic;
                }
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                if (rtb.Selection.GetPropertyValue(Inline.FontStyleProperty).Equals(FontStyles.Normal)) {
                    rtb.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
                }
                else {
                    rtb.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
                }
            }

        }

        /// <summary>
        /// Toggles underline on the selected text in this node.
        /// </summary>
        public virtual void ToggleSelectionUnderline() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                if (textbox.TextDecorations == TextDecorations.Underline) {
                    textbox.TextDecorations = null;
                }
                else {
                    textbox.TextDecorations = TextDecorations.Underline;
                }
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                if (!rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty).Equals(TextDecorations.Underline)) {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                }
                else {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
            }

        }

        /// <summary>
        /// Toggle striketrough on the selected text in this node.
        /// </summary>
        public virtual void ToggleSelectionStriketrough() {

            SetObjectWithLastKeyboardFocus();
            if (LastObjectWithKeyboardFocus == null) return;

            if (LastObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastObjectWithKeyboardFocus;
                if (textbox.TextDecorations == TextDecorations.Strikethrough) {
                    textbox.TextDecorations = null;
                }
                else {
                    textbox.TextDecorations = TextDecorations.Strikethrough;
                }
            }
            else if (LastObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastObjectWithKeyboardFocus;
                if (rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty).Equals(null)) {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                }
                else {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
            }

        }

        /// <summary>
        /// Sets the font of the selection to the FontFamily with the given source.
        /// </summary>
        /// <param name="name"></param>
        public virtual void SetSelectionFont(string name) {

            if (LastNotNullObjectWithKeyboardFocus == null) return;

            FontFamily font = new FontFamily(name);

            if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastNotNullObjectWithKeyboardFocus;
                textbox.FontFamily = font;
            }
            else if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastNotNullObjectWithKeyboardFocus;
                rtb.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, font);
            }

            Keyboard.Focus(LastNotNullObjectWithKeyboardFocus);

        }

        /// <summary>
        /// Sets the font size of the selection to the given size.
        /// </summary>
        /// <param name="size"></param>
        public virtual void SetSelectionFontSize(double size) {

            if (LastNotNullObjectWithKeyboardFocus == null) return;

            if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastNotNullObjectWithKeyboardFocus;
                textbox.FontSize = size;
            }
            else if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastNotNullObjectWithKeyboardFocus;
                rtb.Selection.ApplyPropertyValue(Inline.FontSizeProperty, size);
            }

            Keyboard.Focus(LastNotNullObjectWithKeyboardFocus);

        }

        /// <summary>
        /// Reverts all user changes on the selected text.
        /// </summary>
        public abstract void RemoveFormattingOnSelection();

        /// <summary>
        /// Sets the font color of the selection to the given color.
        /// </summary>
        /// <param name="color"></param>
        public virtual void SetSelectionColor(Color color) {

            if (LastNotNullObjectWithKeyboardFocus == null) return;

            if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastNotNullObjectWithKeyboardFocus;
                textbox.Foreground = new SolidColorBrush(color);
            }
            else if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastNotNullObjectWithKeyboardFocus;
                rtb.Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(color));
            }

            Keyboard.Focus(LastNotNullObjectWithKeyboardFocus);

        }

        /// <summary>
        /// Sets the highlight of the selection to the given color.
        /// </summary>
        /// <param name="color"></param>
        public virtual void SetSelectionHighlight(Color color) {

            if (LastNotNullObjectWithKeyboardFocus == null) return;

            if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(TextBox)) {
                var textbox = (TextBox) LastNotNullObjectWithKeyboardFocus;
                textbox.Background = new SolidColorBrush(color);
            }
            else if (LastNotNullObjectWithKeyboardFocus.GetType() == typeof(RichTextBox)) {
                var rtb = (RichTextBox) LastNotNullObjectWithKeyboardFocus;
                rtb.Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(color));
            }

            Keyboard.Focus(LastNotNullObjectWithKeyboardFocus);

        }

        // UTILS

        /// <summary>
        /// Converts the font of the TextBox to a SearchMapCore TextFont object.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        protected static SearchMapCore.Rendering.TextFont GetTextFontFromTextBox(TextBox box) {

            try {

                return new SearchMapCore.Rendering.TextFont() {
                    FontName = box.FontFamily.Source,
                    FontSize = box.FontSize,
                    IsBold = box.FontWeight == FontWeights.Black,
                    IsItalic = box.FontStyle == FontStyles.Italic,
                    IsUnderlined = box.TextDecorations == TextDecorations.Underline,
                    IsStrikedthrough = box.TextDecorations == TextDecorations.Strikethrough,
                    Color = CoreToWPFUtils.WPFColorToCore(((SolidColorBrush) box.Foreground).Color),
                    HighlightColor = CoreToWPFUtils.WPFColorToCore(((SolidColorBrush) box.Background).Color)
                };

            }
            catch (InvalidCastException) {
                throw new ArgumentException("The given TextBox's Foreground/Background is not a SolidColorBrush.");
            }

        }

        /// <summary>
        /// Applies the given SearchMapCore TextFont to the given TextBox.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="font"></param>
        protected static void ApplyTextFontToTextBox(TextBox box, SearchMapCore.Rendering.TextFont font) {

            if(font == null) {
                SearchMapCore.SearchMapCore.Logger.Error("Tried to apply font null to a TextBox.");
                SearchMapCore.SearchMapCore.Logger.Debug("ArgumentNullException encountered in method ApplyTextFontToTextBox, in class NodeControl");
                return;
            }

            box.FontFamily =  new FontFamily(font.FontName);
            box.FontSize = font.FontSize;
            box.FontWeight = font.IsBold ? FontWeights.Black : FontWeights.Normal;
            box.FontStyle = font.IsItalic ? FontStyles.Italic : FontStyles.Normal;

            if (font.IsUnderlined) {
                box.TextDecorations = TextDecorations.Underline;
            }
            else if (font.IsStrikedthrough) {
                box.TextDecorations = TextDecorations.Strikethrough;
            }
            else {
                box.TextDecorations = null;
            }

            box.Foreground = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(font.Color));
            box.Background = new SolidColorBrush(CoreToWPFUtils.CoreColorToWPF(font.HighlightColor));

        }

    }

}

