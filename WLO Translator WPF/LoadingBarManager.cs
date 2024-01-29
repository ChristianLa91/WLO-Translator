using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WLO_Translator_WPF
{
    static class LoadingBarManager
    {
        private static WindowLoadingBar mWindowLoadingBar;
        private static double mValue;
        public  static double Value     { get { return mValue;      } set { mWindowLoadingBar.Value     = mValue    = value; } }
        private static double mMaximum;
        public  static double Maximum   { get { return mMaximum;    } set { mWindowLoadingBar.Maximum   = mMaximum  = value; } }
        private static bool   mIsValueChanged = false;
        public  static bool   IsValueChanged
        {
            get
            {
                if (mIsValueChanged)
                {
                    mIsValueChanged = false;
                    return true;
                }
                else
                    return false;
            }
        }

        public static void ShowOrInitializeLoadingBar(string type, double value, FileItemProperties fileItemProperties,
            double? maximum = null, bool isTranslating = false)
        {
            // Open loading bar window if it isn't already open
            string action = "Load";
            if (isTranslating)
                action = "Translat";
            string loadingBarText = action + "ing " + type + " "
                + TextManager.GetFileTypeToString(fileItemProperties.FileType) + " Data";

            double steps = 1;
            if (maximum != null)
                steps = maximum.Value;

            if (mWindowLoadingBar == null)
            {
                mWindowLoadingBar = new WindowLoadingBar(action + "ing Data", loadingBarText, "", steps,
                    ProgressBar_ValueChanged, ProgressBar_LayoutUpdated);
                mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                mWindowLoadingBar.Owner = Application.Current.MainWindow;
                _ = Task.Run(() => { Application.Current.Dispatcher.Invoke(() => { mWindowLoadingBar.ShowDialog(); }); });
            }
            else
                mWindowLoadingBar.LabelText.Content = loadingBarText;

            Maximum = steps;
            Value   = value;            
        }

        public static bool IsLoadingBarNull()
        {
            if (mWindowLoadingBar == null)
                return true;
            else
                return false;
        }

        public static bool IsLoadingBarActive()
        {
            return mWindowLoadingBar.IsActive;
        }

        public static bool? GetDialogResult()
        {
            return mWindowLoadingBar.DialogResult;
        }

        public static void CloseLoadingBar()
        {
            if (mWindowLoadingBar == null)
                return;

            // Close loading bar
            if (!IsLoadingBarNull())
            {
                mWindowLoadingBar.Close();
                mWindowLoadingBar = null;
            }
        }

        public static bool IsLoadingCompleted()
        {
            return mValue >= mMaximum;
        }

        private static void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mIsValueChanged = true;
        }

        private static void ProgressBar_LayoutUpdated(object sender, EventArgs e)
        {
            mIsValueChanged = true;
        }

        // Wait until the value of the progress bar have been updated
        public static void WaitUntilValueHasChanged()
        {
            while (!IsValueChanged)
                Thread.Sleep(100);
        }
    }
}
