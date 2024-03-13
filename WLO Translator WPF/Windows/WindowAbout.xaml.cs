﻿using System.Windows;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for WindowAbout.xaml
    /// </summary>
    public partial class WindowAbout : Window
    {
        public WindowAbout(string applicationVersion)
        {
            InitializeComponent();

            LabelVersion.Content = "Version " + System.IO.Path.GetFileNameWithoutExtension(applicationVersion);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
