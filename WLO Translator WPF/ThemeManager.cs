using ScintillaNET.WPF;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    /// <summary>
    /// ThemeManager class handles the theme app
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Used for binding the brushes of the items, so that they can instantly update the colors of the fonts without
        /// the need of iterating every item.
        /// </summary>
        public class BrushesBinding : INotifyPropertyChanged
        {
            public BrushesBinding() { PropertyChanged += BrushesBinding_PropertyChanged; }
            private Brush _brushForegroundLabel;

            public event PropertyChangedEventHandler PropertyChanged;

            public Brush BrushForegroundLabel
            {
                get => _brushForegroundLabel;
                set
                {
                    if (_brushForegroundLabel != value)
                    {
                        _brushForegroundLabel = value;
                        PropertyChanged(this, new PropertyChangedEventArgs("BrushForegroundLabel"));
                    }
                }
            }

            private void BrushesBinding_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
            }
        }

        #region Fields

        // Objects
        private static Grid                 mGridBack                           = null;
        private static Grid[]               mGridsFront                         = null;
        private static Menu                 mMenu                               = null;
        private static ToolBar              mToolBar                            = null;
        private static GroupBox[]           mGroupBoxes                         = null;
        private static CheckBox[]           mCheckBoxes                         = null;
        private static Button[]             mButtons                            = null;
        private static GridSplitter[]       mGridSplitters                      = null;
        private static Label[]              mLabels                             = null;
        private static ScintillaWPF         mScintillaTextBox                   = null;
        private static ListBox[]            mListBoxes                          = null;
        private static TextBox[]            mTextBoxes                          = null;
        private static TextBlock[]          mTextBlocks                         = null;
        private static Grid                 mGridProjectFileName                = null;

        // Brushes
        private static Brush                mBrushMenuDefault                   = default;
        private static Brush                mBackBrushToolBarDefault            = default;
        private static Brush                mBorderBrushGroupBoxDefault         = default;
        private static Brush                mBackBrushCheckBoxDefault           = default;
        private static Brush                mBackBrushButtonDefault             = default;
        private static Brush                mBackBrushGridBackDefault           = default;
        private static Brush                mBackBrushGridFrontDefault          = default;
        private static Brush                mBackBrushGridSplitterDefault       = default;
        private static System.Drawing.Color mBackColorScintiellaDefault         = default;
        private static System.Drawing.Color mForeColorScintiellaDefault         = default;
        private static Brush                mBackBrushTextBoxesDefault          = default;
        private static Brush                mBorderBrushTextBoxesDefault        = default;
        private static Brush                mBackBrushGridProjectFileName       = default;
        private static Brush                mForeBrushGridProjectFileName       = default;

        public static  BrushesBinding       BindingBrushes { get; private set; } = new BrushesBinding();

        // Window Buttons
        private static Button               mButtonMinimize                     = null;
        private static Button               mButtonResizeFullscreen             = null;
        private static Button               mButtonClose                        = null;

        // Window Button Image Sources
        public  static ImageSource          ImageSourceButtonResizeFullscreen   { get; private set; } = null;
        public  static ImageSource          ImageSourceButtonResizeNormal       { get; private set; } = null;

        public  static int                  ThemeIndex                          { get; private set; } = 0;

        private static int                  mThemeNamesCount = 5;

        #endregion

        private enum ThemeNames
        {
            DEFAULT,
            LIGHT,
            DARK,
            COLORFUL_LANDSCAPE,
            GRAY_PATTERNS
        }

        /// <summary>
        /// Initializes the theme manager.
        /// </summary>
        public static void Initialize(ref Grid gridBack, ref Grid[] gridsFront, ref Menu menu, ref ToolBar toolBar, ref GroupBox[] groupBoxes,
                                      ref CheckBox[] checkBoxes, ref Button[] buttons, ref GridSplitter[] gridSplitters, ref Label[] labels,
                                      ref ScintillaWPF scintillaTextBox, ref ListBox[] listBoxes,
                                      ref Button buttonMinimize, ref Button buttonResizeFullscreen, ref Button buttonClose,
                                      ref TextBox[] textBoxes, ref TextBlock[] textBlocks, ref Grid gridProjectFileName)
        {
            // Objects
            mGridBack                       = gridBack;
            mGridsFront                     = gridsFront;
            mMenu                           = menu;
            mToolBar                        = toolBar;
            mGroupBoxes                     = groupBoxes;
            mCheckBoxes                     = checkBoxes;
            mButtons                        = buttons;
            mGridSplitters                  = gridSplitters;
            mLabels                         = labels;
            mScintillaTextBox               = scintillaTextBox;
            mListBoxes                      = listBoxes;
            mTextBoxes                      = textBoxes;
            mTextBlocks                     = textBlocks;
            mGridProjectFileName            = gridProjectFileName;

            // Brushes
            mBrushMenuDefault               = mMenu.Background;
            mBackBrushToolBarDefault        = mToolBar.Background;
            mBorderBrushGroupBoxDefault     = mGroupBoxes[0]?.BorderBrush;
            mBackBrushCheckBoxDefault       = mCheckBoxes[0]?.Background;
            mBackBrushButtonDefault         = mButtons[0]?.Background;
            mBackBrushGridBackDefault       = mGridBack.Background;
            mBackBrushGridFrontDefault      = mGridsFront[0]?.Background;
            mBackBrushGridSplitterDefault   = mGridSplitters[0]?.Background;
            mBackBrushTextBoxesDefault      = listBoxes[0]?.Background;
            mBorderBrushTextBoxesDefault    = listBoxes[0]?.BorderBrush;
            mBackColorScintiellaDefault     = mScintillaTextBox.Styles[ScintillaNET.Style.Default].BackColor;
            mForeColorScintiellaDefault     = mScintillaTextBox.Styles[ScintillaNET.Style.Default].ForeColor;
            mBackBrushGridProjectFileName   = gridProjectFileName.Background;
            mForeBrushGridProjectFileName   = (gridProjectFileName.Children[0] as TextBlock).Foreground;

            // Buttons
            mButtonMinimize                 = buttonMinimize;
            mButtonResizeFullscreen         = buttonResizeFullscreen;
            mButtonClose                    = buttonClose;
        }

        /// <summary>
        /// Retrieves the theme-name from the enum at the specified index.
        /// </summary>
        private static string GetThemeNameFromEnum(int index)
        {
            string output = "";

            string[] splitString = ((ThemeNames)index).ToString().Split('_');
            for (int i = 0; i < splitString.Length; ++i)
            {
                output += splitString[i][0] + splitString[i].ToLower().Remove(0, 1);

                if (i != splitString.Length - 1)
                    output += ' ';
            }

            return output;
        }

        /// <summary>
        /// Sets the theme names to the combo box you can select themes from.
        /// </summary>
        public static void SetThemeNamesToComboBox(ref ComboBox comboBoxThemes)
        {
            for (int i = 0; i < mThemeNamesCount; ++i)
                comboBoxThemes.Items.Add(GetThemeNameFromEnum(i));
        }

        /// <summary>
        /// Updates the menu colors according to the back and front brushes.
        /// </summary>
        private static void SetMenuColors(Brush backgroundBrush, Brush foregroundBrush)
        {
            mMenu.Background = backgroundBrush;
            mMenu.Foreground = foregroundBrush;

            foreach (MenuItem menuItem in mMenu.Items.OfType<MenuItem>())
            {
                menuItem.Foreground     = foregroundBrush;

                // Color children menu items
                foreach (MenuItem menuItemChild in menuItem.Items.OfType<MenuItem>())
                {
                    //menuItemChild.Background    = backgroundBrush;
                    menuItemChild.Foreground    = Brushes.Black;
                }

                //// Color children separators
                //foreach (Separator separatorChild in menuItem.Items.OfType<Separator>())
                //{
                //    separatorChild.Background    = foregroundBrush;
                //}
            }
        }

        /// <summary>
        /// Updates the colors of the different UI-components according to the specified brushes.
        /// </summary>
        private static void SetThemeColors(Brush menuBrush, Brush toolBarBrush, Brush groupBoxesBorderBrush,
            Brush checkBoxesBrush, Brush buttonsBrush, Brush gridBackBrush, Brush gridFrontBrush, Brush gridSplittersBrush,
            Brush foregroundFontBrush, System.Drawing.Color scintiellaBackColor, System.Drawing.Color scintiellaForeColor,
            Brush textBoxesBrush, Brush textBoxesBorderBrush, Brush gridProjectFileNameBrush, Brush gridProjectFileNameForegroundBrush)
        {
            SetMenuColors(menuBrush, foregroundFontBrush);

            // Set toolBar colors
            mToolBar.Background         = toolBarBrush;
            mToolBar.Foreground         = foregroundFontBrush;

            foreach (ComboBox comboBox in mToolBar.Items.OfType<ComboBox>())
            {
                comboBox.Background     = textBoxesBrush;
                comboBox.Foreground     = foregroundFontBrush;
                comboBox.BorderBrush    = textBoxesBorderBrush;
            }

            foreach (TextBox textBox in mToolBar.Items.OfType<TextBox>())
            {
                textBox.Background      = textBoxesBrush;
                textBox.Foreground      = foregroundFontBrush;
                textBox.BorderBrush     = textBoxesBorderBrush;
            }

            foreach (Button button in mToolBar.Items.OfType<Button>())
                button.Foreground       = foregroundFontBrush;

            mGridBack.Background        = gridBackBrush;
            foreach (Grid grid in mGridsFront)
                grid.Background         = gridFrontBrush;

            foreach (GroupBox groupBox in mGroupBoxes)
            {
                groupBox.BorderBrush    = groupBoxesBorderBrush;
                groupBox.Foreground     = foregroundFontBrush;
            }

            foreach (CheckBox checkBox in mCheckBoxes)
            {
                checkBox.Background     = checkBoxesBrush;
                checkBox.Foreground     = foregroundFontBrush;
            }

            foreach (Button button in mButtons)
            {
                button.Background       = buttonsBrush;
                button.Foreground       = foregroundFontBrush;
            }

            foreach (Label label in mLabels)
                label.Foreground        = foregroundFontBrush;

            foreach (ListBox listBox in mListBoxes)
            {
                listBox.Background      = textBoxesBrush;
                listBox.Foreground      = foregroundFontBrush;
                listBox.BorderBrush     = mButtons[0]?.BorderBrush;
            }

            foreach (TextBox textBox in mTextBoxes)
            {
                textBox.Background      = textBoxesBrush;
                textBox.Foreground      = foregroundFontBrush;
            }

            foreach (TextBlock textBlock in mTextBlocks)
                textBlock.Foreground    = foregroundFontBrush;

            mGridProjectFileName.Background = gridProjectFileNameBrush;
            (mGridProjectFileName.Children[0] as TextBlock).Foreground = gridProjectFileNameForegroundBrush;

            // Update has to happen on the main thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                BindingBrushes.BrushForegroundLabel = foregroundFontBrush;
            });

            mScintillaTextBox.Styles[ScintillaNET.Style.Default].BackColor                  = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Default].ForeColor                  = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.LineNumber].BackColor               = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.LineNumber].ForeColor               = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.BraceBad].BackColor                 = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.BraceBad].ForeColor                 = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.BraceLight].BackColor               = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.BraceLight].ForeColor               = scintiellaForeColor;

            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Assignment].BackColor    = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Assignment].ForeColor    = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Comment].BackColor       = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Comment].ForeColor       = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Default].BackColor       = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Default].ForeColor       = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.DefVal].BackColor        = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.DefVal].ForeColor        = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Key].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Key].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Section].BackColor       = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Properties.Section].ForeColor       = scintiellaForeColor;

            mScintillaTextBox.CaretForeColor = ((SolidColorBrush)foregroundFontBrush).Color;
        }

        /// <summary>
        /// Returns a brush with the specified color and transparency.
        /// </summary>
        private static Brush GetTransparentBrush(byte alpha, Brush brush)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            return new SolidColorBrush(
                Color.FromArgb(alpha, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B));
        }

        /// <summary>
        /// Returns a brush with the specified color values and transparency.
        /// </summary>
        private static Brush GetTransparentBrush(byte alpha, byte red, byte green, byte blue)
        {
            return new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
        }

        /// <summary>
        /// Sets the colorful lanscape theme.
        /// </summary>
        private static void SetColorfulLandscapeTheme()
        {
            byte lowTransparancy     = 178;
            byte highTransparancy    = 78;

            Brush menuBrush         = GetTransparentBrush(lowTransparancy, 255, 255, 255);
            Brush toolBarBrush      = GetTransparentBrush(highTransparancy, 255, 255, 255);

            string imageName = "Wonderland Online Landscape";
            ImageBrush groupBoxBackBrush = new ImageBrush((ImageSource)System.Windows.Application.Current.MainWindow.FindResource(imageName));

            SetThemeColors(menuBrush, toolBarBrush, mBorderBrushGroupBoxDefault, mBackBrushCheckBoxDefault,
                mBackBrushButtonDefault, groupBoxBackBrush, GetTransparentBrush(highTransparancy, mBackBrushGridFrontDefault),
                GetTransparentBrush(lowTransparancy, mBackBrushGridFrontDefault), Brushes.Black,
                mBackColorScintiellaDefault, mForeColorScintiellaDefault, mBackBrushTextBoxesDefault, mBorderBrushTextBoxesDefault,
                GetTransparentBrush(lowTransparancy, mBackBrushGridProjectFileName),
                GetTransparentBrush(lowTransparancy, mForeBrushGridProjectFileName));
        }

        /// <summary>
        /// Sets the gray patterns theme.
        /// </summary>
        private static void SetGrayPatternsTheme()
        {
            byte lowTransparancy    = 178;
            byte highTransparancy   = 158;

            Brush menuBrush         = GetTransparentBrush(lowTransparancy, 255, 255, 255);
            Brush toolBarBrush      = GetTransparentBrush(highTransparancy, 255, 255, 255);
            Brush checkBoxBrush     = GetTransparentBrush(highTransparancy, 255, 255, 255);

            string imageName = "Gray Patterns";
            ImageBrush groupBoxBackBrush = new ImageBrush((ImageSource)System.Windows.Application.Current.MainWindow.FindResource(imageName));

            SetThemeColors(menuBrush, toolBarBrush, mBorderBrushGroupBoxDefault, checkBoxBrush,
                mBackBrushButtonDefault, groupBoxBackBrush, GetTransparentBrush(lowTransparancy, mBackBrushGridFrontDefault),
                GetTransparentBrush(lowTransparancy, mBackBrushGridFrontDefault), Brushes.Black,
                mBackColorScintiellaDefault, mForeColorScintiellaDefault, mBackBrushTextBoxesDefault, mBorderBrushTextBoxesDefault,
                mBackBrushGridProjectFileName, mForeBrushGridProjectFileName);
        }

        private enum WindowButtonColor  { NORMAL, BLACK, WHITE }
        private enum WindowButtonType   { MINIMIZE, RESIZE_NORMAL, RESIZE_FULLSCREEN, CLOSE }

        /// <summary>
        /// Sets the images of the window buttons (minimize, resize and close).
        /// </summary>
        private static void SetWindowButtons(WindowButtonColor windowButtonColor)
        {
            System.Windows.Window mainWindow = System.Windows.Application.Current.MainWindow;

            string[] buttonImageNames = { "Button Minimize", "Button Resize Not Fullscreen", "Button Resize Fullscreen", "Button Close" };
            
            // Set either resize normal or resize fullscreen as the image for the resize button
            WindowButtonType windowButtonTypeResize = WindowButtonType.RESIZE_NORMAL;
            if (mainWindow.WindowState == System.Windows.WindowState.Maximized)
                windowButtonTypeResize = WindowButtonType.RESIZE_FULLSCREEN;

            switch (windowButtonColor)
            {
                case WindowButtonColor.BLACK:
                    for (int i = 0; i < buttonImageNames.Length; ++i)
                        buttonImageNames[i] += " Black";
                    break;
                case WindowButtonColor.WHITE:
                    for (int i = 0; i < buttonImageNames.Length; ++i)
                        buttonImageNames[i] += " White";
                    break;
            }

            // Set resize button images so they can be switched from MainWindow
            ImageSourceButtonResizeFullscreen       = (ImageSource)mainWindow.FindResource(
                buttonImageNames[(int)WindowButtonType.RESIZE_FULLSCREEN]);
            ImageSourceButtonResizeNormal    = (ImageSource)mainWindow.FindResource(
                buttonImageNames[(int)WindowButtonType.RESIZE_NORMAL]);

            // Set images of buttons
            (mButtonMinimize.Content as Image).Source =
                (ImageSource)mainWindow.FindResource(buttonImageNames[(int)WindowButtonType.MINIMIZE]);

            if (windowButtonTypeResize == WindowButtonType.RESIZE_NORMAL)
                (mButtonResizeFullscreen.Content as Image).Source = ImageSourceButtonResizeNormal;
            else
                (mButtonResizeFullscreen.Content as Image).Source = ImageSourceButtonResizeFullscreen;

            (mButtonClose.Content as Image).Source = (ImageSource)mainWindow.FindResource(buttonImageNames[(int)WindowButtonType.CLOSE]);
        }

        /// <summary>
        /// Applies the theme with the specified theme index.
        /// </summary>
        public static void ApplyTheme(int themeIndex = -1)
        {
            if (themeIndex != -1)
                ThemeIndex = themeIndex;

            switch (ThemeIndex)
            {
                case 0: // Default color
                    SetThemeColors(mBrushMenuDefault, mBackBrushToolBarDefault, mBorderBrushGroupBoxDefault, 
                        mBackBrushCheckBoxDefault, mBackBrushButtonDefault, mBackBrushGridBackDefault,
                        mBackBrushGridFrontDefault, mBackBrushGridSplitterDefault,
                        Brushes.Black, mBackColorScintiellaDefault, mForeColorScintiellaDefault,
                        mBackBrushTextBoxesDefault, mBorderBrushTextBoxesDefault,
                        mBackBrushGridProjectFileName, mForeBrushGridProjectFileName);
                    SetWindowButtons(WindowButtonColor.NORMAL);
                    break;
                case 1: // Light color
                    Brush white     = Brushes.White;
                    Brush lightGray = GetTransparentBrush(255, 250, 250, 250);
                    SetThemeColors(white, white, lightGray, white, lightGray, lightGray, white, lightGray,
                        Brushes.Black, mBackColorScintiellaDefault, mForeColorScintiellaDefault,
                        mBackBrushTextBoxesDefault, lightGray, lightGray, mForeBrushGridProjectFileName);
                    SetWindowButtons(WindowButtonColor.BLACK);
                    break;
                case 2: // Dark color
                    Brush darkGray      = GetTransparentBrush(255, 80, 80, 85);
                    SolidColorBrush darkerGray = (SolidColorBrush)GetTransparentBrush(255, 60, 60, 65);
                    Brush darkestGray   = GetTransparentBrush(255, 50, 50, 55);
                    Brush lighterGray   = GetTransparentBrush(255, 90, 90, 95);
                    SetThemeColors(darkGray, darkGray, darkerGray, darkerGray, darkerGray, darkerGray, darkGray, darkerGray, 
                        Brushes.White, System.Drawing.Color.FromArgb(255, darkerGray.Color.R, darkerGray.Color.G, darkerGray.Color.B),
                        System.Drawing.Color.White, darkerGray, darkGray, lighterGray, darkestGray);
                    SetWindowButtons(WindowButtonColor.WHITE);
                    break;
                case 3:
                    SetColorfulLandscapeTheme();
                    SetWindowButtons(WindowButtonColor.NORMAL);
                    break;
                case 4:
                    SetGrayPatternsTheme();
                    SetWindowButtons(WindowButtonColor.NORMAL);
                    break;
            }
        }
    }
}
