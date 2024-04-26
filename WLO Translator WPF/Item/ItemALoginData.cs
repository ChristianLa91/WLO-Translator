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

        public string   Text1               { get; set; }
        public string   Text2               { get; set; }
        public int      ItemStartPosition   { get; set; }
        public int      ItemEndPosition     { get; set; }
        public int      Text1StartPosition  { get; set; }
        public int      Text1EndPosition    { get; set; }
        public int      Text2StartPosition  { get; set; }
        public int      Text2EndPosition    { get; set; }
    }
}
