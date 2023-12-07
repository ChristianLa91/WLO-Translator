using System.Windows;
using System.Windows.Documents;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowItemInfo.xaml
    /// </summary>
    public partial class WindowOpenTextReversed : Window
    {
        public WindowOpenTextReversed(string text)
        {
            InitializeComponent();

            RichTextBoxItemData.Document.Blocks.Clear();
            RichTextBoxItemData.Document.Blocks.Add(new Paragraph(new Run(TextManager.ReverseString(text))));
            LabelCharCount.Content = text.Length.ToString();
        }

        private void RichTextBoxItemData_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LabelSelected.Content = RichTextBoxItemData.Selection.Text.Length.ToString();
        }
    }
}
