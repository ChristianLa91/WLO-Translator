using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WLO_Translator_WPF
{
    public class ListBoxItems
    {
        //ObservableCollection<Item> mListBox;
        ListBox                     mListBox;
        IList<Item>                 mListBoxItems;

        private RoutedEventHandler  mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToDescription;

        public ListBoxItems(ref ListBox listBox, IList<Item> listBoxItems,
            RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription)
        {
            mListBox = listBox;
            mListBoxItems = listBoxItems;

            mRoutedEventHandlerButtonClickUpdateItem        = routedEventHandlerButtonClickUpdateItem;
            mRoutedEventHandlerButtonClickJumpToWholeItem   = routedEventHandlerButtonClickJumpToWholeItem;
            mRoutedEventHandlerButtonClickJumpToID          = routedEventHandlerButtonClickJumpToID;
            mRoutedEventHandlerButtonClickJumpToName        = routedEventHandlerButtonClickJumpToName;
            mRoutedEventHandlerButtonClickJumpToDescription = routedEventHandlerButtonClickJumpToDescription;
        }

        public void Add(Item item)
        {
            ((ObservableCollection<Item>)mListBox.ItemsSource).Add(item);
        }

        //public void Add(List<ItemData> items)
        //{
        //    // Get the CollectionView associated with the ListBox
        //    //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(mListBoxItems);
        //    //ObservableCollection<Item> itemSource = mListBoxItems;

        //    List<Item> itemList = new List<Item>();
        //    //using (view.DeferRefresh())
        //    //{
        //        foreach (ItemData item in items)
        //        {
        //            //mListBox.ItemsSource.GetEnumerator().MoveNext();
        //            mListBoxItems.Add(item.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
        //                    mRoutedEventHandlerButtonClickJumpToWholeItem,
        //                    mRoutedEventHandlerButtonClickJumpToID,
        //                    mRoutedEventHandlerButtonClickJumpToName,
        //                    mRoutedEventHandlerButtonClickJumpToDescription));
        //        }

        //        //mListBox.ItemsSource = new ObservableCollection<Item>(itemList);
        //    //}
        //    //// Suspend updates while adding a bunch of items
        //    //using (view.DeferRefresh())
        //    //{
        //    //    foreach (ItemData item in items)
        //    //    {
        //    //        itemSource.Add(item.ToItem(mRoutedEventHandlerButtonClickUpdateItem,
        //    //            mRoutedEventHandlerButtonClickJumpToWholeItem,
        //    //            mRoutedEventHandlerButtonClickJumpToID,
        //    //            mRoutedEventHandlerButtonClickJumpToName,
        //    //            mRoutedEventHandlerButtonClickJumpToDescription));
        //    //    }
        //    //}
        //}

        public void Clear()
        {
            mListBoxItems.Clear();
        }

        public void Remove(Item item)
        {
            ((ObservableCollection<Item>)mListBox.ItemsSource).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((ObservableCollection<Item>)mListBox.ItemsSource).RemoveAt(index);
        }
    }
}
