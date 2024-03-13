using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Handles the translation-process of the by the user opened file (or currently opened by the multi-translator).
    /// </summary>
    static class Translation
    {
        private static int              mProgress           = 0;
        private static int              mOldProgress        = 0;
        private static bool             mCancelRequest      = false;
        private static bool             mIsTranslating      = false;
        private static List<ItemData>   mItemsWithoutTranslations;

        private enum TranslateDataType
        {
            NAME,
            DESCRIPTION,
            EXTRA1,
            EXTRA2
        }

        /// <summary>
        /// Used for specifying substrings for parts of item texts with the corresponding item data positions/indices.
        /// </summary>
        private class ItemDataPart
        {
            public string   Substring               { get; private set; }
            public int      TextPartStartPosition   { get; private set; }
            public int      ItemToStartFromIndex    { get; private set; }
            public int      ItemsToTranslate        { get; private set; }
            public int      Index                   { get; private set; }
            
            public ItemDataPart(string substring, int textPartStartPosition, int itemToStartFromIndex, int itemsToTranslate, int index)
            {
                Substring               = substring;
                TextPartStartPosition   = textPartStartPosition;
                ItemToStartFromIndex    = itemToStartFromIndex;
                ItemsToTranslate        = itemsToTranslate;
                Index                   = index;
            }
        }

        /// <summary>
        /// Asynchronous function that divides the file text that is translated into several tasks/processes that
        /// each and everyone handles their items text data, translates their part, returns the result back to this function and
        /// then puts together the fully translated file text and returns the result.
        /// </summary>
        public static async Task<string> TranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData,
            FileItemProperties fileItemProperties, bool isMultiTranslator = false)
        {
            mCancelRequest = false;

            if (mIsTranslating)
                return null;

            mIsTranslating = true;
            mItemsWithoutTranslations = new List<ItemData>();

            // Open loading bar Window
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadingBarManager.ShowOrInitializeLoadingBar("Found", 0d, fileItemProperties, foundItemData.Count(), true);
            });

            LoadingBarManager.WaitUntilValueHasChanged();

            // Calculate how many tasks and items per task that it should be
            int taskAmount = 0, maxTasks = 1000;
            if (foundItemData.Count() == 1)
                taskAmount = 1;
            else if (foundItemData.Count() < 1000)
                taskAmount = 2;
            else if (foundItemData.Count() / 1000 * 32 < maxTasks)
                taskAmount = foundItemData.Count() / 1000 * 32;
            else
                taskAmount = maxTasks;

            int                 itemsPerTask        = foundItemData.Count() / taskAmount, currentItemsPerTask = itemsPerTask;
            int                 itemsToTranslate    = itemsPerTask;
            ItemData            taskItemLast        = null;
            List<Task>          tasks               = new List<Task>();
            Stack<ItemDataPart> itemDataParts       = new Stack<ItemDataPart>();

            int itemStartPosition = 0;
            int itemStartIndex    = 0;

            foundItemData.Sort((ItemData itemData1, ItemData itemData2) => {
                return itemData1.ItemStartPosition.CompareTo(itemData2.ItemStartPosition); });

            // Collect the info for tasks
            for (int i = 0; i < taskAmount; i++)
            {
                // Get the last task item to translate for this task
                if (i == taskAmount - 1)
                {
                    taskItemLast        = foundItemData.Last();
                    itemsToTranslate    = foundItemData.Count() - i * itemsPerTask;
                }
                else
                    taskItemLast        = foundItemData[itemStartIndex + itemsPerTask - 1];

                if (itemStartPosition > foundItemData[itemStartIndex].ItemStartPosition)
                    throw new ArgumentException("itemStartPosition (" + itemStartPosition.ToString() + ") > "
                        + "foundItemData[itemStartIndex].ItemStartPosition ("
                        + foundItemData[itemStartIndex].ItemStartPosition.ToString() + ")");

                int itemEndPosition     = taskItemLast.ItemEndPosition;

                // Push a new ItemDataPart to the stack with the correct data
                string substring        = text.Substring(itemStartPosition, itemEndPosition - itemStartPosition + 1);
                Console.WriteLine("Substring length: " + substring.Length.ToString());
                itemDataParts.Push(new ItemDataPart(substring, itemStartPosition, itemStartIndex, itemsToTranslate, i));

                itemStartPosition       = itemEndPosition + 1;
                itemStartIndex         += itemsPerTask;
            }

            // Run tasks
            Semaphore semaphore = new Semaphore(1, 1);
            while (itemDataParts.Count > 0)
            {
                semaphore.WaitOne();
                ItemDataPart itemDataPart = itemDataParts.Pop();
                tasks.Add(Task.Run(() => TaskTranslatePartOfString
                (
                    foundItemData, storedItemData, itemDataPart.Substring,
                    itemDataPart.TextPartStartPosition, itemDataPart.ItemToStartFromIndex, itemDataPart.ItemsToTranslate,
                    itemDataPart.Index
                )));
                semaphore.Release();
            }

            bool    translationsDone = false;
            double  loadingBarValue  = 0;
            double  loadngBarMaximum = foundItemData.Count();

            // Update progress
            while (!translationsDone)
            {
                if (mOldProgress < mProgress)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (LoadingBarManager.IsLoadingBarNull() || LoadingBarManager.GetDialogResult() == false)
                                mCancelRequest = true;
                            else
                            {
                                LoadingBarManager.Value = mProgress;
                                loadingBarValue = LoadingBarManager.Value;
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    if (mCancelRequest)
                        break;

                    mOldProgress = mProgress;
                }

                if (loadingBarValue >= loadngBarMaximum)
                    translationsDone = true;

                if (tasks.All((Task task) => { return task.IsCompleted; }))
                    break;

                Thread.Sleep(300);
            }

            // Get result
            string result = "";
            tasks.Reverse();
            foreach (Task<string> task in tasks)
            {
                result += await task;
                Console.WriteLine(task.Result.Substring(0, 25));
                task.Dispose();
            }

            // Close loading bar
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!isMultiTranslator)
                    LoadingBarManager.CloseLoadingBar();
            });

            mIsTranslating = false;

            return result;
        }

        /// <summary>
        /// Task (thread) that handles a part of the total text/string of the file that is being translated.
        /// It uses the ReplaceTextAtPosition-fuction at specific places to fully translate each part of the string that should
        /// be translated.
        /// </summary>
        public static string TaskTranslatePartOfString(List<ItemData> foundItemData, List<ItemData> storedItemData, string textPart,
            int textPartStartPosition, int itemToStartFromIndex, int itemsToTranslate, int index)
        {
            int itemsTranslated = 0;
            var storedItemDataEnumerable = storedItemData.AsEnumerable();

            for (int i = itemToStartFromIndex; i < itemToStartFromIndex + itemsToTranslate; i++)
            {
                if (mCancelRequest)
                    break;

                ItemData foundItem  = foundItemData.ElementAt(i);
                ItemData storedItem = storedItemDataEnumerable.FirstOrDefault((ItemData item) =>
                {
                    return Item.CompareIDs(item.ID, foundItem.ID);
                });

                if (storedItem == null)
                {
                    if (foundItem != null)
                    {
                        mItemsWithoutTranslations.Add(foundItem);
                        Console.WriteLine("Did not find the item \"" + foundItem.Name + "\" (ID: " + TextManager.GetIDToString(foundItem.ID) +
                            ") in storedItemData");
                    }
                    else
                        Console.WriteLine("Did not find the item <NULL> in storedItemData");

                    ++itemsTranslated;
                    ++mProgress;
                    continue;
                }

                int textPartLengthOld = textPart.Length;
                //// Replace item name length
                //string storedItemNameLength = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)storedItem.Name.Length });
                //textPart = TextManager.GetReplacedPartString(textPart, storedItemNameLength,
                //    foundItem.ItemStartPosition - textPartStartPosition,
                //    foundItem.ItemStartPosition - textPartStartPosition + 1);

                //// Replace item name
                //int nameLengthDifference = foundItem.Name.Length - storedItem.Name.Length;
                //textPart = TextManager.GetReplacedPartString(textPart,
                //    TextManager.GetNullStringOfLength(nameLengthDifference) +
                //        storedItem.NameReversed,
                //    foundItem.NameStartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(nameLengthDifference),
                //    foundItem.NameEndPosition - textPartStartPosition);

                ReplaceTextAtPosition(ref textPart, textPartStartPosition, ref foundItem, ref storedItem,/*foundItem.ItemStartPosition, storedItem.NameReversed,
                    foundItem.Name.Length, foundItem.NameStartPosition, foundItem.NameEndPosition,*/ TranslateDataType.NAME);

                if (storedItem.Description != null && storedItem.Description != "")
                {
                    //// Replace item description length
                    //string storedItemDescriptionLength = System.Text.Encoding.GetEncoding(1252).GetString(
                    //    new byte[] { (byte)storedItem.Description.Length });
                    //textPart = TextManager.GetReplacedPartString(textPart, storedItemDescriptionLength,
                    //    foundItem.DescriptionLengthPosition - textPartStartPosition,
                    //    foundItem.DescriptionLengthPosition - textPartStartPosition + 1);

                    //// Replace item description
                    //int descriptionLengthDifference = foundItem.Description.Length - storedItem.Description.Length;
                    //textPart = TextManager.GetReplacedPartString(textPart,
                    //    TextManager.GetNullStringOfLength(descriptionLengthDifference) +
                    //        storedItem.DescriptionReversed,
                    //    foundItem.DescriptionStartPosition - textPartStartPosition
                    //        - TextManager.GetNullsToDecreaseAmount(descriptionLengthDifference),
                    //    foundItem.DescriptionEndPosition - textPartStartPosition);

                    ReplaceTextAtPosition(ref textPart, textPartStartPosition, ref foundItem, ref storedItem, TranslateDataType.DESCRIPTION);
                }
                //else
                //    Console.WriteLine(storedItem.Name + "'s description empty");

                if (storedItem.Extra1 != null && storedItem.Extra1 != "")
                {
                    //// Replace item extra1 length
                    //string storedItemExtra1Length = System.Text.Encoding.GetEncoding(1252).GetString(
                    //    new byte[] { (byte)storedItem.Extra1.Length });
                    //textPart = TextManager.GetReplacedPartString(textPart, storedItemExtra1Length,
                    //    foundItem.Extra1LengthPosition - textPartStartPosition,
                    //    foundItem.Extra1LengthPosition - textPartStartPosition + 1);

                    //// Replace item extra1
                    //int extra1LengthDifference = foundItem.Extra1.Length - storedItem.Extra1.Length;
                    //textPart = TextManager.GetReplacedPartString(textPart,
                    //    TextManager.GetNullStringOfLength(extra1LengthDifference) +
                    //        storedItem.Extra1Reversed,
                    //    foundItem.Extra1StartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(extra1LengthDifference),
                    //    foundItem.Extra1EndPosition - textPartStartPosition);

                    ReplaceTextAtPosition(ref textPart, textPartStartPosition, ref foundItem, ref storedItem, TranslateDataType.EXTRA1);
                }

                if (storedItem.Extra2 != null && storedItem.Extra2 != "")
                {
                    //// Replace item extra2 length
                    //string storedItemExtra2Length = System.Text.Encoding.GetEncoding(1252).GetString(
                    //    new byte[] { (byte)storedItem.Extra2.Length });
                    //textPart = TextManager.GetReplacedPartString(textPart, storedItemExtra2Length,
                    //    foundItem.Extra2LengthPosition - textPartStartPosition,
                    //    foundItem.Extra2LengthPosition - textPartStartPosition + 1);

                    //// Replace item extra2
                    //int extra2LengthDifference = foundItem.Extra2.Length - storedItem.Extra2.Length;
                    //textPart = TextManager.GetReplacedPartString(textPart,
                    //    TextManager.GetNullStringOfLength(extra2LengthDifference) +
                    //        storedItem.Extra2Reversed,
                    //    foundItem.Extra2StartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(extra2LengthDifference),
                    //    foundItem.Extra2EndPosition - textPartStartPosition);

                    ReplaceTextAtPosition(ref textPart, textPartStartPosition, ref foundItem, ref storedItem, TranslateDataType.EXTRA2);
                }

                if (textPartLengthOld != textPart.Length)
                    Console.WriteLine("ERROR: Text length before (" + textPartLengthOld.ToString() +
                        "not equal to the length after:" + textPart.Length.ToString());

                ++itemsTranslated;
                ++mProgress;
            }

            Console.WriteLine("Task items translated: " + itemsTranslated.ToString());

            return textPart;
        }

        /// <summary>
        /// Replaces the found item data text with the stored item data text at the specified positions/indices based on the
        /// provided data type.
        /// </summary>
        //TODO: program so that item names can be moved, in order to make it possible for the item names (in Item.dat at least) to be longer.
        private static void ReplaceTextAtPosition(ref string text, int textOffset, ref ItemData foundItem, ref ItemData storedItem, /*int lengthStartIndex,
            string replace, int oldTextLength, int replaceStartIndex, int replaceEndIndex,*/ TranslateDataType translateDataType)
        {
            int lengthStartIndex, oldTextLength, replaceStartIndex, replaceNextStartIndex = -1, replacePreviousStartIndex = -1, replaceEndIndex;
            string replace;

            switch (translateDataType)
            {
                case TranslateDataType.NAME:
                    lengthStartIndex            = foundItem.ItemStartPosition;
                    oldTextLength               = foundItem.NameOriginal.Length;
                    replaceStartIndex           = foundItem.NameStartPosition;
                    replaceEndIndex             = foundItem.NameEndPosition;
                    replace                     = storedItem.NameReversed;
                    replaceNextStartIndex       = foundItem.DescriptionStartPosition;
                    break;
                case TranslateDataType.DESCRIPTION:
                    lengthStartIndex            = foundItem.DescriptionLengthPosition;
                    oldTextLength               = foundItem.DescriptionOriginal.Length;
                    replaceStartIndex           = foundItem.DescriptionStartPosition;
                    replaceEndIndex             = foundItem.DescriptionEndPosition;
                    replace                     = storedItem.DescriptionReversed;
                    replacePreviousStartIndex   = foundItem.NameStartPosition;
                    replaceNextStartIndex       = foundItem.Extra1StartPosition;
                    break;
                case TranslateDataType.EXTRA1:
                    lengthStartIndex            = foundItem.Extra1LengthPosition;
                    oldTextLength               = foundItem.Extra1Original.Length;
                    replaceStartIndex           = foundItem.Extra1StartPosition;
                    replaceEndIndex             = foundItem.Extra1EndPosition;
                    replace                     = storedItem.Extra1Reversed;
                    replacePreviousStartIndex   = foundItem.DescriptionStartPosition;
                    replaceNextStartIndex       = foundItem.Extra2StartPosition;
                    break;
                case TranslateDataType.EXTRA2:
                    lengthStartIndex            = foundItem.Extra2LengthPosition;
                    oldTextLength               = foundItem.Extra2Original.Length;
                    replaceStartIndex           = foundItem.Extra2StartPosition;
                    replaceEndIndex             = foundItem.Extra2EndPosition;
                    replace                     = storedItem.Extra2Reversed;
                    replacePreviousStartIndex   = foundItem.Extra1StartPosition;
                    break;
                default:
                    Console.WriteLine("ERROR: Translate Data Type \"" + translateDataType.ToString() + "\"");
                    return;
            }

            // Replace length
            string replaceLength = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)replace.Length });
            text = TextManager.GetStringWithReplacedPart(text, replaceLength,
                lengthStartIndex    - textOffset,
                lengthStartIndex    - textOffset + 1);

            //TODO: Check for nulls available
            int movementOffset = 0;
