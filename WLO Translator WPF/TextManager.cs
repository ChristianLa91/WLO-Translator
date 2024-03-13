using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// Handles most of the text-manipulations in the program.
    /// </summary>
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

        public static string GetStringWithReplacedPart(string oldString, string replace, int startPosition, int endPosition)
        {
            //int textLength = oldString.Length;
            //Console.WriteLine("Text to replace old text with: " + replace);
            string substring = oldString.Substring(startPosition, endPosition - startPosition);
            return oldString.Remove(startPosition, endPosition - startPosition).Insert(startPosition, replace);
        }

        public static string GetSubstring(ref string text, int startIndex, int endIndex)
        {
            if (startIndex < 0)
                startIndex = 0;
            if (endIndex > text.Length - 1)
                endIndex = text.Length - 1;

            return text.Substring(startIndex, endIndex - startIndex);
        }

        public static string GetIDToString(int[] id)
        {
            return id[0].ToString("000") + id[1].ToString("000") + id[2].ToString("000");
        }

        public static string CleanStringFromNewLinesAndBadChars(string str)
        {
            char charReplace        = Constants.CHAR_TO_REPLACE_NULL_CHAR;
            char charReplaceOthers  = Constants.CHAR_TO_REPLACE_OTHER_CHAR;
            return str.Replace((char)10, charReplaceOthers).Replace((char)13, charReplaceOthers).Replace('\0', charReplace).
                Replace('', charReplaceOthers).Replace('	', charReplaceOthers).Replace('', charReplaceOthers);
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

        public static string GetFileTypeToString(FileType fileType)
        {
            string result;
            switch (fileType)
            {
                case FileType.ACTIONINFO:
                    result = "ActionInfo";
                    break;
                case FileType.ITEM:
                    result = "Item";
                    break;
                case FileType.MARK:
                    result = "Mark";
                    break;
                case FileType.NPC:
                    result = "Npc";
                    break;
                case FileType.SCENEDATA:
                    result = "SceneData";
                    break;
                case FileType.SKILL:
                    result = "Skill";
                    break;
                case FileType.TALK:
                    result = "Talk";
                    break;
                default:
                    throw new ArgumentException("Name to return for file name \"" + fileType.ToString() + "\" is not implemented");
            }

            return result;
        }

        public static int GetNullsLeftOfIndex(ref string text, int startIndex, int stopAtIndex = -1)
        {
            int nullAmount = 0;

            for (int i = startIndex - 1; i > stopAtIndex; i--)
            {
                if (text[i] == '\0')
                    ++nullAmount;
                else
                    break;
            }

            return nullAmount;
        }

        public static void MoveSubstring(ref string text, int startIndex, int endIndex, int steps)
        {
            //string  textIn1252      = Encoding.GetEncoding(1252).GetString(Encoding.GetEncoding(1252).GetBytes(text));
            int     substringLength = endIndex - startIndex;
            string  substring       = text.Substring(startIndex, substringLength);
            text.Remove(startIndex, substringLength);
            text.Insert(startIndex + steps, substring);

            //System.Windows.MessageBox.Show(CleanStringFromNewLinesAndBadChars(GetSubstring(ref text, startIndex - 1000, endIndex + 1000)));
        }

        public static string GetRichTextBoxText(ref RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        }

        public static void SetRichTextBoxText(ref RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
    }
}
