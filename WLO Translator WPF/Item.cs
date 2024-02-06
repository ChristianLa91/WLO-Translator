using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    public interface IItemBase
    {
        string  Name                        { get; set; }
        int[]   ID                          { get; set; }
        string  Description                 { get; set; }
        string  Extra1                      { get; set; }
        string  Extra2                      { get; set; }
        int     ItemStartPosition           { get; set; }
        int     ItemEndPosition             { get; set; }
        int     IDStartPosition             { get; set; }
        int     IDEndPosition               { get; set; }
        int     NameStartPosition           { get; set; }
        int     NameEndPosition             { get; set; }
        int     DescriptionStartPosition    { get; set; }
        int     DescriptionEndPosition      { get; set; }
        int     Extra1StartPosition         { get; set; }
        int     Extra1EndPosition           { get; set; }
        int     Extra2StartPosition         { get; set; }
        int     Extra2EndPosition           { get; set; }
        int     GetIDValue();
    }

    public class ItemData : IItemBase
    {
        private string mName;
        private string mNameReversed;
        private string mDescription;
        private string mDescriptionReversed;
        private string mExtra1;
        private string mExtra1Reversed;
        private string mExtra2;
        private string mExtra2Reversed;
        public  string Name         { get { return mName; } set { mName = value; mNameReversed = TextManager.ReverseString(value); } }
        public  string NameReversed { get { return mNameReversed; } set { mNameReversed = value; mName = TextManager.ReverseString(value); } }
        public  int[]  ID           { get; set; }
        public  string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                if (mDescription != null)
                    mDescriptionReversed = TextManager.ReverseString(value);
            }
        }
        public string   DescriptionReversed
        {
            get { return mDescriptionReversed; }
            set
            {
                mDescriptionReversed = value;
                if (mDescriptionReversed != null)
                    mDescription = TextManager.ReverseString(value);
            }
        }

        public string Extra1
        {
            get { return mExtra1; }
            set
            {
                mExtra1 = value;
                if (mExtra1 != null)
                    mExtra1Reversed = TextManager.ReverseString(value);
            }
        }

        public string Extra1Reversed
        {
            get { return mExtra1Reversed; }
            set
            {
                mExtra1Reversed = value;
                if (mExtra1Reversed != null)
                    mExtra1 = TextManager.ReverseString(value);
            }
        }

        public string Extra2
        {
            get { return mExtra2; }
            set
            {
                mExtra2 = value;
                if (mExtra2 != null)
                    mExtra2Reversed = TextManager.ReverseString(value);
            }
        }

        public string Extra2Reversed
        {
            get { return mExtra2Reversed; }
            set
            {
                mExtra2Reversed = value;
                if (mExtra2Reversed != null)
                    mExtra2 = TextManager.ReverseString(value);
            }
        }

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
        public int      Extra1LengthPosition        { get; set; }
        public int      Extra1StartPosition         { get; set; }
        public int      Extra1EndPosition           { get; set; }
        public int      Extra2LengthPosition        { get; set; }
        public int      Extra2StartPosition         { get; set; }
        public int      Extra2EndPosition           { get; set; }

        // TextBox Texts
        //public string   TextBoxNameLengthText       { get; set; }

        public Item ToItem(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem, RoutedEventHandler routedEventHandlerButtonClickJumpToID,
            RoutedEventHandler routedEventHandlerButtonClickJumpToName, RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool hasCheckBox = false, RoutedEventHandler routedEventHandlerCheckBoxClick = null)
        {
            Item item = new Item(routedEventHandlerButtonClickUpdateItem, routedEventHandlerButtonClickJumpToWholeItem,
                            routedEventHandlerButtonClickJumpToID, routedEventHandlerButtonClickJumpToName,
                            routedEventHandlerButtonClickJumpToDescription,
                            routedEventHandlerButtonClickJumpToExtra1, routedEventHandlerButtonClickJumpToExtra2,
                            hasDescription, hasExtras, hasCheckBox, routedEventHandlerCheckBoxClick);
            item.Name                       = Name;
            item.ID                         = ID;
            if (mDescription != null)
                item.Description            = Description;
            if (mExtra1 != null)
                item.Extra1                 = Extra1;
            if (mExtra2 != null)
                item.Extra2                 = Extra2;

            item.ItemStartPosition          = ItemStartPosition;
            item.ItemEndPosition            = ItemEndPosition;
            item.IDStartPosition            = IDStartPosition;
            item.IDEndPosition              = IDEndPosition;
            item.NameStartPosition          = NameStartPosition;
            item.NameEndPosition            = NameEndPosition;
            item.DescriptionLengthPosition  = DescriptionLengthPosition;
            item.DescriptionStartPosition   = DescriptionStartPosition;
            item.DescriptionEndPosition     = DescriptionEndPosition;

            item.Extra1LengthPosition       = Extra1LengthPosition;
            item.Extra1StartPosition        = Extra1StartPosition;
            item.Extra1EndPosition          = Extra1EndPosition;
            item.Extra2LengthPosition       = Extra2LengthPosition;
            item.Extra2StartPosition        = Extra2StartPosition;
            item.Extra2EndPosition          = Extra2EndPosition;

            return item;
        }

        public int GetIDValue()
        {
            return ID[0] * 10000 + ID[1] * 100 + ID[2];
        }
    }

    public class Item : ListBoxItem, IItemBase
    {
        private string              mNameWithOriginalEncoding;
        private string              mName;
        private int[]               mID;
        private string              mDescriptionWithOriginalEncoding;
        private string              mDescription;
        // Used for the Mark.dat file
        private string              mExtra1WithOriginalEncoding;
        private string              mExtra1;
        private string              mExtra2WithOriginalEncoding;
        private string              mExtra2;

        private TextBlock           mTextBlockNameLength;
        private TextBlock           mTextBlockID;
        private TextBox             mTextBoxName;
        private TextBox             mTextBoxDescription;
        // Used for the Mark.dat file
        private TextBox             mTextBoxExtra1;
        private TextBox             mTextBoxExtra2;

        private Button              mButtonUpdateItemBasedOnNameLength;
        private Button              mButtonJumpToWholeItem;
        private Button              mButtonJumpToID;
        private Button              mButtonJumpToName;
        private Button              mButtonJumpToDescription;
        // Used for the Mark.dat file
        private Button              mButtonJumpToExtra1;
        private Button              mButtonJumpToExtra2;
        private CheckBox            mCheckBox;

        private RoutedEventHandler  mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToDescription;
        // Used for the Mark.dat file
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra1;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra2;

        private bool                HasDescription  { get; set; }
        private bool                HasExtras       { get; set; }

        public bool? IsChecked
        {
            get { return mCheckBox?.IsChecked; }
            set
            {
                if (mCheckBox != null)
                    mCheckBox.IsChecked = value;
            }
        }

        public new string Name
        {
            get => mName;
            set
            {
                mName               = value;
                mTextBoxName.Text   = mName;//RichTextBoxSetText(mName, ref mTextBoxName);
                mTextBlockNameLength.Text = mName.Length.ToString();

                CheckIllegalChars(ref mTextBoxName);
            }
        }        

        public int[] ID
        {
            get => mID;
            set
            {
                mID = value;
                mTextBlockID.Text = TextManager.GetIDToString(mID);
            }
        }

        public string Description
        {
            get => mDescription;
            set
            {
                mDescription = value;
                mTextBoxDescription.Text = mDescription;

                CheckIllegalChars(ref mTextBoxDescription);
            }
        }

        public string Extra1
        {
            get => mExtra1;
            set
            {
                mExtra1 = value;
                mTextBoxExtra1.Text = mExtra1;

                CheckIllegalChars(ref mTextBoxExtra1);
            }
        }

        public string Extra2
        {
            get => mExtra2;
            set
            {
                mExtra2 = value;
                mTextBoxExtra2.Text = mExtra2;

                CheckIllegalChars(ref mTextBoxExtra2);
            }
        }

        private void CheckIllegalChars(ref TextBox textBox)
        {
            string text = textBox.Text;
            if (TextManager.IsStringContainingIllegalChar(text))
                textBox.Background = Brushes.Red;
            else if (TextManager.IsStringContainingNonConventionalChar(text))
                textBox.Background = Brushes.Orange;
            else
                textBox.Background = Brushes.Transparent;
        }

        public bool TextBoxesContainsIllegalChars()
        {
            return mTextBoxName.Background == Brushes.Red || mTextBoxDescription.Background == Brushes.Red
                || mTextBoxExtra1.Background == Brushes.Red || mTextBoxExtra2.Background == Brushes.Red;
        }

        public bool TextBoxesContainsUnusualChars()
        {
            return mTextBoxName.Background == Brushes.Orange || mTextBoxDescription.Background == Brushes.Orange
                || mTextBoxExtra1.Background == Brushes.Orange || mTextBoxExtra2.Background == Brushes.Orange;
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
        public int          Extra1LengthPosition        { get; set; }
        public int          Extra1StartPosition         { get; set; }
        public int          Extra1EndPosition           { get; set; }
        public int          Extra2LengthPosition        { get; set; }
        public int          Extra2StartPosition         { get; set; }
        public int          Extra2EndPosition           { get; set; }

        // TextBoxes and TextBlocks
        public TextBlock    TextBoxNameLength           { get => mTextBlockNameLength;      set => mTextBlockNameLength     = value; }
        public TextBlock    TextBlockID                 { get => mTextBlockID;              set => mTextBlockID             = value; }
        public TextBox      TextBoxName                 { get => mTextBoxName;              set => mTextBoxName             = value; }
        public TextBox      TextBoxDescription          { get => mTextBoxDescription;       set => mTextBoxDescription      = value; }
        public TextBox      TextBoxExtra1               { get => mTextBoxExtra1;            set => mTextBoxExtra1           = value; }
        public TextBox      TextBoxExtra2               { get => mTextBoxExtra2;            set => mTextBoxExtra2           = value; }

        // Buttons
        public Button       ButtonJumpToWholeItem       { get => mButtonJumpToWholeItem;    set => mButtonJumpToWholeItem   = value; }
        public Button       ButtonJumpToID              { get => mButtonJumpToID;           set => mButtonJumpToID          = value; }
        public Button       ButtonJumpToName            { get => mButtonJumpToName;         set => mButtonJumpToName        = value; }
        public Button       ButtonJumpToDescription     { get => mButtonJumpToDescription;  set => mButtonJumpToDescription = value; }
        public Button       ButtonJumpToExtra1          { get => mButtonJumpToExtra1;       set => mButtonJumpToExtra1      = value; }
        public Button       ButtonJumpToExtra2          { get => mButtonJumpToExtra2;       set => mButtonJumpToExtra2      = value; }

        public Item() : base()
        {
            ItemInitialize(null, null, null, null, null, null, null, false, false);
            Name = "<No Name>";
            Description = "<No Description>";
        }

        public Item(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool hasCheckBox = false, RoutedEventHandler routedEventHandlerCheckBoxClick = null) : base()
        {
            ItemInitialize(routedEventHandlerButtonClickUpdateItem, routedEventHandlerButtonClickJumpToWholeItem,
                routedEventHandlerButtonClickJumpToID, routedEventHandlerButtonClickJumpToName,
                routedEventHandlerButtonClickJumpToDescription,
                routedEventHandlerButtonClickJumpToExtra1, routedEventHandlerButtonClickJumpToExtra2,
                hasDescription, hasExtras, hasCheckBox, routedEventHandlerCheckBoxClick);
        }

        public void ItemInitialize(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool hasCheckBox = false, RoutedEventHandler routedEventHandlerCheckBoxClick = null)
        {
            HasDescription  = hasDescription;
            HasExtras       = hasExtras;

            // Text block and boxes
            mTextBlockNameLength                = new TextBlock()   { Text  = "??" };
            mTextBlockNameLength.TextAlignment  = TextAlignment.Right;
            mTextBlockNameLength.Width          = 20d;
            mTextBlockNameLength.Height         = 22d;
            mTextBlockNameLength.Padding        = new Thickness(4);
            mTextBlockID                        = new TextBlock()   { Text  = "??" };
            mTextBlockID.Height                 = 22d;
            mTextBlockID.Padding                = new Thickness(4);
            int textBoxNameWidth = 120;
            if (!HasDescription && !HasExtras)
                textBoxNameWidth = 200;
            mTextBoxName                        = new TextBox() { MaxWidth  = textBoxNameWidth, Width = textBoxNameWidth };
            mTextBoxName.TextChanged           += TextBoxName_TextChanged;
            mTextBoxDescription                 = new TextBox() { MaxWidth  = 200, Width = 200 };
            mTextBoxDescription.TextChanged    += TextBoxDescription_TextChanged;
            mTextBoxExtra1                      = new TextBox() { MaxWidth  = 200, Width = 200 };
            mTextBoxExtra1.TextChanged         += TextBoxExtra1_TextChanged;
            mTextBoxExtra2                      = new TextBox() { MaxWidth  = 200, Width = 200 };
            mTextBoxExtra2.TextChanged         += TextBoxExtra2_TextChanged;

            // Buttons
            mButtonUpdateItemBasedOnNameLength  = new Button()  { Content   = "Update",     Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToWholeItem              = new Button()  { Content   = "Goto Item",  Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToID                     = new Button()  { Content   = "Goto" };
            mButtonJumpToName                   = new Button()  { Content   = "Goto" };
            mButtonJumpToDescription            = new Button()  { Content   = "Goto",       Margin = new Thickness(10, 0, 0, 0) };
            mButtonJumpToExtra1                 = new Button()  { Content   = "Goto",       Margin = new Thickness(10, 0, 0, 0) };
            mButtonJumpToExtra2                 = new Button()  { Content   = "Goto",       Margin = new Thickness(10, 0, 0, 0) };

            if (routedEventHandlerButtonClickUpdateItem != null)
                mButtonUpdateItemBasedOnNameLength.Click += routedEventHandlerButtonClickUpdateItem;
            if (routedEventHandlerButtonClickJumpToWholeItem != null)
                mButtonJumpToWholeItem.Click                   += routedEventHandlerButtonClickJumpToWholeItem;
            if (routedEventHandlerButtonClickJumpToID != null)
                mButtonJumpToID.Click                          += routedEventHandlerButtonClickJumpToID;
            if (routedEventHandlerButtonClickJumpToName != null)
                mButtonJumpToName.Click                        += routedEventHandlerButtonClickJumpToName;
            if (routedEventHandlerButtonClickJumpToDescription != null)
                mButtonJumpToDescription.Click                 += routedEventHandlerButtonClickJumpToDescription;
            if (routedEventHandlerButtonClickJumpToExtra1 != null)
                mButtonJumpToExtra1.Click                      += routedEventHandlerButtonClickJumpToExtra1;
            if (routedEventHandlerButtonClickJumpToExtra2 != null)
                mButtonJumpToExtra2.Click                      += routedEventHandlerButtonClickJumpToExtra2;

            mButtonUpdateItemBasedOnNameLength.Tag          = this;
            mButtonJumpToWholeItem.Tag                      = this;
            mButtonJumpToID.Tag                             = this;
            mButtonJumpToName.Tag                           = this;
            mButtonJumpToDescription.Tag                    = this;
            mButtonJumpToExtra1.Tag                         = this;
            mButtonJumpToExtra2.Tag                         = this;

            // Routed Event Handlers
            mRoutedEventHandlerButtonClickUpdateItem        = routedEventHandlerButtonClickUpdateItem;
            mRoutedEventHandlerButtonClickJumpToWholeItem   = routedEventHandlerButtonClickJumpToWholeItem;
            mRoutedEventHandlerButtonClickJumpToID          = routedEventHandlerButtonClickJumpToID;
            mRoutedEventHandlerButtonClickJumpToName        = routedEventHandlerButtonClickJumpToName;
            mRoutedEventHandlerButtonClickJumpToDescription = routedEventHandlerButtonClickJumpToDescription;
            mRoutedEventHandlerButtonClickJumpToExtra1      = routedEventHandlerButtonClickJumpToExtra1;
            mRoutedEventHandlerButtonClickJumpToExtra2      = routedEventHandlerButtonClickJumpToExtra2;

            Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Label() { Content = "Length:",  VerticalAlignment = VerticalAlignment.Center },
                    mTextBlockNameLength,

                    new Label() { Content = "ID:",      VerticalAlignment = VerticalAlignment.Center },
                    mTextBlockID,

                    new Label() { Content = "Name: ",   VerticalAlignment = VerticalAlignment.Center },
                    mTextBoxName                    
                }
            };

            // Add description and extras depending on if this should be done for the selected file type
            StackPanel stackPanel = Content as StackPanel;
            if (hasDescription)
            {
                stackPanel.Children.Add(new Label() { Content = "Description: ", VerticalAlignment = VerticalAlignment.Center });
                stackPanel.Children.Add(mTextBoxDescription);
            }

            if (hasExtras)
            {
                stackPanel.Children.Add(new Label() { Content = "Extra 1: ", VerticalAlignment = VerticalAlignment.Center });
                stackPanel.Children.Add(mTextBoxExtra1);
                stackPanel.Children.Add(new Label() { Content = "Extra 2: ", VerticalAlignment = VerticalAlignment.Center });
                stackPanel.Children.Add(mTextBoxExtra2);
            }

            if (hasCheckBox)
            {
                mCheckBox = new CheckBox()
                {
                    VerticalAlignment   = VerticalAlignment.Center,
                    Margin              = new Thickness(0, 0, 10, 0),
                    Tag                 = this
                };

                if (routedEventHandlerCheckBoxClick != null)
                    mCheckBox.Click += routedEventHandlerCheckBoxClick;

                stackPanel.Children.Insert(0, mCheckBox);
            }

            // Bind ThemeManager brushes to Label and TextBoxes' foreground property
            Binding myBinding               = new Binding();
            myBinding.Source                = ThemeManager.BindingBrushes;
            myBinding.Path                  = new PropertyPath("BrushForegroundLabel");
            myBinding.Mode                  = BindingMode.TwoWay;
            myBinding.UpdateSourceTrigger   = UpdateSourceTrigger.PropertyChanged;

            foreach (Label label in stackPanel.Children.OfType<Label>())
                label.SetBinding(ForegroundProperty, myBinding);
            foreach (TextBox textBox in stackPanel.Children.OfType<TextBox>())
                textBox.SetBinding(ForegroundProperty, myBinding);
        }

        public void SetEncoding(FontFamily fontFamily, Encoding encoding)
        {
            // Save strings with original encodings
            if (mNameWithOriginalEncoding == null)
                mNameWithOriginalEncoding = Name;
            if (mDescriptionWithOriginalEncoding == null)
                mDescriptionWithOriginalEncoding = Description;
            if (mExtra1WithOriginalEncoding == null)
                mExtra1WithOriginalEncoding = Extra1;
            if (mExtra2WithOriginalEncoding == null)
                mExtra2WithOriginalEncoding = Extra2;

            TextBoxName.FontFamily          = fontFamily;
            TextBoxDescription.FontFamily   = fontFamily;
            TextBoxExtra1.FontFamily        = fontFamily;
            TextBoxExtra2.FontFamily        = fontFamily;

            if (mNameWithOriginalEncoding != "")
                Name = TextManager.GetStringWithEncoding(mNameWithOriginalEncoding, encoding);
            if (mDescriptionWithOriginalEncoding != "" && Description != null)
                Description = TextManager.GetStringWithEncoding(mDescriptionWithOriginalEncoding, encoding);
            if (mExtra1WithOriginalEncoding != "" && Extra1 != null)
                Extra1 = TextManager.GetStringWithEncoding(mExtra1WithOriginalEncoding, encoding);
            if (mExtra2WithOriginalEncoding != "" && Extra2 != null)
                Extra2 = TextManager.GetStringWithEncoding(mExtra2WithOriginalEncoding, encoding);
        }

        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Name = textBox.Text;
            mTextBlockNameLength.Text = mName.Length.ToString();
        }

        private void TextBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Description = textBox.Text;
        }

        private void TextBoxExtra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Extra1 = textBox.Text;
        }

        private void TextBoxExtra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Extra2 = textBox.Text;
        }

        public ItemData ToItemData()
        {
            ItemData itemData                   = new ItemData();
            itemData.Name                       = Name;
            itemData.ID                         = ID;
            itemData.Description                = Description;
            itemData.Extra1                     = Extra1;
            itemData.Extra2                     = Extra2;

            itemData.ItemStartPosition          = ItemStartPosition;
            itemData.ItemEndPosition            = ItemEndPosition;
            itemData.IDStartPosition            = IDStartPosition;
            itemData.IDEndPosition              = IDEndPosition;
            itemData.NameStartPosition          = NameStartPosition;
            itemData.NameEndPosition            = NameEndPosition;
            itemData.DescriptionLengthPosition  = DescriptionLengthPosition;
            itemData.DescriptionStartPosition   = DescriptionStartPosition;
            itemData.DescriptionEndPosition     = DescriptionEndPosition;

            itemData.Extra1LengthPosition       = Extra1LengthPosition;
            itemData.Extra1StartPosition        = Extra1StartPosition;
            itemData.Extra1EndPosition          = Extra1EndPosition;
            itemData.Extra2LengthPosition       = Extra2LengthPosition;
            itemData.Extra2StartPosition        = Extra2StartPosition;
            itemData.Extra2EndPosition          = Extra2EndPosition;

            return itemData;
        }

        // Create a copy of the item
        public Item Clone()
        {
            Item itemClone = new Item(mRoutedEventHandlerButtonClickUpdateItem, mRoutedEventHandlerButtonClickJumpToWholeItem,
                mRoutedEventHandlerButtonClickJumpToID, mRoutedEventHandlerButtonClickJumpToName,
                mRoutedEventHandlerButtonClickJumpToDescription,
                mRoutedEventHandlerButtonClickJumpToExtra1, mRoutedEventHandlerButtonClickJumpToExtra2, HasDescription, HasExtras);

            // Ints and strings
            itemClone.ID                        = mID;
            itemClone.Name                      = mName;
            itemClone.Description               = mDescription;
            itemClone.Extra1                    = mExtra1;
            itemClone.Extra2                    = mExtra2;

            // TextBoxes and TextBlocks
            itemClone.TextBoxNameLength         = TextBoxNameLength;
            itemClone.TextBlockID               = mTextBlockID;
            itemClone.TextBoxName               = mTextBoxName;
            itemClone.TextBoxDescription        = mTextBoxDescription;
            itemClone.TextBoxExtra1             = mTextBoxExtra1;
            itemClone.TextBoxExtra2             = mTextBoxExtra2;

            // Buttons
            itemClone.ButtonJumpToName          = mButtonJumpToWholeItem;
            itemClone.ButtonJumpToName          = mButtonJumpToID;
            itemClone.ButtonJumpToName          = mButtonJumpToName;
            itemClone.ButtonJumpToDescription   = mButtonJumpToDescription;
            itemClone.ButtonJumpToExtra1        = mButtonJumpToExtra1;
            itemClone.ButtonJumpToExtra2        = mButtonJumpToExtra2;

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

            itemClone.Extra1LengthPosition      = Extra1LengthPosition;
            itemClone.Extra1StartPosition       = Extra1StartPosition;
            itemClone.Extra1EndPosition         = Extra1EndPosition;
            itemClone.Extra2LengthPosition      = Extra2LengthPosition;
            itemClone.Extra2StartPosition       = Extra2StartPosition;
            itemClone.Extra2EndPosition         = Extra2EndPosition;
            
            return itemClone;
        }

        public int GetIDValue()
        {
            return int.Parse(TextBlockID.Text);
        }

        public Dictionary<string, object> GetExcelTextExportList()
        {
            Dictionary<string, object> value;
            if (HasExtras)
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(mID) },
                        { "Name", mName }, { "Description", mDescription }, { "Extra1", mExtra1 }, { "Extra2", mExtra2 } };
            else if (HasDescription)
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(mID) },
                        { "Name", mName }, { "Description", mDescription }, { "Extra1", mExtra1 }, { "Extra2", mExtra2 } };
            else
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(mID) },
                        { "Name", mName }, { "Description", mDescription }, { "Extra1", mExtra1 }, { "Extra2", mExtra2 } };

            return value;
        }

        public static bool CompareIDs(int[] firstID, int[] secondID)
        {
            for (int i = 0; i < firstID.Length; ++i)
            {
                if (firstID[i] != secondID[i])
                    return false;
            }

            return true;
        }

        public static bool IsIDContainingString(int[] id, string idString)
        {
            if (TextManager.GetIDToString(id).Contains(idString))
                return true;
            else
                return false;
        }        
    }
}
