using System.Threading;
using System.Windows;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowLoadingBar.xaml
    /// </summary>
    public partial class WindowLoadingBar : Window
    {
        public WindowLoadingBar(string title, string text, int steps)
        {
            InitializeComponent();

            Title                   = title;
            LabelText.Content       = text;
            ProgressBar.Maximum     = steps;
        }

        public double Value
        {
            get { return ProgressBar.Value;   }
            set
            {
                ProgressBar.Value       = value;

                if (ProgressBar.Maximum != 0)
                {
                    LabelPercentage.Content     = (value / ProgressBar.Maximum * 100).ToString("0.00") + "%";
                    LabelLoadedItems.Content    = "Loaded: " + value.ToString() + " / " + ProgressBar.Maximum.ToString();
                }
            }
        }

        public double Maximum   { get { return ProgressBar.Maximum; } set { ProgressBar.Maximum = value; } }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetText(string text)
        {
            LabelText.Content = text;
        }
    }
}
