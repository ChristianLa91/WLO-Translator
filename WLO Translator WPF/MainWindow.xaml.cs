using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ScintillaNET.WPF;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog          mOpenFileDialog;

        private FileManager             mFileManager;
        //private JoinableTaskContext     mJoinableTaskContext;
        //private JoinableTaskCollection  mJoinableTaskCollection;
        //private JoinableTaskFactory     mJoinableTaskFactory;

        public MainWindow()
        {
            InitializeComponent();

            //FontFamily unicodeFont = new FontFamily("SimHei");

            //scintillaTextBox.FontFamily = unicodeFont;
            EditingCommands.ToggleInsert.Execute(null, scintillaTextBox);
            //scintillaTextBox.Scintilla.Width = 500;

            mFileManager = new FileManager(Dispatcher, ref scintillaTextBox, ref ListBoxFoundItems, ref ListBoxStoredItems,
                ButtonUpdateItemBasedOnNameLength_Click, ButtonJumpToWholeItem_Click, ButtonJumpToID_Click, ButtonJumpToName_Click,
                ButtonJumpToDescription_Click);
        }

        private void ButtonOpenFileToTranslate_Click(object sender, RoutedEventArgs e)
        {
            mOpenFileDialog = new OpenFileDialog();

            if (mOpenFileDialog.ShowDialog() == true)
            {
                LabelFileToTranslatePath.Content = mOpenFileDialog.FileName;
                mFileManager.OpenFileToTranslate(mOpenFileDialog.FileName);
            }
        }

        private void ButtonUpdateItemBasedOnNameLength_Click(object sender, RoutedEventArgs e)
        {
            mFileManager.UpdateItemInfoBasedOnNewNameLength((sender as Button).Tag as Item);
        }

        private void ButtonJumpToWholeItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                ((sender as Button).Tag as Item).ItemStartPosition,
                ((sender as Button).Tag as Item).ItemEndPosition);
        }

        private void ButtonJumpToID_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorRichTextBoxText(ref scintillaTextBox, Colors.Orange,
                ((sender as Button).Tag as Item).IDStartPosition,
                ((sender as Button).Tag as Item).IDEndPosition);
        }

        private void ButtonJumpToName_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorRichTextBoxText(ref scintillaTextBox, Colors.Red,
                ((sender as Button).Tag as Item).NameStartPosition,
                ((sender as Button).Tag as Item).NameEndPosition);
        }

        private void ButtonJumpToDescription_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                ((sender as Button).Tag as Item).DescriptionStartPosition,
                ((sender as Button).Tag as Item).DescriptionEndPosition);
        }

        public void ColorRichTextBoxText(ref ScintillaWPF richTextBox, Color color, int textStartPosition, int textEndPosition)
        {
            richTextBox.SelectionStart = textStartPosition;
            richTextBox.SelectionEnd = textEndPosition;
            //richTextBox.Colorize(textStartPosition, textEndPosition);
            richTextBox.SetSelectionForeColor(true, color);
            richTextBox.ScrollCaret();
            //richTextBox.AddSelection(textStartPosition, textEndPosition);
            //richTextBox.Selection.Select(richTextBox.Document.ContentStart.GetPositionAtOffset(textStartPosition),
            //                    richTextBox.Document.ContentStart.GetPositionAtOffset(textEndPosition));
            //richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        string ConvertTextFromStringToHex(string text)
        {
            string[] stingsSplittedBy1000Chars = text.Split(null, 1000);
            string[] resultStrings = new string[stingsSplittedBy1000Chars.Length];
            int stringAmount = stingsSplittedBy1000Chars.Length;
            Console.WriteLine(stringAmount.ToString());

            for (int i = 0; i < stringAmount; i++)
            {
                if (i == 500)
                    break;
                Thread currentThread = new Thread(() => { /*Dispatcher.Invoke(() => */resultStrings[i] = ConvertStringFromStringToHex(stingsSplittedBy1000Chars[i], i)/*)*/; });
                currentThread.Start();
            }

            string resultString = "";
            foreach (string str in resultStrings)
                resultString += str;

            return resultString;
        }

        string ConvertStringFromStringToHex(string text, int partOfStringID)
        {
            //int amountToStopAt = 1000;
            char[] values = text.ToCharArray();
            string result = "";
            //int i = 0;

            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = System.Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                string hexOutput = string.Format("{0:X}", value);
                result += hexOutput;

                //++i;
                //if (i == amountToStopAt)
                //    break;
            }

            return result;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            WindowAbout windowAbout = new WindowAbout();
            windowAbout.Owner = this;
            windowAbout.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowAbout.ShowDialog();
        }

        private void ButtonTransferAllItemsToStoredItems_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in ListBoxFoundItems.Items)
                ListBoxStoredItems.Items.Add(item.Clone());
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenFileToTranslate_Click(sender, e);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (mOpenFileDialog.FileName == "")
                return;

            mFileManager.SaveAssociatedStoredItemData(mOpenFileDialog.FileName, ref ListBoxStoredItems, ref LabelSaved);

            //FileManager.JsonFileUtils.SimpleWrite(ref ListBoxStoredItems, mOpenFileDialog.FileName);
        }

        private void ButtonClearStoredItems_Click(object sender, RoutedEventArgs e)
        {
            ListBoxStoredItems.Items.Clear();
        }

        private void ComboBoxSelectedEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding encoding;
            FontFamily unicodeFont;

            string selectedEncoding = (ComboBoxSelectedEncoding.SelectedItem as ComboBoxItem).Content as string;

            switch (selectedEncoding)
            {
                case "ANSI":
                    unicodeFont = new FontFamily("Segoi UI");
                    encoding = Encoding.GetEncoding(1252);
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "Unicode":
                    unicodeFont = new FontFamily("Segoi UI");
                    encoding = Encoding.Unicode;
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "UTF8":
                    unicodeFont = new FontFamily("Segoi UI");
                    encoding = Encoding.UTF8;
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "GB2312":
                    unicodeFont = new FontFamily("SimHei");
                    encoding = Encoding.GetEncoding("gb2312");
                    //encoding = Encoding.GetEncoding("gb18030");
                    break;
                case "BIG5":
                    unicodeFont = new FontFamily("SimHei");
                    encoding = Encoding.GetEncoding("big5");
                    //encoding = Encoding.GetEncoding("gb18030");
                    break;
                default:
                    throw new Exception("Encoding \"" + selectedEncoding + "\" is not found");
            }

            if (unicodeFont == null)
                return;

            scintillaTextBox.FontFamily = unicodeFont;
            if (scintillaTextBox.Text != "")
                scintillaTextBox.Text = TextManager.CleanStringFromNewLinesAndBadChars(
                    TextManager.GetStringWithEncoding(mFileManager.OpenFileText, encoding));

            foreach (Item item in ListBoxFoundItems.Items)
            {
                item.SetEncoding(unicodeFont, encoding);
            }
        }              

        private void ButtonBeginTranslate_Click(object sender, RoutedEventArgs e)
        {
            //var task = BeginTranslateAsync();
            List<ItemData> foundItemData = new List<ItemData>();
            foreach (Item item in ListBoxFoundItems.Items)
                foundItemData.Add(item.ToItemData());
            List<ItemData> storedItemData = new List<ItemData>();
            foreach (Item item in ListBoxStoredItems.Items)
                storedItemData.Add(item.ToItemData());
            Task task = BeginTranslateAsync(mFileManager.OpenFileText, foundItemData, storedItemData);

            //task.Wait();

            //task.Dispose();
        }

        private async Task BeginTranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData)
        {
            var task = await Task.Run(() => TranslationManager.TranslateAsync(text, foundItemData, storedItemData));

            mFileManager.TranslatedFileText = task;
            scintillaTextBox.ReadOnly       = false;
            scintillaTextBox.Text           = TextManager.CleanStringFromNewLinesAndBadChars(task);
            scintillaTextBox.ReadOnly       = true;
        }

        private void TextBoxItemSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            ItemStorageManager.UpdateVisableItems(ref ListBoxFoundItems, ref ListBoxStoredItems,
                TextBoxItemSearchBar.Text, CheckBoxShowItemsWithBadCharsOnly.IsChecked.Value);
            //// Only allow searches if CheckBoxShowItemsWithBadCharsOnly is not checked
            //if (CheckBoxShowItemsWithBadCharsOnly.IsChecked.Value)
            //    return;

            //string text = ((TextBox)sender).Text;

            //// Don't allow searches for too small words
            //if (text.Length != 0 && text.Length < 2)
            //    return;

            //List<Item> itemsStored  = new List<Item>();
            //List<Item> itemsFound   = new List<Item>();

            //if (text == "" && mStoredItemsWhileSearching != null)
            //{
            //    // Add the stored items back
            //    itemsStored = mStoredItemsWhileSearching;
            //    itemsFound  = mFoundItemsWhileSearching;
            //    mStoredItemsWhileSearching  = null;
            //    mFoundItemsWhileSearching   = null;
            //}
            //else
            //{
            //    // Add only the found items from the search and temporary store the stored items
            //    if (mStoredItemsWhileSearching == null)
            //    {
            //        mStoredItemsWhileSearching  = ListBoxStoredItems.Items.OfType<Item>().ToList();
            //        mFoundItemsWhileSearching   = ListBoxFoundItems.Items.OfType<Item>().ToList();
            //    }

            //    itemsStored = mStoredItemsWhileSearching.Where((Item item) =>
            //    {
            //        return item.Name.Contains(text);
            //    }).ToList();

            //    foreach (Item itemStored in itemsStored)
            //    {
            //        Item itemFound = mFoundItemsWhileSearching.FirstOrDefault((Item item) =>
            //        {
            //            return item.ID[0] == itemStored.ID[0] && item.ID[1] == itemStored.ID[1];
            //        });

            //        if (itemFound != null)
            //            itemsFound.Add(itemFound);
            //    }
            //}

            //ListBoxStoredItems.Items.Clear();
            //ListBoxFoundItems.Items.Clear();
            //foreach (Item itemCurrent in itemsStored)
            //{
            //    ListBoxStoredItems.Items.Add(itemCurrent);
            //}

            //foreach (Item itemCurrent in itemsFound)
            //{
            //    if (!ListBoxFoundItems.Items.Contains(itemCurrent))
            //        ListBoxFoundItems.Items.Add(itemCurrent);
            //}
        }        

        private void MenuItemExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (scintillaTextBox.Text == null || scintillaTextBox.Text == "")
                return;

            string text = "";
            if (mFileManager.TranslatedFileText != null && mFileManager.TranslatedFileText != "")
                text = mFileManager.TranslatedFileText;
            else if (mFileManager.OpenFileText != null && mFileManager.OpenFileText != "")
                text = mFileManager.OpenFileText;
            else
                return;

            File.WriteAllText(".\\testexportfile.dat", text, Encoding.GetEncoding(1252)/*Encoding.GetEncoding("big5")*/);
        }

        private void CheckBoxShowItemsWithBadCharsOnly_Click(object sender, RoutedEventArgs e)
        {
            ItemStorageManager.UpdateVisableItems(ref ListBoxFoundItems, ref ListBoxStoredItems,
                TextBoxItemSearchBar.Text, CheckBoxShowItemsWithBadCharsOnly.IsChecked.Value);
        }
    }
}
