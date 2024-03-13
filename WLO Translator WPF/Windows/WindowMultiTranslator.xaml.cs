using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// The WindowMultiTranslator window is mainly intended as a much faster way to translate the files after every data-file
    /// already has had their translations stored once.
    /// 
    /// Usage:
    /// It works by first selecting the paths to the files the user wants to translate,
    /// then proceeding to translate the files using the translate-button -- then after the translation process is done --
    /// checking if there are items in the lower right TreeView-box that are left to be translated.
    /// If there are items that are untranslated; the user can then quickly or in their own pace, translate the remaining items there,
    /// click the button that stores the newly translated items, then click the translation-button again, wait for the process to finish.
    /// Regardles if there were any untranslated items, the user can then proceed to export the translated files into a by the user
    /// selected folder.
    /// </summary>
    public partial class WindowMultiTranslator : Window
    {
        private FileManager                                 mFileManager;
        private OpenFileDialog                              mOpenFileDialog;
        private System.Windows.Forms.FolderBrowserDialog    mFolderBrowserDialog;
        private List<ItemData>                              mFoundItems;
        private Dictionary<FileType, List<ItemData>>        mStoredItemsDictionaty;
        private List<string>                                mTranslatedFileTexts;
        private List<FileItemProperties>                    mFileItemProperties;
        private List<LBIFileToTranslateMTData>              mFilesSelectedForTranslation; // Used to see which files should be translated

        public WindowMultiTranslator(ref FileManager fileManager)
        {
            InitializeComponent();

            mFileManager                = fileManager;

            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("Item", FileType.ITEM, 
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("Mark", FileType.MARK,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("Npc", FileType.NPC,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("SceneData", FileType.SCENEDATA,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("Skill", FileType.SKILL,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new LBIFileToTranslateMT("Talk", FileType.TALK,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));

            (ListBoxFiles.Items[0] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorItemDataPath;
            (ListBoxFiles.Items[1] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorMarkDataPath;
            (ListBoxFiles.Items[2] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorNpcDataPath;
            (ListBoxFiles.Items[3] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorSceneDataDataPath;
            (ListBoxFiles.Items[4] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorSkillDataPath;
            (ListBoxFiles.Items[5] as LBIFileToTranslateMT).FilePath = Settings.MultiTranslatorTalkDataPath;
        }

        private void ButtonOpenFileSelect_Click(object sender, RoutedEventArgs e)
        {
            mOpenFileDialog         = new OpenFileDialog();
            mOpenFileDialog.Filter  = "Data File (.dat)|*.dat";

            if (mOpenFileDialog.ShowDialog() == true)
            {
                LBIFileToTranslateMT item = (sender as Button).Tag as LBIFileToTranslateMT;
                item.FilePath   = mOpenFileDialog.FileName;
                item.IsChecked  = true;
            }
        }

        private void CheckBoxItemFileMultiTranslator_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxFiles.Items.OfType<LBIFileToTranslateMT>()
                .All((LBIFileToTranslateMT listBoxItemMultiTranslator) => { return listBoxItemMultiTranslator.IsChecked.Value; }))
                CheckBoxFilesSelectAll.IsChecked = true;
            else
                CheckBoxFilesSelectAll.IsChecked = false;
        }

        private void CheckBoxItemTranslatedFileMultiTranslator_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTranslatedFiles.Items.OfType<LBIFileToTranslateMT>()
                .All((LBIFileToTranslateMT listBoxItemMultiTranslator) => { return listBoxItemMultiTranslator.IsChecked.Value; }))
                CheckBoxTranslatedFilesSelectAll.IsChecked = true;
            else
                CheckBoxTranslatedFilesSelectAll.IsChecked = false;
        }

        private void CheckBoxTreeViewMT_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewUnstoredItems.Items.OfType<TreeViewItemMT>()
                .All((TreeViewItemMT treeViewItemMT) => { return treeViewItemMT.IsChecked.Value; }))
                CheckBoxUnstoredItemsSelectAll.IsChecked = true;
            else
                CheckBoxUnstoredItemsSelectAll.IsChecked = false;
        }

        private void CheckBoxUnstoredItem_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItemMT treeViewItemMT = ((sender as CheckBox).Tag as Item).Parent as TreeViewItemMT;
            if (treeViewItemMT.Items.OfType<Item>()
                .All((Item item) => { return item.IsChecked.Value; }))
                treeViewItemMT.IsChecked = true;
            else
                treeViewItemMT.IsChecked = false;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Settings.MultiTranslatorItemDataPath        = (ListBoxFiles.Items[0] as LBIFileToTranslateMT).FilePath;
            Settings.MultiTranslatorMarkDataPath        = (ListBoxFiles.Items[1] as LBIFileToTranslateMT).FilePath;
            Settings.MultiTranslatorNpcDataPath         = (ListBoxFiles.Items[2] as LBIFileToTranslateMT).FilePath;
            Settings.MultiTranslatorSceneDataDataPath   = (ListBoxFiles.Items[3] as LBIFileToTranslateMT).FilePath;
            Settings.MultiTranslatorSkillDataPath       = (ListBoxFiles.Items[4] as LBIFileToTranslateMT).FilePath;
            Settings.MultiTranslatorTalkDataPath        = (ListBoxFiles.Items[5] as LBIFileToTranslateMT).FilePath;
            mFileManager.SaveProgramSettings();

            Close();
        }

        private void CheckBoxFilesSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (LBIFileToTranslateMT listBoxItemMultiTranslator in ListBoxFiles.Items)
                listBoxItemMultiTranslator.IsChecked = CheckBoxFilesSelectAll.IsChecked.Value;
        }

        private void CheckBoxTranslatedFilesSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (LBIFileToTranslateMT listBoxItemMultiTranslator in ListBoxTranslatedFiles.Items)
                listBoxItemMultiTranslator.IsChecked = CheckBoxTranslatedFilesSelectAll.IsChecked.Value;
        }

        private void CheckBoxUnstoredItemsSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeViewItemMT treeViewItemMT in
                TreeViewUnstoredItems.Items.OfType<TreeViewItemMT>())
            {
                treeViewItemMT.IsChecked = CheckBoxUnstoredItemsSelectAll.IsChecked.Value;
                treeViewItemMT.SetSelectionOfChildrenEqualToParents();
            }
        }

        private void ButtonTranslateSelected_Click(object sender, RoutedEventArgs e)
        {
            mFilesSelectedForTranslation = new List<LBIFileToTranslateMTData>();

            foreach (LBIFileToTranslateMT listBoxItemMultiTranslator in ListBoxFiles.Items.OfType<LBIFileToTranslateMT>())
            {
                if (listBoxItemMultiTranslator.IsChecked == true)
                    mFilesSelectedForTranslation.Add(listBoxItemMultiTranslator.GetDataClass());
            }

            if (mFilesSelectedForTranslation.Count > 0)
            {
                ListBoxTranslatedFiles.Items.Clear();
                TreeViewUnstoredItems.Items.Clear();
                _ = Task.Run(async () => await TranslateSelectedFilesAsync());
            }
        }

        /// <summary>
        /// Translates the selected files to be translated asynchronically
        /// </summary>
        private async Task TranslateSelectedFilesAsync()
        {
            mFoundItems             = new List<ItemData>();
            mStoredItemsDictionaty  = new Dictionary<FileType, List<ItemData>>();
            mTranslatedFileTexts    = new List<string>();
            mFileItemProperties     = new List<FileItemProperties>();

            try
            { 
                foreach (LBIFileToTranslateMTData itemData in mFilesSelectedForTranslation)
                {
                    string fileName = itemData.FilePath;

                    // Get item data from the file being translated
                    Application.Current.Dispatcher.Invoke(() => { mFileManager.OpenTranslationFile(fileName, true); });
                    while (mFileManager.IsFileOpenToTranslateRunning)
                        Thread.Sleep(500);

                    mFoundItems     = mFileManager.GetFoundItems().ToList();
                    mStoredItemsDictionaty.Add(FileManager.FileItemProperties.FileType, mFileManager.GetStoredItems().ToList());

                    // Translate collected text data
                    string translatedText = await Translation.TranslateAsync(mFileManager.GetOpenFileText(), mFoundItems,
                        mStoredItemsDictionaty.Last().Value, FileManager.FileItemProperties, true);

                    // Add unstored items to tree view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Translation.GetItemsWithoutTranslations()?.Count > 0)
                        {
                            // Create a treeViewItemMT that groups the file type items together
                            TreeViewItemMT treeViewItemMT = new TreeViewItemMT(itemData.FilePath, itemData.FileType,
                                CheckBoxTreeViewMT_Click);
                            treeViewItemMT.IsExpanded = true;
                            TreeViewUnstoredItems.Items.Add(treeViewItemMT);

                            // Add items without translations to the tree view
                            foreach (ItemData itemDataUntranslated in Translation.GetItemsWithoutTranslations())
                            {
                                if (itemDataUntranslated == null)
                                    continue;
                                Item item = itemDataUntranslated.ToItem(null, null, null, null, null, null, null,
                                    FileManager.FileItemProperties.HasDescription, FileManager.FileItemProperties.HasExtras, true,
                                    true, CheckBoxUnstoredItem_Click);
                                // Change to Chinese encoding to make it readable to the translator
                                FontFamily  unicodeFont = new FontFamily("SimHei");
                                Encoding    encoding    = Encoding.GetEncoding("big5");
                                item.SetEncoding(unicodeFont, encoding);
                                item.IsChecked = true;
                                treeViewItemMT.Items.Add(item);
                            }
                        }
                    });

                    // Store translated text and file properties
                    mTranslatedFileTexts.Add(translatedText);
                    mFileItemProperties.Add(FileManager.FileItemProperties);

                    // Add an item to ListBoxTranslatedFiles to signify what file has been translated
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LBIFileToTranslateMT newItem = new LBIFileToTranslateMT(itemData.Name, itemData.FileType,
                            CheckBoxItemTranslatedFileMultiTranslator_Click);
                        newItem.FilePath = fileName;
                        ListBoxTranslatedFiles.Items.Add(newItem);
                    });
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingBarManager.CloseLoadingBar();
                    string message = "Selected Files Have Been Translated";
                    TextBlockReportInfo.Text = message;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ButtonExportSelectedFiles_Click(object sender, RoutedEventArgs e)
        {
            if (mTranslatedFileTexts?.Count == 0 == true)
                return;

            mFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (mFolderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = mFolderBrowserDialog.SelectedPath;

                for (int i = 0; i < mTranslatedFileTexts.Count; i++)
                    mFileManager.ExportFile(folderPath + "\\"
                        + TextManager.GetFileTypeToString(mFileItemProperties[i].FileType) + ".dat",
                        mTranslatedFileTexts[i]);

                string message = "Selected Files Have Been Exported";
                TextBlockReportInfo.Text = message;
            }
        }

        //TODO: Switch so that TreeViewUnstoredItems has a more "single source of truth" relationship with mStoredItemLists,
        //      or at least make sure this function works as intended
        private void ButtonStoreItems_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewUnstoredItems.Items.Count == 0)
                return;

            for (int i = 0; i < TreeViewUnstoredItems.Items.Count; ++i)
            {
                TreeViewItemMT  fileToStore     = TreeViewUnstoredItems.Items[i] as TreeViewItemMT;
                bool hasStored = false;
                //if (fileToStore.IsChecked == true)
                //{
                    List<ItemData> storedItemData = mStoredItemsDictionaty[fileToStore.FileType];
                    foreach (Item item in fileToStore.Items.OfType<Item>())
                    {
                        if (item.IsChecked == true)
                        {
                            InsertItemAtDataPositionOrder(storedItemData, item.ToItemData());
                            hasStored = true;
                        }
                    }

                    if (hasStored)
                        mFileManager.SaveStoredItemData(fileToStore.FileType, storedItemData.Cast<IItemBase>().ToList());
                //}
            }

            string message = "Selected Unstored Items Have Been Stored";
            TextBlockReportInfo.Text = message;
        }

        /// <summary>
        /// Inserts the item at the position it has in the untranslated file
        /// </summary>
        private void InsertItemAtDataPositionOrder(List<ItemData> itemDataList, ItemData itemData)
        {
            int index = -1;

            for (int i = 0; i < itemDataList.Count; i++)
            {
                if (itemDataList[i].ItemStartPosition > itemData.ItemStartPosition)
                    index = i;
            }

            itemDataList.Insert(index, itemData);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
