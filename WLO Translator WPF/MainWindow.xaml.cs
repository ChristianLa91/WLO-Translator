using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ScintillaNET.WPF;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog  mOpenFileDialog;
        private FileManager     mFileManager;
        private string          mWindowTitle;

        public MainWindow()
        {
            InitializeComponent();

            mWindowTitle = Title;

            Grid[]          frontGrids      = new Grid[] { GridFrontLeft, GridFrontMiddle, GridFrontRight };
            GroupBox[]      groupBoxes      = new GroupBox[] { GroupBoxItem, GroupBoxItemSearchOptions };
            CheckBox[]      checkBoxes      = (GroupBoxItemSearchOptions.Content as Grid).Children.OfType<CheckBox>().ToArray();            
            Button[]        buttons         = GetAllObjectsOfTypeFromGridFronts<Button>();
            GridSplitter[]  gridSplitters   = new GridSplitter[] { GridSplitterLeft, GridSplitterRight };
            Label[]         labels          = GetAllObjectsOfTypeFromGridFronts<Label>();
            ListBox[]       listBoxes       = new ListBox[] { ListBoxFoundItems, ListBoxStoredItems };

            ThemeManager.Initialize(ref GridBack, ref frontGrids, ref MenuMain, ref ToolBarMain, ref groupBoxes, ref checkBoxes,
                ref buttons, ref gridSplitters, ref labels, ref scintillaTextBox, ref listBoxes);

            EditingCommands.ToggleInsert.Execute(null, scintillaTextBox);

            ListBoxFoundItems.MouseDoubleClick += ListBoxFoundItems_MouseDoubleClick;
            ListBoxFoundItems.MouseRightButtonUp += ListBoxFoundItems_MouseRightButtonUp;
            mFileManager = new FileManager(Dispatcher, ref scintillaTextBox, ref ListBoxFoundItems, ref ListBoxStoredItems,
                ButtonUpdateItemBasedOnNameLength_Click, ButtonJumpToWholeItem_Click, ButtonJumpToID_Click, ButtonJumpToName_Click,
                ButtonJumpToDescription_Click, ButtonJumpToExtra1_Click, ButtonJumpToExtra2_Click);
        }

        private T[] GetAllObjectsOfTypeFromGridFronts<T>()
        {
            List<T> typeList    = (GroupBoxItem.Content as Grid).Children.OfType<T>().ToList();
            List<T> typeList1   = GridFrontLeft.Children.OfType<T>().ToList();
            List<T> typeList2   = GridFrontMiddle.Children.OfType<T>().ToList();
            List<T> typeList3   = GridFrontRight.Children.OfType<T>().ToList();
            typeList.AddRange(typeList1);
            typeList.AddRange(typeList2);
            typeList.AddRange(typeList3);
            return typeList.ToArray();
        }

        private void ListBoxFoundItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ButtonOpenItemInfo_Click(sender, e);
        }

        private void ListBoxFoundItems_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenItemRightClickMenu((Item)ListBoxFoundItems.SelectedItem);
        }

        private void OpenItemRightClickMenu(Item item)
        {
            ContextMenu itemRightClickMenu = (ContextMenu)FindResource("ItemRightClickMenu");            

            foreach (object itemObject in itemRightClickMenu.Items)
            {
                if (itemObject.GetType() != typeof(MenuItem))
                    continue;

                MenuItem menuItem = (MenuItem)itemObject;
                menuItem.Tag = item;
                switch (menuItem.Name)
                {
                    case "MenuItemUpdate":
                        menuItem.Click += ButtonUpdateItemBasedOnNameLength_Click;
                        break;
                    case "MenuItemJumpToWholeItem":
                        menuItem.Click += ButtonJumpToWholeItem_Click;
                        break;
                    case "MenuItemJumpToID":
                        menuItem.Click += ButtonJumpToID_Click;
                        break;
                    case "MenuItemJumpToName":
                        menuItem.Click += ButtonJumpToName_Click;
                        break;
                    case "MenuItemJumpToDescription":
                        menuItem.Click += ButtonJumpToDescription_Click;
                        break;
                    case "MenuItemJumpToExtra1":
                        menuItem.Click += ButtonJumpToExtra1_Click;
                        break;
                    case "MenuItemJumpToExtra2":
                        menuItem.Click += ButtonJumpToExtra2_Click;
                        break;
                    case "MenuItemCopyItemID":
                        menuItem.Click += ButtonCopyItemID_Click;
                        break;
                }
            }

            itemRightClickMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            itemRightClickMenu.IsOpen = true;
        }

        private void ButtonOpenFileToTranslate_Click(object sender, RoutedEventArgs e)
        {
            mOpenFileDialog = new OpenFileDialog();

            if (mOpenFileDialog.ShowDialog() == true)
            {
                Title = mWindowTitle + " | " + mOpenFileDialog.FileName;
                if (mFileManager.OpenFileToTranslate(mOpenFileDialog.FileName))
                {
                    ComboBoxSelectedEncoding.SelectedIndex = 0;
                    ClearSearchBarsAndSearchOptions();
                    UpdateRightClickMenuAndItemButtons();
                }
            }
        }

        private void ClearSearchBarsAndSearchOptions()
        {
            TextBoxItemSearchBar.Text           = "";
            TextBoxFileContentSearchBar.Text    = "";

            foreach (CheckBox checkBox in (GroupBoxItemSearchOptions.Content as Grid).Children.OfType<CheckBox>())
                checkBox.IsChecked = false;
        }

        private void UpdateRightClickMenuAndItemButtons()
        {
            ContextMenu itemRightClickMenu = (ContextMenu)FindResource("ItemRightClickMenu");

            if (mFileManager.FileItemProperties.FileName == FileName.MARK)
            {
                (itemRightClickMenu.Items[6] as MenuItem).Visibility = Visibility.Visible;
                (itemRightClickMenu.Items[6] as MenuItem).IsEnabled  = true;
                (itemRightClickMenu.Items[7] as MenuItem).Visibility = Visibility.Visible;
                (itemRightClickMenu.Items[7] as MenuItem).IsEnabled  = true;
                ButtonJumpToExtra1.IsEnabled    = true;
                ButtonJumpToExtra1.Visibility   = Visibility.Visible;
                ButtonJumpToExtra2.IsEnabled    = true;
                ButtonJumpToExtra2.Visibility   = Visibility.Visible;
            }
            else
            {
                (itemRightClickMenu.Items[6] as MenuItem).Visibility = Visibility.Collapsed;
                (itemRightClickMenu.Items[6] as MenuItem).IsEnabled  = false;
                (itemRightClickMenu.Items[7] as MenuItem).Visibility = Visibility.Collapsed;
                (itemRightClickMenu.Items[7] as MenuItem).IsEnabled  = false;
                ButtonJumpToExtra1.IsEnabled    = false;
                ButtonJumpToExtra1.Visibility   = Visibility.Hidden;
                ButtonJumpToExtra2.IsEnabled    = false;
                ButtonJumpToExtra2.Visibility   = Visibility.Hidden;
            }
        }

        public void InitializeItemSearch()
        {
            ItemSearch.UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBoxFoundItems, ref ListBoxStoredItems);
        }

        private Item GetSelectedItem(object sender)
        {
            Item item;
            if ((sender as FrameworkElement).Tag != null)
                item = (sender as FrameworkElement).Tag as Item;
            else
                item = ListBoxFoundItems.SelectedItem as Item;

            return item;
        }

        private void ButtonUpdateItemBasedOnNameLength_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            mFileManager.UpdateItemInfoBasedOnNewNameLength(item);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToWholeItem_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.ItemStartPosition,
                item.ItemEndPosition + 1);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToID_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Orange,
                item.IDStartPosition,
                item.IDEndPosition + 1);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToName_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Red,
                item.NameStartPosition,
                item.NameEndPosition);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToDescription_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.DescriptionStartPosition,
                item.DescriptionEndPosition);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToExtra1_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.Extra1StartPosition,
                item.Extra1EndPosition);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonJumpToExtra2_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.Extra2StartPosition,
                item.Extra2EndPosition);
            (item.Parent as ListBox).SelectedItem = item;
        }

        private void ButtonCopyItemID_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedItem(sender);
            if (item == null)
                return;

            Clipboard.SetText(item.TextBlockID.Text);
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
            ItemSearch.StoreAllFoundItems(ref ListBoxStoredItems);
        }

        private void ButtonTransferSelectedItemsToStoredItems_Click(object sender, RoutedEventArgs e)
        {
            ItemSearch.StoreSelectedFoundItems(ref ListBoxFoundItems, ref ListBoxStoredItems);
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenFileToTranslate_Click(sender, e);
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            Title = mWindowTitle;
            mFileManager.CloseFileToTranslate();

            ComboBoxSelectedEncoding.SelectedIndex = 0;
            ClearSearchBarsAndSearchOptions();
            ItemSearch.UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBoxFoundItems, ref ListBoxStoredItems);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (mOpenFileDialog.FileName == "")
                return;

            mFileManager.SaveAssociatedStoredItemData(mOpenFileDialog.FileName, ref ListBoxStoredItems, ref LabelSaved);
        }

        private void ButtonAddNewStoredItem_Click(object sender, RoutedEventArgs e)
        {
            Item item = new Item(ButtonUpdateItemBasedOnNameLength_Click, ButtonJumpToWholeItem_Click, ButtonJumpToID_Click,
                ButtonJumpToName_Click, ButtonJumpToDescription_Click, ButtonJumpToExtra1_Click, ButtonJumpToExtra2_Click,
                mFileManager.FileItemProperties.HasDescription, mFileManager.FileItemProperties.HasExtras);
            item.NameStartPosition  = scintillaTextBox.SelectionStart;
            item.NameEndPosition    = scintillaTextBox.SelectionEnd;
            item.Name               = scintillaTextBox.SelectedText;
            ItemSearch.AddStoredItem(ref item, ref ListBoxStoredItems);
            item.Focus();
        }

        private void ButtonClearStoredItems_Click(object sender, RoutedEventArgs e)
        {
            ItemSearch.ClearStoredItemsWhileSearching();
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
                    unicodeFont = new FontFamily("Courier New");
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
                    break;
                case "BIG5":
                    unicodeFont = new FontFamily("SimHei");
                    encoding = Encoding.GetEncoding("big5");
                    break;
                default:
                    throw new Exception("Encoding \"" + selectedEncoding + "\" is not found");
            }

            if (unicodeFont == null)
                return;

            scintillaTextBox.FontFamily = unicodeFont;
            if (scintillaTextBox.Text != "")
            {
                scintillaTextBox.ReadOnly = false;
                scintillaTextBox.Text = TextManager.CleanStringFromNewLinesAndBadChars(
                    TextManager.GetStringWithEncoding(mFileManager.OpenFileText, encoding));
                scintillaTextBox.ReadOnly = true;
            }

            foreach (Item item in ListBoxFoundItems.Items)
            {
                item.SetEncoding(unicodeFont, encoding);
            }
        }        

        private void ButtonBeginTranslate_Click(object sender, RoutedEventArgs e)
        {
            List<ItemData> foundItemData = new List<ItemData>();
            foreach (Item item in ListBoxFoundItems.Items)
                foundItemData.Add(item.ToItemData());

            List<ItemData> storedItemData = new List<ItemData>();
            foreach (Item item in ListBoxStoredItems.Items)
                storedItemData.Add(item.ToItemData());

            Task task = BeginTranslateAsync(mFileManager.OpenFileText, foundItemData, storedItemData);
        }

        private async Task BeginTranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData)
        {
            var task = await Task.Run(() => Translation.TranslateAsync(text, foundItemData, storedItemData));

            if (task != null)
            {
                mFileManager.TranslatedFileText = task;
                scintillaTextBox.ReadOnly       = false;
                scintillaTextBox.Text           = TextManager.CleanStringFromNewLinesAndBadChars(task);
                scintillaTextBox.ReadOnly       = true;
            }
        }

        private void TextBoxFileContentSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileContentSearch.SearchForString((sender as TextBox).Text, ref scintillaTextBox);
        }

        private void ButtonFindPreviousMatchInFileContent_Click(object sender, RoutedEventArgs e)
        {
            FileContentSearch.SearchPrevious(ref scintillaTextBox);
        }

        private void ButtonFindNextMatchInFileContent_Click(object sender, RoutedEventArgs e)
        {
            FileContentSearch.SearchNext(ref scintillaTextBox);
        }

        private void TextBoxItemSearchBar_TextChanged(object sender, TextChangedEventArgs e)            { UpdateVisableItems(); }
        private void CheckBoxShowItemsWithBadCharsOnly_Click(object sender, RoutedEventArgs e)          { UpdateVisableItems(); }
        private void CheckBoxShowItemsWithUnusualCharsOnly_Click(object sender, RoutedEventArgs e)      { UpdateVisableItems(); }
        private void CheckBoxShowItemsWithoutDescriptionsOnly_Click(object sender, RoutedEventArgs e)   { UpdateVisableItems(); }
        private void CheckBoxShowItemsWithNoMatch_Click(object sender, RoutedEventArgs e)               { UpdateVisableItems(); }
        private void ComboBoxSearchOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVisableItems((ItemSearch.SearchOption)ComboBoxSearchOption.SelectedIndex);
        }
        private void CheckBoxShowItemsWithoutFirstCharLetter_Click(object sender, RoutedEventArgs e) { UpdateVisableItems(); }

        private void UpdateVisableItems(ItemSearch.SearchOption searchOption = ItemSearch.SearchOption.DEFAULT)
        {
            ItemSearch.UpdateVisableItems(ref ListBoxFoundItems, ref ListBoxStoredItems,
                TextBoxItemSearchBar.Text, CheckBoxShowItemsWithBadChars.IsChecked.Value, CheckBoxShowItemsWithUnusualChars.IsChecked.Value,
                CheckBoxShowItemsWithoutDescriptions.IsChecked.Value, CheckBoxShowItemsWithSameIDs.IsChecked.Value,
                CheckBoxShowItemsWithNoMatch.IsChecked.Value, CheckBoxShowItemsWithoutFirstCharLetter.IsChecked.Value, searchOption);
        }

        private void MenuItemExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (scintillaTextBox.Text == null || scintillaTextBox.Text == "")
                return;

            string fileName = mFileManager.ExportFile();

            if (fileName == "cancelled")
                return;

            if (fileName != null)
                LabelSaved.Content = "File " + Path.GetFileName(fileName) + " exported";
            else
                LabelSaved.Content = "File export failed";
        }

        private void ButtonOpenItemInfo_Click(object sender, RoutedEventArgs e)
        {
            Item item = (Item)ListBoxFoundItems.SelectedItem;
            if (item == null)
                return;

            string itemData = scintillaTextBox.Text.Substring(item.ItemStartPosition, item.ItemEndPosition - item.ItemStartPosition + 1);
            WindowItemInfo windowItemInfo           = new WindowItemInfo(item.Name, itemData);
            windowItemInfo.WindowStartupLocation    = WindowStartupLocation.CenterOwner;
            windowItemInfo.Owner                    = this;

            windowItemInfo.ShowDialog();
        }

        private void ListBoxStoredItems_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && ListBoxStoredItems.SelectedItem != null)
            {
                int selectedIndex = ListBoxStoredItems.Items.IndexOf(ListBoxStoredItems.SelectedItem);
                ListBoxStoredItems.Items.Remove(ListBoxStoredItems.SelectedItem);

                // Set focus on the next item
                if (ListBoxStoredItems.Items.Count > selectedIndex)
                    (ListBoxStoredItems.Items[selectedIndex] as Item).Focus();
                else if (ListBoxStoredItems.Items.Count == 1)
                    (ListBoxStoredItems.Items[0] as Item).Focus();
                else if (ListBoxStoredItems.Items.Count > selectedIndex - 1)
                    (ListBoxStoredItems.Items[selectedIndex - 1] as Item).Focus();
            }
        }

        private void ButtonOpenTextReversed_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = scintillaTextBox.SelectedText;
            if (selectedText == "")
                return;
            
            WindowOpenTextReversed windowItemInfo = new WindowOpenTextReversed(selectedText);
            windowItemInfo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowItemInfo.Owner = this;

            windowItemInfo.ShowDialog();
        }

        private void ButtonSelectAllDataLengthLocations_Click(object sender, RoutedEventArgs e)
        {
            scintillaTextBox.ClearSelections();

            foreach (Item item in ListBoxFoundItems.Items)
            {
                scintillaTextBox.AddSelection(item.ItemStartPosition, item.ItemStartPosition + 1);
                scintillaTextBox.AddSelection(item.DescriptionLengthPosition, item.DescriptionLengthPosition + 1);
            }

            scintillaTextBox.SetSelectionForeColor(true, Colors.Green);
        }

        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            WindowSettings windowSettings = new WindowSettings();
            windowSettings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowSettings.Owner = this;

            windowSettings.ShowDialog();
        }        
    }
}
