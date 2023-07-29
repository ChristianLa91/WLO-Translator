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
            char charReplace = '.';
            return str.Replace((char)10, charReplace).Replace((char)13, charReplace).Replace('\0', charReplace).Replace('', charReplace).
                Replace('	', charReplace).Replace('', charReplace);
        }

        public static bool IsBytesEqualToString(byte[] bytes, int startIndex, int length, string text)
        {
            for (int i = 0; i < length; ++i)
            {
                if ((char)bytes[i + startIndex] != text[i])
                    return false;
            }

            return true;
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

        public static bool IsStringContainingIllegalChar(string text)
        {
            if (text == null || text.Length == 0)
                return false;

            foreach (char currentChar in text)
            {
                if (currentChar < 32 || (currentChar > 127 && currentChar < 160))
                    return true;
            }

            return false;
        }

        public static bool IsStringContainingNonConventionalChar(string text)
        {
            if (text == null || text.Length == 0)
                return false;

            foreach (char currentChar in text)
            {
                if (!char.IsLetterOrDigit(currentChar) && !char.IsSeparator(currentChar) &&
                    !char.IsPunctuation(currentChar) && !char.IsSymbol(currentChar) && !(currentChar == ','))
                    return true;
            }

            return false;
        }

        public static bool IsStringEndingWithNonNumberOrLetterOrParenthesesChar(string text)
        {
            if (text == null || text.Length == 0)
                return false;

            char lastChar = text[text.Length - 1];
            if (IsStringContainingNonConventionalChar(lastChar.ToString())/*!char.IsLetterOrDigit(lastChar) && lastChar != '(' && lastChar != ')' && lastChar != '´'*/)
                return true;

            return false;
        }

        public static bool IsStringContainingOnlyQuestionMarks(string text)
        {
            if (text == null || text.Length == 0)
                return false;

            foreach (char currentChar in text)
            {
                if (currentChar != '?')
                    return false;
            }

            return true;
        }
    }
}
