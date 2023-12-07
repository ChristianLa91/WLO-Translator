using Microsoft.Win32;
using ScintillaNET.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace WLO_Translator_WPF
{
    public enum FileName
    {
        NULL,
        ACTIONINFO,
        ALOGINEXE,
        ITEM,        
        MARK,
        NPC,
        SCENEDATA,
        SKILL,
        TALK        
    }

    public class FileItemProperties
    {
        public FileName FileName                        { get; private set; } = FileName.NULL;
        public int      LengthPerItem                   { get; private set; } = -1;
        public int      AfterNameToDescriptionLength    { get; private set; } = -1;
        public bool     HasDescription                  { get; private set; } = true;
        public bool     HasExtras                       { get; private set; } = false;
        public bool     DataFromRightToLeft             { get; private set; } = true;

        public FileItemProperties(FileName fileName)
        {
            FileName = fileName;
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            switch (FileName)
            {
                case FileName.ACTIONINFO:
                    LengthPerItem                   = Constants.ACTIONINFO_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ACTIONINFO_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;
                case FileName.ALOGINEXE:
                    LengthPerItem                   = Constants.ALOGIN_EXE_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ALOGIN_EXE_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    DataFromRightToLeft             = false;
                    break;
                case FileName.ITEM:
                    LengthPerItem                   = Constants.ITEM_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ITEM_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;                
                case FileName.MARK:
                    LengthPerItem                   = Constants.MARK_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.MARK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasExtras                       = true;
                    break;
                case FileName.NPC:
                    LengthPerItem                   = Constants.NPC_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.NPC_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
                case FileName.SCENEDATA:
                    LengthPerItem                   = Constants.SCENEDATA_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.SCENEDATA_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
                case FileName.SKILL:
                    LengthPerItem                   = Constants.SKILL_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.SKILL_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;
                case FileName.TALK:
                    LengthPerItem                   = Constants.TALK_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.TALK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
            }
        }
    }

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
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra1;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra2;

        private Semaphore           mStoredItemsToAddSemaphore = new Semaphore(1, 1);
        private Semaphore           mFoundItemsToAddSemaphore  = new Semaphore(1, 1);

        private Dispatcher          mDispatcher;

        private string              mOpenFileText;
        private byte[]              mOpenFileData;
        public  string              OpenFileText { get => mOpenFileText; private set => mOpenFileText = value; }
        private string              mTranslatedFileText;
        public  string              TranslatedFileText { get => mTranslatedFileText; set => mTranslatedFileText = value; }

        public  FileItemProperties  FileItemProperties { get; private set; }

        public  FileName            LoadedFileName { get; private set; } = FileName.NULL;

        private Database            mDatabase;

        enum DataCollectionStates
        {
            FIND_NAME_LENGTH,
            FIND_NAME_AND_ID,
            FIND_DESCRIPTION_LENGTH,
            FIND_DESCRIPTION,
            FIND_EXTRA1_LENGTH,
            FIND_EXTRA1,
            FIND_EXTRA2_LENGTH,
            FIND_EXTRA2
        }

        // Save File Dialog
        //private static SaveFileDialog mSaveFileDialog = new SaveFileDialog();

        public FileManager(Dispatcher mainWindowDispatcher, ref ScintillaWPF scintillaWPFTextBox,
            ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems,
            RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2)
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
            mRoutedEventHandlerButtonClickJumpToExtra1      = routedEventHandlerButtonClickJumpToExtra1;
            mRoutedEventHandlerButtonClickJumpToExtra2      = routedEventHandlerButtonClickJumpToExtra2;

            //mDatabase = new Database("DatabaseProgramSettings", ".\\IO", // Server name
            //    "SQLTestDatabase",
            //    "sa", //DESKTOP-9ANKOSD\\ChriW
            //    "svmp@9108",
            //    true);

            //mDatabase.Connect();

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
            string saveFilePath = Paths.AssociatedStoredItemData + Path.GetFileNameWithoutExtension(filePath);
            string saveFilePathWithExtension = saveFilePath + mFileEnding;
            if (!File.Exists(saveFilePathWithExtension))
            {
                if (!Directory.Exists(Paths.AssociatedStoredItemData))
                    Paths.GenerateRoamingFolders();
                FileStream fileStream = File.Create(saveFilePathWithExtension);
                fileStream.Close();
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

                                //string idString                 = currentItem.ID[0].ToString() + currentItem.ID[1].ToString();

                                //string filePathItemName         = saveFilePath + "." + idString + ".wtn";
                                //string filePathItemDescription  = saveFilePath + "." + idString + ".wtd";

                                // Ints and strings
                                writer.WriteAttributeString("ID1",                      currentItem.ID[0].ToString());
                                writer.WriteAttributeString("ID2",                      currentItem.ID[1].ToString());
                                writer.WriteAttributeString("ID3",                      currentItem.ID[2].ToString());
                                writer.WriteAttributeString("Name",                     currentItem.Name);
                                writer.WriteAttributeString("Description",              currentItem.Description);
                                writer.WriteAttributeString("Extra1",                   currentItem.Extra1);
                                writer.WriteAttributeString("Extra2",                   currentItem.Extra2);
                                
                                // Positions
                                writer.WriteAttributeString("ItemStartPosition",        currentItem.ItemStartPosition.ToString());
                                writer.WriteAttributeString("ItemEndPosition",          currentItem.ItemEndPosition.ToString());
                                writer.WriteAttributeString("IDStartPosition",          currentItem.IDStartPosition.ToString());
                                writer.WriteAttributeString("IDEndPosition",            currentItem.IDEndPosition.ToString());
                                writer.WriteAttributeString("NameStartPosition",        currentItem.NameStartPosition.ToString());
                                writer.WriteAttributeString("NameEndPosition",          currentItem.NameEndPosition.ToString());
                                writer.WriteAttributeString("DescriptionStartPosition", currentItem.DescriptionStartPosition.ToString());
                                writer.WriteAttributeString("DescriptionEndPosition",   currentItem.DescriptionEndPosition.ToString());
                                
                                writer.WriteAttributeString("Extra1StartPosition",      currentItem.Extra1StartPosition.ToString());
                                writer.WriteAttributeString("Extra1EndPosition",        currentItem.Extra1EndPosition.ToString());
                                writer.WriteAttributeString("Extra2StartPosition",      currentItem.Extra2StartPosition.ToString());
                                writer.WriteAttributeString("Extra2EndPosition",        currentItem.Extra2EndPosition.ToString());

                            writer.WriteEndElement();
                        }

                    writer.WriteEndElement();

                writer.WriteEndElement();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            writer.Close();

            labelSaved.Content = Path.GetFileNameWithoutExtension(filePath) + " Stored Items " + " Saved";
            //}
        }        

        private void LoadAssociatedStoredItemData(string fileName)
        {
            string savefilePath = Paths.AssociatedStoredItemData + Path.GetFileNameWithoutExtension(fileName) + mFileEnding;

            // Load last saved data, if file exists
            if (File.Exists(savefilePath))
            {
                mStoredItemsToAdd = new List<ItemData>();
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

                            if (reader.GetAttribute("ID3") != null && reader.GetAttribute("ID2") != null && reader.GetAttribute("ID1") != null)
                                currentItemData.ID = new int[] { int.Parse(reader.GetAttribute("ID1")), int.Parse(reader.GetAttribute("ID2")),
                                int.Parse(reader.GetAttribute("ID3"))};
                            else if (reader.GetAttribute("ID1") != null && reader.GetAttribute("ID2") != null)
                                currentItemData.ID = new int[] { int.Parse(reader.GetAttribute("ID1")), int.Parse(reader.GetAttribute("ID2")), 0};

                            if (reader.GetAttribute("Name") != null)
                                currentItemData.Name = reader.GetAttribute("Name");

                            if (reader.GetAttribute("Description") != null)
                                currentItemData.Description = reader.GetAttribute("Description");

                            if (reader.GetAttribute("Extra1") != null)
                                currentItemData.Extra1 = reader.GetAttribute("Extra1");

                            if (reader.GetAttribute("Extra2") != null)
                                currentItemData.Extra2 = reader.GetAttribute("Extra2");
                            
                            // Positions
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

                            if (reader.GetAttribute("Extra1StartPosition") != null)
                                currentItemData.Extra1StartPosition = int.Parse(reader.GetAttribute("Extra1StartPosition"));

                            if (reader.GetAttribute("Extra1EndPosition") != null)
                                currentItemData.Extra1EndPosition = int.Parse(reader.GetAttribute("Extra1EndPosition"));

                            if (reader.GetAttribute("Extra2StartPosition") != null)
                                currentItemData.Extra2StartPosition = int.Parse(reader.GetAttribute("Extra2StartPosition"));

                            if (reader.GetAttribute("Extra2EndPosition") != null)
                                currentItemData.Extra2EndPosition = int.Parse(reader.GetAttribute("Extra2EndPosition"));

                            mStoredItemsToAddSemaphore.WaitOne();
                                mStoredItemsToAdd.Add(currentItemData);
                            mStoredItemsToAddSemaphore.Release();

                            ++report;
                            ++progressValue;
                            if (report > 100)
                            {
                                // Report progress to progress bar
                                mBackgroundWorkerStoredItemData.ReportProgress(progressValue);
                                //Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);
                                report = 0;
                            }
                        }
                    }

                    mBackgroundWorkerStoredItemData.ReportProgress(loadingBarMaximum);

                    reader.Close();

                    //return items;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception: " + e.Message/*"File \"" + mOpenFileDialog.FileName + "\" couldn't be loaded"*/);
                    Console.WriteLine("Exception: " + e.Message);
                    reader.Close();
                }
            }
            else
                System.Console.WriteLine("Error: " + "The file \"" + savefilePath + "\" couldn't be opened");
        }

        public void LoadItemData(string filePath)
        {
            string lodingBarTitle = "Loading Data", loadingBarText = "Loading Stored Item Data", loadingBarTextBeforeValue = "Loaded";

            if (mBackgroundWorkerStoredItemData != null)
            {
                mBackgroundWorkerStoredItemData.Dispose();
                mBackgroundWorkerStoredItemData = null;
            }

            if (mWindowLoadingBar == null)
            {
                mWindowLoadingBar = new WindowLoadingBar(lodingBarTitle, loadingBarText, loadingBarTextBeforeValue, 0);
                mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                mWindowLoadingBar.Owner = Application.Current.MainWindow;
            }
            else
            {
                mWindowLoadingBar.SetTitle(lodingBarTitle);
                mWindowLoadingBar.SetText(loadingBarText);
                mWindowLoadingBar.Value = 0;
            }

            mBackgroundWorkerStoredItemData = new BackgroundWorker();
            mBackgroundWorkerStoredItemData.WorkerReportsProgress = true;
            mBackgroundWorkerStoredItemData.DoWork              += BackgroundWorkerStoredItemData_DoWork;
            mBackgroundWorkerStoredItemData.ProgressChanged     += BackgroundWorkerStoredItemData_ProgressChanged;
            mBackgroundWorkerStoredItemData.RunWorkerCompleted  += BackgroundWorkerStoredItemData_RunWorkerCompleted;
            mBackgroundWorkerStoredItemData.RunWorkerAsync(filePath);

            if (!mWindowLoadingBar.IsEnabled)
                _ = Task.Run(() => { Application.Current.Dispatcher.Invoke(() => { mWindowLoadingBar.ShowDialog(); }); });
            //mWindowLoadingBar.ShowDialog();
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
                        mRoutedEventHandlerButtonClickJumpToDescription,
                        mRoutedEventHandlerButtonClickJumpToExtra1,
                        mRoutedEventHandlerButtonClickJumpToExtra2,
                        FileItemProperties.HasDescription,
                        FileItemProperties.HasExtras
                        ));
            }

            mWindowLoadingBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorkerStoredItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).InitializeItemSearch();

            // Close loading bar
            if (mWindowLoadingBar != null && mWindowLoadingBar.IsActive)
                mWindowLoadingBar.Close();
        }        

        private void BackgroundWorkerFoundItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument as string;

            // Load Data
            FileStream fileStream   = File.OpenRead(fileName);
            byte[] bytes            = new byte[fileStream.Length];
            int readBytesAmount     = fileStream.Read(bytes, 0, (int)fileStream.Length);

            //string byteString = Encoding.GetEncoding(1252).GetString(bytes);

            int itemStartPosition = -1, itemNameStartPosition = -1, itemNameLength = -1,
                itemDescriptionStartPosition = -1, itemDescriptionLength = -1,
                itemExtra1LengthPosition = -1, itemExtra1StartPosition = -1, itemExtra1Length = -1,
                itemExtra2StartPosition = -1, itemExtra2Length = -1/*, zeroCounter = 0, stopAtItem = 1*/;
            DataCollectionStates dataCollectionState = DataCollectionStates.FIND_NAME_LENGTH;
            ItemData currentItem = null;

            //List<ItemData> items = new List<ItemData>();
            mFoundItemsToAdd = new List<ItemData>();

            mDispatcher.Invoke(() =>
            {
                mWindowLoadingBar.Maximum = bytes.Length;
                mBackgroundWorkerFoundItemData.ReportProgress(0);

                mOpenFileData = bytes;
                //Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);
            });

            int report = 0, startOffset = 0, idOffset = 0;

            switch (FileItemProperties.FileName)
            {
                case FileName.ACTIONINFO:
                    startOffset = FileItemProperties.LengthPerItem + Constants.ACTIONINFO_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.ACTIONINFO_DATA_ID_OFFSET;
                    break;
                case FileName.ALOGINEXE:
                    startOffset = 0;
                    break;
                case FileName.SCENEDATA:
                    startOffset = FileItemProperties.LengthPerItem + Constants.SCENEDATA_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.SCENEDATA_DATA_ID_OFFSET;
                    break;
                case FileName.TALK:
                    startOffset = FileItemProperties.LengthPerItem + Constants.TALK_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.TALK_DATA_ID_OFFSET;
                    break;
                default:
                    startOffset = FileItemProperties.LengthPerItem;
                    break;
            }

            // Find the rest of the items' info
            for (int i = startOffset; i < bytes.Length; ++i)
            {                
                byte currentByte = bytes[i];

                // Find start of item
                if (currentByte != '\0')
                {
                    //zeroCounter = 0;

                    if (dataCollectionState == DataCollectionStates.FIND_NAME_LENGTH)
                    {
                        itemStartPosition               = i;
                        dataCollectionState             = DataCollectionStates.FIND_NAME_AND_ID;
                        currentItem                     = new ItemData();
                        currentItem.ItemStartPosition   = itemStartPosition;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_NAME_AND_ID)
                    {
                        itemNameStartPosition = i;
                        itemNameLength = bytes[itemStartPosition];

                        // Break if outside boundaries
                        if (itemNameStartPosition + itemNameLength >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        StoreDataIntoItem(ref currentItem, ref bytes, itemNameStartPosition, itemNameLength, dataCollectionState,
                            FileItemProperties.DataFromRightToLeft);
                        StoreIDDataIntoItem(ref currentItem, ref bytes, idOffset);

                        // Decide if description should be found of not
                        if (FileItemProperties.AfterNameToDescriptionLength > 0)
                        {
                            dataCollectionState = DataCollectionStates.FIND_DESCRIPTION_LENGTH;
                            i += itemNameLength;
                        }
                        else
                        {
                            dataCollectionState = DataCollectionStates.FIND_NAME_LENGTH;
                            i = currentItem.ItemStartPosition + FileItemProperties.LengthPerItem - 1;

                            if (i < bytes.Length)
                                currentItem.ItemEndPosition = i;
                            else
                                currentItem.ItemEndPosition = bytes.Length - 1;

                            AddItemDataToListThatAddsThemToFoundItemsListBox(ref currentItem);
                        }
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_DESCRIPTION_LENGTH)
                    {
                        i += FileItemProperties.AfterNameToDescriptionLength - 1;
                        itemDescriptionLength = bytes[i];

                        // If the description length becomes 0, decrease i by 1 and get the new description length from that position
                        while (itemDescriptionLength == 0 && i > currentItem.IDEndPosition)
                        {
                            --i;
                            itemDescriptionLength = bytes[i];

                            Console.WriteLine("WARNING: Item \"" + currentItem.Name + "\"'s description length was 0 and is now of length "
                                + itemDescriptionLength.ToString());
                        }

                        currentItem.DescriptionLengthPosition = i;
                        dataCollectionState = DataCollectionStates.FIND_DESCRIPTION;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_DESCRIPTION)
                    {
                        itemDescriptionStartPosition = i;

                        // Break if outside boundaries
                        if (itemDescriptionStartPosition + itemDescriptionLength >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        StoreDataIntoItem(ref currentItem, ref bytes, itemDescriptionStartPosition, itemDescriptionLength, dataCollectionState,
                            FileItemProperties.DataFromRightToLeft);

                        i = currentItem.ItemStartPosition + FileItemProperties.LengthPerItem - 1;

                        if (FileItemProperties.FileName != FileName.MARK)
                        {
                            dataCollectionState = DataCollectionStates.FIND_NAME_LENGTH;
                            currentItem.ItemEndPosition = i;
                            AddItemDataToListThatAddsThemToFoundItemsListBox(ref currentItem);
                        }
                        else
                            dataCollectionState = DataCollectionStates.FIND_EXTRA1_LENGTH;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_EXTRA1_LENGTH)
                    {
                        itemExtra1LengthPosition = i;
                        dataCollectionState = DataCollectionStates.FIND_EXTRA1;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_EXTRA1)
                    {
                        itemExtra1StartPosition = i;
                        itemExtra1Length = bytes[itemExtra1LengthPosition];

                        // Break if outside boundaries
                        if (itemExtra1StartPosition + itemExtra1Length >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        StoreDataIntoItem(ref currentItem, ref bytes, itemExtra1StartPosition, itemExtra1Length, DataCollectionStates.FIND_EXTRA1,
                            FileItemProperties.DataFromRightToLeft);
                        currentItem.Extra1LengthPosition = itemExtra1LengthPosition;

                        i += itemExtra1Length;
                        dataCollectionState = DataCollectionStates.FIND_EXTRA2_LENGTH;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_EXTRA2_LENGTH)
                    {
                        i += FileItemProperties.AfterNameToDescriptionLength - 1;
                        itemExtra2Length = bytes[i];

                        // If the description length becomes 0, decrease i by 1 and get the new description length from that position
                        while (itemExtra2Length == 0 && i > currentItem.DescriptionEndPosition)
                        {
                            --i;
                            itemExtra2Length = bytes[i];

                            Console.WriteLine("WARNING: Item \"" + currentItem.Name + "\"'s extra 2 length was 0 and is now of length "
                                + itemDescriptionLength.ToString());
                        }

                        currentItem.Extra2LengthPosition = i;
                        dataCollectionState = DataCollectionStates.FIND_EXTRA2;
                    }
                    else if (dataCollectionState == DataCollectionStates.FIND_EXTRA2)
                    {
                        itemExtra2StartPosition = i;

                        // Break if outside boundaries
                        if (itemExtra2StartPosition + itemExtra2Length >= bytes.Length)
                        {
                            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);
                            break;
                        }

                        StoreDataIntoItem(ref currentItem, ref bytes, itemExtra2StartPosition, itemExtra2Length, dataCollectionState,
                            FileItemProperties.DataFromRightToLeft);

                        i = currentItem.ItemStartPosition + FileItemProperties.LengthPerItem * 2 - 1;

                        dataCollectionState = DataCollectionStates.FIND_NAME_LENGTH;
                        currentItem.ItemEndPosition = i;
                        AddItemDataToListThatAddsThemToFoundItemsListBox(ref currentItem);
                    }
                }

                ++report;
                if (report > Constants.LOADING_DATA_PER_REPORT)
                {
                    mBackgroundWorkerFoundItemData.ReportProgress(i);
                    //Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);
                    report = 0;
                }
            }

            mBackgroundWorkerFoundItemData.ReportProgress(bytes.Length);

            e.Result = fileName;
        }

        private void StoreDataIntoItem(ref ItemData item, ref byte[] bytes, int startPosition, int length,
            DataCollectionStates dataCollectionState, bool dataFromRightToLeft)
        {
            // Get the item data
            string data = "";
            for (int j = startPosition; j < startPosition + length; ++j)
                data += (char)bytes[j];

            //// Remove chars from the name until no illegal chars or initial non-althabetical or numerical chars are left
            //while (!TextManager.IsStringContainingOnlyQuestionMarks(data) && !(data == ")???(") &&
            //    (TextManager.IsStringContainingIllegalChar(data) ||
            //    TextManager.IsStringEndingWithNonNumberOrLetterOrParenthesesChar(data)))
            //{
            //    data = data.Remove(data.Length - 1);
            //    --length;
            //}

            //// If the description doesn't end there it should, increase the description until it's fully retrieved
            //while (!TextManager.IsStringContainingNonConventionalChar(((char)bytes[j]).ToString())/*(char)bytes[j] != '\u0090' && (char)bytes[j] != '\u0093' && (char)bytes[j] != '\u2013'*/)
            //{
            //    itemDescription += (char)bytes[j];
            //    ++itemDescriptionLength;
            //    ++j;
            //}

            // Clean the item data from bad chars
            data = TextManager.CleanStringFromNewLinesAndBadChars(data);

            switch (dataCollectionState)
            {
                case DataCollectionStates.FIND_NAME_AND_ID:
                    if (dataFromRightToLeft)
                        item.NameReversed           = data;
                    else
                        item.Name                   = data;
                    item.NameStartPosition          = startPosition;
                    item.NameEndPosition            = startPosition + length;
                    break;
                case DataCollectionStates.FIND_DESCRIPTION:
                    if (dataFromRightToLeft)
                        item.DescriptionReversed    = data;
                    else
                        item.Description            = data;
                    item.DescriptionStartPosition   = startPosition;
                    item.DescriptionEndPosition     = startPosition + length;
                    break;
                case DataCollectionStates.FIND_EXTRA1:
                    if (dataFromRightToLeft)
                        item.Extra1Reversed         = data;
                    else
                        item.Extra1                 = data;
                    item.Extra1StartPosition        = startPosition;
                    item.Extra1EndPosition          = startPosition + length;
                    break;
                case DataCollectionStates.FIND_EXTRA2:
                    if (dataFromRightToLeft)
                        item.Extra2Reversed         = data;
                    else
                        item.Extra2                 = data;
                    item.Extra2StartPosition        = startPosition;
                    item.Extra2EndPosition          = startPosition + length;
                    break;
            }
        }

        private void StoreIDDataIntoItem(ref ItemData item, ref byte[] bytes, int idOffset)
        {
            // Extract the item ID
            int nameEndPositionWithOffset;
            if (FileItemProperties.FileName == FileName.ACTIONINFO)
                nameEndPositionWithOffset = item.ItemStartPosition  + idOffset;
            else
                nameEndPositionWithOffset = item.NameEndPosition    + idOffset;

            while (bytes.Length <= nameEndPositionWithOffset + 2)
                nameEndPositionWithOffset--;

            item.ID = new int[]
            {
                bytes[nameEndPositionWithOffset    ],
                bytes[nameEndPositionWithOffset + 1],
                bytes[nameEndPositionWithOffset + 2]
            };
            item.IDStartPosition = nameEndPositionWithOffset;
            item.IDEndPosition   = nameEndPositionWithOffset + 2;
        }

        private void AddItemDataToListThatAddsThemToFoundItemsListBox(ref ItemData itemData)
        {
            mFoundItemsToAddSemaphore.WaitOne();
                int itemEndPosition = -1;
                if (mFoundItemsToAdd.Count > 0 && (itemEndPosition = mFoundItemsToAdd.Last().ItemEndPosition)
                    >= itemData.ItemStartPosition)
                    ;
                mFoundItemsToAdd.Add(itemData);
            mFoundItemsToAddSemaphore.Release();
        }

        // The name length affects where the id of the item is located. This function gathers all data based on the data in the item already.
        public void UpdateItemInfoBasedOnNewNameLength(Item item)
        {
            // Update name end position
            item.NameEndPosition            = item.NameStartPosition    + item.Name.Length;

            // Update ID
            UpdateItemID(ref item);

            // Update description
            if (FileItemProperties.AfterNameToDescriptionLength > 0)
            {
                item.DescriptionLengthPosition = item.NameEndPosition + FileItemProperties.AfterNameToDescriptionLength;
                int descriptionLength = mOpenFileText[item.DescriptionLengthPosition];

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
                item.DescriptionEndPosition = i + descriptionLength;
            }
        }

        public void UpdateItemID(ref Item item)
        {
            int offset = 0;
            switch (FileItemProperties.FileName)
            {
                case FileName.ACTIONINFO:
                    offset = Constants.ACTIONINFO_DATA_ID_OFFSET;
                    break;
                case FileName.SCENEDATA:
                    offset = Constants.SCENEDATA_DATA_ID_OFFSET;
                    break;
                case FileName.TALK:
                    offset = Constants.TALK_DATA_ID_OFFSET;
                    break;
            }

            int nameEndPositionWithOffset;
            if (FileItemProperties.FileName == FileName.ACTIONINFO)
                nameEndPositionWithOffset = item.ItemStartPosition + offset;
            else
                nameEndPositionWithOffset = item.NameEndPosition + offset;

            while (mOpenFileData.Length <= nameEndPositionWithOffset + 2)
                --nameEndPositionWithOffset;

            // Extract the item ID
            item.ID                 = new int[]
            {
                mOpenFileData[nameEndPositionWithOffset],
                mOpenFileData[nameEndPositionWithOffset + 1],
                mOpenFileData[nameEndPositionWithOffset + 2]
            };
            item.IDStartPosition    = nameEndPositionWithOffset;
            item.IDEndPosition      = nameEndPositionWithOffset + 2;
        }

        public bool OpenFileToTranslate(string filePath)
        {
            if (mBackgroundWorkerFoundItemData != null)
            {
                mBackgroundWorkerFoundItemData.Dispose();
                mBackgroundWorkerFoundItemData = null;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
            switch (fileName)
            {
                case "actioninfo":
                    LoadedFileName = FileName.ACTIONINFO;
                    break;
                case "alogin":
                    LoadedFileName = FileName.ALOGINEXE;
                    break;
                case "item":
                    LoadedFileName = FileName.ITEM;
                    break;
                case "mark":
                    LoadedFileName = FileName.MARK;
                    break;
                case "npc":
                    LoadedFileName = FileName.NPC;
                    break;
                case "scenedata":
                    LoadedFileName = FileName.SCENEDATA;
                    break;
                case "skill":
                    LoadedFileName = FileName.SKILL;
                    break;                              
                case "talk":
                    LoadedFileName = FileName.TALK;
                    break;
                default:
                    MessageBox.Show("No Game Files of That Name Recognized by this Translator!");
                    return false;
            }

            FileItemProperties = new FileItemProperties(LoadedFileName);

            mWindowLoadingBar = new WindowLoadingBar("Loading Data", "Loading Item Data From File", "Loaded: ", 1);
            mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            mWindowLoadingBar.Owner = Application.Current.MainWindow;

            // Load in text from file
            mScintillaWPFTextBox.Scintilla.Text = "";
            mListBoxFoundItems.Items.Clear();
            mListBoxStoredItems.Items.Clear();

            string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(1252));
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
            mBackgroundWorkerFoundItemData.RunWorkerAsync(filePath);

            _ = Task.Run(() => { Application.Current.Dispatcher.Invoke(() => { mWindowLoadingBar.ShowDialog(); }); });
            //mWindowLoadingBar.ShowDialog();
            return true;
        }

        public void CloseFileToTranslate()
        {
            // Load in text from file
            mScintillaWPFTextBox.ReadOnly       = false;
            mScintillaWPFTextBox.Scintilla.Text = "";
            mScintillaWPFTextBox.ReadOnly       = true;
            mListBoxFoundItems.Items.Clear();
            mListBoxStoredItems.Items.Clear();

            ItemSearch.ClearStoredItemsWhileSearching();

            mOpenFileText = null;
            mOpenFileData = null;
        }

        private void BackgroundWorkerFoundItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);

                mDispatcher.Invoke(() =>
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
                            mRoutedEventHandlerButtonClickJumpToDescription,
                            mRoutedEventHandlerButtonClickJumpToExtra1,
                            mRoutedEventHandlerButtonClickJumpToExtra2,
                            FileItemProperties.HasDescription,
                            FileItemProperties.HasExtras));

                        if (mListBoxFoundItems.Items.Count > 1 &&
                            (mListBoxFoundItems.Items[mListBoxFoundItems.Items.Count - 2] as Item).ItemEndPosition >=
                            (mListBoxFoundItems.Items[mListBoxFoundItems.Items.Count - 1] as Item).ItemStartPosition)
                            throw new ArgumentException("ItemEndPosition >= ItemStartPosition");
                    }

                    mWindowLoadingBar.Value = e.ProgressPercentage;
                });
            });
        }

        private void BackgroundWorkerFoundItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadItemData(e.Result as string);
        }

        public string ExportFile()
        {
            SaveFileDialog saveFileDialog   = new SaveFileDialog();
            saveFileDialog.Filter           = "Data files (*.dat)|*.dat|Alla filer (*.*)|*.*";
            saveFileDialog.Title            = "Export File";
            saveFileDialog.DefaultExt       = ".dat";
            saveFileDialog.FileName         = TextManager.GetFileNameWithFirstLettersCapitalized(FileItemProperties.FileName) + ".dat";
            var result = saveFileDialog.ShowDialog();

            if (result.Value)
            {
                string text;
                if (TranslatedFileText != null && TranslatedFileText != "")
                    text = TranslatedFileText;
                else if (OpenFileText != null && OpenFileText != "")
                    text = OpenFileText;
                else
                    return null;

                File.WriteAllText(saveFileDialog.FileName, text, Encoding.GetEncoding(1252));

                return saveFileDialog.FileName;
            }

            return "cancelled";
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
