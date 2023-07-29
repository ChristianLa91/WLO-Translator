using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WLO_Translator_WPF
{
    static class TranslationManager
    {
        public static async Task<string> TranslateAsync(string text, List<ItemData> foundItemData, List<ItemData> storedItemData)
        {
            // Calculate how many tasks and items per task that it should be
            int taskAmount = 0, maxTasks = 8;
            if (foundItemData.Count() < 10)
                taskAmount = foundItemData.Count();
            else if (foundItemData.Count() < maxTasks)
                taskAmount = foundItemData.Count() / 2;
            else
                taskAmount = maxTasks;

            int         itemsPerTask            = foundItemData.Count() / taskAmount, currentItemsPerTask = itemsPerTask;
            int         itemsToTranslate        = itemsPerTask;
            ItemData    taskItemLast            = null;
            List<Task>  tasks                   = new List<Task>();
            string[]    substrings              = new string[taskAmount];
            int[]       textPartStartPositions  = new int[taskAmount];
            int[]       itemToStartFromIndice   = new int[taskAmount];
            int[]       itemsToTranslateList    = new int[taskAmount];
            Stack<int>  indice                  = new Stack<int>();

            // Start tasks
            for (int i = 0; i < taskAmount; i++)
            {
                // Get the last task item to translate for this task
                if (i == taskAmount - 1)
                {
                    taskItemLast        = foundItemData.Last();
                    itemsToTranslate    = foundItemData.Count() - i * itemsPerTask;
                }
                else
                    taskItemLast        = foundItemData[i * itemsPerTask + itemsPerTask];

                int itemStartPosition   = foundItemData[i * itemsPerTask].ItemStartPosition;
                if (i == 0)
                    itemStartPosition   = 0;
                int itemEndPosition     = taskItemLast.ItemEndPosition;

                // Set values to the correct index in the arrays
                substrings[i]               = text.Substring(itemStartPosition, itemEndPosition - itemStartPosition);
                textPartStartPositions[i]   = itemStartPosition;
                itemToStartFromIndice[i]    = i * itemsPerTask;
                itemsToTranslateList[i]     = itemsToTranslate;
                indice.Push(i);
            }

            // Run tasks
            Semaphore semaphore = new Semaphore(1, 1);
            foreach (string substring in substrings)
            {
                semaphore.WaitOne();
                int index = indice.Pop();
                tasks.Add(Task.Run(() => TaskTranslatePartOfString
                (
                    foundItemData, storedItemData, substrings[index],
                    textPartStartPositions[index], itemToStartFromIndice[index], itemsToTranslateList[index], index
                )));
                semaphore.Release();
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

            return result;
        }

        public static string TaskTranslatePartOfString(List<ItemData> foundItemData, List<ItemData> storedItemData, string textPart,
            int textPartStartPosition, int itemToStartFromIndex, int itemsToTranslate, int index)
        {
            for (int i = itemToStartFromIndex; i < itemToStartFromIndex + itemsToTranslate; i++)
            {
                ItemData foundItem = foundItemData.ElementAt(i);
                ItemData storedItem = storedItemData.AsEnumerable().Where((ItemData item) =>
                {
                    return TextManager.GetIDToString(item.ID) == TextManager.GetIDToString(foundItem.ID);
                }).FirstOrDefault();

                if (storedItem == null)
                {
                    Console.WriteLine("Did not find the item" + foundItem.ID + " in storedItemData");
                    continue;
                }

                //if (itemToStartFromIndex == 0)
                //    Console.WriteLine("Text before:" + textPart.Substring(0, 100));
                //int textPartLength = textPart.Length;
                //Console.WriteLine((foundItem.NameStartPosition - textPartStartPosition).ToString() + "(" + textPartLength + ")");

                // Replace item name length
                textPart = TextManager.GetReplacedPartString(textPart, ((char)storedItem.Name.Length).ToString(),
                    foundItem.ItemStartPosition - textPartStartPosition,
                    foundItem.ItemStartPosition - textPartStartPosition + 1);

                // Replace item name
                int nameLengthDifference = foundItem.Name.Length - storedItem.Name.Length;
                textPart = TextManager.GetReplacedPartString(textPart,
                    TextManager.GetNullStringOfLength(nameLengthDifference) +
                        storedItem.NameReversed,
                    foundItem.NameStartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(nameLengthDifference),
                    foundItem.NameEndPosition - textPartStartPosition);

                // Replace item description length
                textPart = TextManager.GetReplacedPartString(textPart, ((char)storedItem.Description.Length).ToString(),
                    foundItem.DescriptionLengthPosition - textPartStartPosition,
                    foundItem.DescriptionLengthPosition - textPartStartPosition + 1);

                // Replace item description
                int descriptionLengthDifference = foundItem.Description.Length - storedItem.Description.Length;
                textPart = TextManager.GetReplacedPartString(textPart,
                    TextManager.GetNullStringOfLength(descriptionLengthDifference) +
                        storedItem.DescriptionReversed,
                    foundItem.DescriptionStartPosition - textPartStartPosition - TextManager.GetNullsToDecreaseAmount(descriptionLengthDifference),
                    foundItem.DescriptionEndPosition - textPartStartPosition);

                //if (itemToStartFromIndex == 0)
                //    Console.WriteLine("Text after:" + textPart.Substring(0, 100));

                //if (i > 10)
                //    break;
                //++i;
            }

            return textPart;
        }
    }
}
