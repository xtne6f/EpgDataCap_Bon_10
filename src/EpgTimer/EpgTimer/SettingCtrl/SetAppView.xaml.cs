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
using System.Diagnostics;

namespace EpgTimer
{
    /// <summary>
    /// SetAppView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetAppView : UserControl
    {
        private List<String> ngProcessList = new List<String>();
        private String ngMin = "10";
        public SetAppView()
        {
            InitializeComponent();

            StringBuilder buff = new StringBuilder(512);
            buff.Clear();
            int recEndMode = IniFileHandler.GetPrivateProfileInt("SET", "RecEndMode", 2, SettingPath.TimerSrvIniPath);
            switch (recEndMode)
            {
                case 0:
                    radioButton_none.IsChecked = true;
                    break;
                case 1:
                    radioButton_standby.IsChecked = true;
                    break;
                case 2:
                    radioButton_suspend.IsChecked = true;
                    break;
                case 3:
                    radioButton_shutdown.IsChecked = true;
                    break;
                default: 
                    break;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "Reboot", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_reboot.IsChecked = true;
            }
            else
            {
                checkBox_reboot.IsChecked = false;
            }
            textBox_pcWakeTime.Text = IniFileHandler.GetPrivateProfileInt("SET", "WakeTime", 5, SettingPath.TimerSrvIniPath).ToString();
            textBox_batWait.Text = IniFileHandler.GetPrivateProfileInt("SET", "BatMargin", 10, SettingPath.TimerSrvIniPath).ToString();

            textBox_megine_start.Text = IniFileHandler.GetPrivateProfileInt("SET", "StartMargin", 5, SettingPath.TimerSrvIniPath).ToString();
            textBox_margine_end.Text = IniFileHandler.GetPrivateProfileInt("SET", "EndMargin", 2, SettingPath.TimerSrvIniPath).ToString();
            textBox_appWakeTime.Text = IniFileHandler.GetPrivateProfileInt("SET", "RecAppWakeTime", 2, SettingPath.TimerSrvIniPath).ToString();

            if (IniFileHandler.GetPrivateProfileInt("SET", "RecMinWake", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_appMin.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "RecView", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_appView.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "DropLog", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_appDrop.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "PgInfoLog", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_addPgInfo.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "RecNW", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_appNW.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "RecOverWrite", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_appOverWrite.IsChecked = true;
            }

            int ngCount = IniFileHandler.GetPrivateProfileInt("NO_SUSPEND", "Count", 0, SettingPath.TimerSrvIniPath);
            if (ngCount == 0)
            {
                ngProcessList.Add("EpgDataCap_Bon.exe");
            }
            else
            {
                for (int i = 0; i < ngCount; i++)
                {
                    buff.Clear();
                    IniFileHandler.GetPrivateProfileString("NO_SUSPEND", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                    ngProcessList.Add(buff.ToString());
                }
            }
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("NO_SUSPEND", "NoStandbyTime", "10", buff, 512, SettingPath.TimerSrvIniPath);
            ngMin = buff.ToString();


            comboBox_process.Items.Add("リアルタイム");
            comboBox_process.Items.Add("高");
            comboBox_process.Items.Add("通常以上");
            comboBox_process.Items.Add("通常");
            comboBox_process.Items.Add("通常以下");
            comboBox_process.Items.Add("低");
            comboBox_process.SelectedIndex = IniFileHandler.GetPrivateProfileInt("SET", "ProcessPriority", 3, SettingPath.TimerSrvIniPath);
        }

        public void SaveSetting()
        {
            if (radioButton_none.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecEndMode", "0", SettingPath.TimerSrvIniPath);
            }
            if (radioButton_standby.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecEndMode", "1", SettingPath.TimerSrvIniPath);
            }
            if (radioButton_suspend.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecEndMode", "2", SettingPath.TimerSrvIniPath);
            }
            if (radioButton_shutdown.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecEndMode", "3", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_reboot.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "Reboot", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "Reboot", "0", SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("SET", "WakeTime", textBox_pcWakeTime.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "BatMargin", textBox_batWait.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "StartMargin", textBox_megine_start.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "EndMargin", textBox_margine_end.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "RecAppWakeTime", textBox_appWakeTime.Text, SettingPath.TimerSrvIniPath);

            if (checkBox_appMin.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecMinWake", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecMinWake", "0", SettingPath.TimerSrvIniPath);
            }

            if (checkBox_appView.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecView", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecView", "0", SettingPath.TimerSrvIniPath);
            }

            if (checkBox_appDrop.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "DropLog", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "DropLog", "0", SettingPath.TimerSrvIniPath);
            }

            if (checkBox_addPgInfo.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "PgInfoLog", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "PgInfoLog", "0", SettingPath.TimerSrvIniPath);
            }

            if (checkBox_appNW.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNW", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNW", "0", SettingPath.TimerSrvIniPath);
            }

            if (checkBox_appOverWrite.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecOverWrite", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecOverWrite", "0", SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("NO_SUSPEND", "Count", ngProcessList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < ngProcessList.Count; i++)
            {
                IniFileHandler.WritePrivateProfileString("NO_SUSPEND", i.ToString(), ngProcessList[i], SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("NO_SUSPEND", "NoStandbyTime", ngMin, SettingPath.TimerSrvIniPath);

            IniFileHandler.WritePrivateProfileString("SET", "ProcessPriority", comboBox_process.SelectedIndex.ToString(), SettingPath.TimerSrvIniPath);
        }

        private void button_standbyCtrl_Click(object sender, RoutedEventArgs e)
        {
            SetAppCancelWindow dlg = new SetAppCancelWindow();
            dlg.processList = this.ngProcessList;
            dlg.ngMin = this.ngMin;
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.ShowDialog();

            this.ngProcessList = dlg.processList;
            this.ngMin = dlg.ngMin;
        }
    }
}
