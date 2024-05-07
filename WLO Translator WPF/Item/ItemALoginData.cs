using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WLO_Translator_WPF
{
    public class ItemALoginData
    {
        public ItemALoginData()
        {
        }

        public string   TextOriginalOriginalEncoding    { get; set; }
        public string   TextReplaceOriginalEncoding     { get; set; }

        public string   TextOriginal                    { get; set; }
        public string   TextReplace                     { get; set; }
        public int      ItemStartPosition               { get; set; }
        public int      TextOriginalLength              { get; set; }
        public int      TextReplaceLength               { get; set; }
    }
}
