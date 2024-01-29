﻿using Microsoft.Win32;
using ScintillaNET.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml;

namespace WLO_Translator_WPF
{
    public enum FileType
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
        public FileType FileType                        { get; private set; } = FileType.NULL;
        public int      LengthPerItem                   { get; private set; } = -1;
        public int      AfterNameToDescriptionLength    { get; private set; } = -1;
        public bool     HasDescription                  { get; private set; } = true;
        public bool     HasExtras                       { get; private set; } = false;
        public bool     DataFromRightToLeft             { get; private set; } = true;

        public FileItemProperties(FileType fileType)
        {
            FileType = fileType;
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            switch (FileType)
            {
                case FileType.ACTIONINFO:
                    LengthPerItem                   = Constants.ACTIONINFO_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ACTIONINFO_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;
                case FileType.ALOGINEXE:
                    LengthPerItem                   = Constants.ALOGIN_EXE_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ALOGIN_EXE_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    DataFromRightToLeft             = false;
                    break;
                case FileType.ITEM:
                    LengthPerItem                   = Constants.ITEM_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.ITEM_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;                
                case FileType.MARK:
                    LengthPerItem                   = Constants.MARK_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.MARK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasExtras                       = true;
                    break;
                case FileType.NPC:
                    LengthPerItem                   = Constants.NPC_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.NPC_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
                case FileType.SCENEDATA:
                    LengthPerItem                   = Constants.SCENEDATA_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.SCENEDATA_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
                case FileType.SKILL:
                    LengthPerItem                   = Constants.SKILL_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.SKILL_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    break;
                case FileType.TALK:
                    LengthPerItem                   = Constants.TALK_DATA_LENGTH_PER_ITEM;
                    AfterNameToDescriptionLength    = Constants.TALK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH;
                    HasDescription                  = false;
                    break;
            }
        }
    }

    public class FileManager
    {
        private BackgroundWorker            mBackgroundWorkerStoredItemData;
        private BackgroundWorker            mBackgroundWorkerFoundItemData;
        private List<ItemData>              mFoundItemsToAdd;
        private List<ItemData>              mStoredItemsToAdd;
        public List<ItemData> GetFoundItems()   { return mFoundItemsToAdd;  }
        public List<ItemData> GetStoredItems()  { return mStoredItemsToAdd; }

        private ListBox                     mListBoxFoundItems;
        private ListBox                     mListBoxStoredItems;
        private ObservableCollection<Item>  mItemSourceListBoxFoundItemsTemporary;
        private ObservableCollection<Item>  mItemSourceListBoxStoredItemsTemporary;

        private ScintillaWPF                mScintillaWPFTextBox;

        private RoutedEventHandler          mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToDescription;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToExtra1;
        private RoutedEventHandler          mRoutedEventHandlerButtonClickJumpToExtra2;

        private Dispatcher                  mDispatcher;

        private string                      mOpenFileText;
        private byte[]                      mOpenFileData;
        public  string                      OpenFileText { get => mOpenFileText; private set => mOpenFileText = value; }


        private string                      mTranslatedFileText;
        public  string                      TranslatedFileText { get => mTranslatedFileText; set => mTranslatedFileText = value; }

        public  FileItemProperties          FileItemProperties { get; private set; }

        public  FileType                    LoadedFileName { get; private set; } = FileType.NULL;

        //private Database                    mDatabase;

        public  bool                        IsFileOpenToTranslateRunning            { get; private set; } = false;
        //private bool                        IsFoundItemDataCollectionCompleted      { get; set;         } = false;
        private bool                        IsStoredItemDataToItemsProcessRunning   { get; set;         } = false;

        private Label                       mLabelReportInfo;

        // <summary>
        // Provides utility methods for suspending and resuming the UI update of a ListBox in WPF.
        // </summary>
        public static class ListBoxUpdateSuspender
        {
            // <summary>
            // Suspends the UI update of a ListBox.
            //
            // Parameters:
            // - listBox: The ListBox control whose UI update needs to be suspended.
            // </summary>
            public static void SuspendUpdate(ListBox listBox)
            {
                // Get the handle of the ListBox control.
                IntPtr handle = ((HwndSource)PresentationSource.FromVisual(listBox)).Handle;

                // Suspend the UI update of the ListBox.
                SendMessage(handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }

            // <summary>
            // Resumes the UI update of a ListBox.
            //
            // Parameters:
            // - listBox: The ListBox control whose UI update needs to be resumed.
            // </summary>
            public static void ResumeUpdate(ListBox listBox)
            {
                // Get the handle of the ListBox control.
                IntPtr handle = ((HwndSource)PresentationSource.FromVisual(listBox)).Handle;

                // Resume the UI update of the ListBox.
                SendMessage(handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);

                // Refresh the ListBox to update its UI.
                listBox.InvalidateVisual();
            }

            // <summary>
            // Sends a message to a window or control.
            //
            // Parameters:
            // - hWnd: The handle to the window or control.
            // - msg: The message to send.
            // - wParam: Additional message-specific information.
            // - lParam: Additional message-specific information.
            //
            // Returns:
            // - The result of the message processing; it depends on the message sent.
            // </summary>
            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            private const int WM_SETREDRAW = 0x000B;
        }

        // Save File Dialog
        //private static SaveFileDialog mSaveFileDialog = new SaveFileDialog();

        public FileManager(Dispatcher mainWindowDispatcher, ref ScintillaWPF scintillaWPFTextBox, ref Label labelReportInto,
            ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems,
            RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2)
        {
            mDispatcher                                     = mainWindowDispatcher;
            mScintillaWPFTextBox                            = scintillaWPFTextBox;
            mLabelReportInfo                                = labelReportInto;
            mListBoxFoundItems                              = listBoxFoundItems;
            mListBoxStoredItems                             = listBoxStoredItems;
            mRoutedEventHandlerButtonClickUpdateItem        = routedEventHandlerButtonClickUpdateItem;
            mRoutedEventHandlerButtonClickJumpToWholeItem   = routedEventHandlerButtonClickJumpToWholeItem;
            mRoutedEventHandlerButtonClickJumpToID          = routedEventHandlerButtonClickJumpToID;
            mRoutedEventHandlerButtonClickJumpToName        = routedEventHandlerButtonClickJumpToName;
            mRoutedEventHandlerButtonClickJumpToDescription = routedEventHandlerButtonClickJumpToDescription;
            mRoutedEventHandlerButtonClickJumpToExtra1      = routedEventHandlerButtonClickJumpToExtra1;
            mRoutedEventHandlerButtonClickJumpToExtra2      = routedEventHandlerButtonClickJumpToExtra2;

            string dataDictionary = Path.GetFullPath(Path.Combine(Paths.SourceFolder, @"..\..\"));
#if !DEBUG
            //path = Paths.SourceFolder;
#endif
            //mDatabase = new Database("(LocalDB)\\MSSQLLocalDB",
            //    dataDictionary/*"|DataDirectory|"*/ + "IO\\DatabaseProgramSettings.mdf",
            //    "DatabaseProgramSettings", // Server name
            //    "sa", //DESKTOP-9ANKOSD\\ChriW
            //    "svmp@9108",
            //    true);

            //mDatabase.Connect();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void SaveProgramSettings()
        {
            if (!File.Exists(Paths.ProgramSettingsPath))
            {
                if (!Directory.Exists(Paths.RoamingFolder))
                    Paths.GenerateRoamingFolders();
                FileStream fileStream = File.Create(Paths.ProgramSettingsPath);
                fileStream.Close();
            }

            XmlWriterSettings settings  = new XmlWriterSettings();
            settings.Indent             = true;
            settings.IndentChars        = "\t";
            XmlWriter writer            = XmlWriter.Create(Paths.ProgramSettingsPath, settings);

            try
            {
                writer.WriteStartElement("ProgramSettings");

                    writer.WriteStartElement("SelectedTheme");

                        writer.WriteAttributeString("Index", ThemeManager.ThemeIndex.ToString());

                    writer.WriteEndElement();

                    writer.WriteStartElement("MultiTranslator");

                        writer.WriteAttributeString("MultiTranslatorItemDataPath",      Settings.MultiTranslatorItemDataPath);
                        writer.WriteAttributeString("MultiTranslatorMarkDataPath",      Settings.MultiTranslatorMarkDataPath);
                        writer.WriteAttributeString("MultiTranslatorNpcDataPath",       Settings.MultiTranslatorNpcDataPath);
                        writer.WriteAttributeString("MultiTranslatorSceneDataDataPath", Settings.MultiTranslatorSceneDataDataPath);
                        writer.WriteAttributeString("MultiTranslatorSkillDataPath",     Settings.MultiTranslatorSkillDataPath);
                        writer.WriteAttributeString("MultiTranslatorTalkDataPath",      Settings.MultiTranslatorTalkDataPath);

                    writer.WriteEndElement();

                writer.WriteEndElement();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            writer.Close();
        }

        public void LoadProgramSettings()
        {
            // Load program settings, if file exists
            if (File.Exists(Paths.ProgramSettingsPath))
            {
                mStoredItemsToAdd = new List<ItemData>();
                XmlReader reader = XmlReader.Create(Paths.ProgramSettingsPath);

                try
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "SelectedTheme")
                        {
                            if (reader.GetAttribute("Index") != null)
                                ThemeManager.ApplyTheme(int.Parse(reader.GetAttribute("Index")));
                        }

                        if (reader.Name == "MultiTranslator")
                        {
                            if (reader.GetAttribute("MultiTranslatorItemDataPath") != null)
                                Settings.MultiTranslatorItemDataPath        = reader.GetAttribute("MultiTranslatorItemDataPath");
                            if (reader.GetAttribute("MultiTranslatorMarkDataPath") != null)
                                Settings.MultiTranslatorMarkDataPath        = reader.GetAttribute("MultiTranslatorMarkDataPath");
                            if (reader.GetAttribute("MultiTranslatorNpcDataPath") != null)
                                Settings.MultiTranslatorNpcDataPath         = reader.GetAttribute("MultiTranslatorNpcDataPath");
                            if (reader.GetAttribute("MultiTranslatorSceneDataDataPath") != null)
                                Settings.MultiTranslatorSceneDataDataPath   = reader.GetAttribute("MultiTranslatorSceneDataDataPath");
                            if (reader.GetAttribute("MultiTranslatorSkillDataPath") != null)
                                Settings.MultiTranslatorSkillDataPath       = reader.GetAttribute("MultiTranslatorSkillDataPath");
                            if (reader.GetAttribute("MultiTranslatorTalkDataPath") != null)
                                Settings.MultiTranslatorTalkDataPath        = reader.GetAttribute("MultiTranslatorTalkDataPath");
                        }
                    }

                    reader.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception: " + e.Message);
                    Console.WriteLine("Exception: " + e.Message);
                    reader.Close();
                }
            }
            else
                Console.WriteLine("Error: " + "The file \"" + Paths.ProgramSettingsPath + "\" couldn't be opened");
        }

        /// <summary>
        /// Saves the associated stored item data
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="storedItems"></param>
        /// <param name="labelSaved"></param>
        public void SaveStoredItemData(FileType fileName, List<IItemBase> storedItems)
        {
            string saveFilePath = Paths.AssociatedStoredItemData + TextManager.GetFileTypeToString(fileName);
            string saveFilePathWithExtension = saveFilePath + Constants.FILE_ENDING_STORED_ITEM_DATA;
            if (!File.Exists(saveFilePathWithExtension))
            {
                if (!Directory.Exists(Paths.AssociatedStoredItemData))
                    Paths.GenerateRoamingFolders();
                FileStream fileStream = File.Create(saveFilePathWithExtension);
                fileStream.Close();
            }

            XmlWriterSettings settings  = new XmlWriterSettings();
            settings.Indent             = true;
            settings.IndentChars        = "\t";
            XmlWriter writer            = XmlWriter.Create(saveFilePathWithExtension, settings);

            try
            {
                writer.WriteStartElement("AssociatedStoredItemData");

                    writer.WriteStartElement("Items");

                        writer.WriteAttributeString("ItemCount", storedItems.Count.ToString());

                        for (int i = 0; i < storedItems.Count; i++)
                        {
                            IItemBase currentItem = storedItems[i];

                            writer.WriteStartElement("Item");

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

            mLabelReportInfo.Content = TextManager.GetFileTypeToString(fileName) + " Stored Items " + " Saved";
        }
        /// <summary>
        /// Loads the associated stored item data
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isMultiTranslator"></param>
        private void LoadStoredItemData(string fileName, bool isMultiTranslator)
        {
            string savefilePath = Paths.AssociatedStoredItemData + Path.GetFileNameWithoutExtension(fileName)
                + Constants.FILE_ENDING_STORED_ITEM_DATA;

            double maximumStoredItems = -1, progress = 0;

            // Load last saved data, if file exists
            if (File.Exists(savefilePath))
            {
                mStoredItemsToAdd = new List<ItemData>();
                List<ItemData> storedItemsToAdd = new List<ItemData>();
                XmlReader reader = XmlReader.Create(savefilePath);

                try
                {
                    DateTime lastReportedTime = DateTime.Now;

                    while (reader.Read())
                    {
                        if (reader.Name == "Items")
                        {
                            if (reader.GetAttribute("ItemCount") != null)
                            {
                                maximumStoredItems = double.Parse(reader.GetAttribute("ItemCount"));// });

                                if (!IsStoredItemDataToItemsProcessRunning && IsFileOpenToTranslateRunning)
                                {
                                    mDispatcher.Invoke(() => LoadingBarManager.ShowOrInitializeLoadingBar("Stored", 0d,
                                        FileItemProperties, maximumStoredItems));
                                    LoadingBarManager.WaitUntilValueHasChanged();
                                    IsStoredItemDataToItemsProcessRunning = true;
                                }
                            }
                        }
                        else if (reader.Name == "Item")
                        {                            
                            ItemData currentItemData = new ItemData();

                            if (reader.GetAttribute("ID3") != null && reader.GetAttribute("ID2") != null
                                && reader.GetAttribute("ID1") != null)
                                currentItemData.ID = new int[] { int.Parse(reader.GetAttribute("ID1")),
                                    int.Parse(reader.GetAttribute("ID2")),
                                int.Parse(reader.GetAttribute("ID3"))};
                            else if (reader.GetAttribute("ID1") != null && reader.GetAttribute("ID2") != null)
                                currentItemData.ID = new int[] { int.Parse(reader.GetAttribute("ID1")),
                                    int.Parse(reader.GetAttribute("ID2")), 0};

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

                            storedItemsToAdd.Add(currentItemData);

                            if (lastReportedTime.AddSeconds(0.02) < DateTime.Now)
                            {
                                progress += storedItemsToAdd.Count;
                                ReportProgress(ref storedItemsToAdd, (int)progress, isMultiTranslator, true);
                                lastReportedTime = DateTime.Now;
                            }
                        }
                    }

                    ReportProgress(ref storedItemsToAdd, (int)maximumStoredItems, isMultiTranslator, true);

                    reader.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception: " + e.Message);
                    Console.WriteLine("Exception: " + e.Message);
                    reader.Close();

                    ReportProgress(ref storedItemsToAdd, (int)maximumStoredItems, isMultiTranslator, true);
                }
            }
            else
                Console.WriteLine("Error: " + "The file \"" + savefilePath + "\" couldn't be opened");
        }

        public bool OpenFileToTranslate(string filePath, bool isMultiTranslator = false)
        {
            IsFileOpenToTranslateRunning            = true;
            //IsFoundItemDataCollectionCompleted      = false;
            IsStoredItemDataToItemsProcessRunning   = false;

            if (mBackgroundWorkerFoundItemData != null)
            {
                mBackgroundWorkerFoundItemData.Dispose();
                mBackgroundWorkerFoundItemData = null;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
            switch (fileName)
            {
                case "actioninfo":
                    LoadedFileName = FileType.ACTIONINFO;
                    break;
                case "alogin":
                    LoadedFileName = FileType.ALOGINEXE;
                    break;
                case "item":
                    LoadedFileName = FileType.ITEM;
                    break;
                case "mark":
                    LoadedFileName = FileType.MARK;
                    break;
                case "npc":
                    LoadedFileName = FileType.NPC;
                    break;
                case "scenedata":
                    LoadedFileName = FileType.SCENEDATA;
                    break;
                case "skill":
                    LoadedFileName = FileType.SKILL;
                    break;                              
                case "talk":
                    LoadedFileName = FileType.TALK;
                    break;
                default:
                    MessageBox.Show("No Game Files of That Name Recognized by this Translator!");
                    return false;
            }

            FileItemProperties = new FileItemProperties(LoadedFileName);

            // Load in text from file
            string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(1252));
            mOpenFileText   = fileText;

            // Set file text to the Scintilla text box file viewer
            if (!isMultiTranslator)
            {
                mScintillaWPFTextBox.Scintilla.Text     = "";
                mListBoxFoundItems.ItemsSource          = new ObservableCollection<Item>();
                mListBoxStoredItems.ItemsSource         = new ObservableCollection<Item>();
                mItemSourceListBoxFoundItemsTemporary   = new ObservableCollection<Item>();
                mItemSourceListBoxStoredItemsTemporary  = new ObservableCollection<Item>();

                fileText = TextManager.CleanStringFromNewLinesAndBadChars(fileText);
                _ = Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mScintillaWPFTextBox.Scintilla.SuspendLayout();
                        mScintillaWPFTextBox.ReadOnly = false;

                        mScintillaWPFTextBox.Scintilla.Text = fileText;

                        mScintillaWPFTextBox.ReadOnly = true;
                        mScintillaWPFTextBox.Scintilla.ResumeLayout();
                    });
                });
            }

            // Extract item data from the loaded file
            mBackgroundWorkerFoundItemData = new BackgroundWorker(); // () => ThreadOpenFileToTranslate(fileName)
            mBackgroundWorkerFoundItemData.WorkerReportsProgress = true;
            mBackgroundWorkerFoundItemData.DoWork             += BackgroundWorkerFoundItemData_DoWork;
            mBackgroundWorkerFoundItemData.ProgressChanged    += BackgroundWorkerFoundItemData_ProgressChanged;
            mBackgroundWorkerFoundItemData.RunWorkerCompleted += BackgroundWorkerFoundItemData_RunWorkerCompleted;
            mBackgroundWorkerFoundItemData.RunWorkerAsync(new object[] { filePath, isMultiTranslator });
            
            return true;
        }        

        #region Background worker found item data
        private void BackgroundWorkerFoundItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(250);

            object[] arguments = e.Argument as object[];
            string fileName = arguments[0] as string;
            bool isMultiTranslator = (bool)arguments[1];

            CollectFoundItemDataAsync(fileName, isMultiTranslator).Wait();

            e.Result = new object[] { fileName, isMultiTranslator };
        }

        private class ItemDataPart
        {
            public int StartIndex   { get; private set; }
            public int EndIndex     { get; private set; }

            public ItemDataPart(int startIndex, int endIndex)
            {
                StartIndex  = startIndex;
                EndIndex    = endIndex;
            }
        }

        public async Task CollectFoundItemDataAsync(string fileName, bool isMultiTranslator)
        {
            Thread.Sleep(50);

            // Load Data
            FileStream fileStream = File.OpenRead(fileName);
            byte[] bytes = new byte[fileStream.Length];
            int readBytesAmount = fileStream.Read(bytes, 0, (int)fileStream.Length);

            mFoundItemsToAdd = new List<ItemData>();
            List<ItemData> foundItemsToAdd = new List<ItemData>();

            mDispatcher.Invoke(() =>
            {
                LoadingBarManager.ShowOrInitializeLoadingBar("Found", 0d, FileItemProperties, bytes.Length);
                mOpenFileData = bytes;
            });

            LoadingBarManager.WaitUntilValueHasChanged();

            int startOffset = 0, idOffset = 0;
            DateTime lastReportedTime = DateTime.Now;

            switch (FileItemProperties.FileType)
            {
                case FileType.ACTIONINFO:
                    startOffset = FileItemProperties.LengthPerItem + Constants.ACTIONINFO_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.ACTIONINFO_DATA_ID_OFFSET;
                    break;
                case FileType.ALOGINEXE:
                    startOffset = 0;
                    break;
                case FileType.SCENEDATA:
                    startOffset = FileItemProperties.LengthPerItem + Constants.SCENEDATA_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.SCENEDATA_DATA_ID_OFFSET;
                    break;
                case FileType.TALK:
                    startOffset = FileItemProperties.LengthPerItem + Constants.TALK_DATA_INITIAL_OFFSET;
                    idOffset    = Constants.TALK_DATA_ID_OFFSET;
                    break;
                default:
                    startOffset = FileItemProperties.LengthPerItem;
                    break;
            }

            List<Task<List<ItemData>>> itemDataTasks = new List<Task<List<ItemData>>>();

            int lengthPerItem = FileItemProperties.LengthPerItem;
            if (FileItemProperties.FileType == FileType.MARK)
                lengthPerItem *= 2;
            //int dataPerTask = lengthPerItem * 10;

            // Calculate how many tasks and items per task that it should be
            int taskAmount = 0, maxTasks = 1000;
            if (bytes.Length < 10000)
                taskAmount = 1;
            else if (bytes.Length < 20000)
                taskAmount = 2;
            else if (bytes.Length / 20000 * 32 < maxTasks)
                taskAmount = bytes.Length / 20000 * 32;
            else
                taskAmount = maxTasks;

            int dataPerTask = bytes.Length / taskAmount - bytes.Length / taskAmount % lengthPerItem;

            // Find items' info and store them in ItemData objects
            int i = startOffset;
            Stack<List<ItemDataPart>> itemDataPartStack = new Stack<List<ItemDataPart>>();
            for (; i < bytes.Length; i += dataPerTask)
            {
                // If dataPerTask is too large for the last task(s), make the current task take the rest of the data
                if (i + dataPerTask > bytes.Length)
                    dataPerTask = bytes.Length - i;

                List<ItemDataPart> itemDataParts = new List<ItemDataPart>();
                for (int j = i; j < i + dataPerTask; j += lengthPerItem)
                {
                    itemDataParts.Add(new ItemDataPart(j, j + lengthPerItem));
                }

                itemDataPartStack.Push(itemDataParts);
            }

            while (itemDataPartStack.Count > 0)
            {
                List<ItemDataPart> itemDataParts = itemDataPartStack.Pop();
                itemDataTasks.Add(Task.Run(() =>
                {
                    List<ItemData> itemData = new List<ItemData>();
                    foreach (ItemDataPart itemDataPart in itemDataParts)
                    {
                        itemData.Add(new FoundItemDataCollection(ref bytes, FileItemProperties)
                                .CollectData(itemDataPart.StartIndex, itemDataPart.EndIndex, idOffset));
                    }
                    return itemData;
                }));

                //if (lastReportedTime.AddSeconds(0.1) < DateTime.Now)
                //{
                //    ReportProgress(ref foundItemsToAdd, i, isMultiTranslator, false);
                //    lastReportedTime = DateTime.Now;
                //}
            }

            // Get result
            List<ItemData> result;
            itemDataTasks.Reverse();
            int progress = 0;
            int taskIndex = 0;
            foreach (Task<List<ItemData>> task in itemDataTasks)
            {
                result = await task;
                foreach (ItemData itemData in result)
                {
                    if (itemData != null)
                    {
                        foundItemsToAdd.Add(itemData);
                        //Console.WriteLine("Completed Collection of Found Item With Name: " + itemData.Name);
                    }
                    //else
                        //Console.WriteLine("Uncompleted Collection of Item");
                }
                progress += dataPerTask;
                if (progress >= bytes.Length / 5)
                {
                    ReportProgress(ref foundItemsToAdd, progress, isMultiTranslator, false);
                    progress = 0;
                }
                task.Dispose();
                ++taskIndex;
            }

            ReportProgress(ref foundItemsToAdd, bytes.Length, isMultiTranslator, false);
        }

        private void BackgroundWorkerFoundItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            object[] arguments = (object[])e.UserState;
            List<ItemData> foundItemsToAdd = (List<ItemData>)arguments[0];
            bool isMultiTranslator = (bool)arguments[1];

            _ = StoreItemsInListBoxAsync(mItemSourceListBoxFoundItemsTemporary, foundItemsToAdd, e.ProgressPercentage, false,
                isMultiTranslator);
        }

        private void BackgroundWorkerFoundItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //IsFoundItemDataCollectionCompleted = true;
            object[] arguments = e.Result as object[];
            StartBackgroundWorkerLoadStoredItemData(arguments[0] as string, (bool)arguments[1]);
        }
        #endregion

        public void StartBackgroundWorkerLoadStoredItemData(string filePath, bool isMultiTranslator)
        {
            if (mBackgroundWorkerStoredItemData != null)
            {
                mBackgroundWorkerStoredItemData.Dispose();
                mBackgroundWorkerStoredItemData = null;
            }

            //if (!isMultiTranslator)
            //{
            //    ListBoxUpdateSuspender.SuspendUpdate(mListBoxFoundItems);
            //    ListBoxUpdateSuspender.SuspendUpdate(mListBoxStoredItems);
            //}

            mBackgroundWorkerStoredItemData = new BackgroundWorker();
            mBackgroundWorkerStoredItemData.WorkerReportsProgress = true;
            mBackgroundWorkerStoredItemData.DoWork              += BackgroundWorkerStoredItemData_DoWork;
            mBackgroundWorkerStoredItemData.ProgressChanged     += BackgroundWorkerStoredItemData_ProgressChanged;
            mBackgroundWorkerStoredItemData.RunWorkerCompleted  += BackgroundWorkerStoredItemData_RunWorkerCompleted;
            mBackgroundWorkerStoredItemData.RunWorkerAsync(new object[] { filePath, isMultiTranslator });
        }
        
        #region Background worker stored item data
        private void BackgroundWorkerStoredItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arguments = e.Argument as object[];

            while (!LoadingBarManager.IsLoadingCompleted()/*!IsFoundItemDataCollectionCompleted*/)
                Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);

            LoadStoredItemData(arguments[0] as string, (bool)arguments[1]);

            e.Result = arguments[1];
        }

        private void BackgroundWorkerStoredItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            object[]        arguments           = (object[])e.UserState;
            List<ItemData>  storedItemsToAdd    = (List<ItemData>)arguments[0];
            bool            isMultiTranslator   = (bool)arguments[1];

            _ = StoreItemsInListBoxAsync(mItemSourceListBoxStoredItemsTemporary, storedItemsToAdd, e.ProgressPercentage, true,
                isMultiTranslator);
        }

        private void BackgroundWorkerStoredItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool isMultiTranslator = (bool)e.Result;
            if (!isMultiTranslator)
                ((MainWindow)Application.Current.MainWindow).InitializeItemSearch();
        }
        #endregion

        private void ReportProgress(ref List<ItemData> itemData, int progress, bool isMultiTranslator, bool isStoredItems)
        {
            // Pick background worker
            BackgroundWorker backgroundWorker = mBackgroundWorkerFoundItemData;
            if (isStoredItems)
                backgroundWorker = mBackgroundWorkerStoredItemData;

            // Report progress, clear the list and sleep
            backgroundWorker.ReportProgress(progress, new object[] { itemData.ToList(), isMultiTranslator });
            itemData.Clear();
            int sleepTime = 500;//Constants.LOADING_SLEEP_LENGTH * (isStoredItems ? 2 : 1);

            DateTime waitEnd = DateTime.Now.AddSeconds(5);
            while (!LoadingBarManager.IsValueChanged && waitEnd > DateTime.Now)
                Thread.Sleep(sleepTime);
        }

        // The name length affects where the id of the item is located.
        // This function gathers all data based on the data in the item already.
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
            switch (FileItemProperties.FileType)
            {
                case FileType.ACTIONINFO:
                    offset = Constants.ACTIONINFO_DATA_ID_OFFSET;
                    break;
                case FileType.SCENEDATA:
                    offset = Constants.SCENEDATA_DATA_ID_OFFSET;
                    break;
                case FileType.TALK:
                    offset = Constants.TALK_DATA_ID_OFFSET;
                    break;
            }

            int nameEndPositionWithOffset;
            if (FileItemProperties.FileType == FileType.ACTIONINFO)
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

        public void SetProjectOpenFileName(ref Grid gridProjectFileName, string fileName)
        {
            string name = TextManager.GetFileTypeToString(FileItemProperties.FileType);
            (gridProjectFileName.Children[0] as TextBlock).Text     = name;
            (gridProjectFileName.Children[0] as TextBlock).ToolTip  = fileName;
             gridProjectFileName.Visibility = Visibility.Visible;
        }

        public void RemoveProjectOpenFileName(ref Grid gridProjectFileName)
        {
            (gridProjectFileName.Children[0] as TextBlock).Text     = "";
            (gridProjectFileName.Children[0] as TextBlock).ToolTip  = "";
             gridProjectFileName.Visibility = Visibility.Hidden;
        }

        public void CloseFileToTranslate()
        {
            // Load in text from file
            mScintillaWPFTextBox.ReadOnly       = false;
            mScintillaWPFTextBox.Scintilla.Text = "";
            mScintillaWPFTextBox.ReadOnly       = true;
            mListBoxFoundItems.ItemsSource      = new ObservableCollection<Item>();
            mListBoxStoredItems.ItemsSource     = new ObservableCollection<Item>();
            FileItemProperties = null;

            ItemSearch.ClearStoredItemsWhileSearching();

            mOpenFileText = null;
            mOpenFileData = null;
        }        

        private async Task StoreItemsInListBoxAsync(ObservableCollection<Item> observableItemCollection, List<ItemData> listItemsToAdd,
            double progressPercentage, bool isStoredItems, bool isMultiTranslator)
        {
            if (!isMultiTranslator)
            {
                List<Task<List<Item>>> tasks = new List<Task<List<Item>>>();
                Stack<List<int>> indiceStack = new Stack<List<int>>();

                int itemsPerTask = 80;
                for (int i = 0; i < listItemsToAdd.Count; i += itemsPerTask)
                {
                    if (i + itemsPerTask > listItemsToAdd.Count)
                        itemsPerTask = listItemsToAdd.Count - i;

                    List<int> indice = new List<int>();
                    for (int j = 0; j < itemsPerTask; ++j)
                    {
                        ItemData itemData = listItemsToAdd[i + j];
                        indice.Add(i + j);
                    }

                    indiceStack.Push(indice);
                }

                Random random = new Random();
                indiceStack.Reverse();
                
                while (indiceStack.Count > 0)
                {
                    try
                    {
                        Thread.Sleep(50 + random.Next(0, 50));
                        List<int> indice = indiceStack.Pop();
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                return mDispatcher.Invoke(() =>
                                {
                                    using (var d = mDispatcher.DisableProcessing())
                                    {
                                        List<Item> items = new List<Item>();
                                        foreach (int index in indice)
                                        {
                                            ItemData itemData = listItemsToAdd[index];
                                            items.Add(itemData.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
                                                mRoutedEventHandlerButtonClickJumpToWholeItem,
                                                mRoutedEventHandlerButtonClickJumpToID,
                                                mRoutedEventHandlerButtonClickJumpToName,
                                                mRoutedEventHandlerButtonClickJumpToDescription,
                                                mRoutedEventHandlerButtonClickJumpToExtra1,
                                                mRoutedEventHandlerButtonClickJumpToExtra2,
                                                FileItemProperties.HasDescription,
                                                FileItemProperties.HasExtras));
                                        }

                                        return items;
                                    }
                                });
                            }
                            catch (ArgumentException e)
                            {
                                MessageBox.Show(e.Message);
                                throw e;
                            }
                        }));
                    }
                    catch (ArgumentException e)
                    {
                        MessageBox.Show(e.Message);
                        throw e;
                    }
                }

                List<Item> result;
                tasks.Reverse();

                foreach (Task<List<Item>> task in tasks)
                {
                    result = await task;
                    foreach (Item item in result)
                    {
                        observableItemCollection.Add(item);

                        if (observableItemCollection.Count > 1 &&
                            observableItemCollection[observableItemCollection.Count - 2].ItemEndPosition >=
                            observableItemCollection[observableItemCollection.Count - 1].ItemStartPosition)
                            MessageBox.Show("ItemEndPosition >= ItemStartPosition");
                    }
                }
            }
            else
            {
                if (isStoredItems)
                    mStoredItemsToAdd.AddRange(listItemsToAdd);
                else
                    mFoundItemsToAdd.AddRange(listItemsToAdd);
            }

            if (!LoadingBarManager.IsLoadingBarNull())
            {
                if (progressPercentage > LoadingBarManager.Value)
                    LoadingBarManager.Value  = progressPercentage;
            }

            if (isStoredItems && IsFileOpenToTranslateRunning && LoadingBarManager.IsLoadingCompleted())
            {
                if (!isMultiTranslator)
                {
                    LoadingBarManager.CloseLoadingBar();
                    mListBoxFoundItems.ItemsSource      = mItemSourceListBoxFoundItemsTemporary;
                    mListBoxStoredItems.ItemsSource     = mItemSourceListBoxStoredItemsTemporary;
                }

                IsFileOpenToTranslateRunning            = false;
                IsStoredItemDataToItemsProcessRunning   = false;
            }
        }

        public string ExportFile(string path = null, string text = null)
        {
            if (path == null)
            {
                SaveFileDialog saveFileDialog   = new SaveFileDialog();
                saveFileDialog.Filter           = "Data files (*.dat)|*.dat|Alla filer (*.*)|*.*";
                saveFileDialog.Title            = "Export File";
                saveFileDialog.DefaultExt       = ".dat";
                saveFileDialog.FileName         = TextManager.GetFileTypeToString(FileItemProperties.FileType) + ".dat";
                var result = saveFileDialog.ShowDialog();

                if (result.Value)
                    path = saveFileDialog.FileName;
                else
                    return "cancelled";
            }

            if (text == null)
            {
                if ((text = GetOpenFileText()) == null)
                    return null;
            }

            File.WriteAllText(path, text, Encoding.GetEncoding(1252));

            return path;
        }

        public void SetReportInfoText(string text)
        {
            mLabelReportInfo.Content = text;
        }

        public string GetOpenFileText()
        {
            if (TranslatedFileText != null && TranslatedFileText != "")
                return TranslatedFileText;
            else if (OpenFileText != null && OpenFileText != "")
                return OpenFileText;
            else
                return null;
        }        
    }
}
