using System.Windows;
using System.Windows.Documents;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowItemInfo.xaml
    /// </summary>
    public partial class WindowItemInfo : Window
    {
        public WindowItemInfo(string name, string itemData)
        {
            InitializeComponent();

            Title = "Item Info for " + name;
            RichTextBoxItemData.Document.Blocks.Clear();
            RichTextBoxItemData.Document.Blocks.Add(new Paragraph(new Run(itemData)));
            LabelCharCount.Content = itemData.Length.ToString();
        }

        private void RichTextBoxItemData_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LabelSelected.Content = RichTextBoxItemData.Selection.Text.Length.ToString();
        }
    }
}
