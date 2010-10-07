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
using System.IO;

namespace EpgTimer
{
    /// <summary>
    /// SetTVTestView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetTVTestView : UserControl
    {
        public SetTVTestView()
        {
            InitializeComponent();

            try
            {
                textBox_exe.Text = Settings.Instance.TvTestExe;
                textBox_cmd.Text = Settings.Instance.TvTestCmd;

                string[] files = Directory.GetFiles(SettingPath.SettingFolderPath, "*.ChSet4.txt");
                SortedList<Int32, TunerInfo> tunerInfo = new SortedList<Int32, TunerInfo>();
                foreach (string info in files)
                {
                    try
                    {
                        String bonName = "";
                        String fileName = System.IO.Path.GetFileName(info);
                        bonName = GetBonFileName(fileName);
                        bonName += ".dll";
                        comboBox_bon.Items.Add(bonName);
                    }
                    catch
                    {
                    }
                }
                if (comboBox_bon.Items.Count > 0)
                {
                    comboBox_bon.SelectedIndex = 0;
                }
            }
            catch
            {
            }

            StringBuilder buff = new StringBuilder(512);
            buff.Clear();

            int num = IniFileHandler.GetPrivateProfileInt("TVTEST", "Num", 0, SettingPath.TimerSrvIniPath);
            for (uint i = 0; i < num; i++)
            {
                buff.Clear();
                IniFileHandler.GetPrivateProfileString("TVTEST", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                if (buff.Length > 0)
                {
                    listBox_bon.Items.Add(buff.ToString());
                }
            }

        }

        private String GetBonFileName(String src)
        {
            int pos = src.LastIndexOf(")");
            if (pos < 1)
            {
                return src;
            }

            int count = 1;
            for (int i = pos - 1; i >= 0; i--)
            {
                if (src[i] == '(')
                {
                    count--;
                }
                else if (src[i] == ')')
                {
                    count++;
                }
                if (count == 0)
                {
                    return src.Substring(0, i);
                }
            }
            return src;
        }

        public void SaveSetting()
        {
            Settings.Instance.TvTestExe = textBox_exe.Text;
            Settings.Instance.TvTestCmd = textBox_cmd.Text;

            IniFileHandler.WritePrivateProfileString("TVTEST", "Num", listBox_bon.Items.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < listBox_bon.Items.Count; i++)
            {
                string val = listBox_bon.Items[i] as string;
                IniFileHandler.WritePrivateProfileString("TVTEST", i.ToString(), val, SettingPath.TimerSrvIniPath);
            }
        }

        private void button_exe_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "exe Files (.exe)|*.exe;|all Files(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                textBox_exe.Text = dlg.FileName;
            }
        }

        private void button_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_bon.SelectedItem != null)
            {
                listBox_bon.Items.RemoveAt(listBox_bon.SelectedIndex);
            }
        }

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(comboBox_bon.Text) == false)
            {
                foreach (String info in listBox_bon.Items)
                {
                    if (String.Compare(comboBox_bon.Text, info, true) == 0)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                listBox_bon.Items.Add(comboBox_bon.Text);
            }
        }
    }
}
