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
using System.Windows.Interop;

using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// SetApp2View.xaml の相互作用ロジック
    /// </summary>
    public partial class SetApp2View : UserControl
    {
        private List<String> extList = new List<string>();
        private List<String> delChkFolderList = new List<string>();
        private RecSettingData recSet = new RecSettingData();
        private SearchKeyInfo searchSet = new SearchKeyInfo();
        public SetApp2View()
        {
            InitializeComponent();
            /*
            if (IniFileHandler.GetPrivateProfileInt("SET", "BackPriority", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_back_priority.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "EventRelay", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_enable_relay.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "ResAutoChgTitle", 1, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_chgTitle.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "ResAutoChkTime", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_chk_TimeOnly.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "AutoDel", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_autoDel.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "RecNamePlugIn", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_recname.IsChecked = true;
            }
            
            StringBuilder buff = new StringBuilder(512);
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("SET", "RecNamePlugInFile", "RecName_Macro.dll", buff, 512, SettingPath.TimerSrvIniPath);
            String plugInFile = buff.ToString();
            */
            try
            {
                checkBox_closeMin.IsChecked = Settings.Instance.CloseMin;
                checkBox_minWake.IsChecked = Settings.Instance.WakeMin;
            }
            catch
            {
            }
            /*
            int count = IniFileHandler.GetPrivateProfileInt("DEL_EXT", "Count", 0, SettingPath.TimerSrvIniPath);
            if (count == 0)
            {
                extList.Add(".err");
                extList.Add(".program.txt");
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    buff.Clear();
                    IniFileHandler.GetPrivateProfileString("DEL_EXT", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                    extList.Add(buff.ToString());
                }
            }

            count = IniFileHandler.GetPrivateProfileInt("DEL_CHK", "Count", 0, SettingPath.TimerSrvIniPath);
            for (int i = 0; i < count; i++)
            {
                buff.Clear();
                IniFileHandler.GetPrivateProfileString("DEL_CHK", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                delChkFolderList.Add(buff.ToString());
            }

            
            try
            {
                string[] files = Directory.GetFiles(SettingPath.ModulePath + "\\RecName", "RecName*.dll");
                int select = 0;
                foreach (string info in files)
                {
                    int index = comboBox_recname.Items.Add(System.IO.Path.GetFileName(info));
                    if (String.Compare(System.IO.Path.GetFileName(info), plugInFile, true) == 0)
                    {
                        select = index;
                    }
                }
                if (comboBox_recname.Items.Count != 0)
                {
                    comboBox_recname.SelectedIndex = select;
                }
            }
            catch
            {
            }
            */
            recSet.RecMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RecMode", 1, SettingPath.TimerSrvIniPath);
            recSet.Priority = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "Priority", 2, SettingPath.TimerSrvIniPath);
            recSet.TuijyuuFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "TuijyuuFlag", 1, SettingPath.TimerSrvIniPath);
            recSet.ServiceMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "ServiceMode", 0, SettingPath.TimerSrvIniPath);
            recSet.PittariFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "PittariFlag", 0, SettingPath.TimerSrvIniPath);

            StringBuilder buff = new StringBuilder(512);
            buff.Clear();
            IniFileHandler.GetPrivateProfileString("REC_DEF", "BatFilePath", "", buff, 512, SettingPath.TimerSrvIniPath);
            recSet.BatFilePath = buff.ToString();

            int count = IniFileHandler.GetPrivateProfileInt("REC_DEF_FOLDER", "Count", 0, SettingPath.TimerSrvIniPath);
            for (int i = 0; i < count; i++)
            {
                RecFileSetInfo folderInfo = new RecFileSetInfo();
                buff.Clear();
                IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                folderInfo.RecFolder = buff.ToString();
                IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", "WritePlugIn" + i.ToString(), "Write_Default.dll", buff, 512, SettingPath.TimerSrvIniPath);
                folderInfo.WritePlugIn = buff.ToString();

                recSet.RecFolderList.Add(folderInfo);
            }

            recSet.SuspendMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "SuspendMode", 0, SettingPath.TimerSrvIniPath);
            recSet.RebootFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RebootFlag", 0, SettingPath.TimerSrvIniPath);
            recSet.UseMargineFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "UseMargineFlag", 0, SettingPath.TimerSrvIniPath);
            recSet.StartMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "StartMargine", 0, SettingPath.TimerSrvIniPath);
            recSet.EndMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "EndMargine", 0, SettingPath.TimerSrvIniPath);
            /*
            if (IniFileHandler.GetPrivateProfileInt("SET", "EnableTCPSrv", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_tcpServer.IsChecked = true;
            }
            textBox_tcpPort.Text = IniFileHandler.GetPrivateProfileInt("SET", "TCPPort", 4510, SettingPath.TimerSrvIniPath).ToString();
            */

            try
            {
                searchSet.RegExp = Settings.Instance.SearchKeyRegExp;
                searchSet.AimaiFlag = Settings.Instance.SearchKeyAimaiFlag;
                searchSet.TitleOnly = Settings.Instance.SearchKeyTitleOnly;
                searchSet.ContentList = Settings.Instance.SearchKeyContentList;
                searchSet.DateItemList = Settings.Instance.SearchKeyDateItemList;
                searchSet.ServiceList = new List<long>();
                foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                {
                    if (info.SearchFlag == 1)
                    {
                        searchSet.ServiceList.Add((long)info.Key);
                    }
                }

                checkBox_noToolTips.IsChecked = Settings.Instance.NoToolTip;
            }
            catch
            {
            }
        }

        public void SaveSetting()
        {
            /*
            if (checkBox_back_priority.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "BackPriority", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "BackPriority", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_enable_relay.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "EventRelay", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "EventRelay", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_chgTitle.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "ResAutoChgTitle", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "ResAutoChgTitle", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_chk_TimeOnly.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "ResAutoChkTime", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "ResAutoChkTime", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_autoDel.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "AutoDel", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "AutoDel", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_recname.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNamePlugIn", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNamePlugIn", "0", SettingPath.TimerSrvIniPath);
            }
            if (comboBox_recname.SelectedItem != null)
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNamePlugInFile", (string)comboBox_recname.SelectedItem, SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "RecNamePlugInFile", "", SettingPath.TimerSrvIniPath);
            }
            */
            Settings.Instance.CloseMin = (bool)checkBox_closeMin.IsChecked;
            Settings.Instance.WakeMin = (bool)checkBox_minWake.IsChecked;
            /*
            IniFileHandler.WritePrivateProfileString("DEL_EXT", "Count", extList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < extList.Count; i++)
            {
                IniFileHandler.WritePrivateProfileString("DEL_EXT", i.ToString(), extList[i], SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("DEL_CHK", "Count", delChkFolderList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < delChkFolderList.Count; i++)
            {
                IniFileHandler.WritePrivateProfileString("DEL_CHK", i.ToString(), delChkFolderList[i], SettingPath.TimerSrvIniPath);
            }
            */
            IniFileHandler.WritePrivateProfileString("REC_DEF", "RecMode", recSet.RecMode.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "Priority", recSet.Priority.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "TuijyuuFlag", recSet.TuijyuuFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "ServiceMode", recSet.ServiceMode.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "PittariFlag", recSet.PittariFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "BatFilePath", recSet.BatFilePath, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "RecMode", recSet.RecMode.ToString(), SettingPath.TimerSrvIniPath);

            IniFileHandler.WritePrivateProfileString("REC_DEF_FOLDER", "Count", recSet.RecFolderList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < recSet.RecFolderList.Count; i++)
            {
                IniFileHandler.WritePrivateProfileString("REC_DEF_FOLDER", i.ToString(), recSet.RecFolderList[i].RecFolder, SettingPath.TimerSrvIniPath);
                IniFileHandler.WritePrivateProfileString("REC_DEF_FOLDER", "WritePlugIn" + i.ToString(), recSet.RecFolderList[i].WritePlugIn, SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("REC_DEF", "SuspendMode", recSet.SuspendMode.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "RebootFlag", recSet.RebootFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "UseMargineFlag", recSet.UseMargineFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "StartMargine", recSet.StartMargine.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "EndMargine", recSet.EndMargine.ToString(), SettingPath.TimerSrvIniPath);

            Settings.Instance.SearchKeyRegExp = searchSet.RegExp;
            Settings.Instance.SearchKeyAimaiFlag = searchSet.AimaiFlag;
            Settings.Instance.SearchKeyTitleOnly = searchSet.TitleOnly;
            Settings.Instance.SearchKeyContentList = searchSet.ContentList;
            Settings.Instance.SearchKeyDateItemList = searchSet.DateItemList;
            foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
            {
                info.SearchFlag = 0;
            }
            foreach (Int64 info in searchSet.ServiceList)
            {
                try
                {
                    if (ChSet5.Instance.ChList.ContainsKey((ulong)info) == true)
                    {
                        ChSet5.Instance.ChList[(ulong)info].SearchFlag = 1;
                    }
                }
                catch
                {
                }
            }
            /*
            if (checkBox_tcpServer.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "EnableTCPSrv", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "EnableTCPSrv", "0", SettingPath.TimerSrvIniPath);
            }
            IniFileHandler.WritePrivateProfileString("SET", "TCPPort", textBox_tcpPort.Text, SettingPath.TimerSrvIniPath);
            */

            if (checkBox_noToolTips.IsChecked == true)
            {
                Settings.Instance.NoToolTip = true;
            }
            else
            {
                Settings.Instance.NoToolTip = false;
            }
        }

        private void button_recname_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (comboBox_recname.SelectedItem != null)
            {
                string name = comboBox_recname.SelectedItem as string;
                string filePath = SettingPath.ModulePath + "\\RecName\\" + name;

                RecNamePluginClass plugin = new RecNamePluginClass();
                HwndSource hwnd = (HwndSource)HwndSource.FromVisual(this);

                plugin.Setting(filePath, hwnd.Handle);
            }
            */
        }

        private void button_autoDel_Click(object sender, RoutedEventArgs e)
        {
            /*
            SetApp2DelWindow dlg = new SetApp2DelWindow();
            dlg.extList = this.extList;
            dlg.delChkFolderList = this.delChkFolderList;
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.ShowDialog();

            this.extList = dlg.extList;
            this.delChkFolderList = dlg.delChkFolderList;
            */
        }

        private void button_recDef_Click(object sender, RoutedEventArgs e)
        {
            SetApp2DefRecWindow dlg = new SetApp2DefRecWindow();
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.recSet = this.recSet;
            dlg.ShowDialog();
            this.recSet = dlg.recSet;
        }

        private void button_searchDef_Click(object sender, RoutedEventArgs e)
        {
            SetApp2DefSearchWindow dlg = new SetApp2DefSearchWindow();
            dlg.searchSet = this.searchSet;
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.ShowDialog();
            this.searchSet = dlg.searchSet;
        }
    }
}
