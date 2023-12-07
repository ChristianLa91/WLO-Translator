using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace WLO_Translator_WPF
{
    enum HistoryActionTypes
    {
        NULL,
        STORED_ALL_ITEMS,
        DELETED_ITEM
    }

    class HistoryAction
    {
        public HistoryAction(HistoryActionTypes typeOfAction)
        {
            TypeOfAction    = typeOfAction;
        }

        public HistoryAction(HistoryActionTypes typeOfAction,
                             List<Item> listBoxStoredItems)
        {
            TypeOfAction    = typeOfAction;
            ListStoredItems = listBoxStoredItems;
        }

        public HistoryAction(HistoryActionTypes typeOfAction,
                             string text)
        {
            TypeOfAction    = typeOfAction;
            Text            = text;
        }

        public HistoryActionTypes   TypeOfAction    { get; private set; } = HistoryActionTypes.NULL;
        public List<Item>           ListStoredItems { get; private set; } = null;

        public string               Text            { get; set;         } = null;
    }

    public static class HistoryManager
    {
        public  static ListBox              ListBoxStoredItems  { get; private set; }

        private static List<HistoryAction>  HistoryActions      { get; set;         } = new List<HistoryAction>();

        private static bool                 mHasBeenInitialized = false;

        public static void Initialize(ref ListView listBoxStoredItems)
        {
            ListBoxStoredItems  = listBoxStoredItems;

            mHasBeenInitialized = true;
        }

        public static void Undo()
        {
            switch (HistoryActions.Last().TypeOfAction)
            {
                case HistoryActionTypes.STORED_ALL_ITEMS:
                    ListBoxStoredItems.Items.Clear();
                    //ListBoxStoredItems.Items = HistoryActions.Last().ListStoredItems;
                    break;
            }
        }

        public static void Redo()
        {

        }
    }
}
