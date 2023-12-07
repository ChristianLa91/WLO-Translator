using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WLO_Translator_WPF
{
    static class Translation
    {
        private static WindowLoadingBar mWindowLoadingBar;
        private static int              mProgress           = 0;
        private static int              mOldProgress        = 0;
        private static bool             mCancelRequest      = false;

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

        public static async Task<string> TranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData)
        {
            mCancelRequest = false;

            if (mWindowLoadingBar != null)
                return null;

            // Open loading bar Window
            Application.Current.Dispatcher.Invoke(() =>
            {
                mWindowLoadingBar = new WindowLoadingBar("Translating", "Translating Found Items", "Translated: ", foundItemData.Count());
                mWindowLoadingBar.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                mWindowLoadingBar.Owner = Application.Current.MainWindow;
                mWindowLoadingBar.Value = 0;
                //mWindowLoadingBar.Maximum = foundItemData.Count();
                _ = Task.Run(() => { Application.Current.Dispatcher.Invoke(() => { mWindowLoadingBar.ShowDialog(); }); });
            });

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

            foundItemData.Sort((ItemData itemData1, ItemData itemData2) => { return itemData1.ItemStartPosition.CompareTo(itemData2.ItemStartPosition); });

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
                        + "foundItemData[itemStartIndex].ItemStartPosition (" + foundItemData[itemStartIndex].ItemStartPosition.ToString() + ")");

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
                    itemDataPart.TextPartStartPosition, itemDataPart.ItemToStartFromIndex, itemDataPart.ItemsToTranslate, itemDataPart.Index
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (mWindowLoadingBar == null || mWindowLoadingBar.DialogResult == false)
                            mCancelRequest = true;
                        else
                        {
                            mWindowLoadingBar.Value = mProgress;
                            loadingBarValue = mWindowLoadingBar.Value;
                        }
                    });

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
                mWindowLoadingBar?.Close();
                mWindowLoadingBar = null;
            });

            return result;
        }

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
                ItemData storedItem = storedItemDataEnumerable.First((ItemData item) =>
                {
                    //return TextManager.GetIDToString(item.ID) == TextManager.GetIDToString(foundItem.ID);
                    return Item.CompareIDs(item.ID, foundItem.ID);
                });

                if (storedItem == null)
                {
                    Console.WriteLine("Did not find the item \"" + foundItem.Name + "\" (ID: " + TextManager.GetIDToString(foundItem.ID) +
                        ") in storedItemData");
                    ++itemsTranslated;
                    ++mProgress;
                    continue;
                }

                //if (itemToStartFromIndex == 0)
                //    Console.WriteLine("Text before:" + textPart.Substring(0, 100));
                //int textPartLength = textPart.Length;
                //Console.WriteLine((foundItem.NameStartPosition - textPartStartPosition).ToString() + "(" + textPartLength + ")");

                int textPartLengthOld = textPart.Length;
                // Replace item name length
                string storedItemNameLength = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)storedItem.Name.Length });
                textPart = TextManager.GetReplacedPartString(textPart, storedItemNameLength,
                    foundItem.ItemStartPosition - textPartStartPosition,
                    foundItem.ItemStartPosition - textPartStartPosition + 1);

                // Replace item name
                int nameLengthDifference = foundItem.Name.Length - storedItem.Name.Length;
                textPart = TextManager.GetReplacedPartString(textPart,
                    TextManager.GetNullStringOfLength(nameLengthDifference) +
                        storedItem.NameReversed,
                    foundItem.NameStartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(nameLengthDifference),
                    foundItem.NameEndPosition - textPartStartPosition);

                if (storedItem.Description != null && storedItem.Description != "")
                {
                    // Replace item description length
                    string storedItemDescriptionLength = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)storedItem.Description.Length });
                    textPart = TextManager.GetReplacedPartString(textPart, storedItemDescriptionLength,
                        foundItem.DescriptionLengthPosition - textPartStartPosition,
                        foundItem.DescriptionLengthPosition - textPartStartPosition + 1);

                    // Replace item description
                    int descriptionLengthDifference = foundItem.Description.Length - storedItem.Description.Length;
                    textPart = TextManager.GetReplacedPartString(textPart,
                        TextManager.GetNullStringOfLength(descriptionLengthDifference) +
                            storedItem.DescriptionReversed,
                        foundItem.DescriptionStartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(descriptionLengthDifference),
                        foundItem.DescriptionEndPosition - textPartStartPosition);
                }
                //else
                //    Console.WriteLine(storedItem.Name + "'s description empty");

                if (storedItem.Extra1 != null && storedItem.Extra1 != "")
                {
                    // Replace item extra1 length
                    string storedItemExtra1Length = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)storedItem.Extra1.Length });
                    textPart = TextManager.GetReplacedPartString(textPart, storedItemExtra1Length,
                        foundItem.Extra1LengthPosition - textPartStartPosition,
                        foundItem.Extra1LengthPosition - textPartStartPosition + 1);

                    // Replace item extra1
                    int extra1LengthDifference = foundItem.Extra1.Length - storedItem.Extra1.Length;
                    textPart = TextManager.GetReplacedPartString(textPart,
                        TextManager.GetNullStringOfLength(extra1LengthDifference) +
                            storedItem.Extra1Reversed,
                        foundItem.Extra1StartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(extra1LengthDifference),
                        foundItem.Extra1EndPosition - textPartStartPosition);
                }

                if (storedItem.Extra2 != null && storedItem.Extra2 != "")
                {
                    // Replace item extra2 length
                    string storedItemExtra2Length = System.Text.Encoding.GetEncoding(1252).GetString(new byte[] { (byte)storedItem.Extra2.Length });
                    textPart = TextManager.GetReplacedPartString(textPart, storedItemExtra2Length,
                        foundItem.Extra2LengthPosition - textPartStartPosition,
                        foundItem.Extra2LengthPosition - textPartStartPosition + 1);

                    // Replace item extra2
                    int extra2LengthDifference = foundItem.Extra2.Length - storedItem.Extra2.Length;
                    textPart = TextManager.GetReplacedPartString(textPart,
                        TextManager.GetNullStringOfLength(extra2LengthDifference) +
                            storedItem.Extra2Reversed,
                        foundItem.Extra2StartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(extra2LengthDifference),
                        foundItem.Extra2EndPosition - textPartStartPosition);
                }

                //if (itemToStartFromIndex == 0)
                //    Console.WriteLine("Text after:" + textPart.Substring(0, 100));
                if (textPartLengthOld != textPart.Length)
                    Console.WriteLine("ERROR: Text length before (" + textPartLengthOld.ToString() +
                        "not equal to the length after:" + textPart.Length.ToString());

                //if (i > 10)
                //    break;
                //++i;
                ++itemsTranslated;
                ++mProgress;
            }

            Console.WriteLine("Task items translated: " + itemsTranslated.ToString());

            return textPart;
        }
    }
}
