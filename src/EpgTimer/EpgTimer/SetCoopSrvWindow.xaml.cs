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
    /// SetCoopSrvWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SetCoopSrvWindow : Window
    {
        public SetCoopSrvWindow()
        {
            InitializeComponent();

            textBox_port.Text = "4510";
        }

        public void SetDefSetting(String ip, UInt32 port)
        {
            textBox_ip.Text = ip;
            textBox_port.Text = port.ToString();
        }

        public void GetSetting(ref String ip, ref UInt32 port)
        {
            try
            {
                ip = textBox_ip.Text;
                port = Convert.ToUInt32(textBox_port.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_ip.Text.Length == 0)
            {
                MessageBox.Show("アドレスが入力されていません");
                return;
            }
            if (textBox_port.Text.Length == 0)
            {
                MessageBox.Show("ポートが入力されていません");
                return;
            }
            DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
