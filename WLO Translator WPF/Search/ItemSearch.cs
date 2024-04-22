using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// The ItemSearch class is used to search for specific items in the found and stored items ListBoxes.
    /// 
    /// Usage:
    /// There is a search box that strings can be typed into that filters items based on criteria such as as which box
    /// is selected in the drop-down menu. The filtering process is also affected by various check-boxes that filters based on
    /// critera such as if items contains illegal chars, unconventional chars or if the item has a stored representation with the same
    /// item ID.
    /// </summary>
    public static class ItemSearch
    {
        private static List<Item>                   mFoundItemsWhileSearching;
        private static List<ItemData>               mFoundItemDataWhileSearching;
        private static Dictionary<ItemData, Item>   mFoundItemAndDataItemDictionary;
        private static List<Item>                   mStoredItemsWhileSearching;
        private static List<ItemData>               mStoredItemDataWhileSearching;
        private static Dictionary<ItemData, Item>   mStoredItemAndDataItemDictionary;
        private static SearchOption                 mSearchOption = SearchOption.FOUND_ITEM_NAME;

        private static string                       mSearchStringOld;
        private static bool                         mShowItemsWithBadCharsOld;
        private static bool                         mShowItemsWithUnusualCharsOld;
        //private static bool                       mShowItemsWithoutDescriptionsOld;
        //private static bool                       mShowItemsWithSameIDsOld;
        //private static bool                       mShowItemsWithoutMatchOld;
        //private static bool                       mShowItemsWithoutFirstCharBeingALetterOld;
        private static bool                         mShowItemsWithTooLongTranslationsOld;

        public enum SearchOption
        {
            FOUND_ITEM_NAME,
            STORED_ITEM_NAME,
            ITEM_ID,
            DEFAULT
        }

        /// <summary>
        /// Updated the stored and found items that are used to bring back the unfiltered items when the filering options
        /// are changed by the user.
        /// </summary>
        public static void UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems)
        {
            mFoundItemsWhileSearching           = listBoxFoundItems.ItemsSource.OfType<Item>().ToList();
            mStoredItemsWhileSearching          = listBoxStoredItems.ItemsSource.OfType<Item>().ToList();

            mFoundItemDataWhileSearching        = new List<ItemData>();
            mStoredItemDataWhileSearching       = new List<ItemData>();
            mFoundItemAndDataItemDictionary     = new Dictionary<ItemData, Item>();
            mStoredItemAndDataItemDictionary    = new Dictionary<ItemData, Item>();

            foreach (Item item in mFoundItemsWhileSearching)
            {
                ItemData itemData = item.GetItemData();
                mFoundItemDataWhileSearching.Add(itemData);
                mFoundItemAndDataItemDictionary.Add(itemData, item);
            }

            foreach (Item item in mStoredItemsWhileSearching)
            {
                ItemData itemData = item.GetItemData();
                mStoredItemDataWhileSearching.Add(itemData);
                mStoredItemAndDataItemDictionary.Add(itemData, item);
            }
        }

        /// <summary>
        /// Clears the stored items
        /// </summary>
        public static void ClearStoredItemsWhileSearching()
        {
            mStoredItemsWhileSearching?.Clear();
            mStoredItemDataWhileSearching?.Clear();
            mStoredItemAndDataItemDictionary?.Clear();
        }

        /// <summary>
        /// Updates old filtering settings so that actions in the filtering process can be skipped if updates don't need to be made.
        /// </summary>
        private static void UpdateOldVariables(string searchString,
            bool showItemsWithBadChars, bool showItemsWithUnusualChars, bool showItemsWithoutDescriptions, bool showItemsWithSameIDs,
            bool showItemsWithoutMatch, bool showItemsWithoutFirstCharBeingALetter, bool showItemsWithTooLongTranslationsOld)
        {
            mSearchStringOld                            = searchString;
            mShowItemsWithBadCharsOld                   = showItemsWithBadChars;
            mShowItemsWithUnusualCharsOld               = showItemsWithUnusualChars;
            //mShowItemsWithoutDescriptionsOld            = showItemsWithoutDescriptions;
            //mShowItemsWithSameIDsOld                    = showItemsWithSameIDs;
            //mShowItemsWithoutMatchOld                   = showItemsWithoutMatch;
            //mShowItemsWithoutFirstCharBeingALetterOld   = showItemsWithoutFirstCharBeingALetter;
            mShowItemsWithTooLongTranslationsOld        = showItemsWithTooLongTranslationsOld;
        }

        /// <summary>
        /// Updates which items should be visible in the list boxes depending on which options have been checked by the user and
        /// whether or not the user have changed the text in the search box.
        /// </summary>
        public static void UpdateVisibleItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems, string searchString,
            bool showItemsWithBadChars, bool showItemsWithUnusualChars, bool showItemsWithoutDescriptions, bool showItemsWithSameIDs,
            bool showItemsWithoutMatch, bool showItemsWithoutFirstCharBeingALetter, bool showItemsWithTooLongTranslations,
            SearchOption searchOption = SearchOption.DEFAULT)
        {
            if (mFoundItemsWhileSearching == null && mStoredItemsWhileSearching == null)
                return;

            ObservableCollection<Item>  itemSourceFoundItems            = (ObservableCollection<Item>)listBoxFoundItems.ItemsSource;
            ObservableCollection<Item>  itemSourceStoredItems           = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;
            ObservableCollection<Item>  itemSourceFoundItemsTemporary   = new ObservableCollection<Item>();
            ObservableCollection<Item>  itemSourceStoredItemsTemporary  = new ObservableCollection<Item>();
            List<ItemData>              itemsStored                     = new List<ItemData>();
            List<ItemData>              itemsFound                      = new List<ItemData>();

            if (searchOption != SearchOption.DEFAULT)
                mSearchOption = searchOption;

            if (searchString == "" && !showItemsWithBadChars && !showItemsWithUnusualChars && !showItemsWithoutDescriptions
                && !showItemsWithSameIDs && !showItemsWithoutMatch && !showItemsWithoutFirstCharBeingALetter && !showItemsWithTooLongTranslations
                && mStoredItemsWhileSearching != null)
            {
                // Add the "while searching" items back
                itemsStored = mStoredItemDataWhileSearching;
                itemsFound  = mFoundItemDataWhileSearching;
            }
            else
            {
                // Add the "while searching" items back depending on the old variables
                if (mShowItemsWithBadCharsOld && !mShowItemsWithUnusualCharsOld && showItemsWithUnusualChars  // Unusual chars added to bad chars
                    || (mShowItemsWithUnusualCharsOld && !mShowItemsWithBadCharsOld && showItemsWithBadChars) // Bad chars added to unusual chars
                    || (mSearchStringOld != null && mSearchStringOld.Contains(searchString))
                    || (searchString != mSearchStringOld && mSearchStringOld != null && !mSearchStringOld.Contains(searchString))
                    || (searchString == "" && mSearchStringOld != ""))
                {
                    if (mStoredItemsWhileSearching != null)
                        itemsStored = mStoredItemDataWhileSearching.ToList();
                    if (mFoundItemsWhileSearching != null)
                        itemsFound  = mFoundItemDataWhileSearching.ToList();
                }

                // Search for items containing a string
                if (searchString != "")
                {
                    switch (mSearchOption)
                    {
                        case SearchOption.FOUND_ITEM_NAME:
                            SearchItemsByString(searchString, ref itemsFound, ref itemsStored,
                                ref mFoundItemDataWhileSearching, ref mStoredItemDataWhileSearching);
                            break;
                        case SearchOption.STORED_ITEM_NAME:
                            SearchItemsByString(searchString, ref itemsStored, ref itemsFound,
                                ref mStoredItemDataWhileSearching, ref mFoundItemDataWhileSearching);
                            break;
                        case SearchOption.ITEM_ID:
                            SearchItemsByString(searchString, ref itemsFound, ref itemsStored,
                                ref mFoundItemDataWhileSearching, ref mStoredItemDataWhileSearching, true);
                            OrderListsByID(ref itemsFound);
                            OrderListsByID(ref itemsStored);
                            break;
                    }
                }

                // If showItemsWithBadCharsOnly, remove all items that don't contain bad chars in their text boxes
                if (showItemsWithBadChars || showItemsWithUnusualChars)
                {
                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        Item itemFound;
                        mFoundItemAndDataItemDictionary.TryGetValue(itemsFound[i], out itemFound);
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemFound.IsTextBoxesContainingIllegalChars()
                                && !itemFound.IsTextBoxesContainingUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemFound.IsTextBoxesContainingIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemFound.IsTextBoxesContainingUnusualChars()))
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        Item itemStored;
                        mStoredItemAndDataItemDictionary.TryGetValue(itemsStored[i], out itemStored);
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemStored.IsTextBoxesContainingIllegalChars()
                                && !itemStored.IsTextBoxesContainingUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemStored.IsTextBoxesContainingIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemStored.IsTextBoxesContainingUnusualChars()))
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                }

                // If showItemsWithTooLongTranslations, remove all items that don't contain too long translations
                if (showItemsWithTooLongTranslations)
                {
                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        Item itemFound;
                        mFoundItemAndDataItemDictionary.TryGetValue(itemsFound[i], out itemFound);
                        if (!itemFound.IsTranslationsTooLong())
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        Item itemStored;
                        mStoredItemAndDataItemDictionary.TryGetValue(itemsStored[i], out itemStored);
                        if (!itemStored.IsTranslationsTooLong())
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }
                }

                // If showItemsWithoutDescriptionsOnly, remove all items that has a description that's not equal to ""
                if (showItemsWithoutDescriptions)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        if (itemsStored[i].Description != null && itemsStored[i].Description != "") { itemsStored.RemoveAt(i); --i; }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        if (itemsFound[i].Description != null && itemsFound[i].Description != "") { itemsFound.RemoveAt(i); --i; }
                    }
                }

                // If showItemsWithSameIDs, remove all items that hasn't the same ID as any other item
                if (showItemsWithSameIDs)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        ItemData itemWithSameID = itemsStored.Find((ItemData item) =>
                        {
                            return Item.CompareIDs(itemsStored[i].ID, item.ID) && itemsStored[i] != item;
                        });

                        if (itemWithSameID == null) { itemsStored.RemoveAt(i); --i; }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        ItemData itemWithSameID = itemsFound.Find((ItemData item) =>
                        {
                            return Item.CompareIDs(itemsFound[i].ID, item.ID) && itemsFound[i] != item;
                        });

                        if (itemWithSameID == null) { itemsFound.RemoveAt(i); --i; }
                    }
                }

                // if showItemsWithoutMatch, remove all items that have matches
                if (showItemsWithoutMatch)
                {
                    for (int i = 0; i < mFoundItemDataWhileSearching?.Count; ++i)
                    {
                        ItemData itemsWithSameIDs = mStoredItemDataWhileSearching.Find((ItemData item) =>
                            { return Item.CompareIDs(mFoundItemDataWhileSearching[i].ID, item.ID); });
                        if (itemsWithSameIDs != null)
                        {
                            itemsStored.Remove(itemsWithSameIDs);
                        }
                    }

                    for (int i = 0; i < mStoredItemDataWhileSearching?.Count; ++i)
                    {
                        ItemData itemsWithSameIDs = mFoundItemDataWhileSearching.Find((ItemData item) =>
                            { return Item.CompareIDs(mStoredItemDataWhileSearching[i].ID, item.ID); });
                        if (itemsWithSameIDs != null)
                        {
                            itemsFound.Remove(itemsWithSameIDs);
                        }
                    }                    
                }

                // If showItemsWithoutFirstCharBeingALetter, remove all items where the first char is a letter
                if (showItemsWithoutFirstCharBeingALetter)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        Item itemStored;
                        mStoredItemAndDataItemDictionary.TryGetValue(itemsStored[i], out itemStored);
                        if (char.IsLetterOrDigit(itemStored.TextBoxName.Text[0]) || itemStored.TextBoxName.Text[0] == '?')
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        Item itemFound;
                        mFoundItemAndDataItemDictionary.TryGetValue(itemsFound[i], out itemFound);
                        if (char.IsLetterOrDigit(itemFound.TextBoxName.Text[0]) || itemFound.TextBoxName.Text[0] == '?')
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }
                }

                if (showItemsWithSameIDs || showItemsWithoutMatch)
                {
                    // Order lists after item IDs
                    OrderListsByID(ref itemsFound);
                    OrderListsByID(ref itemsStored);
                }
            }

            // Add the items that should be visable to the listboxes
            itemSourceFoundItems?.Clear();
            itemSourceStoredItems?.Clear();

            foreach (ItemData item in itemsFound)
            {
                Item itemFound;
                mFoundItemAndDataItemDictionary.TryGetValue(item, out itemFound);
                if (!itemSourceFoundItems.Contains(itemFound))
                    itemSourceFoundItemsTemporary.Add(itemFound);
            }

            foreach (ItemData item in itemsStored)
            {
                Item itemStored;
                mStoredItemAndDataItemDictionary.TryGetValue(item, out itemStored);
                if (!itemSourceStoredItems.Contains(itemStored))
                    itemSourceStoredItemsTemporary.Add(itemStored);
            }

            listBoxFoundItems.ItemsSource   = itemSourceFoundItemsTemporary;
            listBoxStoredItems.ItemsSource  = itemSourceStoredItemsTemporary;

            UpdateOldVariables(searchString, showItemsWithBadChars, showItemsWithUnusualChars, showItemsWithoutDescriptions,
                showItemsWithSameIDs, showItemsWithoutMatch, showItemsWithoutFirstCharBeingALetter,
                showItemsWithTooLongTranslations);
        }

        /// <summary>
        /// Search for items those name or ID matches the given search string and add the ones that matched to the result search list.
        /// This function also adds the corresponding matched item (if found) to the result match list.
        /// </summary>
        private static void SearchItemsByString(string searchString, ref List<ItemData> resultSearchList, ref List<ItemData> resultMatchList,
            ref List<ItemData> sourceSearchList, ref List<ItemData> sourceMatchList, bool isSearchForID = false)
        {
            if (!isSearchForID)
            {
                resultSearchList = sourceSearchList.Where((ItemData item) => { return item.Name.Contains(searchString); }).ToList();

                resultMatchList = new List<ItemData>();
                foreach (ItemData itemStored in resultSearchList)
                {
                    ItemData itemFound = sourceMatchList.FirstOrDefault((ItemData item) => { return Item.CompareIDs(item.ID, itemStored.ID); });

                    if (itemFound != null)
                        resultMatchList.Add(itemFound);
                }
            }
            else
            {
                if (sourceSearchList != null)
                    resultSearchList    = sourceSearchList.Where((ItemData item) =>
                        { return Item.IsIDContainingString(item.ID, searchString); }).ToList();
                if (resultMatchList != null)
                    resultMatchList     = sourceMatchList .Where((ItemData item) =>
                        { return Item.IsIDContainingString(item.ID, searchString); }).ToList();
            }            
        }

        /// <summary>
        /// Stores all the found items into the stored items' ListBox
        /// </summary>
        public static void StoreAllFoundItems(ref ListBox listBoxStoredItems)
        {
            StoreItems(ref listBoxStoredItems, mFoundItemsWhileSearching);
        }

        /// <summary>
        /// Stores all the found items into the found items' ListBox
        /// </summary>
        public static void StoreSelectedFoundItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems)
        {
            StoreItems(ref listBoxStoredItems, listBoxFoundItems.SelectedItems.Cast<Item>());
        }

        /// <summary>
        /// Stores all the items from "items" into the provided ListBox
        /// </summary>
        private static void StoreItems(ref ListBox listBoxStoredItems, IEnumerable<Item> items)
        {
            if (mFoundItemsWhileSearching == null || mFoundItemsWhileSearching.Count == 0)
                return;

            foreach (Item item in items)
            {
                if (mStoredItemsWhileSearching.Find((Item itemToCheck) => { return Item.CompareIDs(itemToCheck.ID, item.ID); }) == null)
                {
                    Item itemClone = item.Clone();
                    AddStoredItem(ref itemClone, ref listBoxStoredItems);
                }
                //else
                //{
                //    //TODO: Update Item Info (?)
                //}
            }
        }

        /// <summary>
        /// Addes an item to the stored items' ListBox
        /// </summary>
        public static void AddStoredItem(ref Item item, ref ListBox listBoxStoredItems)
        {
            ObservableCollection<Item> itemSourceStoredItems = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            bool insert = false;
            if (itemSourceStoredItems.Count > 0 && item.ItemStartPosition < itemSourceStoredItems.Last().ItemStartPosition)
                insert = true;

            ItemData itemData = item.GetItemData();

            if (!insert)
            {
                itemSourceStoredItems.Add(item);
                mStoredItemsWhileSearching.Add(item);                
                mStoredItemDataWhileSearching.Add(itemData);            // Store the item to the itemData list
                mStoredItemAndDataItemDictionary.Add(itemData, item);   // Store the item and itemData to the dictionary
            }
            else
            {
                for (int i = itemSourceStoredItems.Count - 1; i > -1; --i)
                {
                    if (item.ItemStartPosition < itemSourceStoredItems[i].ItemStartPosition)
                    {
                        itemSourceStoredItems.Insert(i, item);
                        mStoredItemsWhileSearching.Insert(i, item);
                        mStoredItemDataWhileSearching.Insert(i, itemData);      // Store the item to the itemData list
                        mStoredItemAndDataItemDictionary.Add(itemData, item);   // Store the item and itemData to the dictionary
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the item that is currently the selected one in the stored items' ListBox.
        /// </summary>
        public static void DeleteSelectedItemFromStoredItems(ref ListBox listBoxStoredItems)
        {
            ObservableCollection<Item> itemSourceStoredItems = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            // Remove selected item from both listBoxStoredItems and mStoredItemsWhileSearching
            Item selectedItem = listBoxStoredItems.SelectedItem as Item;
            int selectedIndex = itemSourceStoredItems.IndexOf(selectedItem);
            ItemData storedItemDataWhileSearching = mStoredItemDataWhileSearching.Find((ItemData itemData) =>
            { return Item.CompareIDs(itemData.ID, selectedItem.ID); });
            Item itemStored;
            mStoredItemAndDataItemDictionary.TryGetValue(storedItemDataWhileSearching, out itemStored);

            // Remove the item from memory
            itemSourceStoredItems.Remove(selectedItem);                             // Remove the item from the listBox
            mStoredItemsWhileSearching.Remove(itemStored);                          // Remove the item from the item list
            mStoredItemDataWhileSearching.Remove(storedItemDataWhileSearching);     // Remove the item from the itemData list
            mStoredItemAndDataItemDictionary.Remove(storedItemDataWhileSearching);  // Remove the item and itemData from the dictionary

            // Set focus on the next item
            if (itemSourceStoredItems.Count == 0)
                return;

            if (itemSourceStoredItems.Count > selectedIndex)
                itemSourceStoredItems[selectedIndex].Focus();
            else if (itemSourceStoredItems.Count == 1)
                itemSourceStoredItems[0].Focus();
            else if (itemSourceStoredItems.Count > selectedIndex - 1)
                itemSourceStoredItems[selectedIndex - 1].Focus();
        }

        /// <summary>
        /// Order the list by ID
        /// </summary>
        private static void OrderListsByID(ref List<ItemData> list)
        {
            list = list.OrderBy((ItemData item) => { return item.GetIDValue(); }).ToList();
        }
    }
}
