using System;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    public static class ItemManager
    {
        private static ListBox      mListBoxFoundItems;
        //private static FileManager  mFileManager;

        public static void Initialize(ref ListBox listBoxFoundItems/*, ref FileManager fileManager*/)
        {
            mListBoxFoundItems  = listBoxFoundItems;
            //mFileManager        = fileManager;
        }

        public static void SetNullLength(IItemBase foundItem, IItemBase storedItem)
        {
            if (foundItem == null)
                return;

            storedItem.NameNullsLeft        = foundItem.NameNullsLeft   - (storedItem.Name.Length - foundItem.Name.Length);

            if (FileManager.FileItemProperties.HasDescription)
            {
                storedItem.DescriptionNullsLeft = foundItem.DescriptionNullsLeft
                    - (storedItem.Description.Length - foundItem.Description.Length);
            }

            if (FileManager.FileItemProperties.HasExtras)
            {
                storedItem.Extra1NullsLeft = foundItem.Extra1NullsLeft - (storedItem.Extra1.Length - foundItem.Extra1.Length);
                storedItem.Extra2NullsLeft = foundItem.Extra2NullsLeft - (storedItem.Extra2.Length - foundItem.Extra2.Length);
            }
        }

        //TODO: This doesn't work because we don't know the original name/description/extra1/extra2 lengths
        //We need them in order to judge how much to add to the total (oldNameLength - newNameLength + nullsLeft)
        public static int GetNullsLeftForChars(Item item)
        {
            //string openFileText = mFileManager.GetOpenFileText();

            // Get found item
            Item foundItem = item;
            if (item.IsStored)
            {
                if (item.NameNullsLeft == null)
                {
                    foundItem = mListBoxFoundItems.ItemsSource.OfType<Item>().FirstOrDefault((Item itemFound) =>
                    {
                        return Item.CompareIDs(itemFound.ID, item.ID);
                    });

                    if (foundItem == null)
                        return 0;
                }
            }

            //int textOffset = foundItem.ItemStartPosition;
            //int foundItemLength = foundItem.ItemEndPosition - foundItem.ItemStartPosition;
            //if (textOffset + foundItemLength >= mFileManager.GetOpenFileText().Length)
            //{
            //    Console.WriteLine("Error at GetNullsLeftForChars() with item\n" + item.Name + "\n: Item's lenght out of range of the file");
            //    foundItemLength = openFileText.Length - textOffset;
            //    Console.WriteLine("Item length used corrected to " + foundItemLength.ToString());
            //}

            //if (foundItemLength < 1)
            //    return 0;

            //string  wholeItemText   = openFileText.Substring(textOffset, foundItemLength);
            int total = 0;//, nullsLeftForName, nullsLeftForDescription, nullsLeftForExtra1, nullsLeftForExtra2;

            // Name
            //nullsLeftForName = TextManager.GetNullsLeftOfIndex(ref wholeItemText, foundItem.NameStartPosition - textOffset);
            if (foundItem.NameNullsLeft.HasValue)
                total = foundItem.NameNullsLeft.Value + (foundItem.NameOriginal.Length - item.Name.Length);

            // Description
            if (foundItem.HasDescription && foundItem.Description != null)
            {
                //nullsLeftForDescription = TextManager.GetNullsLeftOfIndex(ref wholeItemText, foundItem.DescriptionStartPosition - textOffset,
                //    foundItem.NameEndPosition - textOffset);
                if (foundItem.DescriptionNullsLeft.HasValue)
                    total += foundItem.DescriptionNullsLeft.Value + (foundItem.DescriptionOriginal.Length - item.Description.Length);
            }

            // Extras
            if (foundItem.HasExtras && foundItem.Extra1 != null && foundItem.Extra2 != null)
            {
                //nullsLeftForExtra1 = TextManager.GetNullsLeftOfIndex(ref wholeItemText, foundItem.Extra1StartPosition - textOffset,
                //    foundItem.DescriptionEndPosition - textOffset);
                //nullsLeftForExtra2 = TextManager.GetNullsLeftOfIndex(ref wholeItemText, foundItem.Extra2StartPosition - textOffset,
                //    foundItem.Extra1EndPosition - textOffset);
                if (foundItem.Extra1NullsLeft.HasValue && foundItem.Extra2NullsLeft.HasValue)
                    total += foundItem.Extra1NullsLeft.Value + (foundItem.Extra1Original.Length - item.Extra1.Length)
                           + foundItem.Extra2NullsLeft.Value + (foundItem.Extra2Original.Length - item.Extra2.Length);
            }

            //Console.WriteLine("Nulls left for chars" + total.ToString());

            return total;
        }
    }
}
