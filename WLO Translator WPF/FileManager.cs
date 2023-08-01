using ScintillaNET.WPF;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace WLO_Translator_WPF
{
    public class FileManager
    {
        //private static string mSaveFilePath;
        private string              mFileEnding = ".wtsi";
        private WindowLoadingBar    mWindowLoadingBar;
        private BackgroundWorker    mBackgroundWorkerStoredItemData;
        private BackgroundWorker    mBackgroundWorkerFoundItemData;
        private List<ItemData>      mStoredItemsToAdd;
        private List<ItemData>      mFoundItemsToAdd;

        private ListBox             mListBoxFoundItems;
        private ListBox             mListBoxStoredItems;

        private ScintillaWPF        mScintillaWPFTextBox;

        private RoutedEventHandler  mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToDescription;

        private Semaphore           mStoredItemsToAddSemaphore = new Semaphore(1, 1);
        private Semaphore           mFoundItemsToAddSemaphore  = new Semaphore(1, 1);

        private Dispatcher          mDispatcher;

        private string              mOpenFileText;
        public string OpenFileText { get => mOpenFileText; private set => mOpenFileText = value; }
        private string              mTranslatedFileText;
        public string TranslatedFileText { get => mTranslatedFileText; set => mTranslatedFileText = value; }

        enum ItemInfoCollectionStates
        {
            FIND_NAME_LENGHT,
            FIND_NAME_AND_ID,
            FIND_DESCRIPTION_LENGHT,
            FIND_DESCRIPTION
        }

        // Save File Dialog
        //private static SaveFileDialog mSaveFileDialog = new SaveFileDialog();

        public FileManager(Dispatcher mainWindowDispatcher, ref ScintillaWPF scintillaWPFTextBox,
            ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems,
            RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription)
        {
            mDispatcher             = mainWindowDispatcher;
            mScintillaWPFTextBox    = scintillaWPFTextBox;
            mListBoxFoundItems      = listBoxFoundItems;
            mListBoxStoredItems     = listBoxStoredItems;
            mRoutedEventHandlerButtonClickUpdateItem        = routedEventHandlerButtonClickUpdateItem;
            mRoutedEventHandlerButtonClickJumpToWholeItem   = routedEventHandlerButtonClickJumpToWholeItem;
            mRoutedEventHandlerButtonClickJumpToID          = routedEventHandlerButtonClickJumpToID;
            mRoutedEventHandlerButtonClickJumpToName        = routedEventHandlerButtonClickJumpToName;
            mRoutedEventHandlerButtonClickJumpToDescription = routedEventHandlerButtonClickJumpToDescription;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void SaveAssociatedStoredItemData(string filePath, ref ListBox listBoxStoredItems, ref Label labelSaved)
        {
            //bool save = false, saveAs = false;

            //try { if (((Button)sender).Content.ToString() == "Save As...") { saveAs = true; } }
            //catch
            //{
            //    try { if (((MenuItem)sender).Header.ToString() == "Save As...") { saveAs = true; } }
            //    catch
            //    {
            //        try { if (((MainWindow)sender) != null) { saveAs = false; } }
            //        catch { throw new System.ArgumentException("Not Button, ToolStripMenuItem or ToolBarButton"); } // Neither Button, MenuItem nor FormMain
            //    }
            //}

            //if (mSaveFilePath == "" || saveAs)
            //{
            //    if (mSaveFileDialog.ShowDialog() == true)
            //    {
            //        mSaveFilePath = mSaveFileDialog.FileName;
            //        save = true;
            //    }
            //}
            //else
            //    save = true;

            //if (save)
            //{
            string saveFilePath = Directory.GetCurrentDirectory() + "\\" + Path.GetFileNameWithoutExtension(filePath);
            string saveFilePathWithExtension = saveFilePath + mFileEnding;
            if (!File.Exists(saveFilePathWithExtension))
            {
                File.Create(saveFilePathWithExtension);
                //labelSaved.Content = "File " + Path.GetFileNameWithoutExtension(filePath) + "Couldn't Be Saved";
                //return;
            }

            XmlWriterSettings settings  = new XmlWriterSettings();
            settings.Indent             = true;
            settings.IndentChars        = "\t";
            XmlWriter writer            = XmlWriter.Create(saveFilePathWithExtension, settings);

            try
            {
                writer.WriteStartElement("AssociatedStoredItemData");

                    writer.WriteStartElement("Items");

                        writer.WriteAttributeString("ItemCount", listBoxStoredItems.Items.Count.ToString());

                        for (int i = 0; i < listBoxStoredItems.Items.Count; i++)
                        {
                            Item currentItem = listBoxStoredItems.Items[i] as Item;

                            writer.WriteStartElement("Item");

                                string idString                 = currentItem.ID[0].ToString() + currentItem.ID[1].ToString();

                                string filePathItemName         = saveFilePath + "." + idString + ".wtn";
                                string filePathItemDescription  = saveFilePath + "." + idString + ".wtd";

                                // Ints and strings
                                writer.WriteAttributeString("ID1",                      currentItem.ID[0].ToString());
                                writer.WriteAttributeString("ID2",                      currentItem.ID[1].ToString());
                                writer.WriteAttributeString("Name",                     currentItem.Name);
                                writer.WriteAttributeString("Description",              currentItem.Description);

                                // Positions
                                writer.WriteAttributeString("ItemStartPosition",        currentItem.ItemStartPosition.ToString());
                                writer.WriteAttributeString("ItemEndPosition",          currentItem.ItemEndPosition.ToString());
                                writer.WriteAttributeString("IDStartPosition",          currentItem.IDStartPosition.ToString());
                                writer.WriteAttributeString("IDEndPosition",            currentItem.IDEndPosition.ToString());
                                writer.WriteAttributeString("NameStartPosition",        currentItem.NameStartPosition.ToString());
                                writer.WriteAttributeString("NameEndPosition",          currentItem.NameEndPosition.ToString());
                                writer.WriteAttributeString("DescriptionStartPosition", currentItem.DescriptionStartPosition.ToString());
                                writer.WriteAttributeString("DescriptionEndPosition",   currentItem.DescriptionEndPosition.ToString());

                            writer.WriteEndElement();
                        }

                    writer.WriteEndElement();

                writer.WriteEndElement();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Exception: " + e.Message);
            }

            writer.Close();

            labelSaved.Content = "File " + Path.GetFileNameWithoutExtension(filePath) + " Saved";
            //}
        }

        private void LoadAssociatedStoredItemData(string fileName)
        {
            string savefilePath = Directory.GetCurrentDirectory() + "\\" + Path.GetFileNameWithoutExtension(fileName) + mFileEnding;

            // Load last saved data, if file exists
            if (File.Exists(savefilePath))
            {
                mStoredItemsToAdd = new List<ItemData>();
                mDispatcher.Invoke(() => { mListBoxStoredItems.Items.Clear(); });

                XmlReader reader = XmlReader.Create(savefilePath);

                try
                {
                    //List<ItemData> items = new List<ItemData>();
                    int report = 0, progressValue = 0, loadingBarMaximum = 0;

                    while (reader.Read())
                    {
                        if (reader.Name == "Items")
                        {
                            if (reader.GetAttribute("ItemCount") != null)
                                mDispatcher.Invoke(() => { mWindowLoadingBar.Maximum = loadingBarMaximum = int.Parse(reader.GetAttribute("ItemCount")); });
                        }
                        else if (reader.Name == "Item")
                        {                            
                            ItemData currentItemData = new ItemData();

                            //currentItem = new Item(mRoutedEventHandlerButtonClickJumpToWholeItem, mRoutedEventHandlerButtonClickJumpToID,
                            //    mRoutedEventHandlerButtonClickJumpToName, mRoutedEventHandlerButtonClickJumpToDescription);

                            if (reader.GetAttribute("ID1") != null && reader.GetAttribute("ID2") != null)
                                currentItemData.ID = new int[] { int.Parse(reader.GetAttribute("ID1")), int.Parse(reader.GetAttribute("ID2")) };

                            if (reader.GetAttribute("Name") != null)
                                currentItemData.Name = reader.GetAttribute("Name");

                            if (reader.GetAttribute("Description") != null)
                                currentItemData.Description = reader.GetAttribute("Description");

                            if (reader.GetAttribute("ItemStartPosition") != null)
                                currentItemData.ItemStartPosition = int.Parse(reader.GetAttribute("ItemStartPosition"));

                            if (reader.GetAttribute("ItemEndPosition") != null)
                                currentItemData.ItemEndPosition = int.Parse(reader.GetAttribute("ItemEndPosition"));

                            if (reader.GetAttribute("IDStartPosition") != null)
                                currentItemData.IDStartPosition = int.Parse(reader.GetAttribute("IDStartPosition"));

                            if (reader.GetAttribute("IDEndPosition") != null)
                                currentItemData.IDEndPosition = int.Parse(reader.GetAttribute("IDEndPosition"));

                            if (reader.GetAttribute("NameStartPosition") != null)
                                currentItemData.NameStartPosition = int.Parse(reader.GetAttribute("NameStartPosition"));

                            if (reader.GetAttribute("NameEndPosition") != null)
                                currentItemData.NameEndPosition = int.Parse(reader.GetAttribute("NameEndPosition"));

                            if (reader.GetAttribute("DescriptionStartPosition") != null)
                                currentItemData.DescriptionStartPosition = int.Parse(reader.GetAttribute("DescriptionStartPosition"));

                            if (reader.GetAttribute("DescriptionEndPosition") != null)
                                currentItemData.DescriptionEndPosition = int.Parse(reader.GetAttribute("DescriptionEndPosition"));

                            mStoredItemsToAddSemaphore.WaitOne();
                                mStoredItemsToAdd.Add(currentItemData);
                            mStoredItemsToAddSemaphore.Release();

                            if (report > 100)
                            {
                                // Report progress to progress bar
                                mBackgroundWorkerStoredItemData.ReportProgress(progressValue);
                                Thread.Sleep(80);
                                report = 0;
                            }
                            ++report;
                            ++progressValue;
                        }
                    }

                    mBackgroundWorkerStoredItemData.ReportProgress(loadingBarMaximum);

                    reader.Close();

                    //return items;
                }
                catch (System.Exception e)
                {
                    MessageBox.Show("Exception: " + e.Message/*"File \"" + mOpenFileDialog.FileName + "\" couldn't be loaded"*/);
                    System.Console.WriteLine("Exception: " + e.Message);
                    reader.Close();
                }
            }
            else
                System.Console.WriteLine("Error: " + "The file \"" + savefilePath + "\" couldn't be opened");
        }

        public void LoadItemData(string filePath)
        {
            string lodingBarTitle = "Loading Data", loadingBarText = "Loading Stored Item Data";

            if (mBackgroundWorkerStoredItemData != null)
            {
                mBackgroundWorkerStoredItemData.Dispose();
                mBackgroundWorkerStoredItemData = null;
            }

            if (mWindowLoadingBar == null)
            {
                mWindowLoadingBar = new WindowLoadingBar(lodingBarTitle, loadingBarText, 0);
                mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                mWindowLoadingBar.Owner = Application.Current.MainWindow;
            }
            else
            {
                mWindowLoadingBar.SetTitle(lodingBarTitle);
                mWindowLoadingBar.SetText(loadingBarText);
                mWindowLoadingBar.Value = 0;
            }

            if (!mWindowLoadingBar.IsActive)
                mWindowLoadingBar.Show();

            mBackgroundWorkerStoredItemData = new BackgroundWorker();
            mBackgroundWorkerStoredItemData.WorkerReportsProgress = true;
            mBackgroundWorkerStoredItemData.DoWork              += BackgroundWorkerStoredItemData_DoWork;
            mBackgroundWorkerStoredItemData.ProgressChanged     += BackgroundWorkerStoredItemData_ProgressChanged;
            mBackgroundWorkerStoredItemData.RunWorkerCompleted  += BackgroundWorkerStoredItemData_RunWorkerCompleted;
            mBackgroundWorkerStoredItemData.RunWorkerAsync(filePath);
        }
        private void BackgroundWorkerStoredItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadAssociatedStoredItemData(e.Argument as string);
        }

        private void BackgroundWorkerStoredItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Get the contents of the mStoredItemsToAdd list as an atomic operation
            mStoredItemsToAddSemaphore.WaitOne();
                ItemData[] storedItemsToAdd = new ItemData[mStoredItemsToAdd.Count];
                mStoredItemsToAdd.CopyTo(storedItemsToAdd);
                mStoredItemsToAdd.Clear();
            mStoredItemsToAddSemaphore.Release();

            foreach (ItemData item in storedItemsToAdd)
            {
                //mListBox.ItemsSource.GetEnumerator().MoveNext();
                mListBoxStoredItems.Items.Add(item.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
                        mRoutedEventHandlerButtonClickJumpToWholeItem,
                        mRoutedEventHandlerButtonClickJumpToID,
                        mRoutedEventHandlerButtonClickJumpToName,
                        mRoutedEventHandlerButtonClickJumpToDescription));
            }

            mWindowLoadingBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerStoredItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Close loading bar
            if (mWindowLoadingBar != null && mWindowLoadingBar.IsActive)
                mWindowLoadingBar.Close();
        }

        private void BackgroundWorkerFoundItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument as string;

            // Load Data
            FileStream fileStream = File.OpenRead(fileName);
            byte[] bytes = new byte[fileStream.Length];
            int readBytesAmount = fileStream.Read(bytes, 0, (int)fileStream.Length);

            //string byteString = Encoding.GetEncoding(1252).GetString(bytes);

            int itemStartPosition = -1, itemNameStartPosition = -1, itemNameLength = -1,
                itemDescriptionStartPosition = -1, itemDescriptionLength = -1/*, zeroCounter = 0, stopAtItem = 1*/;
            ItemInfoCollectionStates itemInfoCollectionState = ItemInfoCollectionStates.FIND_NAME_LENGHT;
            ItemData currentItem = null;

            //List<ItemData> items = new List<ItemData>();
            mFoundItemsToAdd = new List<ItemData>();

            mDispatcher.Invoke(() =>
            {
                mWindowLoadingBar.Maximum = bytes.Length;
                mBackgroundWorkerFoundItemData.ReportProgress(0);
                Thread.Sleep(10);
            });

            int report = 0;

            // Find the rest of the items' info
            for (int i = 0; i < bytes.Length; ++i)
            {                
                byte currentByte = bytes[i];

                // Find start of item
                if (currentByte != '\0' && !(currentItem == null && currentByte == '\t'))
                {
                    //zeroCounter = 0;

                    if (itemInfoCollectionState == ItemInfoCollectionStates.FIND_NAME_LENGHT)
                    {
                        itemStartPosition               = i;
                        itemInfoCollectionState         = ItemInfoCollectionStates.FIND_NAME_AND_ID;
                        currentItem                     = new ItemData();
                        currentItem.ItemStartPosition   = itemStartPosition;
                    }
                    else if (itemInfoCollectionState == ItemInfoCollectionStates.FIND_NAME_AND_ID)
                    {
                        itemNameStartPosition = i;
                        itemNameLength = bytes[itemStartPosition];
                        currentItem.TextBoxNameLengthText = itemNameLength.ToString();

                        // Break if outside boundaries
                        if (itemNameStartPosition + itemNameLength >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        // Get the item name starting from itemNameStartPosition to itemNameStartPosition + itemNameLength
                        string itemName = "";
                        for (int j = itemNameStartPosition; j < itemNameStartPosition + itemNameLength; ++j)
                            itemName += (char)bytes[j];

                        itemName = TextManager.CleanStringFromNewLinesAndBadChars(itemName);

                        // Remove chars from the name until no illegal chars or initial non-althabetical or numerical chars are left
                        while (!TextManager.IsStringContainingOnlyQuestionMarks(itemName) && !(itemName == ")???(") &&
                            (TextManager.IsStringContainingIllegalChar(itemName) ||
                            TextManager.IsStringEndingWithNonNumberOrLetterOrParenthesesChar(itemName)))
                        {
                            itemName = itemName.Remove(itemName.Length - 1);
                            --itemNameLength;
                        }

                        currentItem.NameReversed = itemName;

                        // Reverse the string so it becomes readable (unreversed), since it's stored reversed in the files
                        itemName = TextManager.ReverseString(currentItem.NameReversed);

                        currentItem.Name                = itemName;
                        currentItem.NameStartPosition   = itemNameStartPosition;
                        currentItem.NameEndPosition     = itemNameStartPosition + itemNameLength;

                        // Get the item ID
                        if (itemNameStartPosition + itemNameLength + 2 < bytes.Length)
                        {
                            currentItem.ID = new int[]
                            {
                                bytes[itemNameStartPosition + itemNameLength    ],
                                bytes[itemNameStartPosition + itemNameLength + 1]
                            };
                            currentItem.IDStartPosition = itemNameStartPosition + itemNameLength;
                            currentItem.IDEndPosition = itemNameStartPosition + itemNameLength + 1;
                        }

                        mFoundItemsToAddSemaphore.WaitOne();
                        mFoundItemsToAdd.Add(currentItem);
                        mFoundItemsToAddSemaphore.Release();
                        itemInfoCollectionState = ItemInfoCollectionStates.FIND_DESCRIPTION_LENGHT;

                        i += itemNameLength;// + 2;
                    }
                    else if (itemInfoCollectionState == ItemInfoCollectionStates.FIND_DESCRIPTION_LENGHT)
                    {
                        i += Constants.AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH - 1;

                        itemDescriptionLength = bytes[i];

                        // If the description length becomes 0, decrease i by 1 and get the new description length from that position
                        while (itemDescriptionLength == 0 && i > currentItem.IDEndPosition)
                        {
                            --i;
                            itemDescriptionLength = bytes[i];

                            System.Console.WriteLine("WARNING: Item \"" + currentItem.Name + "\"'s description length was 0");
                        }

                        currentItem.DescriptionLengthPosition = i;
                        itemInfoCollectionState = ItemInfoCollectionStates.FIND_DESCRIPTION;
                    }
                    else if (itemInfoCollectionState == ItemInfoCollectionStates.FIND_DESCRIPTION)
                    {
                        itemDescriptionStartPosition = i;
                        string itemDescription = "";

                        // Break if outside boundaries
                        if (itemDescriptionStartPosition + itemDescriptionLength >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        // Get description
                        int j = itemDescriptionStartPosition;
                        for (; j < itemDescriptionStartPosition + itemDescriptionLength; ++j)
                            itemDescription += (char)bytes[j];

                        // If the description doesn't end there it should, increase the description until it's fully retrieved
                        while (!TextManager.IsStringContainingNonConventionalChar(((char)bytes[j]).ToString())/*(char)bytes[j] != '\u0090' && (char)bytes[j] != '\u0093' && (char)bytes[j] != '\u2013'*/)
                        {
                            itemDescription += (char)bytes[j];
                            ++j;
                        }

                        if (itemDescription == "")
                            System.Console.WriteLine("WARNING: Item \"" + currentItem.Name + "\" didn't get a description");

                        currentItem.DescriptionReversed = TextManager.CleanStringFromNewLinesAndBadChars(itemDescription);
                            
                        // Reverse the string so it becomes readable (unreversed), since it's stored reversed in the files
                        itemDescription = TextManager.ReverseString(currentItem.DescriptionReversed);

                        currentItem.Description                 = itemDescription;
                        currentItem.DescriptionStartPosition    = itemDescriptionStartPosition;
                        currentItem.DescriptionEndPosition      = itemDescriptionStartPosition + itemDescriptionLength;

                        itemInfoCollectionState                 = ItemInfoCollectionStates.FIND_NAME_LENGHT;

                        i = currentItem.ItemStartPosition + 450;

                        currentItem.ItemEndPosition = i + 1;
                    }
                }

                if (report > 2500)
                {
                    mBackgroundWorkerFoundItemData.ReportProgress(i);
                    Thread.Sleep(10);
                    report = 0;
                }
                ++report;
            }

            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);

            e.Result = fileName;
        }

        //private void UpdateListBoxItems(ListBox listBox, List<ItemData> items)
        //{
        //    //int report = 0;

        //    //// Create an ObservableCollection to hold your items
        //    //ObservableCollection<Item> yourItemsCollection = new ObservableCollection<Item>();

        //    //// Set the collection as the ItemsSource for the ListBox
        //    //listBox.ItemsSource = yourItemsCollection;

        //    // Get the CollectionView associated with the ListBox
        //    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listBox.ItemsSource);

        //    // Store all found items in the ListBox
        //    // Suspend updates while adding a bunch of items
        //    using (view.DeferRefresh())
        //    {
        //        foreach (ItemData itemData in items)
        //        {
        //            listBox.Items.Add(itemData.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
        //                mRoutedEventHandlerButtonClickJumpToWholeItem,
        //                mRoutedEventHandlerButtonClickJumpToID,
        //                mRoutedEventHandlerButtonClickJumpToName,
        //                mRoutedEventHandlerButtonClickJumpToDescription));

        //            //if (report > 1000)
        //            //    Thread.Sleep(1);
        //            //++report;
        //        }
        //    }
        //}

        public void UpdateItemInfoBasedOnNewNameLength(Item item)
        {
            item.NameEndPosition            = item.NameStartPosition + item.Name.Length;
            item.DescriptionLengthPosition  = item.NameEndPosition + Constants.AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
            int descriptionLength           = mOpenFileText[item.DescriptionLengthPosition];

            // Find description
            int i = item.DescriptionLengthPosition + 1;
            char currentChar = mOpenFileText[i];
            while (currentChar == '\0')
            {
                ++i;
                currentChar = mOpenFileText[i];
            }

            //i++;
            item.Description = TextManager.ReverseString(mOpenFileText.Substring(i, descriptionLength));
            item.DescriptionStartPosition = i;
            item.DescriptionEndPosition = i + descriptionLength - 1;
        }

        public void OpenFileToTranslate(string fileName)
        {
            if (mBackgroundWorkerFoundItemData != null)
            {
                mBackgroundWorkerFoundItemData.Dispose();
                mBackgroundWorkerFoundItemData = null;
            }

            mWindowLoadingBar = new WindowLoadingBar("Loading Data", "Loading Item Data From File", 1);
            mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            mWindowLoadingBar.Owner = Application.Current.MainWindow;
            mWindowLoadingBar.Show();

            // Load in text from file
            mScintillaWPFTextBox.Scintilla.Text = "";
            mListBoxFoundItems.Items.Clear();
            mListBoxStoredItems.Items.Clear();

            string fileText = File.ReadAllText(fileName, Encoding.GetEncoding(1252));
            mOpenFileText   = fileText;
            fileText        = TextManager.CleanStringFromNewLinesAndBadChars(fileText);

            mScintillaWPFTextBox.ReadOnly       = false;
            mScintillaWPFTextBox.Scintilla.Text = fileText;
            mScintillaWPFTextBox.ReadOnly       = true;

            // Extract item data from the loaded file
            mBackgroundWorkerFoundItemData = new BackgroundWorker(); // () => ThreadOpenFileToTranslate(fileName)
            mBackgroundWorkerFoundItemData.WorkerReportsProgress = true;
            mBackgroundWorkerFoundItemData.DoWork             += BackgroundWorkerFoundItemData_DoWork;
            mBackgroundWorkerFoundItemData.ProgressChanged    += BackgroundWorkerFoundItemData_ProgressChanged;
            mBackgroundWorkerFoundItemData.RunWorkerCompleted += BackgroundWorkerFoundItemData_RunWorkerCompleted;
            mBackgroundWorkerFoundItemData.RunWorkerAsync(fileName);
        }

        private void BackgroundWorkerFoundItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Get the contents of the mFoundItemsToAdd list as an atomic operation
            mFoundItemsToAddSemaphore.WaitOne();
                ItemData[] foundItemsToAdd = new ItemData[mFoundItemsToAdd.Count];
                mFoundItemsToAdd.CopyTo(foundItemsToAdd);
                mFoundItemsToAdd.Clear();
            mFoundItemsToAddSemaphore.Release();

            foreach (ItemData item in foundItemsToAdd)
            {
                //mListBox.ItemsSource.GetEnumerator().MoveNext();
                mListBoxFoundItems.Items.Add(item.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
                        mRoutedEventHandlerButtonClickJumpToWholeItem,
                        mRoutedEventHandlerButtonClickJumpToID,
                        mRoutedEventHandlerButtonClickJumpToName,
                        mRoutedEventHandlerButtonClickJumpToDescription));
            }

            mWindowLoadingBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerFoundItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //ItemStorageManager.ListBoxItemsFound.Add((List<ItemData>)((object[])e.Result)[0]);
            //UpdateListBoxItems(mListBoxFoundItems, (List<ItemData>)((object[])e.Result)[0]);
            LoadItemData(e.Result as string);
        }

        string StringFromRichTextBox(RichTextBox richTextBox)
        {
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange
            (
                // TextPointer to the start of content in the RichTextBox.
                richTextBox.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                richTextBox.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }
    }
}
