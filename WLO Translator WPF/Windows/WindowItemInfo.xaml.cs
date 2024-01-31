using System.Windows;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowItemInfo.xaml
    /// </summary>
    public partial class WindowItemInfo : Window
    {
        string mOriginalItemDataText;

        public WindowItemInfo(string name, string itemDataText)
        {
            InitializeComponent();

            mOriginalItemDataText   = itemDataText;
            Title                   = name;
            LabelCharCount.Content  = itemDataText.Length.ToString();
            TextManager.SetRichTextBoxText(ref RichTextBoxItemData, mOriginalItemDataText);
        }

        private void RichTextBoxItemData_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LabelSelected.Content = RichTextBoxItemData.Selection.Text.Length.ToString();
        }

        private void CheckBoxReverseShownText_Click(object sender, RoutedEventArgs e)
        {
            string text;

            if ((sender as CheckBox).IsChecked.Value)
                text = TextManager.ReverseString(mOriginalItemDataText);
            else
                text = mOriginalItemDataText;

            TextManager.SetRichTextBoxText(ref RichTextBoxItemData, text);
        }
    }
}
