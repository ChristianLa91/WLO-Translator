using Microsoft.Win32;
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

    /// <summary>
    /// FileItemProperties is a class that is used to store properties about the opened file;
    /// such as file type (Item.dat, SkillData.dat, Talk.dat, etc.), item data lenght, etc.
    /// </summary>
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

        /// <summary>
        /// Updates the file item properties
        /// </summary>
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

    /// <summary>
    /// FileManager handles the IO/storing and loading of item data from and to files.
    /// It handles the crucial job of item creation/instantiation and iteration through
    /// the game files with help of the FoundItemDataCollection class.
    /// Files are saved and loaded in XML-format. Implementations of database-solution with SQL has been begun,
    /// but it is still a solution for future implementation.
    /// </summary>
    public class FileManager
    {
        #region Fields

        private BackgroundWorker            mBackgroundWorkerStoredItemData;
        private BackgroundWorker            mBackgroundWorkerFoundItemData;
        private List<ItemData>              mFoundItemsToAdd;
        private List<ItemData>              mStoredItemsToAdd;
        public  List<ItemData>              GetFoundItems()     { return mFoundItemsToAdd; }
        public  List<ItemData>              GetStoredItems()    { return mStoredItemsToAdd; }

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

        private Label                       mLabelReportInfo;

        #endregion

        #region Properties

        public string                      OpenFileText        { get => mOpenFileText; private set => mOpenFileText = value; }


        private string                      mTranslatedFileText;
        public  string                      TranslatedFileText { get => mTranslatedFileText; set => mTranslatedFileText = value; }

        public  static FileItemProperties    FileItemProperties                     { get; private set; }

        public  FileType                    LoadedFileName                          { get; private set; } = FileType.NULL;

        //private Database                    mDatabase;

        public  bool                        IsFileOpenToTranslateRunning            { get; private set; }   = false;
        private bool                        IsStoredItemDataToItemsProcessRunning   { get; set; }           = false;


        public  string                      ApplicationVersion                      { get; private set; }
        private bool                        IsSorted                                { get; set; } = true;

        #endregion

        /// <summary>
        /// Returns if the What's New Window should be shown
        /// </summary>
        public bool ShowWhatsNew()
        {
            string currentVersion = GetCurrentApplicationVersion();
            if (currentVersion != ApplicationVersion)
            {
                ApplicationVersion = currentVersion;
                SaveProgramSettings();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Provides utility methods for suspending and resuming the UI update of a ListBox in WPF.
        /// </summary>
        public static class ListBoxUpdateSuspender
        {
            /// <summary>
            /// Suspends the UI update of a ListBox.
            ///
            /// Parameters:
            /// - listBox: The ListBox control whose UI update needs to be suspended.
            /// </summary>
            public static void SuspendUpdate(ListBox listBox)
            {
                // Get the handle of the ListBox control.
                IntPtr handle = ((HwndSource)PresentationSource.FromVisual(listBox)).Handle;

                // Suspend the UI update of the ListBox.
                SendMessage(handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }

            /// <summary>
            /// Resumes the UI update of a ListBox.
            ///
            /// Parameters:
            /// - listBox: The ListBox control whose UI update needs to be resumed.
            /// </summary>
            public static void ResumeUpdate(ListBox listBox)
            {
                // Get the handle of the ListBox control.
                IntPtr handle = ((HwndSource)PresentationSource.FromVisual(listBox)).Handle;

                // Resume the UI update of the ListBox.
                SendMessage(handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);

                // Refresh the ListBox to update its UI.
                listBox.InvalidateVisual();
            }

            /// <summary>
            /// Sends a message to a window or control.
            ///
            /// Parameters:
            /// - hWnd: The handle to the window or control.
            /// - msg: The message to send.
            /// - wParam: Additional message-specific information.
            /// - lParam: Additional message-specific information.
            ///
            /// Returns:
            /// - The result of the message processing; it depends on the message sent.
            /// </summary>
            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            private const int WM_SETREDRAW = 0x000B;
        }

        /// <summary>
        /// Constructs and initializes the FileManager.
        /// </summary>
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

        /// <summary>
        /// Returns the current application version with the data extracted from the current executing assembly.
        /// </summary>
        private string GetCurrentApplicationVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyVersion = System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();
            return assemblyVersion;
        }

        /// <summary>
        /// Saves the program settings to the Paths.ProgramSettingsPath.
        /// It stores things like general settings, selected theme and multi-translator paths into and xml-file.
        /// </summary>
        public  void SaveProgramSettings()
        {
            if (!File.Exists(Paths.ProgramSettingsPath))
            {
                if (!Directory.Exists(Paths.RoamingFolder))
                    Paths.GenerateRoamingFolders();
                FileStream fileStream = File.Create(Paths.ProgramSettingsPath);
                fileStream.Close();
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(Paths.ProgramSettingsPath, settings);

            try
            {
                writer.WriteStartElement("ProgramSettings");

                    writer.WriteStartElement("GeneralSettings");
                                        
                        string version = GetCurrentApplicationVersion();
                        writer.WriteAttributeString("Version", version);

                    writer.WriteEndElement();

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

        /// <summary>
        /// Saves the program settings to the Paths.ProgramSettingsPath.
        /// It stores things like general settings, selected theme and multi-translator paths into and xml-file.
        /// </summary>
        public  void LoadProgramSettings()
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
                        if (reader.Name == "GeneralSettings")
                        {
                            if (reader.GetAttribute("Version") != null)
                                ApplicationVersion = reader.GetAttribute("Version");
                        }

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
        /// Saves the associated stored item data into an XML-file. This is the data meant to hold all the
        /// translations of the already translated files.
        /// </summary>
        public  void SaveStoredItemData(FileType fileName, List<IItemBase> storedItems)
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

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(saveFilePathWithExtension, settings);

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
                    writer.WriteAttributeString("ID1", currentItem.ID[0].ToString());
                    writer.WriteAttributeString("ID2", currentItem.ID[1].ToString());
                    writer.WriteAttributeString("ID3", currentItem.ID[2].ToString());
                    writer.WriteAttributeString("Name", currentItem.Name);
                    writer.WriteAttributeString("Description", currentItem.Description);
                    writer.WriteAttributeString("Extra1", currentItem.Extra1);
                    writer.WriteAttributeString("Extra2", currentItem.Extra2);

                    // Positions
                    writer.WriteAttributeString("ItemStartPosition", currentItem.ItemStartPosition.ToString());
                    writer.WriteAttributeString("ItemEndPosition", currentItem.ItemEndPosition.ToString());
                    writer.WriteAttributeString("IDStartPosition", currentItem.IDStartPosition.ToString());
                    writer.WriteAttributeString("IDEndPosition", currentItem.IDEndPosition.ToString());
                    writer.WriteAttributeString("NameStartPosition", currentItem.NameStartPosition.ToString());
                    writer.WriteAttributeString("NameEndPosition", currentItem.NameEndPosition.ToString());
                    writer.WriteAttributeString("DescriptionStartPosition", currentItem.DescriptionStartPosition.ToString());
                    writer.WriteAttributeString("DescriptionEndPosition", currentItem.DescriptionEndPosition.ToString());

                    writer.WriteAttributeString("Extra1StartPosition", currentItem.Extra1StartPosition.ToString());
                    writer.WriteAttributeString("Extra1EndPosition", currentItem.Extra1EndPosition.ToString());
                    writer.WriteAttributeString("Extra2StartPosition", currentItem.Extra2StartPosition.ToString());
                    writer.WriteAttributeString("Extra2EndPosition", currentItem.Extra2EndPosition.ToString());

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

                            // Get nulls left
                            if (isMultiTranslator)
                            {
                                ItemData foundItemData = mFoundItemsToAdd.FirstOrDefault((ItemData itemFound) =>
                                {
                                    return Item.CompareIDs(itemFound.ID, currentItemData.ID);
                                });

                                if (currentItemData != null)
                                    ItemManager.SetNullLength(foundItemData, currentItemData);
                            }
                            else
                            {
                                mDispatcher.Invoke(() =>
                                {
                                    Item foundItem = mItemSourceListBoxFoundItemsTemporary.FirstOrDefault((Item itemFound) =>
                                    {
                                        return Item.CompareIDs(itemFound.ID, currentItemData.ID);
                                    });

                                    if (foundItem != null)
                                        ItemManager.SetNullLength(foundItem, currentItemData);
                                });
                            }

                            storedItemsToAdd.Add(currentItemData);

                            if (lastReportedTime.AddSeconds(0.5) < DateTime.Now)
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
            {
                Console.WriteLine("Currently no stored items file at: \"" + savefilePath);
                FinishOpeningTranslationFile(isMultiTranslator);
            }
        }

        /// <summary>
        /// The start of the opening/loading of the file which is to be translated, or the translations are to be
        /// translated from. This process uses multi-threading to speed up the process. Still have some performance issues,
        /// but is faster than it was before multi-threading.
        /// </summary>
        public  bool OpenTranslationFile(string filePath, bool isMultiTranslator = false)
        {
            IsFileOpenToTranslateRunning = true;
            //IsFoundItemDataCollectionCompleted      = false;
            IsStoredItemDataToItemsProcessRunning = false;
            IsSorted = true;

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
            mOpenFileText = fileText;

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
            mBackgroundWorkerFoundItemData.DoWork += BackgroundWorkerFoundItemData_DoWork;
            mBackgroundWorkerFoundItemData.ProgressChanged += BackgroundWorkerFoundItemData_ProgressChanged;
            mBackgroundWorkerFoundItemData.RunWorkerCompleted += BackgroundWorkerFoundItemData_RunWorkerCompleted;
            mBackgroundWorkerFoundItemData.RunWorkerAsync(new object[] { filePath, isMultiTranslator });

            return true;
        }

        #region Background Worker Found Item Data

        /// <summary>
        /// The work-function for the found-itemdata collection's background worker.
        /// Thread used for work and reporting progress to loading bar.
        /// </summary>
        private void BackgroundWorkerFoundItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(250);

            object[] arguments = e.Argument as object[];
            string fileName = arguments[0] as string;
            bool isMultiTranslator = (bool)arguments[1];

            CollectFoundItemDataAsync(fileName, isMultiTranslator).Wait();

            e.Result = new object[] { fileName, isMultiTranslator };
        }

        /// <summary>
        /// Class that is used to store the indices for parts of the data that is collected
        /// </summary>
        private class ItemDataPart
        {
            public int StartIndex { get; private set; }
            public int EndIndex { get; private set; }

            public ItemDataPart(int startIndex, int endIndex)
            {
                StartIndex  = startIndex;
                EndIndex    = endIndex;
            }
        }

        /// <summary>
        /// Task (thread) function that asynchronically handles the found item data collection.
        /// </summary>
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
                    idOffset = Constants.ACTIONINFO_DATA_ID_OFFSET;
                    break;
                case FileType.ALOGINEXE:
                    startOffset = 0;
                    break;
                case FileType.SCENEDATA:
                    startOffset = FileItemProperties.LengthPerItem + Constants.SCENEDATA_DATA_INITIAL_OFFSET;
                    idOffset = Constants.SCENEDATA_DATA_ID_OFFSET;
                    break;
                case FileType.TALK:
                    startOffset = FileItemProperties.LengthPerItem + Constants.TALK_DATA_INITIAL_OFFSET;
                    idOffset = Constants.TALK_DATA_ID_OFFSET;
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
                if (task.Exception != null)
                    MessageBox.Show("Task Exception: " + task.Exception.Message);
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

        /// <summary>
        /// The progress-function for the found-itemdata collection's background worker.
        /// Reports progress to loading bar and stores the ready items into the listbox.
        /// </summary>
        private void BackgroundWorkerFoundItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            object[] arguments = (object[])e.UserState;
            List<ItemData> foundItemsToAdd = (List<ItemData>)arguments[0];
            bool isMultiTranslator = (bool)arguments[1];

            _ = StoreItemsInListBoxAsync(mItemSourceListBoxFoundItemsTemporary, foundItemsToAdd, e.ProgressPercentage, false,
                isMultiTranslator);
        }

        /// <summary>
        /// The completed-function for the found-itemdata collection's background worker.
        /// Starts the process of loading the stored items.
        /// </summary>
        private void BackgroundWorkerFoundItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            object[] arguments = e.Result as object[];
            StartBackgroundWorkerLoadStoredItemData(arguments[0] as string, (bool)arguments[1]);

            Console.WriteLine("Found Item Data Worker Completed");
        }

        #endregion

        /// <summary>
        /// Starts the background worker for loading the stored items.
        /// </summary>
        public void StartBackgroundWorkerLoadStoredItemData(string filePath, bool isMultiTranslator)
        {
            if (mBackgroundWorkerStoredItemData != null)
            {
                mBackgroundWorkerStoredItemData.Dispose();
                mBackgroundWorkerStoredItemData = null;
            }

            mListBoxFoundItems.ItemsSource = mItemSourceListBoxFoundItemsTemporary;

            //if (!isMultiTranslator)
            //{
            //    ListBoxUpdateSuspender.SuspendUpdate(mListBoxFoundItems);
            //    ListBoxUpdateSuspender.SuspendUpdate(mListBoxStoredItems);
            //}

            mBackgroundWorkerStoredItemData                         = new BackgroundWorker();
            mBackgroundWorkerStoredItemData.WorkerReportsProgress   = true;
            mBackgroundWorkerStoredItemData.DoWork                 += BackgroundWorkerStoredItemData_DoWork;
            mBackgroundWorkerStoredItemData.ProgressChanged        += BackgroundWorkerStoredItemData_ProgressChanged;
            mBackgroundWorkerStoredItemData.RunWorkerCompleted     += BackgroundWorkerStoredItemData_RunWorkerCompleted;
            mBackgroundWorkerStoredItemData.RunWorkerAsync(new object[] { filePath, isMultiTranslator });
        }

        #region Background Worker Stored Item Data

        /// <summary>
        /// The work-function for the stored-itemdata collection's background worker.
        /// Thread used for work and reporting progress to loading bar.
        /// </summary>
        private void BackgroundWorkerStoredItemData_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] arguments = e.Argument as object[];

            while (!LoadingBarManager.IsLoadingCompleted()/*!IsFoundItemDataCollectionCompleted*/)
                Thread.Sleep(Constants.LOADING_SLEEP_LENGTH);

            Console.WriteLine("Loading Stored Item Data");

            LoadStoredItemData(arguments[0] as string, (bool)arguments[1]);

            e.Result = arguments[1];
        }

        /// <summary>
        /// The progress-function for the stored-itemdata collection's background worker.
        /// Reports progress to loading bar and stores the ready items into the listbox.
        /// </summary>
        private void BackgroundWorkerStoredItemData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            object[] arguments = (object[])e.UserState;
            List<ItemData> storedItemsToAdd = (List<ItemData>)arguments[0];
            bool isMultiTranslator = (bool)arguments[1];

            _ = StoreItemsInListBoxAsync(mItemSourceListBoxStoredItemsTemporary, storedItemsToAdd, e.ProgressPercentage, true,
                isMultiTranslator);
        }

        //TODO: Remove unnecessary completion function
        private void BackgroundWorkerStoredItemData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //bool isMultiTranslator = (bool)e.Result;
            //if (!isMultiTranslator)
            //    ((MainWindow)Application.Current.MainWindow).InitializeItemSearch();
        }

        #endregion

        /// <summary>
        /// Reports the progress to the loading bar and starts the process of adding the items to it's list.
        /// </summary>
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

        // <summary>
        // The name length affects where the id of the item is located.
        // This function gathers all data based on the data in the item already.
        // WARNING: THIS FUNCTION UPDATES THE ID, NAME AND DESCRIPTION OF THE ITEM.
        // IT CHANGES THE END-POSITION LENGTHS AND THUS THE PLACES FROM WHERE THE TRANSLATIONS ARE BEING REPLACED
        // IT'S IMPORTANT THAT THESE POSITIONS ARE RIGHT FOR THINGS TO WORK PROPERLY.
        // </summary>
        public void UpdateItemInfoBasedOnNewNameLength(Item item)
        {
            if (MessageBox.Show("Warning: This will update the ID, Name and Description positions."
                + "\nOnly do this if the ID has been extracted from the wrong place due to faulty data lengths (Name, Description, Extras)"
                + "and maybe also extracted the wrong description because of it.", "Warning",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                return;

            // Update name end position
            item.NameEndPosition = item.NameStartPosition + item.Name.Length;

            // Update ID
            UpdateItemID(ref item);

            // Update description
            if (FileItemProperties.HasDescription)
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

            //TODO: Add update extra
        }

        /// <summary>
        /// Updates the item's ID based on it's offsets
        /// </summary>
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
            item.ID = new int[]
            {
                mOpenFileData[nameEndPositionWithOffset],
                mOpenFileData[nameEndPositionWithOffset + 1],
                mOpenFileData[nameEndPositionWithOffset + 2]
            };
            item.IDStartPosition    = nameEndPositionWithOffset;
            item.IDEndPosition      = nameEndPositionWithOffset + 2;
        }

        /// <summary>
        /// Sets the project's open file name.
        /// </summary>
        public void SetProjectOpenFileName(ref Grid gridProjectFileName, string fileName)
        {
            string name = TextManager.GetFileTypeToString(FileItemProperties.FileType);
            (gridProjectFileName.Children[0] as TextBlock).Text     = name;
            (gridProjectFileName.Children[0] as TextBlock).ToolTip  = fileName;
            gridProjectFileName.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Removes the project's open file name.
        /// </summary>
        public void RemoveProjectOpenFileName(ref Grid gridProjectFileName)
        {
            (gridProjectFileName.Children[0] as TextBlock).Text     = "";
            (gridProjectFileName.Children[0] as TextBlock).ToolTip  = "";
            gridProjectFileName.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Closes the file that was opened to translate or grab translations from.
        /// </summary>
        public void CloseFileToTranslateOrGrabTranslationsFrom()
        {
            // Load in text from file
            mScintillaWPFTextBox.ReadOnly = false;
            mScintillaWPFTextBox.Scintilla.Text = "";
            mScintillaWPFTextBox.ReadOnly = true;
            mListBoxFoundItems.ItemsSource = new ObservableCollection<Item>();
            mListBoxStoredItems.ItemsSource = new ObservableCollection<Item>();
            FileItemProperties = null;

            ItemSearch.ClearStoredItemsWhileSearching();

            mOpenFileText = null;
            mOpenFileData = null;
        }

        /// <summary>
        /// Task (thread) that stores the items into a ListBox asynchronically.
        /// </summary>
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
                                                FileItemProperties.HasExtras,
                                                isStoredItems));
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
                    if (task.Exception != null)
                        MessageBox.Show("Task Exception: " + task.Exception.Message);
                    foreach (Item item in result)
                    {
                        observableItemCollection.Add(item);

                        if (observableItemCollection.Count > 1 &&
                            observableItemCollection[observableItemCollection.Count - 2].ItemEndPosition >=
                            observableItemCollection[observableItemCollection.Count - 1].ItemStartPosition)
                        {
                            IsSorted = false;
                            Console.WriteLine("ItemEndPosition >= ItemStartPosition");
#if DEBUG
                            MessageBox.Show("ItemEndPosition >= ItemStartPosition");
#endif
                        }
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
                    LoadingBarManager.Value = progressPercentage;
            }

            if (isStoredItems && IsFileOpenToTranslateRunning && LoadingBarManager.IsLoadingCompleted())
            {
                FinishOpeningTranslationFile(isMultiTranslator);
            }
        }

        /// <summary>
        /// Finishes the loading of the file that the user is translating or grabbing the translations from.
        /// </summary>
        private void FinishOpeningTranslationFile(bool isMultiTranslator)
        {
            if (!isMultiTranslator)
            {
                mDispatcher.Invoke(() =>
                {
                    LoadingBarManager.CloseLoadingBar();
                    
                    if (!IsSorted)
                        OrderStoredTemporaryItemListByItemStartPosition();

                    mListBoxStoredItems.ItemsSource = mItemSourceListBoxStoredItemsTemporary;

                    ((MainWindow)Application.Current.MainWindow).InitializeItemSearch();
                });
            }

            IsFileOpenToTranslateRunning            = false;
            IsStoredItemDataToItemsProcessRunning   = false;
        }

        /// <summary>
        /// Orders the stored temporary items' List by item start position
        /// </summary>
        private void OrderStoredTemporaryItemListByItemStartPosition()
        {
            List<Item> items = mItemSourceListBoxStoredItemsTemporary.OrderBy((Item item) => { return item.ItemStartPosition; }).ToList();
            mItemSourceListBoxStoredItemsTemporary = new ObservableCollection<Item>(items);
            
            IsSorted    = true;
        }

        /// <summary>
        /// Exports the file (meant to do after translating it) to the by the user chosen path.
        /// </summary>
        public string ExportFile(string path = null, string text = null)
        {
            if (path == null)
            {
                SaveFileDialog saveFileDialog   = new SaveFileDialog();
                saveFileDialog.Filter           = "Data files (*.dat)|*.dat|Alla filer (*.*)|*.*";
                saveFileDialog.Title            = "Export File";
                saveFileDialog.DefaultExt       = ".dat";
                saveFileDialog.FileName         = TextManager.GetFileTypeToString(FileItemProperties.FileType) + ".dat";
                var result                      = saveFileDialog.ShowDialog();

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

        #region Export Items as Excel Document

        public void ExportFoundItemsAsExcelDocument()
        {
            ExportItemsAsExcelDocument("Found", ref mListBoxFoundItems);
        }

        public void ExportStoredItemsAsExcelDocument()
        {
            ExportItemsAsExcelDocument("Stored", ref mListBoxStoredItems);
        }

        /// <summary>
        /// Exports items in the provided listBox to a path chosen by the user as an excel-document.
        /// </summary>
        public void ExportItemsAsExcelDocument(string typeOfItems, ref ListBox listBox)
        {
            if (FileItemProperties.FileType == FileType.NULL)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".xlsx";
            saveFileDialog.Filter = "Excel File|.xlsx";
            saveFileDialog.Title = "Export " + typeOfItems + " Item Data as Excel File";

            if (saveFileDialog.ShowDialog() == true)
            {
                List<Item> items = listBox.Items.OfType<Item>().ToList();
                List<Dictionary<string, object>> stringLists = new List<Dictionary<string, object>>();
                for (int i = 0; i < items.Count; i++)
                {
                    Item item = items[i];
                    stringLists.Add(item.GetExcelTextExportList());
                }

                MiniExcelLibs.MiniExcel.SaveAs(saveFileDialog.FileName, stringLists);

                mLabelReportInfo.Content = "Export Stored Items as an Excel Document at \"" + saveFileDialog.FileName + "\"";
            }
        }

        #endregion

        /// <summary>
        /// Sets the report info text, that is the Label down left of the MainWindow.
        /// </summary>
        public void SetReportInfoText(string text)
        {
            mLabelReportInfo.Content = text;
        }

        /// <summary>
        /// Returns the opened file text, which is either the TranslatedFileText, the OpenFileText,
        /// or null if no one of them isn't qual to null.
        /// </summary>
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
