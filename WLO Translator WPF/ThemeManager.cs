using ScintillaNET.WPF;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace WLO_Translator_WPF
{
    public static class ThemeManager
    {
        // Objects
        private static Grid                 mGridBack                       = null;
        private static Grid[]               mGridsFront                     = null;
        private static Menu                 mMenu                           = null;
        private static ToolBar              mToolBar                        = null;
        private static GroupBox[]           mGroupBoxes                     = null;
        private static CheckBox[]           mCheckBoxes                     = null;
        private static Button[]             mButtons                        = null;
        private static GridSplitter[]       mGridSplitters                  = null;
        private static Label[]              mLabels                         = null;
        private static ScintillaWPF         mScintillaTextBox               = null;
        private static ListBox[]            mListBoxes                      = null;

        // Brushes
        private static Brush                mBrushMenuDefault               = default;
        private static Brush                mBackBrushToolBarDefault        = default;
        private static Brush                mBackBrushGroupBoxDefault       = default;
        private static Brush                mBorderBrushGroupBoxDefault     = default;
        private static Brush                mBackBrushCheckBoxDefault       = default;
        private static Brush                mBackBrushButtonDefault         = default;
        private static Brush                mBackBrushGridBackDefault       = default;
        private static Brush                mBackBrushGridFrontDefault      = default;
        private static Brush                mBackBrushGridSplitterDefault   = default;
        private static System.Drawing.Color mBackColorScintiellaDefault     = default;
        private static System.Drawing.Color mForeColorScintiellaDefault     = default;
        private static Brush                mBackBrushTextBoxesDefault      = default;

        public static int ThemeIndex { get; private set; } = 0;

        private static int mThemeNamesCount = 5;
        private enum ThemeNames
        {
            DEFAULT_THEME,
            LIGHT_THEME,
            DARK_THEME,
            COLORFUL_LANDSCAPE_LESS_ENVIRONMENT_THEME,
            COLORFUL_LANDSCAPE_THEME
        }

        public static void Initialize(ref Grid gridBack, ref Grid[] gridsFront, ref Menu menu, ref ToolBar toolBar, ref GroupBox[] groupBoxes,
                                      ref CheckBox[] checkBoxes, ref Button[] buttons, ref GridSplitter[] gridSplitters, ref Label[] labels,
                                      ref ScintillaWPF scintillaTextBox, ref ListBox[] listBoxes)
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

            // Brushes
            mBrushMenuDefault               = mMenu.Background;
            mBackBrushToolBarDefault        = mToolBar.Background;
            mBackBrushGroupBoxDefault       = mGroupBoxes[0]?.Background;
            mBorderBrushGroupBoxDefault     = mGroupBoxes[0]?.BorderBrush;
            mBackBrushCheckBoxDefault       = mCheckBoxes[0]?.Background;
            mBackBrushButtonDefault         = mButtons[0]?.Background;
            mBackBrushGridBackDefault       = mGridBack.Background;
            mBackBrushGridFrontDefault      = mGridsFront[0]?.Background;
            mBackBrushGridSplitterDefault   = mGridSplitters[0]?.Background;
            mBackBrushTextBoxesDefault      = mScintillaTextBox.Background;
            mBackColorScintiellaDefault     = mScintillaTextBox.Styles[ScintillaNET.Style.Default].BackColor;
            mForeColorScintiellaDefault     = mScintillaTextBox.Styles[ScintillaNET.Style.Default].ForeColor;
        }

        private static string GetEnumName(int index)
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

        public static void SetThemesToComboBox(ref ComboBox comboBoxThemes)
        {
            for (int i = 0; i < mThemeNamesCount; ++i)
                comboBoxThemes.Items.Add(GetEnumName(i));
        }

        private static void SetThemeColors(Brush menuBrush, Brush toolBarBrush, Brush groupBoxesBorderBrush,
            Brush checkBoxesBrush, Brush buttonsBrush, Brush gridBackBrush, Brush gridFrontBrush, Brush gridSplittersBrush,
            Brush foregroundFontBrush, System.Drawing.Color scintiellaBackColor, System.Drawing.Color scintiellaForeColor, Brush textBoxesBrush)
        {
            mMenu.Background            = menuBrush;
            mMenu.Foreground            = foregroundFontBrush;
            mToolBar.Background         = toolBarBrush;
            mToolBar.Foreground         = foregroundFontBrush;

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
            }

            //mScintillaTextBox.Lexer = ScintillaNET.Lexer.Xml;
            mScintillaTextBox.Styles[ScintillaNET.Style.Default].BackColor              = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Default].ForeColor              = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.LineNumber].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.LineNumber].ForeColor           = scintiellaForeColor;
            //mScintillaTextBox.Styles[ScintillaNET.Style.Default].FillLine   = false;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Default].BackColor          = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Default].ForeColor          = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Asp].BackColor              = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Asp].ForeColor              = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.AspAt].BackColor            = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.AspAt].ForeColor            = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Attribute].BackColor        = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Attribute].ForeColor        = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.AttributeUnknown].BackColor = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.AttributeUnknown].ForeColor = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.CData].BackColor            = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.CData].ForeColor            = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Comment].BackColor          = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Comment].ForeColor          = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.DoubleString].BackColor     = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.DoubleString].ForeColor     = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Entity].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Entity].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Number].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Number].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Other].BackColor            = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Other].ForeColor            = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Question].BackColor         = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Question].ForeColor         = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Script].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Script].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.SingleString].BackColor     = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.SingleString].ForeColor     = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Tag].BackColor              = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Tag].ForeColor              = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.TagEnd].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.TagEnd].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Value].BackColor            = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.Value].ForeColor            = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XcComment].BackColor        = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XcComment].ForeColor        = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XmlEnd].BackColor           = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XmlEnd].ForeColor           = scintiellaForeColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XmlStart].BackColor         = scintiellaBackColor;
            mScintillaTextBox.Styles[ScintillaNET.Style.Xml.XmlStart].ForeColor         = scintiellaForeColor;
        }

        private static Brush GetTransparantBrush(byte transparancy, Brush brush)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            return new SolidColorBrush(Color.FromArgb(transparancy, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B));
        }

        private static Brush GetTransparantBrush(byte transparancy, byte red, byte green, byte blue)
        {
            return new SolidColorBrush(Color.FromArgb(transparancy, red, green, blue));
        }

        private static void SetColorfulLandscapeTheme(bool isHalfVisable)
        {
            byte lowTransparancy     = 178;
            byte highTransparancy    = 158;

            Brush menuBrush         = GetTransparantBrush(lowTransparancy, 255, 255, 255);
            Brush toolBarBrush      = GetTransparantBrush(highTransparancy, 255, 255, 255);
            Brush checkBoxBrush     = GetTransparantBrush(highTransparancy, 255, 255, 255);

            string imageName = "Wonderland Online Colorful Landscape";
            if (isHalfVisable)
                imageName = "Wonderland Online Less Colorful Landscape";
            ImageBrush groupBoxBackBrush = new ImageBrush((ImageSource)System.Windows.Application.Current.MainWindow.FindResource(imageName));

            SetThemeColors(menuBrush, toolBarBrush, mBorderBrushGroupBoxDefault, checkBoxBrush,
                mBackBrushButtonDefault, groupBoxBackBrush, GetTransparantBrush(lowTransparancy, mBackBrushGridFrontDefault),
                GetTransparantBrush(lowTransparancy, mBackBrushGridFrontDefault), Brushes.Black,
                mBackColorScintiellaDefault, mForeColorScintiellaDefault, mBackBrushTextBoxesDefault);
        }

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
                        Brushes.Black, mBackColorScintiellaDefault, mForeColorScintiellaDefault, mBackBrushTextBoxesDefault);
                    break;
                case 1: // Light color
                    Brush lightGray = GetTransparantBrush(255, 250, 250, 250);
                    Brush white = Brushes.White;
                    SetThemeColors(lightGray, lightGray, white, lightGray, white, white, lightGray, white,
                        Brushes.Black, mBackColorScintiellaDefault, mForeColorScintiellaDefault, mBackBrushTextBoxesDefault);
                    break;
                case 2: // Dark color
                    Brush darkGray      = GetTransparantBrush(255, 100, 100, 110);
                    SolidColorBrush darkerGray = (SolidColorBrush)GetTransparantBrush(255, 80, 80, 90);
                    SetThemeColors(darkGray, darkGray, darkerGray, darkerGray, darkerGray, darkerGray, darkGray, darkerGray, 
                        Brushes.White, System.Drawing.Color.FromArgb(255, darkerGray.Color.R, darkerGray.Color.G, darkerGray.Color.B),
                        System.Drawing.Color.White, darkerGray);
                    break;
                case 3:
                    SetColorfulLandscapeTheme(true);
                    break;
                case 4:
                    SetColorfulLandscapeTheme(false);
                    break;
            }
        }
    }
}
