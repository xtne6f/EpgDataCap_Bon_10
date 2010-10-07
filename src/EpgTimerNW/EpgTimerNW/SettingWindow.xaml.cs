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
using System.Windows.Shapes;

namespace EpgTimer
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public bool ServiceStop = false;
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            ChSet5.LoadFile();
            setApp2View.SaveSetting();
            setApp3View.SaveSetting(); 
            setEpgColor.SaveSetting();
            setEpgServiceView.SaveSetting();
            setIEPG1View.SaveSetting();
            Settings.SaveToXmlFile();
            ChSet5.SaveFile();

            this.DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
