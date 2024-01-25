using System.Windows;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    public class ListBoxItemMTData
    {
        public string   Name        { get; private set; }
        public FileType FileType    { get; private set; }
        public string   FilePath    { get; private set; }
        public bool?    IsChecked   { get; private set; }

        public ListBoxItemMTData(string name, FileType fileType, string filePath, bool? isChecked)
        {
            Name        = name;
            FileType    = fileType;
            FilePath    = filePath;
            IsChecked   = isChecked;
        }
    }

    public class ListBoxItemMT : ListBoxItem
    {
        private string      mName;
        public  FileType    FileType    { get; private set; }
        private CheckBox    CheckBox    { get; set; }
        private TextBox     TextBoxPath { get; set; }
        public  string      FilePath    { get { return TextBoxPath.Text;    } set { TextBoxPath.Text    = value; } }
        public  bool?       IsChecked   { get { return CheckBox.IsChecked;  } set { CheckBox.IsChecked  = value; } }

        public ListBoxItemMT(string name, FileType fileType, RoutedEventHandler checkBoxItemMultiTranslator_Click,
            RoutedEventHandler routedEventHandlerButtonClick = null) : base()
        {
            mName       = name;
            FileType    = fileType;

            // Stretch child content to fill whole box
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            CheckBox = new CheckBox() { VerticalAlignment = VerticalAlignment.Center, IsChecked = true };

            CheckBox.Click += checkBoxItemMultiTranslator_Click;

            TextBoxPath = new TextBox()
            {
                Height                      = 23,
                Margin                      = new Thickness(5, 0, 0, 0),
                VerticalAlignment           = VerticalAlignment.Center,
                HorizontalAlignment         = HorizontalAlignment.Stretch
            };

            if (routedEventHandlerButtonClick == null)
                TextBoxPath.IsReadOnly = true;

            Label labelName = new Label()
            {
                Content                     = mName,
                Margin                      = new Thickness(5, 0, 0, 0),
                Width                       = 70,
                Height                      = 23,
                VerticalContentAlignment    = VerticalAlignment.Center,
                Padding                     = new Thickness(5, 0, 0, 0)
            };

            Content = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    CheckBox, labelName,
                    TextBoxPath
                }
            };

            DockPanel.SetDock(CheckBox, Dock.Left);
            DockPanel.SetDock(labelName, Dock.Left);
            DockPanel.SetDock(TextBoxPath, Dock.Top);

            if (routedEventHandlerButtonClick != null)
            {
                Button button = new Button()
                {
                    Content             = "Open Select",
                    Margin              = new Thickness(5, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Width               = 75,
                    Height              = 23,
                    Tag                 = this
                };
                button.Click += routedEventHandlerButtonClick;
                (Content as DockPanel).Children.Insert((Content as DockPanel).Children.Count - 2, button);

                DockPanel.SetDock(button, Dock.Right);
            }
        }

        public ListBoxItemMTData GetDataClass()
        {
            return new ListBoxItemMTData(mName, FileType, FilePath, IsChecked);
        }
    }
}
