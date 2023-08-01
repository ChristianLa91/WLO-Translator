using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WLO_Translator_WPF
{
    public class DataObject
    {
        public DataObject()
        {
            ListBoxFoundItems = new List<Item>();
            ListBoxStoredItems = new List<Item>();
            //ListBoxFoundItems = new ObservableCollection<Item>();
            //ListBoxStoredItems = new ObservableCollection<Item>();
        }

        public IList<Item>/*ObservableCollection<Item>*/ ListBoxFoundItems    { get; set; }
        public IList<Item>/*ObservableCollection<Item>*/ ListBoxStoredItems   { get; set; }
    }
}
