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
            //Console.WriteLine("Text to replace old text with: " + replace);
            return oldString.Remove(startPosition, endPosition - startPosition).Insert(startPosition, replace);
        }

        public static string GetIDToString(int[] id)
        {
            return id[0].ToString("000") + id[1].ToString("000") + id[2].ToString("000");
        }

        public static string CleanStringFromNewLinesAndBadChars(string str)
        {
            char charReplace        = Constants.CHAR_TO_REPLACE_NULL_CHAR;
            char charReplaceOthers  = Constants.CHAR_TO_REPLACE_OTHER_CHAR;
            return str.Replace((char)10, charReplaceOthers).Replace((char)13, charReplaceOthers).Replace('\0', charReplace).Replace('', charReplaceOthers).
                Replace('	', charReplaceOthers).Replace('', charReplaceOthers);
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
            //if (length < 0)
            //    Console.WriteLine("GetNullStringOfLength() reporting negative length" + length.ToString());

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
                if (!IsStandardLetterOrDigit(currentChar, false) && /*!char.IsNumber(currentChar) &&*/ !IsStandardSeparator(currentChar) &&
                    !char.IsPunctuation(currentChar) && !IsAcceptedSymbol(currentChar) && !(currentChar == ','))
                    return true;
                //else if (currentChar == '¤' || currentChar == '¥' || currentChar == '¦' || currentChar == '§')
                //    return true;
            }

            return false;
        }

        public static bool IsStandardLetterOrDigit(char c, bool includeExtendedASCIILetters = true)
        {
            string extendedCharsToInclude = "ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜø£Ø×ƒáíóúñÑÁÂÀãÃðÐÊËÈÍÎÏÌÓßÔÒõÕµþÞÚÛÙýÝ";
            bool isStandardASCIILetter = (c > 47 && c < 58)   // Digit
                || (c > 64 && c < 91)     // Upper case letter
                || (c > 96 && c < 123);    // Upper case letter

            if (includeExtendedASCIILetters)
                return isStandardASCIILetter || extendedCharsToInclude.Contains(c.ToString());
            else
                return isStandardASCIILetter;
        }

        public static bool IsStandardSeparator(char c/*, bool includeExtendedASCIILetters = true*/)
        {
            //string extendedCharsToInclude = "";
            string separators = "!\"'? ";

            //if (includeExtendedASCIILetters)
            //    return separators.Contains(c.ToString()) || extendedCharsToInclude.Contains(c.ToString());
            //else
                return separators.Contains(c.ToString());
        }

        public static bool IsAcceptedSymbol(char c)
        {
            string acceptedSymbols = "%+-~<>=";

            return acceptedSymbols.Contains(c.ToString());
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

        public static string GetFileNameWithFirstLettersCapitalized(FileName fileName)
        {
            string result;
            switch (fileName)
            {
                case FileName.ACTIONINFO:
                    result = "ActionInfo";
                    break;
                case FileName.ITEM:
                    result = "Item";
                    break;
                case FileName.MARK:
                    result = "Mark";
                    break;
                case FileName.NPC:
                    result = "Npc";
                    break;
                case FileName.SCENEDATA:
                    result = "SceneData";
                    break;
                case FileName.SKILL:
                    result = "Skill";
                    break;
                case FileName.TALK:
                    result = "Talk";
                    break;
                default:
                    throw new ArgumentException("Name to return for file name \"" + fileName.ToString() + "\" is not implemented");
            }

            return result;
        }
    }
}
