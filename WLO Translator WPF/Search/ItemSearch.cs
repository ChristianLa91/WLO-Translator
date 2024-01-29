using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
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

        public enum SearchOption
        {
            FOUND_ITEM_NAME,
            STORED_ITEM_NAME,
            ITEM_ID,
            DEFAULT
        }

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
                ItemData itemData = item.ToItemData();
                mFoundItemDataWhileSearching.Add(itemData);
                mFoundItemAndDataItemDictionary.Add(itemData, item);
            }
            foreach (Item item in mStoredItemsWhileSearching)
            {
                ItemData itemData = item.ToItemData();
                mStoredItemDataWhileSearching.Add(itemData);
                mStoredItemAndDataItemDictionary.Add(itemData, item);
            }
        }

        public static void ClearStoredItemsWhileSearching()
        {
            mStoredItemsWhileSearching?.Clear();
        }

        private static void UpdateOldVariables(string searchString,
            bool showItemsWithBadChars, bool showItemsWithUnusualChars, bool showItemsWithoutDescriptions, bool showItemsWithSameIDs,
            bool showItemsWithoutMatch, bool showItemsWithoutFirstCharBeingALetter)
        {
            mSearchStringOld                            = searchString;
            mShowItemsWithBadCharsOld                   = showItemsWithBadChars;
            mShowItemsWithUnusualCharsOld               = showItemsWithUnusualChars;
            //mShowItemsWithoutDescriptionsOld            = showItemsWithoutDescriptions;
            //mShowItemsWithSameIDsOld                    = showItemsWithSameIDs;
            //mShowItemsWithoutMatchOld                   = showItemsWithoutMatch;
            //mShowItemsWithoutFirstCharBeingALetterOld   = showItemsWithoutFirstCharBeingALetter;
        }

        public static void UpdateVisableItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems, string searchString,
            bool showItemsWithBadChars, bool showItemsWithUnusualChars, bool showItemsWithoutDescriptions, bool showItemsWithSameIDs,
            bool showItemsWithoutMatch, bool showItemsWithoutFirstCharBeingALetter, SearchOption searchOption = SearchOption.DEFAULT)
        {
            if (mFoundItemsWhileSearching == null && mStoredItemsWhileSearching == null)
                return;

            ObservableCollection<Item> itemSourceFoundItems     = (ObservableCollection<Item>)listBoxFoundItems.ItemsSource;
            ObservableCollection<Item> itemSourceStoredItems    = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;
            ObservableCollection<Item> itemSourceFoundItemsTemporary = new ObservableCollection<Item>();
            ObservableCollection<Item> itemSourceStoredItemsTemporary = new ObservableCollection<Item>();
            List<ItemData> itemsStored  = new List<ItemData>();
            List<ItemData> itemsFound   = new List<ItemData>();

            if (searchOption != SearchOption.DEFAULT)
                mSearchOption = searchOption;

            if (searchString == "" && !showItemsWithBadChars && !showItemsWithUnusualChars && !showItemsWithoutDescriptions
                && !showItemsWithSameIDs && !showItemsWithoutMatch && !showItemsWithoutFirstCharBeingALetter
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
                            SearchStoredItems(searchString, ref itemsFound, ref itemsStored,
                                ref mFoundItemDataWhileSearching, ref mStoredItemDataWhileSearching);
                            break;
                        case SearchOption.STORED_ITEM_NAME:
                            SearchStoredItems(searchString, ref itemsStored, ref itemsFound,
                                ref mStoredItemDataWhileSearching, ref mFoundItemDataWhileSearching);
                            break;
                        case SearchOption.ITEM_ID:
                            SearchStoredItems(searchString, ref itemsFound, ref itemsStored,
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
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemFound.TextBoxesContainsIllegalChars()
                                && !itemFound.TextBoxesContainsUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemFound.TextBoxesContainsIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemFound.TextBoxesContainsUnusualChars()))
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        Item itemStored;
                        mStoredItemAndDataItemDictionary.TryGetValue(itemsStored[i], out itemStored);
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemStored.TextBoxesContainsIllegalChars()
                                && !itemStored.TextBoxesContainsUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemStored.TextBoxesContainsIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemStored.TextBoxesContainsUnusualChars()))
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
                showItemsWithSameIDs, showItemsWithoutMatch, showItemsWithoutFirstCharBeingALetter);
        }

        private static void SearchStoredItems(string searchString, ref List<ItemData> resultSearchList, ref List<ItemData> resultMatchList,
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

        public static void StoreAllFoundItems(ref ListBox listBoxStoredItems)
        {
            if (mFoundItemsWhileSearching == null || mFoundItemsWhileSearching.Count == 0)
                return;

            ObservableCollection<Item> itemSourceStoredItems = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            foreach (Item item in mFoundItemsWhileSearching)
            {
                if (mStoredItemsWhileSearching.Find((Item itemToCheck) => { return Item.CompareIDs(itemToCheck.ID, item.ID); }) == null)
                {
                    itemSourceStoredItems.Add(item.Clone());
                    mStoredItemsWhileSearching.Add(item.Clone());
                }
            }
        }

        public static void StoreSelectedFoundItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems)
        {
            ObservableCollection<Item> itemSourceStoredItems    = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            foreach (Item item in listBoxFoundItems.SelectedItems)
            {
                if (mStoredItemsWhileSearching.Find((Item itemToCheck) => { return Item.CompareIDs(itemToCheck.ID, item.ID); }) == null)
                {
                    itemSourceStoredItems.Add(item.Clone());
                    mStoredItemsWhileSearching.Add(item.Clone());
                }
            }
        }

        public static void AddStoredItem(ref Item item, ref ListBox listBoxStoredItems)
        {
            ObservableCollection<Item> itemSourceStoredItems = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            mStoredItemsWhileSearching.Add(item);
            itemSourceStoredItems.Add(item);
        }

        public static void DeleteItemFromStoredItems(ref ListBox listBoxStoredItems)
        {
            ObservableCollection<Item> itemSourceStoredItems = (ObservableCollection<Item>)listBoxStoredItems.ItemsSource;

            // Remove selected item from both listBoxStoredItems and mStoredItemsWhileSearching
            Item selectedItem = listBoxStoredItems.SelectedItem as Item;
            int selectedIndex = itemSourceStoredItems.IndexOf(selectedItem);
            Item storedItemWhileSearching = mStoredItemsWhileSearching.Find((Item item) => { return Item.CompareIDs(item.ID, selectedItem.ID); });
            itemSourceStoredItems.Remove(selectedItem);
            mStoredItemsWhileSearching.Remove(storedItemWhileSearching);

            // Set focus on the next item
            if (itemSourceStoredItems.Count > selectedIndex)
                (itemSourceStoredItems[selectedIndex] as Item).Focus();
            else if (itemSourceStoredItems.Count == 1)
                (itemSourceStoredItems[0] as Item).Focus();
            else if (itemSourceStoredItems.Count > selectedIndex - 1)
                (itemSourceStoredItems[selectedIndex - 1] as Item).Focus();
        }

        private static void OrderListsByID(ref List<ItemData> list)
        {
            list = list.OrderBy((ItemData item) => { return item.GetIDValue(); }).ToList();
        }
    }
}
