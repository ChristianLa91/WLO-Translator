using System;
using System.Windows;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowProperties.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        //private UserControlSettingsGeneral          mUserControlSettingsGeneral;
        //private UserControlSettingsPaths            mUserControlSettingsPaths;
        private UserControlGeneralSettings  mUserControlGeneralSettings;
        private UserControl                 mUserControlLastUsed;

        public  bool                        SettingsChangedFlag { get; set; } = false;

        // Old values
        private string  mWLOProgramPathOld;
        private string  mWLOUpdateProgramPathOld;
        private bool    mUseWLOProgramFolderForUpdateProgramOld;
        private int     mThemeIndexOld;

        public WindowSettings(/*string WLOProgramPath, string WLOUpdateProgramPath, bool useWLOProgramFolderForUpdateProgram*/)
        {
            InitializeComponent();

            // Store old values to check against if save is necessary
            //mWLOProgramPathOld                      = WLOProgramPath;
            //mWLOUpdateProgramPathOld                = WLOUpdateProgramPath;
            //mUseWLOProgramFolderForUpdateProgramOld = useWLOProgramFolderForUpdateProgram;
            mThemeIndexOld                          = ThemeManager.ThemeIndex;

            //mUserControlSettingsGeneral     = new UserControlSettingsGeneral();
            //mUserControlSettingsPaths       = new UserControlSettingsPaths(WLOProgramPath, WLOUpdateProgramPath, useWLOProgramFolderForUpdateProgram);
            mUserControlGeneralSettings  = new UserControlGeneralSettings(/*sleepLengthShort, sleepLengthLong,
                                                                          timeBeforeRetry, retryTimes, timeBetweenClientStarts,
                                                                          lengthFromScreenSidesH, lengthFromScreenSidesV*/);
            //InitializeUserControl(mUserControlSettingsGeneral);
            //InitializeUserControl(mUserControlSettingsPaths);
            InitializeUserControl(mUserControlGeneralSettings);
            mUserControlLastUsed            = mUserControlGeneralSettings;
            mUserControlLastUsed.IsEnabled  = true;
            mUserControlLastUsed.Visibility = Visibility.Visible;
        }

        private void InitializeUserControl(UserControl userControl)
        {
            GridSettings.Children.Add(userControl);
            userControl.VerticalAlignment   = VerticalAlignment.Top;
            userControl.Visibility          = Visibility.Hidden;
        }
        
        private void ButtonOK_Click(object sender, System.EventArgs e)
        {
            // Keep Changes
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            // Discard Changes
            if ((DialogResult == null || DialogResult == false) && mUserControlGeneralSettings.OldThemeIndex != -1)
                ThemeManager.ApplyTheme(mUserControlGeneralSettings.OldThemeIndex);
        }

        private void TreeViewItemSettings_Selected(object sender, RoutedEventArgs e)
        {
            switch ((sender as TreeViewItem).Header.ToString())
            {
                case "General":
                    SetUserControl(mUserControlGeneralSettings);
                    break;
                case "Paths":
                    //SetUserControl(mUserControlSettingsPaths);
                    break;
                case "Client Starter":
                    //SetUserControl(mUserControlClientStarter);
                    break;
            }
        }

        private void SetUserControl(UserControl userControl)
        {
            mUserControlLastUsed.IsEnabled  = false;
            mUserControlLastUsed.Visibility = Visibility.Hidden;
            userControl.IsEnabled           = true;
            userControl.Visibility          = Visibility.Visible;
            mUserControlLastUsed            = userControl;
        }

        public string GetWLOProgramPath()
        {
            return null;//mUserControlSettingsPaths.TextBoxWLOProgramPath.Text;
        }

        public string GetWLOUpdateProgramPath()
        {
            return null;//mUserControlSettingsPaths.TextBoxWLOUpdateProgramPath.Text;
        }

        public bool GetUseWLOProgramFolderForUpdateProgram()
        {
            return false;//mUserControlSettingsPaths.CheckBoxSameFolder.IsChecked.Value;
        }

        public bool SettingsChanged()
        {
            if (GetUseWLOProgramFolderForUpdateProgram()    == mUseWLOProgramFolderForUpdateProgramOld &&
                GetWLOProgramPath()                         == mWLOProgramPathOld && 
                GetWLOUpdateProgramPath()                   == mWLOUpdateProgramPathOld &&
                ThemeManager.ThemeIndex                     == mThemeIndexOld)
                return false;
            else
                return true;
        }

        private void TextBoxSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}
