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
using System.Collections.ObjectModel;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// The main window of the application.
    /// This program is meant to translate pc-versions of the game Wonderland Online.
    /// It has been tested to work with the most current pc-version of the game (that isn't located in main-land China):
    /// Wonderland Online - Rhodes Island.
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog              mOpenFileDialog;
        private FileManager                 mFileManager;
        private Point?                      mCursorOldPosition = null;

        private ObservableCollection<Item>  mLazyLoadFoundItemData;
        private ObservableCollection<Item>  mLazyLoadStoredItemData;

        public MainWindow()
        {
            InitializeComponent();

            mLazyLoadFoundItemData  = new ObservableCollection<Item>();
            mLazyLoadStoredItemData = new ObservableCollection<Item>();
            ListBoxFoundItems.ItemsSource   = mLazyLoadFoundItemData;
            ListBoxStoredItems.ItemsSource  = mLazyLoadStoredItemData;

            Grid[]          frontGrids      = new Grid[] { GridFrontLeft, GridFrontMiddle, GridFrontRight };
            GroupBox[]      groupBoxes      = new GroupBox[] { GroupBoxItem, GroupBoxItemSearchOptions };
            CheckBox[]      checkBoxes      = (GroupBoxItemSearchOptions.Content as Grid).Children.OfType<CheckBox>().ToArray();            
            Button[]        buttons         = GetAllObjectsOfTypeFromGridFronts<Button>();
            GridSplitter[]  gridSplitters   = new GridSplitter[] { GridSplitterLeft, GridSplitterRight };
            Label[]         labels          = GetAllObjectsOfTypeFromGridFronts<Label>();
            ListBox[]       listBoxes       = new ListBox[] { ListBoxFoundItems, ListBoxStoredItems };
            TextBox[]       textBoxes       = new TextBox[] { TextBoxFileContentSearchBar };
            TextBlock[]     textBlocks      = new TextBlock[] { ProjectFileName };


            ThemeManager.Initialize(ref GridBack, ref frontGrids, ref MenuMain, ref ToolBarMain, ref groupBoxes, ref checkBoxes,
                ref buttons, ref gridSplitters, ref labels, ref scintillaTextBox, ref listBoxes,
                ref ButtonMinimize, ref ButtonResize, ref ButtonClose, ref textBoxes, ref textBlocks, ref GridBackgroundProjectFileName);

            EditingCommands.ToggleInsert.Execute(null, scintillaTextBox);

            ListBoxFoundItems.MouseDoubleClick += ListBoxFoundItems_MouseDoubleClick;
            ListBoxFoundItems.MouseRightButtonUp += ListBoxFoundItems_MouseRightButtonUp;
            mFileManager = new FileManager(Dispatcher, ref scintillaTextBox, ref LabelReportInfo, ref ListBoxFoundItems,
                ref ListBoxStoredItems, ButtonUpdateItemBasedOnNameLength_Click, ButtonJumpToWholeItem_Click, ButtonJumpToID_Click,
                ButtonJumpToName_Click, ButtonJumpToDescription_Click, ButtonJumpToExtra1_Click, ButtonJumpToExtra2_Click);

            mFileManager.LoadProgramSettings();

            ItemManager.Initialize(ref ListBoxFoundItems);

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 8d;            
        }

        private void WindowMain_Activated(object sender, EventArgs e)
        {
            // Show the What's New window if it's the first time this version has been installed.
            if (mFileManager.ShowWhatsNew())
            {
                WindowWhatsNew windowWhatsNew           = new WindowWhatsNew();
                windowWhatsNew.Owner                    = this;
                windowWhatsNew.WindowStartupLocation    = WindowStartupLocation.CenterOwner;
                windowWhatsNew.ShowDialog();
            }
        }

        /// <summary>
        /// Retrieves all the objects of the UI-components from the GroupBox or the Grids that matches the specified type T
        /// </summary>
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
            OpenFoundItemRightClickMenu((Item)ListBoxFoundItems.SelectedItem);
        }

        private void ListBoxStoredItems_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenStoredItemRightClickMenu((Item)ListBoxStoredItems.SelectedItem);
        }

        private void OpenFoundItemRightClickMenu(Item item)
        {
            ContextMenu itemRightClickMenu = (ContextMenu)FindResource("FoundItemRightClickMenu");

            foreach (object itemObject in itemRightClickMenu.Items)
            {
                if (itemObject.GetType() != typeof(MenuItem))
                    continue;

                MenuItem menuItem   = (MenuItem)itemObject;
                menuItem.Tag        = item;
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
                    case "MenuItemCopyFoundItemID":
                        menuItem.Click += ButtonCopyFoundItemID_Click;
                        break;
                }
            }

            itemRightClickMenu.Placement    = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            itemRightClickMenu.IsOpen       = true;
        }

        private void OpenStoredItemRightClickMenu(Item item)
        {
            ContextMenu itemRightClickMenu = (ContextMenu)FindResource("StoredItemRightClickMenu");

            foreach (object itemObject in itemRightClickMenu.Items)
            {
                if (itemObject.GetType() != typeof(MenuItem))
                    continue;

                MenuItem menuItem   = (MenuItem)itemObject;
                menuItem.Tag        = item;
                switch (menuItem.Name)
                {
                    case "MenuItemScrollToMatchingFoundItem":
                        menuItem.Click += ButtonScrollToMatchingFoundItem_Click;
                        break;
                    case "MenuItemCopyStoredItemID":
                        menuItem.Click += ButtonCopyFoundItemID_Click;
                        break;
                }
            }

            itemRightClickMenu.Placement    = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            itemRightClickMenu.IsOpen       = true;
        }

        /// <summary>
        /// Opens the .dat file that should be translated or translations should be grabbed from.
        /// </summary>
        private void ButtonOpenTranslationFile_Click(object sender, RoutedEventArgs e)
        {
            mOpenFileDialog = new OpenFileDialog();

            if (mOpenFileDialog.ShowDialog() == true)
            {                
                if (mFileManager.OpenTranslationFile(mOpenFileDialog.FileName))
                {
                    ComboBoxSelectedEncoding.SelectedIndex = 0;
                    ClearSearchBarsAndSearchOptions();
                    UpdateRightClickMenuAndItemButtons();

                    mFileManager.SetProjectOpenFileName(ref GridBackgroundProjectFileName, mOpenFileDialog.FileName);
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

        /// <summary>
        /// Adds/removes item menu buttons from the right-click menu for the found items and adds/removes buttons in
        /// items buttons depending on if the opened file items have descriptions and/or extras.
        /// </summary>
        private void UpdateRightClickMenuAndItemButtons()
        {
            ContextMenu itemRightClickMenu = (ContextMenu)FindResource("FoundItemRightClickMenu");

            if (FileManager.FileItemProperties.HasExtras)
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

            if (FileManager.FileItemProperties.HasDescription)
            {
                (itemRightClickMenu.Items[5] as MenuItem).Visibility = Visibility.Visible;
                (itemRightClickMenu.Items[5] as MenuItem).IsEnabled  = true;
                ButtonJumpToDescription.IsEnabled    = true;
                ButtonJumpToDescription.Visibility   = Visibility.Visible;
            }
            else
            {
                (itemRightClickMenu.Items[5] as MenuItem).Visibility = Visibility.Collapsed;
                (itemRightClickMenu.Items[5] as MenuItem).IsEnabled  = false;
                ButtonJumpToDescription.IsEnabled    = false;
                ButtonJumpToDescription.Visibility   = Visibility.Hidden;
            }
        }

        public void InitializeItemSearch()
        {
            ItemSearch.UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBoxFoundItems, ref ListBoxStoredItems);
        }

        private Item GetSelectedFoundItem(object sender)
        {
            Item item;
            if ((sender as FrameworkElement).Tag != null)
                item = (sender as FrameworkElement).Tag as Item;
            else
                item = ListBoxFoundItems.SelectedItem as Item;

            return item;
        }

        private Item GetSelectedStoredItem(object sender)
        {
            Item item;
            if ((sender as FrameworkElement).Tag != null)
                item = (sender as FrameworkElement).Tag as Item;
            else
                item = ListBoxStoredItems.SelectedItem as Item;

            return item;
        }

        // Found right-click menu items

        private void ButtonUpdateItemBasedOnNameLength_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            mFileManager.UpdateItemInfoBasedOnNewNameLength(item);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToWholeItem_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.ItemStartPosition,
                item.ItemEndPosition + 1);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToID_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Orange,
                item.IDStartPosition,
                item.IDEndPosition + 1);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToName_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Red,
                item.NameStartPosition,
                item.NameEndPosition);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToDescription_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.DescriptionStartPosition,
                item.DescriptionEndPosition);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToExtra1_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.Extra1StartPosition,
                item.Extra1EndPosition);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonJumpToExtra2_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            ColorRichTextBoxText(ref scintillaTextBox, Colors.Green,
                item.Extra2StartPosition,
                item.Extra2EndPosition);
            ListBoxFoundItems.SelectedItem = item;
        }

        private void ButtonCopyFoundItemID_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedFoundItem(sender);
            if (item == null)
                return;

            Clipboard.SetText(item.TextBlockID.Text);
        }

        // Stored right-click menu items

        private void ButtonScrollToMatchingFoundItem_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedStoredItem(sender);
            if (item == null)
                return;

            ListBoxStoredItems.SelectedItem = item;

            Item itemFound = ListBoxFoundItems.ItemsSource.OfType<Item>().First((Item foundItem) => { return Item.CompareIDs(foundItem.ID, item.ID); });

            if (itemFound == null) return;

            ListBoxFoundItems.SelectedItem  = itemFound;
            ListBoxFoundItems.ScrollIntoView(itemFound);
        }

        private void ButtonCopyStoredItemID_Click(object sender, RoutedEventArgs e)
        {
            Item item = GetSelectedStoredItem(sender);
            if (item == null)
                return;

            Clipboard.SetText(item.TextBlockID.Text);
        }

        public void ColorRichTextBoxText(ref ScintillaWPF richTextBox, Color color, int textStartPosition, int textEndPosition)
        {
            richTextBox.SelectionStart  = textStartPosition;
            richTextBox.SelectionEnd    = textEndPosition;
            richTextBox.SetSelectionForeColor(true, color);
            richTextBox.ScrollCaret();
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
                Thread currentThread = new Thread(() => { /*Dispatcher.Invoke(() => */resultStrings[i] = 
                    ConvertStringFromStringToHex(stingsSplittedBy1000Chars[i], i)/*)*/; });
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
            WindowAbout windowAbout             = new WindowAbout(mFileManager.ApplicationVersion);
            windowAbout.Owner                   = this;
            windowAbout.WindowStartupLocation   = WindowStartupLocation.CenterOwner;
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
            ButtonOpenTranslationFile_Click(sender, e);
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            mFileManager.RemoveProjectOpenFileName(ref GridBackgroundProjectFileName);
            mFileManager.CloseFileToTranslateOrGrabTranslationsFrom();

            ComboBoxSelectedEncoding.SelectedIndex = 0;
            ClearSearchBarsAndSearchOptions();
            ItemSearch.UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBoxFoundItems, ref ListBoxStoredItems);
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (mOpenFileDialog == null || mOpenFileDialog.FileName == "")
                return;

            if (ListBoxStoredItems.Items.OfType<Item>().Any((Item item) => { return item.IsTextBoxesContainingIllegalChars(); }))
            {
                MessageBox.Show("Some items contains illegal chars and cannot be saved (text boxes marked red)");
                return;
            }

            mFileManager.SaveStoredItemData(FileManager.FileItemProperties.FileType, ListBoxStoredItems.Items.OfType<IItemBase>().ToList());
        }

        //private void ButtonAddNewStoredItem_Click(object sender, RoutedEventArgs e)
        //{
        //    Item item = new Item(ButtonUpdateItemBasedOnNameLength_Click, ButtonJumpToWholeItem_Click, ButtonJumpToID_Click,
        //        ButtonJumpToName_Click, ButtonJumpToDescription_Click, ButtonJumpToExtra1_Click, ButtonJumpToExtra2_Click,
        //        mFileManager.FileItemProperties.HasDescription, mFileManager.FileItemProperties.HasExtras);
        //    item.NameStartPosition  = scintillaTextBox.SelectionStart;
        //    item.NameEndPosition    = scintillaTextBox.SelectionEnd;
        //    item.Name               = scintillaTextBox.SelectedText;
        //    ItemSearch.AddStoredItem(ref item, ref ListBoxStoredItems);
        //    item.Focus();
        //}

        private void ButtonClearStoredItems_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to clear the stored items?\n(cannot be undone)",
                "Clear Stored Items", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                ItemSearch.ClearStoredItemsWhileSearching();
                (ListBoxStoredItems.ItemsSource as ObservableCollection<Item>).Clear();
            }
        }

        private void ComboBoxSelectedEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding encoding;
            FontFamily unicodeFont;

            string selectedEncoding = (ComboBoxSelectedEncoding.SelectedItem as ComboBoxItem).Content as string;

            switch (selectedEncoding)
            {
                case "ANSI":
                    unicodeFont                 = new FontFamily("Courier New");
                    encoding                    = Encoding.GetEncoding(1252);
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "Unicode":
                    unicodeFont                 = new FontFamily("Segoi UI");
                    encoding                    = Encoding.Unicode;
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "UTF8":
                    unicodeFont                 = new FontFamily("Segoi UI");
                    encoding                    = Encoding.UTF8;
                    scintillaTextBox.FontFamily = unicodeFont;
                    break;
                case "GB2312":
                    unicodeFont                 = new FontFamily("SimHei");
                    encoding                    = Encoding.GetEncoding("gb2312");
                    break;
                case "BIG5":
                    unicodeFont                 = new FontFamily("SimHei");
                    encoding                    = Encoding.GetEncoding("big5");
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
                item.SetEncoding(unicodeFont, encoding);
        }        

        /// <summary>
        /// Starts the translation process of the opened file when the button is pressed
        /// </summary>
        private void ButtonBeginTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (FileManager.FileItemProperties == null
                || ListBoxFoundItems == null || ListBoxFoundItems.Items.Count == 0
                || ListBoxStoredItems == null || ListBoxStoredItems.Items.Count == 0)
                return;

            if (ListBoxStoredItems.Items.OfType<Item>().Any((Item item) => { return item.IsTranslationsTooLong(); }))
            {
                MessageBox.Show("Some items have too long translations (text boxes marked gray)");
                return;
            }

            List<ItemData> foundItemData = new List<ItemData>();
            foreach (Item item in ListBoxFoundItems.Items)
                foundItemData.Add(item.GetItemData());

            List<ItemData> storedItemData = new List<ItemData>();
            foreach (Item item in ListBoxStoredItems.Items)
                storedItemData.Add(item.GetItemData());
            _ = BeginTranslateAsync(mFileManager.OpenFileText, foundItemData, storedItemData);
        }

        // Begins the translation process of the opened file by communicating with the Translation class.
        private async Task BeginTranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData)
        {
            var task = await Task.Run(() => Translation.TranslateAsync(text, foundItemData, storedItemData, FileManager.FileItemProperties));

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
        private void CheckBoxShowItemsWithoutFirstCharLetter_Click(object sender, RoutedEventArgs e)    { UpdateVisableItems(); }
        private void CheckBoxShowItemsWithTooLongTranslations_Click(object sender, RoutedEventArgs e)   { UpdateVisableItems(); }

        private void UpdateVisableItems(ItemSearch.SearchOption searchOption = ItemSearch.SearchOption.DEFAULT)
        {
            ItemSearch.UpdateVisibleItems(ref ListBoxFoundItems, ref ListBoxStoredItems,
                TextBoxItemSearchBar.Text, CheckBoxShowItemsWithBadChars.IsChecked.Value, CheckBoxShowItemsWithUnusualChars.IsChecked.Value,
                CheckBoxShowItemsWithoutDescriptions.IsChecked.Value, CheckBoxShowItemsWithSameIDs.IsChecked.Value,
                CheckBoxShowItemsWithNoMatch.IsChecked.Value, CheckBoxShowItemsWithoutFirstCharLetter.IsChecked.Value,
                CheckBoxShowItemsWithTooLongTranslations.IsChecked.Value, searchOption);
        }

        private void MenuItemExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (scintillaTextBox.Text == null || scintillaTextBox.Text == "")
                return;

            string fileName = mFileManager.ExportFile();

            if (fileName == "cancelled")
                return;

            if (fileName != null)
                LabelReportInfo.Content = "File " + Path.GetFileName(fileName) + " exported";
            else
                LabelReportInfo.Content = "File export failed";
        }

        private void ButtonOpenItemInfo_Click(object sender, RoutedEventArgs e)
        {
            Item item = (Item)ListBoxFoundItems.SelectedItem;
            if (item == null)
                return;

            string itemData = scintillaTextBox.Text.Substring(item.ItemStartPosition, item.ItemEndPosition - item.ItemStartPosition + 1);
            WindowItemInfo windowItemInfo           = new WindowItemInfo("Item Info for " + item.Name, itemData);
            windowItemInfo.WindowStartupLocation    = WindowStartupLocation.CenterOwner;
            windowItemInfo.Owner                    = this;

            windowItemInfo.ShowDialog();
        }

        private void ListBoxStoredItems_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && ListBoxStoredItems.SelectedItem != null)
                ItemSearch.DeleteSelectedItemFromStoredItems(ref ListBoxStoredItems);
        }

        private void ButtonOpenText_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = scintillaTextBox.SelectedText;
            if (selectedText == "")
                return;
            
            WindowItemInfo windowItemInfo           = new WindowItemInfo("File Selected Data", selectedText);
            windowItemInfo.WindowStartupLocation    = WindowStartupLocation.CenterOwner;
            windowItemInfo.Owner                    = this;

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

            if (windowSettings.ShowDialog() == true)
                mFileManager.SaveProgramSettings();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SetWindowState(WindowState windowState)
        {
            switch (windowState)
            {
                case WindowState.Normal:
                    Image image     = ButtonResize.Content as Image;
                    image.Source    = ThemeManager.ImageSourceButtonResizeNormal;
                    WindowState     = WindowState.Normal;
                    break;
                case WindowState.Maximized:
                    image           = ButtonResize.Content as Image;
                    image.Source    = ThemeManager.ImageSourceButtonResizeFullscreen;
                    WindowState     = WindowState.Maximized;
                    break;
                case WindowState.Minimized:
                    WindowState     = WindowState.Minimized;
                    break;
            }
        }

        private void ButtonResize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                SetWindowState(WindowState.Maximized);
            else if (WindowState == WindowState.Maximized)
                SetWindowState(WindowState.Normal);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuMain_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (MenuItem menuItem in MenuMain.Items)
                if (menuItem.IsMouseOver)
                    return;

            ButtonResize_Click(sender, e);
        }

        private void MenuMain_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mCursorOldPosition = null;
        }

        private void MenuMain_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            foreach (MenuItem menuItem in MenuMain.Items)
                if (menuItem.IsMouseOver)
                    return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Point cursorNewPosition = MouseOperations.GetMousePosition();

                    if (mCursorOldPosition != null)
                    {
                        double distance = Point.Subtract(mCursorOldPosition.Value, cursorNewPosition).Length;
                        if (distance > 5d)
                        {
                            SetWindowState(WindowState.Normal);
                            Top = 0;
                        }
                    }

                    mCursorOldPosition = cursorNewPosition;
                }

                DragMove();
            }
        }

        private void WindowMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetWindowState(WindowState);
        }

        private void MenuMain_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu topBarRightClickMenu    = (ContextMenu)FindResource("TopBarRightClickMenu");
            UpdateTopBarRightClickMenu(ref topBarRightClickMenu, WindowState);

            topBarRightClickMenu.Placement      = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            topBarRightClickMenu.IsOpen         = true;
        }

        private void ImageProgramIcon_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MenuMain_PreviewMouseRightButtonUp(sender, e);
        }

        private void ImageProgramIcon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //ContextMenu topBarRightClickMenu        = (ContextMenu)FindResource("TopBarRightClickMenu");
            //UpdateTopBarRightClickMenu(ref topBarRightClickMenu, WindowState);

            //topBarRightClickMenu.PlacementTarget    = (UIElement)sender;
            //topBarRightClickMenu.Placement          = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            //topBarRightClickMenu.IsOpen             = true;
        }

        enum TopBarRightClickMenuItems
        {
            RESET,
            MOVE,
            RESIZE,
            MINIMIZE,
            MAXIMIZE,
            CLOSE
        }

        private void UpdateTopBarRightClickMenu(ref ContextMenu contextMenu, WindowState windowState)
        {
            MenuItem menuItemReset      = contextMenu.Items[(int)TopBarRightClickMenuItems.RESET    ] as MenuItem;
            MenuItem menuItemMove       = contextMenu.Items[(int)TopBarRightClickMenuItems.MOVE     ] as MenuItem;
            MenuItem menuItemResize     = contextMenu.Items[(int)TopBarRightClickMenuItems.RESIZE   ] as MenuItem;
            MenuItem menuItemMaximize   = contextMenu.Items[(int)TopBarRightClickMenuItems.MAXIMIZE ] as MenuItem;

            if (windowState == WindowState.Normal)
            {
                menuItemReset.IsEnabled     = false;
                menuItemMove.IsEnabled      = true;
                menuItemResize.IsEnabled    = true;
                menuItemMaximize.IsEnabled  = true;

                ((Image)menuItemReset.Icon).Source      = (ImageSource)FindResource("TopBarRightClickMenuResizeUnenabled");
                ((Image)menuItemMaximize.Icon).Source   = (ImageSource)FindResource("TopBarRightClickMenuMaximize");
            }
            else if (windowState == WindowState.Maximized)
            {
                menuItemReset.IsEnabled     = true;
                menuItemMove.IsEnabled      = false;
                menuItemResize.IsEnabled    = false;
                menuItemMaximize.IsEnabled  = false;

                ((Image)menuItemReset.Icon).Source      = (ImageSource)FindResource("TopBarRightClickMenuResize");
                ((Image)menuItemMaximize.Icon).Source   = (ImageSource)FindResource("TopBarRightClickMenuMaximizeUnenabled");
            }
        }

        private void MenuItemProgramClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOpenMultiTranslator_Click(object sender, RoutedEventArgs e)
        {
            WindowMultiTranslator windowMultiTranslator = new WindowMultiTranslator(ref mFileManager);
            windowMultiTranslator.Owner                 = this;
            windowMultiTranslator.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            windowMultiTranslator.ShowDialog();
        }

        private void MenuItemExportFoundItemsAsExcelDocument_Click(object sender, RoutedEventArgs e)
        {
            mFileManager.ExportFoundItemsAsExcelDocument();
        }

        private void MenuItemExportStoredItemsAsExcelDocument_Click(object sender, RoutedEventArgs e)
        {
            mFileManager.ExportStoredItemsAsExcelDocument();
        }        
    }
}
