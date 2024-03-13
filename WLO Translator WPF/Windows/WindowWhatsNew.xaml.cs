using System.Windows;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowWhatsNew.xaml
    /// </summary>
    public partial class WindowWhatsNew : Window
    {
        public WindowWhatsNew()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
