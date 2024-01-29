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
    /// Interaction logic for WindowMultiTranslator.xaml
    /// </summary>
    public partial class WindowMultiTranslator : Window
    {
        private FileManager                                 mFileManager;
        private OpenFileDialog                              mOpenFileDialog;
        private System.Windows.Forms.FolderBrowserDialog    mFolderBrowserDialog;
        private List<ItemData>                              mFoundItems;
        private List<List<ItemData>>                        mStoredItemLists;
        private List<string>                                mTranslatedFileTexts;
        private List<FileItemProperties>                    mFileItemProperties;
        private List<ListBoxItemMTData>                     mListBoxItemMTData; // Used to see which files should be translated

        public WindowMultiTranslator(ref FileManager fileManager)
        {
            InitializeComponent();

            mFileManager                = fileManager;

            ListBoxFiles.Items.Add(new ListBoxItemMT("Item", FileType.ITEM, 
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new ListBoxItemMT("Mark", FileType.MARK,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new ListBoxItemMT("Npc", FileType.NPC,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new ListBoxItemMT("SceneData", FileType.SCENEDATA,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new ListBoxItemMT("Skill", FileType.SKILL,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));
            ListBoxFiles.Items.Add(new ListBoxItemMT("Talk", FileType.TALK,
                CheckBoxItemFileMultiTranslator_Click, ButtonOpenFileSelect_Click));

            (ListBoxFiles.Items[0] as ListBoxItemMT).FilePath = Settings.MultiTranslatorItemDataPath;
            (ListBoxFiles.Items[1] as ListBoxItemMT).FilePath = Settings.MultiTranslatorMarkDataPath;
            (ListBoxFiles.Items[2] as ListBoxItemMT).FilePath = Settings.MultiTranslatorNpcDataPath;
            (ListBoxFiles.Items[3] as ListBoxItemMT).FilePath = Settings.MultiTranslatorSceneDataDataPath;
            (ListBoxFiles.Items[4] as ListBoxItemMT).FilePath = Settings.MultiTranslatorSkillDataPath;
            (ListBoxFiles.Items[5] as ListBoxItemMT).FilePath = Settings.MultiTranslatorTalkDataPath;
        }

        private void ButtonOpenFileSelect_Click(object sender, RoutedEventArgs e)
        {
            mOpenFileDialog         = new OpenFileDialog();
            mOpenFileDialog.Filter  = "Data File (.dat)|*.dat";

            if (mOpenFileDialog.ShowDialog() == true)
            {
                ListBoxItemMT item = (sender as Button).Tag as ListBoxItemMT;
                item.FilePath   = mOpenFileDialog.FileName;
                item.IsChecked  = true;
            }
        }

        private void CheckBoxItemFileMultiTranslator_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxFiles.Items.OfType<ListBoxItemMT>()
                .All((ListBoxItemMT listBoxItemMultiTranslator) => { return listBoxItemMultiTranslator.IsChecked.Value; }))
                CheckBoxFilesSelectAll.IsChecked = true;
            else
                CheckBoxFilesSelectAll.IsChecked = false;
        }

        private void CheckBoxItemTranslatedFileMultiTranslator_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTranslatedFiles.Items.OfType<ListBoxItemMT>()
                .All((ListBoxItemMT listBoxItemMultiTranslator) => { return listBoxItemMultiTranslator.IsChecked.Value; }))
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
            Settings.MultiTranslatorItemDataPath        = (ListBoxFiles.Items[0] as ListBoxItemMT).FilePath;
            Settings.MultiTranslatorMarkDataPath        = (ListBoxFiles.Items[1] as ListBoxItemMT).FilePath;
            Settings.MultiTranslatorNpcDataPath         = (ListBoxFiles.Items[2] as ListBoxItemMT).FilePath;
            Settings.MultiTranslatorSceneDataDataPath   = (ListBoxFiles.Items[3] as ListBoxItemMT).FilePath;
            Settings.MultiTranslatorSkillDataPath       = (ListBoxFiles.Items[4] as ListBoxItemMT).FilePath;
            Settings.MultiTranslatorTalkDataPath        = (ListBoxFiles.Items[5] as ListBoxItemMT).FilePath;
            mFileManager.SaveProgramSettings();

            Close();
        }

        private void CheckBoxFilesSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItemMT listBoxItemMultiTranslator in ListBoxFiles.Items)
                listBoxItemMultiTranslator.IsChecked = CheckBoxFilesSelectAll.IsChecked.Value;
        }

        private void CheckBoxTranslatedFilesSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxItemMT listBoxItemMultiTranslator in ListBoxTranslatedFiles.Items)
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
            mListBoxItemMTData = new List<ListBoxItemMTData>();

            foreach (ListBoxItemMT listBoxItemMultiTranslator in ListBoxFiles.Items.OfType<ListBoxItemMT>())
            {
                if (listBoxItemMultiTranslator.IsChecked == true)
                    mListBoxItemMTData.Add(listBoxItemMultiTranslator.GetDataClass());
            }

            if (mListBoxItemMTData.Count > 0)
            {
                ListBoxTranslatedFiles.Items.Clear();
                TreeViewUnstoredItems.Items.Clear();
                _ = Task.Run(async () => await TranslateSelectedFilesAsync());
            }
        }

        private async Task TranslateSelectedFilesAsync()
        {
            mFoundItems             = new List<ItemData>();
            mStoredItemLists        = new List<List<ItemData>>();
            mTranslatedFileTexts    = new List<string>();
            mFileItemProperties     = new List<FileItemProperties>();

            try
            { 
                foreach (ListBoxItemMTData itemData in mListBoxItemMTData)
                {
                    string fileName = itemData.FilePath;

                    // Get item data from the file being translated
                    Application.Current.Dispatcher.Invoke(() => { mFileManager.OpenFileToTranslate(fileName, true); });
                    while (mFileManager.IsFileOpenToTranslateRunning)
                        Thread.Sleep(500);

                    mFoundItems     = mFileManager.GetFoundItems().ToList();
                    mStoredItemLists.Add(mFileManager.GetStoredItems().ToList());

                    // Translate collected text data
                    string translatedText = await Translation.TranslateAsync(mFileManager.GetOpenFileText(), mFoundItems,
                        mStoredItemLists.Last(), mFileManager.FileItemProperties, true);

                    // Add unstored items to tree view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Translation.GetItemsWithoutTranslations()?.Count > 0)
                        {
                            // Create a treeViewItemMT that groups the file type items together
                            TreeViewItemMT treeViewItemMT = new TreeViewItemMT(itemData.FilePath, itemData.FileType, CheckBoxTreeViewMT_Click);
                            treeViewItemMT.IsExpanded = true;
                            TreeViewUnstoredItems.Items.Add(treeViewItemMT);

                            // Add items without translations to the tree view
                            foreach (ItemData itemDataUntranslated in Translation.GetItemsWithoutTranslations())
                            {
                                Item item = itemDataUntranslated.ToItem(null, null, null, null, null, null, null,
                                    mFileManager.FileItemProperties.HasDescription, mFileManager.FileItemProperties.HasExtras,
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
                    mFileItemProperties.Add(mFileManager.FileItemProperties);

                    // Add an item to ListBoxTranslatedFiles to signify what file has been translated
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ListBoxItemMT newItem = new ListBoxItemMT(itemData.Name, itemData.FileType,
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

        //TODO: Add the former? untranslated items to items which groups them so that they can easily added to each stored item list
        private void ButtonStoreItems_Click(object sender, RoutedEventArgs e)
        {
            if (mListBoxItemMTData == null || mListBoxItemMTData.Count == 0)
                return;

            for (int i = 0; i < mListBoxItemMTData.Count; ++i)
            {
                ListBoxItemMTData data = mListBoxItemMTData[i];
                foreach (Item item in (TreeViewUnstoredItems.Items[0] as TreeViewItemMT).Items.OfType<Item>())
                {
                    if (item.IsChecked == true)
                        mStoredItemLists[i].Add(item.ToItemData());
                }

                mFileManager.SaveStoredItemData(data.FileType, mStoredItemLists[i].Cast<IItemBase>().ToList());
            }

            string message = "Selected Unstored Items Have Been Stored";
            TextBlockReportInfo.Text = message;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
