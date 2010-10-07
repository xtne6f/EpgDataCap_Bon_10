using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace EpgTimer
{
    /// <summary>
    /// SetEpgColor.xaml の相互作用ロジック
    /// </summary>
    public partial class SetEpgColor : UserControl
    {
        private Dictionary<string, ColorSelectionItem> colorList = new Dictionary<string,ColorSelectionItem>();
        public SetEpgColor()
        {
            InitializeComponent();

            foreach (string name in ColorDef.ColorNames)
            {
                colorList.Add(name, new ColorSelectionItem(name, ColorDef.Instance.ColorTable[name]));
            }
            comboBox0.DataContext = colorList.Values;
            comboBox1.DataContext = colorList.Values;
            comboBox2.DataContext = colorList.Values;
            comboBox3.DataContext = colorList.Values;
            comboBox4.DataContext = colorList.Values;
            comboBox5.DataContext = colorList.Values;
            comboBox6.DataContext = colorList.Values;
            comboBox7.DataContext = colorList.Values;
            comboBox8.DataContext = colorList.Values;
            comboBox9.DataContext = colorList.Values;
            comboBox10.DataContext = colorList.Values;
            comboBox11.DataContext = colorList.Values;
            comboBox12.DataContext = colorList.Values;
            comboBox13.DataContext = colorList.Values;
            comboBox_reserveNormal.DataContext = colorList.Values;
            comboBox_reserveNo.DataContext = colorList.Values;
            comboBox_reserveNoTuner.DataContext = colorList.Values;
            try
            {
                comboBox0.SelectedItem = colorList[Settings.Instance.ContentColorList[0x00]];
                comboBox1.SelectedItem = colorList[Settings.Instance.ContentColorList[0x01]];
                comboBox2.SelectedItem = colorList[Settings.Instance.ContentColorList[0x02]];
                comboBox3.SelectedItem = colorList[Settings.Instance.ContentColorList[0x03]];
                comboBox4.SelectedItem = colorList[Settings.Instance.ContentColorList[0x04]];
                comboBox5.SelectedItem = colorList[Settings.Instance.ContentColorList[0x05]];
                comboBox6.SelectedItem = colorList[Settings.Instance.ContentColorList[0x06]];
                comboBox7.SelectedItem = colorList[Settings.Instance.ContentColorList[0x07]];
                comboBox8.SelectedItem = colorList[Settings.Instance.ContentColorList[0x08]];
                comboBox9.SelectedItem = colorList[Settings.Instance.ContentColorList[0x09]];
                comboBox10.SelectedItem = colorList[Settings.Instance.ContentColorList[0x0A]];
                comboBox11.SelectedItem = colorList[Settings.Instance.ContentColorList[0x0B]];
                comboBox12.SelectedItem = colorList[Settings.Instance.ContentColorList[0x0F]];
                comboBox13.SelectedItem = colorList[Settings.Instance.ContentColorList[0x10]];

                comboBox_reserveNormal.SelectedItem = colorList[Settings.Instance.ReserveRectColorNormal];
                comboBox_reserveNo.SelectedItem = colorList[Settings.Instance.ReserveRectColorNo];
                comboBox_reserveNoTuner.SelectedItem = colorList[Settings.Instance.ReserveRectColorNoTuner];
                checkBox_reserveBackground.IsChecked = Settings.Instance.ReserveRectBackground;

                foreach (FontFamily family in Fonts.SystemFontFamilies)
                {
                    LanguageSpecificStringDictionary dictionary = family.FamilyNames;

                    XmlLanguage FLanguage = XmlLanguage.GetLanguage("ja-JP");
                    if (dictionary.ContainsKey(FLanguage) == true)
                    {
                        string s = dictionary[FLanguage] as string;
                        comboBox_font.Items.Add(s);
                        if (String.Compare(s, Settings.Instance.FontName) == 0)
                        {
                            comboBox_font.SelectedItem = s;
                        }
                    }
                }
                if (comboBox_font.SelectedItem == null)
                {
                    comboBox_font.SelectedIndex = 0;
                }
                textBox_fontSize.Text = Settings.Instance.FontSize.ToString();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public void SaveSetting()
        {
            Settings.Instance.ContentColorList[0x00] = ((ColorSelectionItem)(comboBox0.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x01] = ((ColorSelectionItem)(comboBox1.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x02] = ((ColorSelectionItem)(comboBox2.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x03] = ((ColorSelectionItem)(comboBox3.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x04] = ((ColorSelectionItem)(comboBox4.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x05] = ((ColorSelectionItem)(comboBox5.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x06] = ((ColorSelectionItem)(comboBox6.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x07] = ((ColorSelectionItem)(comboBox7.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x08] = ((ColorSelectionItem)(comboBox8.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x09] = ((ColorSelectionItem)(comboBox9.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x0A] = ((ColorSelectionItem)(comboBox10.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x0B] = ((ColorSelectionItem)(comboBox11.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x0F] = ((ColorSelectionItem)(comboBox12.SelectedItem)).ColorName;
            Settings.Instance.ContentColorList[0x10] = ((ColorSelectionItem)(comboBox13.SelectedItem)).ColorName;
            Settings.Instance.ReserveRectColorNormal = ((ColorSelectionItem)(comboBox_reserveNormal.SelectedItem)).ColorName;
            Settings.Instance.ReserveRectColorNo = ((ColorSelectionItem)(comboBox_reserveNo.SelectedItem)).ColorName;
            Settings.Instance.ReserveRectColorNoTuner = ((ColorSelectionItem)(comboBox_reserveNoTuner.SelectedItem)).ColorName;
            if (checkBox_reserveBackground.IsChecked == true)
            {
                Settings.Instance.ReserveRectBackground = true;
            }
            else
            {
                Settings.Instance.ReserveRectBackground = false;
            }
            if (comboBox_font.SelectedItem != null)
            {
                Settings.Instance.FontName = comboBox_font.SelectedItem as string;
            }
            Settings.Instance.FontSize = Convert.ToDouble(textBox_fontSize.Text);
        }
    }

    /// <summary>
    /// Define a item for chosing texty color 
    /// </summary>
    public class ColorSelectionItem
    {
        private SolidColorBrush color;
        private string colorName;

        public ColorSelectionItem(string ColorName, SolidColorBrush c)
        {
            color = c;
            colorName = ColorName;
        }
        /// <summary>
        /// return the name of a color
        /// </summary>
        /// <returns></returns>
        public string ColorName
        {
            get { return colorName; }
        }
        /// <summary>
        /// Return the color as a solid colorBrush
        /// </summary>
        public SolidColorBrush Color
        {
            get
            {
                return color;
            }
        }
    }
}
