using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    public class ItemData
    {
        public string   Name                        { get; set; }
        public string   NameReversed                { get; set; }
        public int[]    ID                          { get; set; }
        public string   Description                 { get; set; }
        public string   DescriptionReversed         { get; set; }

        // Positions
        public int      ItemStartPosition           { get; set; }
        public int      ItemEndPosition             { get; set; }
        public int      IDStartPosition             { get; set; }
        public int      IDEndPosition               { get; set; }
        public int      NameStartPosition           { get; set; }
        public int      NameEndPosition             { get; set; }
        public int      DescriptionLengthPosition   { get; set; }
        public int      DescriptionStartPosition    { get; set; }
        public int      DescriptionEndPosition      { get; set; }

        // TextBox Texts
        public string   TextBoxNameLengthText       { get; set; }

        public Item ToItem(RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
                           RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
                           RoutedEventHandler routedEventHandlerButtonClickJumpToDescription)
        {
            Item item = new Item(routedEventHandlerButtonClickUpdateItem, routedEventHandlerButtonClickJumpToWholeItem,
                            routedEventHandlerButtonClickJumpToID, routedEventHandlerButtonClickJumpToName,
                            routedEventHandlerButtonClickJumpToDescription);
            item.Name                       = Name;
            item.ID                         = ID;
            item.Description                = Description;
            item.ItemStartPosition          = ItemStartPosition;
            item.ItemEndPosition            = ItemEndPosition;
            item.IDStartPosition            = IDStartPosition;
            item.IDEndPosition              = IDEndPosition;
            item.NameStartPosition          = NameStartPosition;
            item.NameEndPosition            = NameEndPosition;
            item.DescriptionLengthPosition  = DescriptionLengthPosition;
            item.DescriptionStartPosition   = DescriptionStartPosition;
            item.DescriptionEndPosition     = DescriptionEndPosition;
            item.TextBoxNameLength.Text     = TextBoxNameLengthText;

            return item;
        }
    }

    public class Item : ListViewItem
    {
        private string              mNameWithOriginalEncoding;
        private string              mName;
        private int[]               mID;
        private string              mDescriptionWithOriginalEncoding;
        private string              mDescription;

        private TextBlock           mTextBlockNameLength;
        private TextBlock           mTextBlockID;
        private TextBox             mTextBoxName;
        private TextBox             mTextBoxDescription;
        private Button              mButtonUpdateItemBasedOnNameLength;
        private Button              mButtonJumpToWholeItem;
        private Button              mButtonJumpToID;
        private Button              mButtonJumpToName;
        private Button              mButtonJumpToDescription;

        private RoutedEventHandler  mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToDescription;

        private bool                mSkipTextChangedHandling = false;

        public new string Name
        {
            get => mName;
            set
            {
                mName               = value;
                mTextBoxName.Text   = mName;//RichTextBoxSetText(mName, ref mTextBoxName);

                CheckIllegalChars(ref mTextBoxName);
            }
        }        

        public int[] ID
        {
            get => mID;
            set
            {
                mID = value;
                mTextBlockID.Text = (mID[0] + mID[1]).ToString();
            }
        }

        public string Description
        {
            get => mDescription;
            set
            {
                mDescription = value;
                mTextBoxDescription.Text = mDescription;//RichTextBoxSetText(mDescription, ref mTextBoxDescription);

                CheckIllegalChars(ref mTextBoxDescription);
            }
        }

        private bool ContainsIllegalChar(string mName)
        {
            foreach (char currentChar in mName)
            {
                if (currentChar < 32 || (currentChar > 127 && currentChar < 160))
                    return true;
            }

            return false;
        }

        private bool ContainsNoneNumberOrLetterChar(string mName)
        {
            foreach (char currentChar in mName)
            {
                if (!char.IsLetterOrDigit(currentChar) && !char.IsSeparator(currentChar) &&
                    !char.IsPunctuation(currentChar) && !char.IsSymbol(currentChar) && !(currentChar == ','))
                    return true;
            }

            return false;
        }

        private void CheckIllegalChars(ref TextBox textBox)
        {
            string text = textBox.Text;//RichTextBoxGetText(ref textBox);
            if (ContainsIllegalChar(text))
                textBox.Background = Brushes.Red;
            else if (ContainsNoneNumberOrLetterChar(text))
                textBox.Background = Brushes.Orange;
            else
                textBox.Background = Brushes.Transparent;
        }

        public bool TextBoxesContainsIllegalChars()
        {
            return ContainsIllegalChar(mTextBoxName.Text) || ContainsIllegalChar(mTextBoxDescription.Text);
        }

        // Positions
        public int          ItemStartPosition           { get; set; }
        public int          ItemEndPosition             { get; set; }
        public int          IDStartPosition             { get; set; }
        public int          IDEndPosition               { get; set; }
        public int          NameStartPosition           { get; set; }
        public int          NameEndPosition             { get; set; }
        public int          DescriptionLengthPosition   { get; set; }
        public int          DescriptionStartPosition    { get; set; }
        public int          DescriptionEndPosition      { get; set; }

        // TextBoxes and TextBlocks
        public TextBlock    TextBoxNameLength           { get => mTextBlockNameLength;      set => mTextBlockNameLength     = value; }
        public TextBlock    TextBlockID                 { get => mTextBlockID;              set => mTextBlockID             = value; }
        public TextBox      TextBoxName                 { get => mTextBoxName;              set => mTextBoxName             = value; }
        public TextBox      TextBoxDescription          { get => mTextBoxDescription;       set => mTextBoxDescription      = value; }

        // Buttons
        public Button       ButtonJumpToWholeItem       { get => mButtonJumpToWholeItem;    set => mButtonJumpToWholeItem   = value; }
        public Button       ButtonJumpToID              { get => mButtonJumpToID;           set => mButtonJumpToID          = value; }
        public Button       ButtonJumpToName            { get => mButtonJumpToName;         set => mButtonJumpToName        = value; }
        public Button       ButtonJumpToDescription     { get => mButtonJumpToDescription;  set => mButtonJumpToDescription = value; }

        public Item(RoutedEventHandler routedEventHandlerButtonClickUpdateItem, RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription) : base()
        {
            //mID                             = new int[2];
            mTextBlockNameLength                = new TextBlock()   { Text      = "??" };
            mTextBlockID                        = new TextBlock()   { Text      = "??" };
            mTextBoxName                        = new TextBox()     { MaxWidth  = 200, Width = 200 };
            mTextBoxName.TextChanged           += TextBoxName_TextChanged;
            mTextBoxDescription                 = new TextBox()     { MaxWidth  = 200, Width = 200 };
            mTextBoxDescription.TextChanged    += TextBoxDescription_TextChanged;

            mButtonUpdateItemBasedOnNameLength  = new Button()      { Content   = "Update", Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToWholeItem              = new Button()      { Content   = "Goto Item", Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToID                     = new Button()      { Content   = "Goto" };
            mButtonJumpToName                   = new Button()      { Content   = "Goto" };
            mButtonJumpToDescription            = new Button()      { Content   = "Goto", Margin = new Thickness(10, 0, 0, 0) };

            mButtonUpdateItemBasedOnNameLength.Click += routedEventHandlerButtonClickUpdateItem;
            mButtonJumpToWholeItem.Click       += routedEventHandlerButtonClickJumpToWholeItem;
            mButtonJumpToID.Click              += routedEventHandlerButtonClickJumpToID;
            mButtonJumpToName.Click            += routedEventHandlerButtonClickJumpToName;
            mButtonJumpToDescription.Click     += routedEventHandlerButtonClickJumpToDescription;

            mButtonUpdateItemBasedOnNameLength.Tag  = this;
            mButtonJumpToWholeItem.Tag              = this;
            mButtonJumpToID.Tag                     = this;
            mButtonJumpToName.Tag                   = this;
            mButtonJumpToDescription.Tag            = this;

            mRoutedEventHandlerButtonClickUpdateItem        = routedEventHandlerButtonClickUpdateItem;
            mRoutedEventHandlerButtonClickJumpToWholeItem   = routedEventHandlerButtonClickJumpToWholeItem;
            mRoutedEventHandlerButtonClickJumpToID          = routedEventHandlerButtonClickJumpToID;
            mRoutedEventHandlerButtonClickJumpToName        = routedEventHandlerButtonClickJumpToName;
            mRoutedEventHandlerButtonClickJumpToDescription = routedEventHandlerButtonClickJumpToDescription;


            Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    mButtonUpdateItemBasedOnNameLength,

                    mButtonJumpToWholeItem,

                    new Label() { Content = "Length: " },
                    mTextBlockNameLength,

                    mButtonJumpToID,
                    new Label() { Content = "ID: " },
                    mTextBlockID,

                    mButtonJumpToName,
                    new Label() { Content = "Name: " },
                    mTextBoxName,

                    mButtonJumpToDescription,
                    new Label() { Content = "Description: " },
                    mTextBoxDescription
                }                
            };
        }        

        public void SetEncoding(FontFamily fontFamily, Encoding encoding)
        {
            // Save strings with original encodings
            if (mNameWithOriginalEncoding == null)
                mNameWithOriginalEncoding = Name;
            if (mDescriptionWithOriginalEncoding == null)
                mDescriptionWithOriginalEncoding = Description;

            TextBoxName.FontFamily          = fontFamily;
            TextBoxDescription.FontFamily   = fontFamily;

            if (mNameWithOriginalEncoding != "")
                Name = TextManager.GetStringWithEncoding(mNameWithOriginalEncoding, encoding);
            if (mDescriptionWithOriginalEncoding != "")
                Description = TextManager.GetStringWithEncoding(mDescriptionWithOriginalEncoding, encoding);
        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (mSkipTextChangedHandling)
            //    return;

            TextBox textBox = (TextBox)sender;
            Name = textBox.Text; //RichTextBoxGetText(ref richTextBox);
            //CheckIllegalChars(ref mTextBoxName);
            mTextBlockNameLength.Text = mName.Length.ToString();
        }

        private void TextBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (mSkipTextChangedHandling)
            //    return;

            TextBox textBox = (TextBox)sender;
            Description = textBox.Text;//RichTextBoxGetText(ref textBox);
            //CheckIllegalChars(ref mTextBoxDescription);
        }

        private string RichTextBoxGetText(ref RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        }

        private void RichTextBoxSetText(string text, ref RichTextBox richTextBox)
        {
            mSkipTextChangedHandling = true;
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            mSkipTextChangedHandling = false;
            CheckIllegalChars(ref mTextBoxDescription);
        }

        public ItemData ToItemData()
        {
            ItemData itemData                   = new ItemData();
            itemData.Name                       = Name;
            itemData.NameReversed               = TextManager.ReverseString(Name);
            itemData.ID                         = ID;
            itemData.Description                = Description;
            itemData.DescriptionReversed        = TextManager.ReverseString(Description);
            itemData.ItemStartPosition          = ItemStartPosition;
            itemData.ItemEndPosition            = ItemEndPosition;
            itemData.IDStartPosition            = IDStartPosition;
            itemData.IDEndPosition              = IDEndPosition;
            itemData.NameStartPosition          = NameStartPosition;
            itemData.NameEndPosition            = NameEndPosition;
            itemData.DescriptionLengthPosition  = DescriptionLengthPosition;
            itemData.DescriptionStartPosition   = DescriptionStartPosition;
            itemData.DescriptionEndPosition     = DescriptionEndPosition;
            itemData.TextBoxNameLengthText      = TextBoxNameLength.Text;

            return itemData;
        }

        // Create a copy of the item
        public Item Clone()
        {
            Item itemClone = new Item(mRoutedEventHandlerButtonClickUpdateItem, mRoutedEventHandlerButtonClickJumpToWholeItem,
                mRoutedEventHandlerButtonClickJumpToID, mRoutedEventHandlerButtonClickJumpToName, mRoutedEventHandlerButtonClickJumpToDescription);

            // Ints and strings
            itemClone.ID                        = mID;
            itemClone.Name                      = mName;
            itemClone.Description               = mDescription;

            // TextBoxes and TextBlocks
            itemClone.TextBoxNameLength         = TextBoxNameLength;
            itemClone.TextBlockID               = mTextBlockID;
            itemClone.TextBoxName               = mTextBoxName;
            itemClone.TextBoxDescription        = mTextBoxDescription;

            // Buttons
            itemClone.ButtonJumpToName          = mButtonJumpToWholeItem;
            itemClone.ButtonJumpToName          = mButtonJumpToID;
            itemClone.ButtonJumpToName          = mButtonJumpToName;
            itemClone.ButtonJumpToDescription   = mButtonJumpToDescription;

            // Positions
            itemClone.ItemStartPosition         = ItemStartPosition;
            itemClone.ItemEndPosition           = ItemEndPosition;
            itemClone.IDStartPosition           = IDStartPosition;
            itemClone.IDEndPosition             = IDEndPosition;
            itemClone.NameStartPosition         = NameStartPosition;
            itemClone.NameEndPosition           = NameEndPosition;
            itemClone.DescriptionLengthPosition = DescriptionLengthPosition;
            itemClone.DescriptionStartPosition  = DescriptionStartPosition;
            itemClone.DescriptionEndPosition    = DescriptionEndPosition;
            
            return itemClone;
        }
    }
}