#if DEBUG
            int nullsLeftOfReplaceStartIndex = TextManager.GetNullsLeftOfIndex(ref text, replaceStartIndex - textOffset);

            int nullsLeftOfNextStartIndex = -1, nullsLeftOfPreviousStartIndex = -1;
            if (replaceNextStartIndex != -1)
                nullsLeftOfNextStartIndex    = TextManager.GetNullsLeftOfIndex(ref text, replaceNextStartIndex - textOffset);
            if (replacePreviousStartIndex != -1)
                nullsLeftOfPreviousStartIndex = TextManager.GetNullsLeftOfIndex(ref text, replacePreviousStartIndex - textOffset);
            switch (translateDataType)
            {
                case TranslateDataType.NAME:
                    if (replace.Length - oldTextLength > nullsLeftOfReplaceStartIndex)
                    {
                        if (nullsLeftOfReplaceStartIndex + nullsLeftOfNextStartIndex >= replace.Length)
                        {
                            movementOffset = replace.Length - oldTextLength - nullsLeftOfReplaceStartIndex;
                            TextManager.MoveSubstring(ref text, replaceStartIndex - textOffset,
                                replaceNextStartIndex - textOffset - nullsLeftOfNextStartIndex,
                                movementOffset);
                        }
                        else
                            Console.WriteLine("Translation Interrupted Due to Error: Occurence(s) of Translations with Too Long Name + Description Together."
                                + "\nFirst Occurance ID: \"" + TextManager.GetIDToString(storedItem.ID) + "\", Name: \"" + storedItem.Name + "\"");
                    }
                    break;
                case TranslateDataType.DESCRIPTION:
                    if (replace.Length - oldTextLength > nullsLeftOfReplaceStartIndex)
                    {
                        //TODO: Get nulls left for name and use that to move the substring instead of nullsLeftOfNextStartIndex
                        if (nullsLeftOfReplaceStartIndex + nullsLeftOfPreviousStartIndex >= replace.Length)
                        {
                            movementOffset = -(storedItem.Name.Length - foundItem.NameOriginal.Length - nullsLeftOfReplaceStartIndex);
                            TextManager.MoveSubstring(ref text, foundItem.NameStartPosition - textOffset,
                                replaceStartIndex - textOffset - nullsLeftOfReplaceStartIndex,
                                movementOffset);
                        }
                        else
                            Console.WriteLine("Translation Interrupted Due to Error: Occurence(s) of Translations with Too Long Name + Description Together."
                                + "\nFirst Occurance ID: \"" + TextManager.GetIDToString(storedItem.ID) + "\", Name: \"" + storedItem.Name + "\"");
                    }
                    break;
            }
#endif

            // Replace text
            int lengthDifference = oldTextLength - replace.Length;
            int offsetToAdd = 0;

            //if (storedItem.Name.Length - foundItem.NameOriginal.Length > nullsLeftOfPreviousStartIndex &&
            //    translateDataType == TranslateDataType.DESCRIPTION)
            //    offsetToAdd = storedItem.Name.Length - foundItem.NameOriginal.Length - nullsLeftOfPreviousStartIndex;
            text = TextManager.GetStringWithReplacedPart(text, TextManager.GetNullStringOfLength(lengthDifference) + replace,
                replaceStartIndex   - textOffset + offsetToAdd - TextManager.GetNullsToDecreaseAmount(lengthDifference) + movementOffset,
                replaceEndIndex     - textOffset + offsetToAdd + movementOffset);
        }

        /// <summary>
        /// Returns the items that couldn't be translated, which are grabbed by the multi-translator and listed there.
        /// </summary>
        public static List<ItemData> GetItemsWithoutTranslations()
        {
            return mItemsWithoutTranslations;
        }
    }
}
