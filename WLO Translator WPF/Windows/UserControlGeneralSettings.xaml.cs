using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for UserControlGeneralSettings.xaml
    /// </summary>
    public partial class UserControlGeneralSettings : UserControl
    {
        public int      OldThemeIndex { get; set; } = -1;
        private bool    mInitializing = true;

        public UserControlGeneralSettings()
        {
            InitializeComponent();

            OldThemeIndex = ThemeManager.ThemeIndex;
            ThemeManager.SetThemesToComboBox(ref ComboBoxThemes);
            ComboBoxThemes.SelectedIndex = OldThemeIndex;
            mInitializing = false;
        }

        //private void ComboBoxThemes_SelectedIndexChanged(object sender, System.EventArgs e)
        private void ComboBoxThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mInitializing == false)
            {
                ThemeManager.ApplyTheme(((ComboBox)sender).SelectedIndex);
            }
        }
    }
}
