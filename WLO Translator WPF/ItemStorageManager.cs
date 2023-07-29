using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    public static class ItemStorageManager
    {
        private static List<Item> mStoredItemsWhileSearching;
        private static List<Item> mFoundItemsWhileSearching;

        public static void UpdateVisableItems(ref ListBox listBoxFoundItems, ref ListBox listBoxStoredItems,
            string searchString, bool showItemsWithBadCharsOnly, bool showItemsWithoutDescriptionsOnly)
        {
            List<Item> itemsStored = new List<Item>();
            List<Item> itemsFound = new List<Item>();

            if (searchString == "" && !showItemsWithBadCharsOnly && mStoredItemsWhileSearching != null)
            {
                // Add the stored items back
                itemsStored = mStoredItemsWhileSearching;
                itemsFound = mFoundItemsWhileSearching;
                mStoredItemsWhileSearching = null;
                mFoundItemsWhileSearching = null;
            }
            else
            {
                // Temporary store the items before manipulations                
                if (mStoredItemsWhileSearching == null)
                {
                    mStoredItemsWhileSearching = listBoxStoredItems.Items.OfType<Item>().ToList();
                    mFoundItemsWhileSearching = listBoxFoundItems.Items.OfType<Item>().ToList();
                }

                // Search for items containing a string
                if (searchString != "" && searchString.Length < 2)
                {
                    // Find items and temporary store them localy
                    itemsStored = mStoredItemsWhileSearching.Where((Item item) =>
                    {
                        return item.Name.Contains(searchString);
                    }).ToList();

                    foreach (Item itemStored in itemsStored)
                    {
                        Item itemFound = mFoundItemsWhileSearching.FirstOrDefault((Item item) =>
                        {
                            return item.ID[0] == itemStored.ID[0] && item.ID[1] == itemStored.ID[1];
                        });

                        if (itemFound != null)
                            itemsFound.Add(itemFound);
                    }
                }
                else
                {
                    itemsStored = listBoxStoredItems.Items.OfType<Item>().ToList();
                    itemsFound = listBoxFoundItems.Items.OfType<Item>().ToList();
                }

                // If showItemsWithBadCharsOnly, remove all items that don't contain bad chars in their text boxes
                if (showItemsWithBadCharsOnly)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        if (!itemsStored[i].TextBoxesContainsIllegalChars())
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        if (!itemsFound[i].TextBoxesContainsIllegalChars())
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }
                }

                // If showItemsWithoutDescriptionsOnly, remove all items that has a description that's not equal to ""
                if (showItemsWithoutDescriptionsOnly)
                {
                    for (int i = 0; i < itemsStored.Count; ++i)
                    {
                        if (itemsStored[i].Description != "")
                        {
                            itemsStored.RemoveAt(i);
                            --i;
                        }
                    }

                    for (int i = 0; i < itemsFound.Count; ++i)
                    {
                        if (itemsFound[i].Description != "")
                        {
                            itemsFound.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }

            // Add the items that should be visable to the listboxes
            listBoxStoredItems.Items.Clear();
            listBoxFoundItems.Items.Clear();
            foreach (Item itemCurrent in itemsStored)
            {
                listBoxStoredItems.Items.Add(itemCurrent);
            }

            foreach (Item itemCurrent in itemsFound)
            {
                if (!listBoxFoundItems.Items.Contains(itemCurrent))
                    listBoxFoundItems.Items.Add(itemCurrent);
            }
        }
    }
}
