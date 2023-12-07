using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    public static class ItemSearch
    {
        private static List<Item>   mStoredItemsWhileSearching;
        private static List<Item>   mFoundItemsWhileSearching;
        private static SearchOption mSearchOption = SearchOption.FOUND_ITEM_NAME;

        private static string       mSearchStringOld;
        private static bool         mShowItemsWithBadCharsOld;
        private static bool         mShowItemsWithUnusualCharsOld;
        private static bool         mShowItemsWithoutDescriptionsOld;
        private static bool         mShowItemsWithSameIDsOld;
        private static bool         mShowItemsWithoutMatchOld;
        private static bool         mShowItemsWithoutFirstCharBeingALetterOld;

        public enum SearchOption
        {
            FOUND_ITEM_NAME,
            STORED_ITEM_NAME,
            ITEM_ID,
            DEFAULT
        }

        public static void UpdateStoredAndFoundItemsWhileSearchingLists(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems)
        {
            mStoredItemsWhileSearching  = listBoxStoredItems.Items.OfType<Item>().ToList();
            mFoundItemsWhileSearching   = listBoxFoundItems.Items.OfType<Item>().ToList();
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
            mShowItemsWithoutDescriptionsOld            = showItemsWithoutDescriptions;
            mShowItemsWithSameIDsOld                    = showItemsWithSameIDs;
            mShowItemsWithoutMatchOld                   = showItemsWithoutMatch;
            mShowItemsWithoutFirstCharBeingALetterOld   = showItemsWithoutFirstCharBeingALetter;
        }

        public static void UpdateVisableItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems, string searchString,
            bool showItemsWithBadChars, bool showItemsWithUnusualChars, bool showItemsWithoutDescriptions, bool showItemsWithSameIDs,
            bool showItemsWithoutMatch, bool showItemsWithoutFirstCharBeingALetter, SearchOption searchOption = SearchOption.DEFAULT)
        {
            List<Item> itemsStored  = new List<Item>();
            List<Item> itemsFound   = new List<Item>();

            if (searchOption != SearchOption.DEFAULT)
                mSearchOption = searchOption;

            if (searchString == "" && !showItemsWithBadChars && !showItemsWithUnusualChars && !showItemsWithoutDescriptions && !showItemsWithSameIDs && !showItemsWithoutMatch &&
                !showItemsWithoutFirstCharBeingALetter && mStoredItemsWhileSearching != null)
            {
                // Add the "while searching" items back
                itemsStored = mStoredItemsWhileSearching;
                itemsFound  = mFoundItemsWhileSearching;
            }
            else
            {
                // Add the "while searching" items back depending on the old variables
                if (mShowItemsWithBadCharsOld && !mShowItemsWithUnusualCharsOld && showItemsWithUnusualChars    // unusual chars added to bad chars
                    || (mShowItemsWithUnusualCharsOld && !mShowItemsWithBadCharsOld && showItemsWithBadChars)   // bad chars added to unusual chars
                    || (mSearchStringOld != null && mSearchStringOld.Contains(searchString))
                    || (searchString != mSearchStringOld && mSearchStringOld != null && !mSearchStringOld.Contains(searchString))
                    || (searchString == "" && mSearchStringOld != ""))
                {
                    if (mStoredItemsWhileSearching != null)
                        itemsStored = mStoredItemsWhileSearching.ToList();
                    if (mFoundItemsWhileSearching != null)
                        itemsFound  = mFoundItemsWhileSearching.ToList();
                }

                // Search for items containing a string
                if (searchString != "")
                {
                    switch (mSearchOption)
                    {
                        case SearchOption.FOUND_ITEM_NAME:
                            SearchStoredItems(searchString, ref itemsFound, ref itemsStored,
                                ref mFoundItemsWhileSearching, ref mStoredItemsWhileSearching);
                            break;
                        case SearchOption.STORED_ITEM_NAME:
                            SearchStoredItems(searchString, ref itemsStored, ref itemsFound,
                                ref mStoredItemsWhileSearching, ref mFoundItemsWhileSearching);
                            break;
                        case SearchOption.ITEM_ID:
                            SearchStoredItems(searchString, ref itemsFound, ref itemsStored,
                                ref mFoundItemsWhileSearching, ref mStoredItemsWhileSearching, true);
                            OrderListsByID(ref itemsFound);
                            OrderListsByID(ref itemsStored);
                            break;
                    }
                }

                // If showItemsWithBadCharsOnly, remove all items that don't contain bad chars in their text boxes
                if (showItemsWithBadChars || showItemsWithUnusualChars)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemsStored[i].TextBoxesContainsIllegalChars() && !itemsStored[i].TextBoxesContainsUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemsStored[i].TextBoxesContainsIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemsStored[i].TextBoxesContainsUnusualChars()))
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        if ((showItemsWithBadChars && showItemsWithUnusualChars && !itemsFound[i].TextBoxesContainsIllegalChars() && !itemsFound[i].TextBoxesContainsUnusualChars())
                            || (showItemsWithBadChars && !showItemsWithUnusualChars && !itemsFound[i].TextBoxesContainsIllegalChars())
                            || (!showItemsWithBadChars && showItemsWithUnusualChars && !itemsFound[i].TextBoxesContainsUnusualChars()))
                        {
                            itemsFound.RemoveAt(i);
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
                        List<Item> itemsWithSameIDs = itemsStored.FindAll((Item item) =>
                        {
                            return Item.CompareIDs(itemsStored[i].ID, item.ID) && itemsStored[i] != item;
                        });

                        if (itemsWithSameIDs == null || itemsWithSameIDs.Count == 0) { itemsStored.RemoveAt(i); --i; }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        List<Item> itemsWithSameIDs = itemsFound.FindAll((Item item) =>
                        {
                            return Item.CompareIDs(itemsFound[i].ID, item.ID) && itemsFound[i] != item;
                        });

                        if (itemsWithSameIDs == null || itemsWithSameIDs.Count == 0) { itemsFound.RemoveAt(i); --i; }
                    }

                    // Order lists after item IDs
                    OrderListsByID(ref itemsFound);
                    OrderListsByID(ref itemsStored);
                }

                if (showItemsWithoutMatch)
                {
                    for (int i = 0; i < mFoundItemsWhileSearching?.Count; ++i)
                    {
                        List<Item> itemsWithSameIDs = mStoredItemsWhileSearching.FindAll((Item item) =>
                            { return Item.CompareIDs(mFoundItemsWhileSearching[i].ID, item.ID); });
                        if (itemsWithSameIDs == null || itemsWithSameIDs.Count > 0) { itemsStored.Remove(itemsWithSameIDs[0]); }
                    }

                    for (int i = 0; i < mStoredItemsWhileSearching?.Count; ++i)
                    {
                        List<Item> itemsWithSameIDs = mFoundItemsWhileSearching.FindAll((Item item) =>
                            { return Item.CompareIDs(mStoredItemsWhileSearching[i].ID, item.ID); });
                        if (itemsWithSameIDs == null || itemsWithSameIDs.Count > 0) { itemsFound.Remove(itemsWithSameIDs[0]); }
                    }

                    // Order lists after item IDs
                    OrderListsByID(ref itemsFound);
                    OrderListsByID(ref itemsStored);
                }

                if (showItemsWithoutFirstCharBeingALetter)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        if (char.IsLetterOrDigit(itemsStored[i].TextBoxName.Text[0]) || itemsStored[i].TextBoxName.Text[0] == '?')
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        if (char.IsLetterOrDigit(itemsFound[i].TextBoxName.Text[0]) || itemsFound[i].TextBoxName.Text[0] == '?')
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }

            // Add the items that should be visable to the listboxes
            listBoxFoundItems.Items.Clear();
            listBoxStoredItems.Items.Clear();

            foreach (Item item in itemsFound)
            {
                if (!listBoxFoundItems.Items.Contains(item))
                    listBoxFoundItems.Items.Add(item);
            }

            foreach (Item item in itemsStored)
            {
                if (!listBoxStoredItems.Items.Contains(item))
                    listBoxStoredItems.Items.Add(item);
            }

            UpdateOldVariables(searchString, showItemsWithBadChars, showItemsWithUnusualChars, showItemsWithoutDescriptions, showItemsWithSameIDs,
                showItemsWithoutMatch, showItemsWithoutFirstCharBeingALetter);
        }

        private static void SearchStoredItems(string searchString, ref List<Item> resultSearchList, ref List<Item> resultMatchList,
            ref List<Item> sourceSearchList, ref List<Item> sourceMatchList, bool isSearchForID = false)
        {
            if (!isSearchForID)
            {
                resultSearchList = sourceSearchList.Where((Item item) => { return item.Name.Contains(searchString); }).ToList();

                resultMatchList = new List<Item>();
                foreach (Item itemStored in resultSearchList)
                {
                    Item itemFound = sourceMatchList.FirstOrDefault((Item item) => { return Item.CompareIDs(item.ID, itemStored.ID); });

                    if (itemFound != null)
                        resultMatchList.Add(itemFound);
                }
            }
            else
            {
                if (sourceSearchList != null)
                    resultSearchList    = sourceSearchList.Where((Item item) => { return Item.IsIDContainingString(item.ID, searchString); }).ToList();
                if (resultMatchList != null)
                    resultMatchList     = sourceMatchList .Where((Item item) => { return Item.IsIDContainingString(item.ID, searchString); }).ToList();
            }            
        }

        public static void StoreAllFoundItems(ref ListBox listBoxStoredItems)
        {
            foreach (Item item in mFoundItemsWhileSearching)
            {
                if (mStoredItemsWhileSearching.Find((Item itemToCheck) => { return Item.CompareIDs(itemToCheck.ID, item.ID); }) == null)
                {
                    listBoxStoredItems.Items.Add(item.Clone());
                    mStoredItemsWhileSearching.Add(item.Clone());
                }
            }
        }

        public static void StoreSelectedFoundItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems)
        {
            foreach (Item item in listBoxFoundItems.SelectedItems)
            {
                if (mStoredItemsWhileSearching.Find((Item itemToCheck) => { return Item.CompareIDs(itemToCheck.ID, item.ID); }) == null)
                {
                    listBoxStoredItems.Items.Add(item.Clone());
                    mStoredItemsWhileSearching.Add(item.Clone());
                }
            }
        }

        public static void AddStoredItem(ref Item item, ref ListBox listBoxStoredItems)
        {
            mStoredItemsWhileSearching.Add(item);
            listBoxStoredItems.Items.Add(item);
        }

        public static void DeleteItemFromStoredItems(ref ListBox listBoxStoredItems)
        {
            // Remove selected item from both listBoxStoredItems and mStoredItemsWhileSearching
            Item selectedItem = listBoxStoredItems.SelectedItem as Item;
            int selectedIndex = listBoxStoredItems.Items.IndexOf(selectedItem);
            Item storedItemWhileSearching = mStoredItemsWhileSearching.Find((Item item) => { return Item.CompareIDs(item.ID, selectedItem.ID); });
            listBoxStoredItems.Items.Remove(selectedItem);
            mStoredItemsWhileSearching.Remove(storedItemWhileSearching);

            // Set focus on the next item
            if (listBoxStoredItems.Items.Count > selectedIndex)
                (listBoxStoredItems.Items[selectedIndex] as Item).Focus();
            else if (listBoxStoredItems.Items.Count == 1)
                (listBoxStoredItems.Items[0] as Item).Focus();
            else if (listBoxStoredItems.Items.Count > selectedIndex - 1)
                (listBoxStoredItems.Items[selectedIndex - 1] as Item).Focus();
        }

        private static void OrderListsByID(ref List<Item> list)
        {
            list = list.OrderBy((Item item) => { return item.GetIDValue(); }).ToList();
        }
    }
}
