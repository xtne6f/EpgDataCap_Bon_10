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
    /// SetTunerView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetTunerView : UserControl
    {
        public SetTunerView()
        {
            InitializeComponent();

            try
            {
                string[] files = Directory.GetFiles(SettingPath.SettingFolderPath, "*.ChSet4.txt");
                SortedList<Int32, TunerInfo> tunerInfo = new SortedList<Int32, TunerInfo>();
                foreach (string info in files)
                {
                    try
                    {
                        TunerInfo item = new TunerInfo();
                        String fileName = System.IO.Path.GetFileName(info);
                        item.BonDriver = GetBonFileName(fileName);
                        item.BonDriver += ".dll";
                        item.TunerNum = IniFileHandler.GetPrivateProfileInt(item.BonDriver, "Count", 0, SettingPath.TimerSrvIniPath).ToString();
                        if (IniFileHandler.GetPrivateProfileInt(item.BonDriver, "GetEpg", 1, SettingPath.TimerSrvIniPath) == 0)
                        {
                            item.IsEpgCap = false;
                        }
                        else
                        {
                            item.IsEpgCap = true;
                        }
                        int priority = IniFileHandler.GetPrivateProfileInt(item.BonDriver, "Priority", 0xFFFF, SettingPath.TimerSrvIniPath);
                        while (true)
                        {
                            if (tunerInfo.ContainsKey(priority) == true)
                            {
                                priority++;
                            }
                            else
                            {
                                tunerInfo.Add(priority, item);
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                foreach (TunerInfo info in tunerInfo.Values)
                {
                    listBox_bon.Items.Add(info);
                }
                if (listBox_bon.Items.Count > 0)
                {
                    listBox_bon.SelectedIndex = 0;
                }
            }
            catch
            {
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
            for (int i = 0; i < listBox_bon.Items.Count; i++)
            {
                TunerInfo info = listBox_bon.Items[i] as TunerInfo;

                IniFileHandler.WritePrivateProfileString(info.BonDriver, "Count", info.TunerNum, SettingPath.TimerSrvIniPath);
                if (info.IsEpgCap == true)
                {
                    IniFileHandler.WritePrivateProfileString(info.BonDriver, "GetEpg", "1", SettingPath.TimerSrvIniPath);
                }
                else
                {
                    IniFileHandler.WritePrivateProfileString(info.BonDriver, "GetEpg", "0", SettingPath.TimerSrvIniPath);
                }
                IniFileHandler.WritePrivateProfileString(info.BonDriver, "Priority", i.ToString(), SettingPath.TimerSrvIniPath);
            }
        }

        private void button_bon_up_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_bon.SelectedItem != null)
            {
                if (listBox_bon.SelectedIndex >= 1)
                {
                    object temp = listBox_bon.SelectedItem;
                    int index = listBox_bon.SelectedIndex;
                    listBox_bon.Items.RemoveAt(listBox_bon.SelectedIndex);
                    listBox_bon.Items.Insert(index - 1, temp);
                    listBox_bon.SelectedIndex = index - 1;
                }
            }
        }

        private void button_bon_down_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_bon.SelectedItem != null)
            {
                if (listBox_bon.SelectedIndex < listBox_bon.Items.Count - 1)
                {
                    object temp = listBox_bon.SelectedItem;
                    int index = listBox_bon.SelectedIndex;
                    listBox_bon.Items.RemoveAt(listBox_bon.SelectedIndex);
                    listBox_bon.Items.Insert(index + 1, temp);
                    listBox_bon.SelectedIndex = index + 1;
                }
            }
        }

        private void listBox_bon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_bon.SelectedItem != null)
            {
                TunerInfo info = listBox_bon.SelectedItem as TunerInfo;
                textBox_bon_num.DataContext = info;
                checkBox_bon_epg.DataContext = info;
            }
        }
    }

    class TunerInfo
    {
        public String BonDriver
        {
            get;
            set;
        }
        public String TunerNum
        {
            get;
            set;
        }
        public bool IsEpgCap
        {
            get;
            set;
        }
        public override string ToString()
        {
            return BonDriver;
        }
    }
}
