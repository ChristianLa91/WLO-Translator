using ScintillaNET.WPF;
using System;

namespace WLO_Translator_WPF
{
    public static class FileContentSearch
    {
        private static string   mSearchStringOld;
        private static int      mTargetEndOld;

        enum SearchOption
        {
            SEARCH_FIRST,
            SEARCH_NEXT,
            SEARCH_PREVIOUS
        }

        public static void SearchForString(string searchString, ref ScintillaWPF scintillaTextBox)
        {
            mTargetEndOld       = default;

            SelectAndScrollToFirstMatch(searchString, ref scintillaTextBox);

            mSearchStringOld    = searchString;
            mTargetEndOld       = scintillaTextBox.TargetEnd;
        }

        public static void SearchNext(ref ScintillaWPF scintillaTextBox)
        {
            SelectAndScrollToFirstMatch(mSearchStringOld, ref scintillaTextBox, SearchOption.SEARCH_NEXT);

            mTargetEndOld       = scintillaTextBox.TargetEnd;
        }

        public static void SearchPrevious(ref ScintillaWPF scintillaTextBox)
        {
            SelectAndScrollToFirstMatch(mSearchStringOld, ref scintillaTextBox, SearchOption.SEARCH_PREVIOUS);
        }

        private static void SelectAndScrollToFirstMatch(string searchString, ref ScintillaWPF scintillaTextBox, SearchOption searchOption = SearchOption.SEARCH_FIRST)
        {
            if (searchString == null || searchString == "") return;

            scintillaTextBox.TargetWholeDocument();
            switch (searchOption)
            {
                case SearchOption.SEARCH_NEXT:
                    if (mTargetEndOld == default) return;
                    scintillaTextBox.TargetStart    = mTargetEndOld;
                    break;
                case SearchOption.SEARCH_PREVIOUS:
                    if (mTargetEndOld == default) return;
                    scintillaTextBox.TargetEnd      = scintillaTextBox.TargetStart;
                    scintillaTextBox.TargetStart    = mTargetEndOld;
                    break;
            }

            if (scintillaTextBox.SearchInTarget(searchString) != -1)
            {
                scintillaTextBox.SetSelectionBackColor(true, System.Windows.Media.Colors.Red);
                scintillaTextBox.SelectionStart     = scintillaTextBox.TargetStart;
                scintillaTextBox.SelectionEnd       = scintillaTextBox.TargetEnd;
                scintillaTextBox.ScrollCaret();
            }
        }        
    }
}
