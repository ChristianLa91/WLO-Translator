using System;
using System.Text;

namespace WLO_Translator_WPF
{
    public static class TextManager
    {
        public static string GetStringWithEncoding(string text, Encoding encoding)
        {
            // Get text to new encoding from original encoding
            return encoding.GetString(Encoding.GetEncoding(1252).GetBytes(text));
        }

        public static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string GetReplacedPartString(string oldString, string replace, int startPosition, int endPosition)
        {
            //int textLength = oldString.Length;
            return oldString.Remove(startPosition, endPosition - startPosition).Insert(startPosition, replace);
        }

        public static string GetIDToString(int[] id)
        {
            return (id[0] + id[1]).ToString();
        }

        public static string CleanStringFromNewLinesAndBadChars(string str)
        {
            char charReplace = '*';
            return str.Replace((char)10, charReplace).Replace((char)13, charReplace).Replace('\0', charReplace).Replace('', charReplace).
                Replace('	', charReplace).Replace('', charReplace);
        }

        public static string GetNullStringOfLength(int length)
        {
            if (length < 0)
                Console.WriteLine("GetNullStringOfLength() reporting negative length" + length.ToString());

            string nullString = "";
            for (int i = 0; i < length; ++i)
            {
                nullString += '\0';
            }

            return nullString;
        }

        public static int GetNullsToDecreaseAmount(int length)
        {
            if (length < 0)
                return Math.Abs(length);
            else
                return 0;
        }
    }
}
