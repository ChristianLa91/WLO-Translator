using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System;

namespace WLO_Translator_WPF
{
    public class TreeViewItemMTData
    {
        public string   Name        { get; private set; }
        public FileType FileType    { get; private set; }
        public string   FilePath    { get; private set; }
        public bool?    IsChecked   { get; private set; }

        public TreeViewItemMTData(string filePath, FileType fileType, bool? isChecked)
        {
            FilePath    = filePath;
            FileType    = fileType;
            IsChecked   = isChecked;
        }
    }

    public class TreeViewItemMT : TreeViewItem
    {
        public  FileType    FileType    { get; private set; }
        private CheckBox    CheckBox    { get; set; }
        private Label       LabelPath   { get; set; }
        public  string      FilePath    { get { return LabelPath.Content as string; } private set { LabelPath.Content = value; } }
        public  bool?       IsChecked   { get { return CheckBox.IsChecked;  } set { CheckBox.IsChecked  = value; } }

        public TreeViewItemMT(string filePath, FileType fileType, RoutedEventHandler checkBoxItemMultiTranslator_Click) : base()
        {
            FileType = fileType;

            // Stretch child content to fill whole box
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            CheckBox = new CheckBox() { /*VerticalAlignment = VerticalAlignment.Center,*/ IsChecked = true };

            CheckBox.Click += checkBoxItemMultiTranslator_Click;
            CheckBox.Click += CheckBox_Click;

            LabelPath = new Label()
            {
                //Height                      = 23,
                Margin                      = new Thickness(5, 0, 0, 0),
                //VerticalAlignment           = VerticalAlignment.Center,
                HorizontalAlignment         = HorizontalAlignment.Stretch,
            };

            FilePath = filePath;

            Label labelName = new Label()
            {
                Content                     = TextManager.GetFileTypeToString(FileType),
                Margin                      = new Thickness(5, 0, 0, 0),
                //Width                       = 70,
                //Height                      = 23,
                //VerticalContentAlignment    = VerticalAlignment.Center,
                Padding                     = new Thickness(5, 0, 0, 0)
            };

            Header = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children =
                {
                    CheckBox, labelName
                }
            };

            //DockPanel.SetDock(CheckBox, Dock.Left);
            //DockPanel.SetDock(labelName, Dock.Left);
            //DockPanel.SetDock(LabelPath, Dock.Top);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in Items.OfType<Item>())
                item.IsChecked = CheckBox.IsChecked;
        }

        public TreeViewItemMTData GetDataClass()
        {
            return new TreeViewItemMTData(FilePath, FileType, IsChecked);
        }

        public void SetSelectionOfChildrenEqualToParents()
        {
            CheckBox_Click(CheckBox, null);
        }
    }
}
