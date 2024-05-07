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

        int?    NameNullsLeft               { get; set; }
        int?    DescriptionNullsLeft        { get; set; }
        int?    Extra1NullsLeft             { get; set; }
        int?    Extra2NullsLeft             { get; set; }

        int     GetIDValue();
    }

    // <summary>
    // The ItemData class is used to store the item data without the need to call the main-/UI-thread,
    // it's also lighter than the Item class in terms of contents.
    // </summary>
    public class ItemData : IItemBase
    {
        public  string NameOriginal         { get; private set; }
        private string mName;
        private string mNameReversed;
        public  string DescriptionOriginal  { get; private set; }
        private string mDescription;
        private string mDescriptionReversed;
        public  string Extra1Original       { get; private set; }
        private string mExtra1;
        private string mExtra1Reversed;
        public  string Extra2Original       { get; private set; }
        private string mExtra2;
        private string mExtra2Reversed;

        #region Properties

        public string Name
        {
            get { return mName; }
            set 
            {
                mName           = value;
                if (NameOriginal == null)
                    NameOriginal = mName;
                mNameReversed   = TextManager.ReverseString(value);
            }
        }

        public  string NameReversed
        {
            get { return mNameReversed; }
            set
            {
                mNameReversed   = value;
                mName           = TextManager.ReverseString(value);
                if (NameOriginal == null)
                    NameOriginal = mName;
            }        
        }

        public  int[]  ID           { get; set; }

        public  string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                if (DescriptionOriginal == null)
                    DescriptionOriginal = mDescription;
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
                if (DescriptionOriginal == null)
                    DescriptionOriginal = mDescription;
            }
        }

        public string Extra1
        {
            get { return mExtra1; }
            set
            {
                mExtra1 = value;
                if (Extra1Original == null)
                    Extra1Original = mExtra1;
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
                if (Extra1Original == null)
                    Extra1Original = mExtra1;
            }
        }

        public string Extra2
        {
            get { return mExtra2; }
            set
            {
                mExtra2 = value;
                if (Extra2Original == null)
                    Extra2Original = mExtra2;
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
                if (Extra2Original == null)
                    Extra2Original = mExtra2;
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

        // Nulls Left
        public int?     NameNullsLeft               { get; set; }
        public int?     DescriptionNullsLeft        { get; set; }
        public int?     Extra1NullsLeft             { get; set; }
        public int?     Extra2NullsLeft             { get; set; }

        #endregion

        /// <summary>
        /// Construct an Item from the ItemData's data
        /// </summary>
        public Item ToItem(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem, RoutedEventHandler routedEventHandlerButtonClickJumpToID,
            RoutedEventHandler routedEventHandlerButtonClickJumpToName, RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool isStored, bool hasCheckBox = false, RoutedEventHandler routedEventHandlerCheckBoxClick = null)
        {
            Item item = new Item(routedEventHandlerButtonClickUpdateItem, routedEventHandlerButtonClickJumpToWholeItem,
                            routedEventHandlerButtonClickJumpToID, routedEventHandlerButtonClickJumpToName,
                            routedEventHandlerButtonClickJumpToDescription,
                            routedEventHandlerButtonClickJumpToExtra1, routedEventHandlerButtonClickJumpToExtra2,
                            hasDescription, hasExtras, isStored, hasCheckBox, routedEventHandlerCheckBoxClick);
            
            // Don't check bad states for performance
            item.IsCheckingTooLongTranslations  = false;

            item.Name                           = Name;
            item.ID                             = ID;
            if (mDescription != null)
                item.Description                = Description;
            if (mExtra1 != null)
                item.Extra1                     = Extra1;
            if (mExtra2 != null)
                item.Extra2                     = Extra2;

            item.ItemStartPosition              = ItemStartPosition;
            item.ItemEndPosition                = ItemEndPosition;
            item.IDStartPosition                = IDStartPosition;
            item.IDEndPosition                  = IDEndPosition;
            item.NameStartPosition              = NameStartPosition;
            item.NameEndPosition                = NameEndPosition;
            item.DescriptionLengthPosition      = DescriptionLengthPosition;
            item.DescriptionStartPosition       = DescriptionStartPosition;
            item.DescriptionEndPosition         = DescriptionEndPosition;

            item.Extra1LengthPosition           = Extra1LengthPosition;
            item.Extra1StartPosition            = Extra1StartPosition;
            item.Extra1EndPosition              = Extra1EndPosition;
            item.Extra2LengthPosition           = Extra2LengthPosition;
            item.Extra2StartPosition            = Extra2StartPosition;
            item.Extra2EndPosition              = Extra2EndPosition;

            item.NameNullsLeft                  = NameNullsLeft;
            item.DescriptionNullsLeft           = DescriptionNullsLeft;
            item.Extra1NullsLeft                = Extra1NullsLeft;
            item.Extra2NullsLeft                = Extra2NullsLeft;

            item.IsCheckingTooLongTranslations  = true;

            return item;
        }

        public int GetIDValue()
        {
            return ID[0] * 10000 + ID[1] * 100 + ID[2];
        }
    }

    /// <summary>
    /// The Item class is used to store all item data and all the UI-components connected to it.
    /// Item inherits from ListBoxItem and is used to make a visual representation of the items
    /// that are listed in ListBoxes in the program.
    /// 
    /// An item contains an ID and a name, it may also have a description and the Mark.dat file
    /// also have "extras", which means two more variables and text boxes.
    /// </summary>
    public class Item : ListBoxItem, IItemBase
    {
        private ItemData            ItemData                { get; set; }
        public  string              NameOriginal            { get; private set; }
        private string              mCurrentNameOriginalEncoding; // Used to temporary store the current name with original encoding
        //private string              mName;
        //private int[]               mID;
        public  string              DescriptionOriginal     { get; private set; }
        private string              mCurrentDescriptionOriginalEncoding; // Used to temporary store the current description with original encoding
        //private string              mDescription;
        // Used for the Mark.dat file
        public  string              Extra1Original          { get; private set; }
        private string              mCurrentExtra1OriginalEncoding; // Used to temporary store the current extra 1 with original encoding
        //private string              mExtra1;
        public  string              Extra2Original          { get; private set; }
        private string              mCurrentExtra2OriginalEncoding; // Used to temporary store the current extra 2 with original encoding
        //private string              mExtra2;

        private TextBlock           mTextBlockCharsLeft;
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

        public  bool                IsStored { get; private set; }

        private RoutedEventHandler  mRoutedEventHandlerButtonClickUpdateItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToWholeItem;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToID;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToName;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToDescription;
        // Used for the Mark.dat file
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra1;
        private RoutedEventHandler  mRoutedEventHandlerButtonClickJumpToExtra2;

        public  bool HasDescription { get; set; }
        public  bool HasExtras      { get; set; }

        private bool isCheckingTooLongTranslations  = true;
        private bool isUpdatingOriginalEncoding     = true;

        #region Properties

        public bool IsCheckingTooLongTranslations
        {
            get => isCheckingTooLongTranslations;
            set
            {
                isCheckingTooLongTranslations = value;

                if (isCheckingTooLongTranslations)
                    ColorTextBoxesIfTranslationsAreTooLong();
            }
        }

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
            get => ItemData.Name;
            set
            {
                ItemData.Name = value;

                if (NameOriginal == null)
                    NameOriginal = Name;

                mTextBoxName.Text = ItemData.Name;

                ColorTextBoxesBasedOnIllegalOrNonConventionalChars(ref mTextBoxName);
                ColorTextBoxesIfTranslationsAreTooLong();
            }
        }

        public int[] ID
        {
            get => ItemData.ID;
            set
            {
                ItemData.ID         = value;
                if (ItemData.ID != null)
                    mTextBlockID.Text   = TextManager.GetIDToString(ItemData.ID);
            }
        }

        public string Description
        {
            get => ItemData.Description;
            set
            {
                ItemData.Description = value;
                mTextBoxDescription.Text = ItemData.Description;

                if (DescriptionOriginal == null)
                    DescriptionOriginal = Description;

                ColorTextBoxesBasedOnIllegalOrNonConventionalChars(ref mTextBoxDescription);
                ColorTextBoxesIfTranslationsAreTooLong();
            }
        }

        public string Extra1
        {
            get => ItemData.Extra1;
            set
            {
                ItemData.Extra1     = value;
                mTextBoxExtra1.Text = ItemData.Extra1;

                if (Extra1Original == null)
                    Extra1Original  = Extra1;

                ColorTextBoxesBasedOnIllegalOrNonConventionalChars(ref mTextBoxExtra1);
                ColorTextBoxesIfTranslationsAreTooLong();
            }
        }

        public string Extra2
        {
            get => ItemData.Extra2;
            set
            {
                ItemData.Extra2 = value;
                mTextBoxExtra2.Text = ItemData.Extra2;

                if (Extra2Original == null)
                    Extra2Original = Extra2;

                ColorTextBoxesBasedOnIllegalOrNonConventionalChars(ref mTextBoxExtra2);
                ColorTextBoxesIfTranslationsAreTooLong();
            }
        }

        #endregion

        /// <summary>
        /// Colors the textboxes according based on if the text boxes contain illagal chars (chars that cannot (with the current setup)
        /// and shouldn't be saved as item text data). The textboxe gets colored red if they do contain illegal chars,
        /// orange if they contain non-conventional chars (chars that are not numbers, letters or normal special characters)
        /// and transparent else
        /// </summary>
        private void ColorTextBoxesBasedOnIllegalOrNonConventionalChars(ref TextBox textBox)
        {
            string text = textBox.Text;
            if (TextManager.IsStringContainingIllegalChar(text))
                textBox.Background = Brushes.Red;
            else if (TextManager.IsStringContainingNonConventionalChar(text))
                textBox.Background = Brushes.Orange;
            else
                textBox.Background = Brushes.Transparent;
        }

        /// <summary>
        /// Colors text boxes gray if the text boxes contain too long text strings for the amount of available char places
        /// </summary>
        private void ColorTextBoxesIfTranslationsAreTooLong()
        {
            if (!IsCheckingTooLongTranslations)
            //    || (mTextBoxName.Background     == Brushes.Gray && mTextBoxDescription.Background   == Brushes.Gray &&
            //        mTextBoxExtra1.Background   == Brushes.Gray && mTextBoxExtra2.Background        == Brushes.Gray)
            //    || (HasDescription && mDescription == null)
            //    || (HasExtras && (mExtra1 == null || mExtra2 == null))
            //    || mName == null)
                return;

            int charsLeft = ItemManager.GetNullsLeftForChars(this);
            mTextBlockCharsLeft.Text = charsLeft.ToString();

            bool nameLengthTooLong = false;

#if !DEBUG
            if (FileManager.FileItemProperties.FileType == FileType.ITEM && Name.Length > 14)
                nameLengthTooLong = true;
#endif

            if (FileManager.FileItemProperties.FileType == FileType.ITEM)
            {
                if (charsLeft < 0 || nameLengthTooLong)
                {
                    mTextBoxName.Background = Brushes.Gray;
                    if (HasDescription)
                        mTextBoxDescription.Background = Brushes.Gray;
                    if (HasExtras)
                    {
                        mTextBoxExtra1.Background = Brushes.Gray;
                        mTextBoxExtra2.Background = Brushes.Gray;
                    }
                }
                else
                {
                    if (mTextBoxName.Background == Brushes.Gray)
                        mTextBoxName.Background = Brushes.Transparent;
                    if (HasDescription && mTextBoxDescription.Background == Brushes.Gray)
                        mTextBoxDescription.Background = Brushes.Transparent;
                    if (HasExtras)
                    {
                        if (mTextBoxExtra1.Background == Brushes.Gray)
                            mTextBoxExtra1.Background = Brushes.Transparent;
                        if (mTextBoxExtra2.Background == Brushes.Gray)
                            mTextBoxExtra2.Background = Brushes.Transparent;
                    }
                }
            }
        }

        public bool IsTextBoxesContainingIllegalChars()
        {
            return mTextBoxName?.Background == Brushes.Red || mTextBoxDescription?.Background == Brushes.Red
                || mTextBoxExtra1?.Background == Brushes.Red || mTextBoxExtra2?.Background == Brushes.Red;
        }

        public bool IsTextBoxesContainingUnusualChars()
        {
            return mTextBoxName?.Background == Brushes.Orange || mTextBoxDescription?.Background == Brushes.Orange
                || mTextBoxExtra1?.Background == Brushes.Orange || mTextBoxExtra2?.Background == Brushes.Orange;
        }

        public bool IsTranslationsTooLong()
        {
            return mTextBoxName?.Background == Brushes.Gray || mTextBoxDescription?.Background == Brushes.Gray
                || mTextBoxExtra1?.Background == Brushes.Gray || mTextBoxExtra2?.Background == Brushes.Gray;
        }

        #region Properties

        // Positions
        public int ItemStartPosition         { get => ItemData.ItemStartPosition;         set => ItemData.ItemStartPosition         = value; }
        public int ItemEndPosition           { get => ItemData.ItemEndPosition;           set => ItemData.ItemEndPosition           = value; }
        public int IDStartPosition           { get => ItemData.IDStartPosition;           set => ItemData.IDStartPosition           = value; }
        public int IDEndPosition             { get => ItemData.IDEndPosition;             set => ItemData.IDEndPosition             = value; }
        public int NameStartPosition         { get => ItemData.NameStartPosition;         set => ItemData.NameStartPosition         = value; }
        public int NameEndPosition           { get => ItemData.NameEndPosition;           set => ItemData.NameEndPosition           = value; }
        public int DescriptionLengthPosition { get => ItemData.DescriptionLengthPosition; set => ItemData.DescriptionLengthPosition = value; }
        public int DescriptionStartPosition  { get => ItemData.DescriptionStartPosition;  set => ItemData.DescriptionStartPosition  = value; }
        public int DescriptionEndPosition    { get => ItemData.DescriptionEndPosition;    set => ItemData.DescriptionEndPosition    = value; }
        public int Extra1LengthPosition      { get => ItemData.Extra1LengthPosition;      set => ItemData.Extra1LengthPosition      = value; }
        public int Extra1StartPosition       { get => ItemData.Extra1StartPosition;       set => ItemData.Extra1StartPosition       = value; }
        public int Extra1EndPosition         { get => ItemData.Extra1EndPosition;         set => ItemData.Extra1EndPosition         = value; }
        public int Extra2LengthPosition      { get => ItemData.Extra2LengthPosition;      set => ItemData.Extra2LengthPosition      = value; }
        public int Extra2StartPosition       { get => ItemData.Extra2StartPosition;       set => ItemData.Extra2StartPosition       = value; }
        public int Extra2EndPosition         { get => ItemData.Extra2EndPosition;         set => ItemData.Extra2EndPosition         = value; }

        // Nulls Left
        public int? NameNullsLeft            { get; set; }
        public int? DescriptionNullsLeft     { get; set; }
        public int? Extra1NullsLeft          { get; set; }
        public int? Extra2NullsLeft          { get; set; }

        // TextBoxes and TextBlocks
        public  TextBlock   TextBlockCharsLeft  { get => mTextBlockCharsLeft;   set => mTextBlockCharsLeft  = value; }
        public  TextBlock   TextBlockID         { get => mTextBlockID;          set => mTextBlockID         = value; }
        public  TextBox     TextBoxName         { get => mTextBoxName;          set => mTextBoxName         = value; }
        public  TextBox     TextBoxDescription  { get => mTextBoxDescription;   set => mTextBoxDescription  = value; }
        public  TextBox     TextBoxExtra1       { get => mTextBoxExtra1;        set => mTextBoxExtra1       = value; }
        public  TextBox     TextBoxExtra2       { get => mTextBoxExtra2;        set => mTextBoxExtra2       = value; }

        // Buttons
        public  Button ButtonJumpToWholeItem    { get => mButtonJumpToWholeItem;    set => mButtonJumpToWholeItem   = value; }
        public  Button ButtonJumpToID           { get => mButtonJumpToID;           set => mButtonJumpToID          = value; }
        public  Button ButtonJumpToName         { get => mButtonJumpToName;         set => mButtonJumpToName        = value; }
        public  Button ButtonJumpToDescription  { get => mButtonJumpToDescription;  set => mButtonJumpToDescription = value; }
        public  Button ButtonJumpToExtra1       { get => mButtonJumpToExtra1;       set => mButtonJumpToExtra1      = value; }
        public  Button ButtonJumpToExtra2       { get => mButtonJumpToExtra2;       set => mButtonJumpToExtra2      = value; }

        #endregion

        /// <summary>
        /// Constructs the item.
        /// </summary>
        public Item(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool isStored, bool hasCheckBox = false,
            RoutedEventHandler routedEventHandlerCheckBoxClick = null) : base()
        {
            ItemData = new ItemData();

            ItemInitialize(routedEventHandlerButtonClickUpdateItem, routedEventHandlerButtonClickJumpToWholeItem,
                routedEventHandlerButtonClickJumpToID, routedEventHandlerButtonClickJumpToName,
                routedEventHandlerButtonClickJumpToDescription,
                routedEventHandlerButtonClickJumpToExtra1, routedEventHandlerButtonClickJumpToExtra2,
                hasDescription, hasExtras, isStored, hasCheckBox, routedEventHandlerCheckBoxClick);
        }

        /// <summary>
        /// Initializes the item data.
        /// </summary>
        public void ItemInitialize(RoutedEventHandler routedEventHandlerButtonClickUpdateItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToWholeItem,
            RoutedEventHandler routedEventHandlerButtonClickJumpToID, RoutedEventHandler routedEventHandlerButtonClickJumpToName,
            RoutedEventHandler routedEventHandlerButtonClickJumpToDescription,
            RoutedEventHandler routedEventHandlerButtonClickJumpToExtra1, RoutedEventHandler routedEventHandlerButtonClickJumpToExtra2,
            bool hasDescription, bool hasExtras, bool isStored, bool hasCheckBox = false,
            RoutedEventHandler routedEventHandlerCheckBoxClick = null)
        {
            IsStored                            = isStored;

            HasDescription                      = hasDescription;
            HasExtras                           = hasExtras;

            // Text block and boxes
            mTextBlockCharsLeft                 = new TextBlock() { Text = "??" };
            mTextBlockCharsLeft.TextAlignment   = TextAlignment.Right;
            mTextBlockCharsLeft.Width           = 30d;
            mTextBlockCharsLeft.Height          = 22d;
            mTextBlockCharsLeft.Padding         = new Thickness(4);
            mTextBlockID                        = new TextBlock() { Text = "??" };
            mTextBlockID.Height                 = 22d;
            mTextBlockID.Padding                = new Thickness(4);
            int textBoxNameWidth                = 120;
            if (!HasDescription && !HasExtras)
                textBoxNameWidth                = 200;
            mTextBoxName                        = new TextBox() { MaxWidth = textBoxNameWidth, Width = textBoxNameWidth };
            mTextBoxName.TextChanged           += TextBoxName_TextChanged;
            mTextBoxDescription                 = new TextBox() { MaxWidth = 200, Width = 200 };
            mTextBoxDescription.TextChanged    += TextBoxDescription_TextChanged;
            mTextBoxExtra1                      = new TextBox() { MaxWidth = 200, Width = 200 };
            mTextBoxExtra1.TextChanged         += TextBoxExtra1_TextChanged;
            mTextBoxExtra2                      = new TextBox() { MaxWidth = 200, Width = 200 };
            mTextBoxExtra2.TextChanged         += TextBoxExtra2_TextChanged;

            // Buttons
            mButtonUpdateItemBasedOnNameLength  = new Button() { Content = "Update", Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToWholeItem              = new Button() { Content = "Goto Item", Margin = new Thickness(0, 0, 10, 0) };
            mButtonJumpToID                     = new Button() { Content = "Goto" };
            mButtonJumpToName                   = new Button() { Content = "Goto" };
            mButtonJumpToDescription            = new Button() { Content = "Goto", Margin = new Thickness(10, 0, 0, 0) };
            mButtonJumpToExtra1                 = new Button() { Content = "Goto", Margin = new Thickness(10, 0, 0, 0) };
            mButtonJumpToExtra2                 = new Button() { Content = "Goto", Margin = new Thickness(10, 0, 0, 0) };

            if (routedEventHandlerButtonClickUpdateItem != null)
                mButtonUpdateItemBasedOnNameLength.Click    += routedEventHandlerButtonClickUpdateItem;
            if (routedEventHandlerButtonClickJumpToWholeItem != null)
                mButtonJumpToWholeItem.Click                += routedEventHandlerButtonClickJumpToWholeItem;
            if (routedEventHandlerButtonClickJumpToID != null)
                mButtonJumpToID.Click                       += routedEventHandlerButtonClickJumpToID;
            if (routedEventHandlerButtonClickJumpToName != null)
                mButtonJumpToName.Click                     += routedEventHandlerButtonClickJumpToName;
            if (routedEventHandlerButtonClickJumpToDescription != null)
                mButtonJumpToDescription.Click              += routedEventHandlerButtonClickJumpToDescription;
            if (routedEventHandlerButtonClickJumpToExtra1 != null)
                mButtonJumpToExtra1.Click                   += routedEventHandlerButtonClickJumpToExtra1;
            if (routedEventHandlerButtonClickJumpToExtra2 != null)
                mButtonJumpToExtra2.Click                   += routedEventHandlerButtonClickJumpToExtra2;

            mButtonUpdateItemBasedOnNameLength.Tag  = this;
            mButtonJumpToWholeItem.Tag              = this;
            mButtonJumpToID.Tag                     = this;
            mButtonJumpToName.Tag                   = this;
            mButtonJumpToDescription.Tag            = this;
            mButtonJumpToExtra1.Tag                 = this;
            mButtonJumpToExtra2.Tag                 = this;

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
                    new Label() { Content = "Chars left:",  VerticalAlignment = VerticalAlignment.Center },
                    mTextBlockCharsLeft,

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

        /// <summary>
        /// Sets the encoding for the item text
        /// </summary>
        public void SetEncoding(FontFamily fontFamily, Encoding encoding)
        {
            TextBoxName.FontFamily          = fontFamily;
            TextBoxDescription.FontFamily   = fontFamily;
            TextBoxExtra1.FontFamily        = fontFamily;
            TextBoxExtra2.FontFamily        = fontFamily;

            isUpdatingOriginalEncoding      = false;
            if (mCurrentNameOriginalEncoding != "" && Name != null)
                Name = TextManager.GetStringWithEncoding(mCurrentNameOriginalEncoding, encoding);
            if (mCurrentDescriptionOriginalEncoding != "" && Description != null)
                Description = TextManager.GetStringWithEncoding(mCurrentDescriptionOriginalEncoding, encoding);
            if (mCurrentExtra1OriginalEncoding != "" && Extra1 != null)
                Extra1 = TextManager.GetStringWithEncoding(mCurrentExtra1OriginalEncoding, encoding);
            if (mCurrentExtra2OriginalEncoding != "" && Extra2 != null)
                Extra2 = TextManager.GetStringWithEncoding(mCurrentExtra2OriginalEncoding, encoding);
            isUpdatingOriginalEncoding      = true;
        }

        #region Text Changes

        //TODO: Turn off assignment of original enconding texts if assignment are programmatic
        private void TextBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Name            = textBox.Text;
            if (isUpdatingOriginalEncoding)
                mCurrentNameOriginalEncoding = Name;
        }

        private void TextBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Description     = textBox.Text;
            if (isUpdatingOriginalEncoding)
                mCurrentDescriptionOriginalEncoding = Description;
        }

        private void TextBoxExtra1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Extra1          = textBox.Text;
            if (isUpdatingOriginalEncoding)
                mCurrentExtra1OriginalEncoding = Extra1;
        }

        private void TextBoxExtra2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Extra2          = textBox.Text;
            if (isUpdatingOriginalEncoding)
                mCurrentExtra2OriginalEncoding = Extra1;
        }

        #endregion

        /// <summary>
        /// Creates and returns an ItemData object with the item's data.
        /// </summary>
        public ItemData GetItemData() { return ItemData; }

        /// <summary>
        /// Creates a clone/copy of the item
        /// </summary>
        public Item Clone()
        {
            Item itemClone = new Item(mRoutedEventHandlerButtonClickUpdateItem, mRoutedEventHandlerButtonClickJumpToWholeItem,
                mRoutedEventHandlerButtonClickJumpToID, mRoutedEventHandlerButtonClickJumpToName,
                mRoutedEventHandlerButtonClickJumpToDescription,
                mRoutedEventHandlerButtonClickJumpToExtra1, mRoutedEventHandlerButtonClickJumpToExtra2, HasDescription, HasExtras, IsStored);

            // Don't check bad states for performance
            itemClone.IsCheckingTooLongTranslations = false;

            // Ints and strings
            itemClone.ID                        = ItemData.ID;
            itemClone.Name                      = ItemData.Name;
            itemClone.Description               = ItemData.Description;
            itemClone.Extra1                    = ItemData.Extra1;
            itemClone.Extra2                    = ItemData.Extra2;

            // TextBoxes and TextBlocks
            itemClone.TextBlockCharsLeft        = mTextBlockCharsLeft;
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

            itemClone.IsCheckingTooLongTranslations = true;

            return itemClone;
        }

        /// <summary>
        /// Converts the ID text to int and returns the ID if the Item
        /// </summary>
        public int GetIDValue()
        {
            return int.Parse(TextBlockID.Text);
        }

        /// <summary>
        /// Returns the text data as a Dictionary, which is used by a parser to save the item data as an excel-file.
        /// </summary>
        public Dictionary<string, object> GetExcelTextExportList()
        {
            Dictionary<string, object> value;
            if (HasExtras)
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(ItemData.ID) },
                        { "Name", ItemData.Name }, { "Description", ItemData.Description }, { "Extra1", ItemData.Extra1 },
                        { "Extra2", ItemData.Extra2 } };
            else if (HasDescription)
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(ItemData.ID) },
                        { "Name", ItemData.Name }, { "Description", ItemData.Description }, { "Extra1", ItemData.Extra1 },
                        { "Extra2", ItemData.Extra2 } };
            else
                value = new Dictionary<string, object> { { "ID", TextManager.GetIDToString(ItemData.ID) },
                        { "Name", ItemData.Name }, { "Description", ItemData.Description }, { "Extra1", ItemData.Extra1 },
                        { "Extra2", ItemData.Extra2 } };

            return value;
        }

        /// <summary>
        /// Compares two item IDs, returns true if they match or false if they don't.
        /// </summary>
        public static bool CompareIDs(int[] firstID, int[] secondID)
        {
            if (firstID == null || secondID == null)
                return false;

            for (int i = 0; i < firstID.Length; ++i)
            {
                if (firstID[i] != secondID[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns if the ID contains the ID-string.
        /// </summary>
        public static bool IsIDContainingString(int[] id, string idString)
        {
            if (TextManager.GetIDToString(id).Contains(idString))
                return true;
            else
                return false;
        }
    }
}
