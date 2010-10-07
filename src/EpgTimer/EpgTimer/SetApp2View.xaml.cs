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
        private  List<String> extList = new List<string>();
        private RecSettingData recSet = new RecSettingData();
        private SearchKeyInfo searchSet = new SearchKeyInfo();
        public SetApp2View()
        {
            InitializeComponent();

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

            if (IniFileHandler.GetPrivateProfileInt("SET", "CloseMin", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_closeMin.IsChecked = true;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "IniMin", 0, SettingPath.TimerSrvIniPath) == 1)
            {
                checkBox_minWake.IsChecked = true;
            }

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

            recSet.RecMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RecMode", 1, SettingPath.TimerSrvIniPath);
            recSet.Priority = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "Priority", 2, SettingPath.TimerSrvIniPath);
            recSet.TuijyuuFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "TuijyuuFlag", 1, SettingPath.TimerSrvIniPath);
            recSet.ServiceMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "ServiceMode", 0, SettingPath.TimerSrvIniPath);
            recSet.PittariFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "PittariFlag", 0, SettingPath.TimerSrvIniPath);

            buff.Clear();
            IniFileHandler.GetPrivateProfileString("REC_DEF", "BatFilePath", "", buff, 512, SettingPath.TimerSrvIniPath);
            recSet.BatFilePath = buff.ToString();

            count = IniFileHandler.GetPrivateProfileInt("REC_DEF_FOLDER", "Count", 0, SettingPath.TimerSrvIniPath);
            for (int i = 0; i < count; i++)
            {
                buff.Clear();
                IniFileHandler.GetPrivateProfileString("REC_DEF_FOLDER", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                recSet.RecFolderList.Add(buff.ToString());
            }

            recSet.SuspendMode = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "SuspendMode", 0, SettingPath.TimerSrvIniPath);
            recSet.RebootFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "RebootFlag", 0, SettingPath.TimerSrvIniPath);
            recSet.UseMargineFlag = (Byte)IniFileHandler.GetPrivateProfileInt("REC_DEF", "UseMargineFlag", 0, SettingPath.TimerSrvIniPath);
            recSet.StartMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "StartMargine", 0, SettingPath.TimerSrvIniPath);
            recSet.EndMargine = IniFileHandler.GetPrivateProfileInt("REC_DEF", "EndMargine", 0, SettingPath.TimerSrvIniPath);


            searchSet.RegExp = Settings.Instance.SearchKeyRegExp;
            searchSet.TitleOnly = Settings.Instance.SearchKeyTitleOnly;
            searchSet.ContentList = Settings.Instance.SearchKeyContentList;
            searchSet.DateItemList = Settings.Instance.SearchKeyDateItemList;
            searchSet.ChList = ChSet5.Instance.ChList.Values.ToList();
        }

        public void SaveSetting()
        {
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
            if (checkBox_closeMin.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "CloseMin", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "CloseMin", "0", SettingPath.TimerSrvIniPath);
            }
            if (checkBox_minWake.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "IniMin", "1", SettingPath.TimerSrvIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "IniMin", "0", SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("DEL_EXT", "Count", extList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < extList.Count; i++)
            {
                IniFileHandler.WritePrivateProfileString("DEL_EXT", i.ToString(), extList[i], SettingPath.TimerSrvIniPath);
            }

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
                IniFileHandler.WritePrivateProfileString("REC_DEF_FOLDER", i.ToString(), recSet.RecFolderList[i], SettingPath.TimerSrvIniPath);
            }

            IniFileHandler.WritePrivateProfileString("REC_DEF", "SuspendMode", recSet.SuspendMode.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "RebootFlag", recSet.RebootFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "UseMargineFlag", recSet.UseMargineFlag.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "StartMargine", recSet.StartMargine.ToString(), SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("REC_DEF", "EndMargine", recSet.EndMargine.ToString(), SettingPath.TimerSrvIniPath);

            Settings.Instance.SearchKeyRegExp = searchSet.RegExp;
            Settings.Instance.SearchKeyTitleOnly = searchSet.TitleOnly;
            Settings.Instance.SearchKeyContentList = searchSet.ContentList;
            Settings.Instance.SearchKeyDateItemList = searchSet.DateItemList;
            foreach (ChSet5Item info in searchSet.ChList)
            {
                try
                {
                    UInt64 key = ((UInt64)info.ONID) << 32 | ((UInt64)info.TSID) << 16 | ((UInt64)info.SID);
                    ChSet5.Instance.ChList[key].SearchFlag = info.SearchFlag;
                }
                catch
                {
                }
            }
        }

        private void button_recname_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_recname.SelectedItem != null)
            {
                string name = comboBox_recname.SelectedItem as string;
                string filePath = SettingPath.ModulePath + "\\RecName\\" + name;

                RecNamePluginClass plugin = new RecNamePluginClass();
                HwndSource hwnd = (HwndSource)HwndSource.FromVisual(this);

                plugin.Setting(filePath, hwnd.Handle);
            }
        }

        private void button_autoDel_Click(object sender, RoutedEventArgs e)
        {
            SetApp2DelWindow dlg = new SetApp2DelWindow();
            dlg.extList = this.extList;
            dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
            dlg.ShowDialog();

            this.extList = dlg.extList;
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
