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

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// RecSettingView.xaml の相互作用ロジック
    /// </summary>
    public partial class RecSettingView : UserControl
    {
        private RecSettingData defKey = null;
        private List<TunerSelectInfo> tunerList = new List<TunerSelectInfo>();
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        
        public RecSettingView()
        {
            InitializeComponent();

            comboBox_recMode.DataContext = EpgTimerDef.Instance.RecModeDictionary.Values;
            comboBox_recMode.SelectedIndex = 1;
            comboBox_tuijyu.DataContext = EpgTimerDef.Instance.YesNoDictionary.Values;
            comboBox_tuijyu.SelectedIndex = 1;
            comboBox_pittari.DataContext = EpgTimerDef.Instance.YesNoDictionary.Values;
            comboBox_pittari.SelectedIndex = 0;
            comboBox_priority.DataContext = EpgTimerDef.Instance.PriorityDictionary.Values;
            comboBox_priority.SelectedIndex = 2;

            try
            {
                defKey = new RecSettingData();
                Settings.GetDefRecSetting(0, ref defKey);
                String plugInFile = "Write_Default.dll";
                String recNamePlugInFile = "";

                List<String> fileList = new List<string>();
                uint err = cmd.SendEnumPlugIn(2, ref fileList);
                if (err == 205)
                {
                    MessageBox.Show("サーバーに接続できませんでした");
                    return;
                }
                int select = 0;
                foreach (string info in fileList)
                {
                    int index = comboBox_writePlugIn.Items.Add(info);
                    if (String.Compare(info, plugInFile, true) == 0)
                    {
                        select = index;
                    }
                }
                if (comboBox_writePlugIn.Items.Count != 0)
                {
                    comboBox_writePlugIn.SelectedIndex = select;
                }

                fileList.Clear();
                err = cmd.SendEnumPlugIn(1, ref fileList);
                if (err == 205)
                {
                    MessageBox.Show("サーバーに接続できませんでした");
                    return;
                }
                select = 0;
                comboBox_recNamePlugIn.Items.Add("なし");
                foreach (string info in fileList)
                {
                    int index = comboBox_recNamePlugIn.Items.Add(info);
                    if (String.Compare(info, recNamePlugInFile, true) == 0)
                    {
                        select = index;
                    }
                }
                if (comboBox_recNamePlugIn.Items.Count != 0)
                {
                    comboBox_recNamePlugIn.SelectedIndex = select;
                }

                List<CtrlCmdCLI.Def.TunerReserveInfo> tunerReserveList = new List<CtrlCmdCLI.Def.TunerReserveInfo>();
                err = cmd.SendEnumTunerReserve(ref tunerReserveList);
                if (err == 205)
                {
                    MessageBox.Show("サーバーに接続できませんでした");
                    return;
                }
                tunerList.Add(new TunerSelectInfo("自動", 0));
                foreach (TunerReserveInfo info in tunerReserveList)
                {
                    if (info.tunerID != 0xFFFFFFFF)
                    {
                        tunerList.Add(new TunerSelectInfo(info.tunerName, info.tunerID));
                    }
                }
                comboBox_tuner.ItemsSource = tunerList;
                comboBox_tuner.SelectedIndex = 0;
            }
            catch
            {
                defKey = null;
            }
        }

        public void SetDefRecSetting(RecSettingData key)
        {
            defKey = key;
        }

        public bool GetRecSetting(ref RecSettingData key)
        {
            if (defKey != null)
            {
                //一度も画面開かれていない
                key = defKey;
                return true;
            }

            key.RecMode = ((RecModeInfo)comboBox_recMode.SelectedItem).Value;
            key.Priority = ((PriorityInfo)comboBox_priority.SelectedItem).Value;
            key.TuijyuuFlag = ((YesNoInfo)comboBox_tuijyu.SelectedItem).Value;
            if (checkBox_serviceMode.IsChecked == true)
            {
                key.ServiceMode = 0;
            }
            else
            {
                key.ServiceMode = 1;
                if (checkBox_serviceCaption.IsChecked == true)
                {
                    key.ServiceMode |= 0x10;
                }
                if (checkBox_serviceData.IsChecked == true)
                {
                    key.ServiceMode |= 0x20;
                }
            }
            key.PittariFlag = ((YesNoInfo)comboBox_pittari.SelectedItem).Value;
            key.BatFilePath = textBox_bat.Text;
            key.RecFolderList.Clear();
            foreach (RecFileSetInfoItem info in listBox_recFolder.Items)
            {
                key.RecFolderList.Add(info.Info);
            }
            if (checkBox_suspendDef.IsChecked == true)
            {
                key.SuspendMode = 0;
                key.RebootFlag = 0;
            }
            else
            {
                key.SuspendMode = 0;
                if (radioButton_standby.IsChecked == true)
                {
                    key.SuspendMode = 1;
                }
                else if (radioButton_supend.IsChecked == true)
                {
                    key.SuspendMode = 2;
                }
                else if (radioButton_shutdown.IsChecked == true)
                {
                    key.SuspendMode = 3;
                }
                else if (radioButton_non.IsChecked == true)
                {
                    key.SuspendMode = 4;
                }

                if (checkBox_reboot.IsChecked == true)
                {
                    key.RebootFlag = 1;
                }
                else
                {
                    key.RebootFlag = 0;
                }
            }
            if (checkBox_margineDef.IsChecked == true)
            {
                key.UseMargineFlag = 0;
            }
            else
            {
                key.UseMargineFlag = 1;
                if (textBox_margineStart.Text.Length == 0 || textBox_margineEnd.Text.Length == 0)
                {
                    MessageBox.Show("マージンが入力されていません");
                    return false;
                }
                key.StartMargine = Convert.ToInt32(textBox_margineStart.Text);
                key.EndMargine = Convert.ToInt32(textBox_margineEnd.Text);
            }
            if (checkBox_partial.IsChecked == true)
            {
                key.PartialRecFlag = 1;
            }
            else
            {
                key.PartialRecFlag = 0;
            }
            if (checkBox_continueRec.IsChecked == true)
            {
                key.ContinueRecFlag = 1;
            }
            else
            {
                key.ContinueRecFlag = 0;
            }

            TunerSelectInfo tuner = comboBox_tuner.SelectedItem as TunerSelectInfo;
            key.TunerID = tuner.ID;

            return true;
        }

        private void button_recFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "フォルダ選択";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox_recFolder.Text = dlg.SelectedPath;
            }
        }

        private void button_recFolder_add_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textBox_recFolder.Text) == false)
            {
                foreach (RecFileSetInfoItem info in listBox_recFolder.Items)
                {
                    if (String.Compare(textBox_recFolder.Text, info.Info.RecFolder, true) == 0 &&
                        String.Compare((String)comboBox_writePlugIn.SelectedItem, info.Info.WritePlugIn, true) == 0 &&
                        String.Compare((String)comboBox_recNamePlugIn.SelectedItem, info.Info.RecNamePlugIn, true) == 0)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                RecFileSetInfo item = new RecFileSetInfo();
                item.RecFolder = textBox_recFolder.Text;
                item.WritePlugIn = (String)comboBox_writePlugIn.SelectedItem;
                item.RecNamePlugIn = (String)comboBox_recNamePlugIn.SelectedItem;
                if (String.Compare(item.RecNamePlugIn, "なし", true) == 0)
                {
                    item.RecNamePlugIn = "";
                }

                listBox_recFolder.Items.Add(new RecFileSetInfoItem(item));
            }
        }

        private void button_recFolder_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_recFolder.SelectedItem != null)
            {
                listBox_recFolder.Items.RemoveAt(listBox_recFolder.SelectedIndex);
            }
        }

        private void button_bat_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bat";
            dlg.Filter = "bat Files (.bat)|*.bat;|all Files(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                textBox_bat.Text = dlg.FileName;
            }
        }

        private void radioButton_suspend_Click(object sender, RoutedEventArgs e)
        {
            checkBox_suspendDef.IsChecked = false;
        }

        private void checkBox_suspendDef_Click(object sender, RoutedEventArgs e)
        {
            checkBox_reboot.IsChecked = false;
        }

        private void checkBox_serviceCaption_Checked(object sender, RoutedEventArgs e)
        {
            checkBox_serviceMode.IsChecked = false;
        }

        private void checkBox_serviceMode_Checked(object sender, RoutedEventArgs e)
        {
            checkBox_serviceCaption.IsChecked = false;
            checkBox_serviceData.IsChecked = false;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefKey();
            defKey = null;
        }

        private void LoadDefKey()
        {
            if (defKey != null)
            {
                foreach (RecModeInfo info in comboBox_recMode.Items)
                {
                    if (info.Value == defKey.RecMode)
                    {
                        comboBox_recMode.SelectedItem = info;
                    }
                }
                foreach (PriorityInfo info in comboBox_priority.Items)
                {
                    if (info.Value == defKey.Priority)
                    {
                        comboBox_priority.SelectedItem = info;
                    }
                }
                foreach (YesNoInfo info in comboBox_tuijyu.Items)
                {
                    if (info.Value == defKey.TuijyuuFlag)
                    {
                        comboBox_tuijyu.SelectedItem = info;
                    }
                }

                if (defKey.ServiceMode == 0)
                {
                    checkBox_serviceMode.IsChecked = true;
                }
                else
                {
                    checkBox_serviceMode.IsChecked = false;
                    if ((defKey.ServiceMode & 0x10) > 0)
                    {
                        checkBox_serviceCaption.IsChecked = true;
                    }
                    else
                    {
                        checkBox_serviceCaption.IsChecked = false;
                    }
                    if ((defKey.ServiceMode & 0x20) > 0)
                    {
                        checkBox_serviceData.IsChecked = true;
                    }
                    else
                    {
                        checkBox_serviceData.IsChecked = false;
                    }
                }

                foreach (YesNoInfo info in comboBox_pittari.Items)
                {
                    if (info.Value == defKey.PittariFlag)
                    {
                        comboBox_pittari.SelectedItem = info;
                    }
                }


                textBox_bat.Text = defKey.BatFilePath;
                foreach (RecFileSetInfo info in defKey.RecFolderList)
                {
                    listBox_recFolder.Items.Add(new RecFileSetInfoItem(info));
                }

                if (defKey.SuspendMode == 0)
                {
                    checkBox_suspendDef.IsChecked = true;
                    checkBox_reboot.IsChecked = false;
                }
                else
                {
                    checkBox_suspendDef.IsChecked = false;

                    if (defKey.SuspendMode == 1)
                    {
                        radioButton_standby.IsChecked = true;
                    }
                    if (defKey.SuspendMode == 2)
                    {
                        radioButton_supend.IsChecked = true;
                    }
                    if (defKey.SuspendMode == 3)
                    {
                        radioButton_shutdown.IsChecked = true;
                    }
                    if (defKey.SuspendMode == 4)
                    {
                        radioButton_non.IsChecked = true;
                    }
                    if (defKey.RebootFlag == 1)
                    {
                        checkBox_reboot.IsChecked = true;
                    }
                    else
                    {
                        checkBox_reboot.IsChecked = false;
                    }
                }
                if (defKey.UseMargineFlag == 0)
                {
                    checkBox_margineDef.IsChecked = true;
                }
                else
                {
                    checkBox_margineDef.IsChecked = false;
                    textBox_margineStart.Text = defKey.StartMargine.ToString();
                    textBox_margineEnd.Text = defKey.EndMargine.ToString();
                }

                if (defKey.ContinueRecFlag == 1)
                {
                    checkBox_continueRec.IsChecked = true;
                }
                else
                {
                    checkBox_continueRec.IsChecked = false;
                }
                if (defKey.PartialRecFlag == 1)
                {
                    checkBox_partial.IsChecked = true;
                }
                else
                {
                    checkBox_partial.IsChecked = false;
                }


                foreach (TunerSelectInfo info in comboBox_tuner.Items)
                {
                    if (info.ID == defKey.TunerID)
                    {
                        comboBox_tuner.SelectedItem = info;
                        break;
                    }
                }
            }
        }

        private void button_pluginSet_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (comboBox_writePlugIn.SelectedItem != null)
            {
                string name = comboBox_writePlugIn.SelectedItem as string;
                string filePath = SettingPath.ModulePath + "\\Write\\" + name;

                WritePlugInClass plugin = new WritePlugInClass();
                HwndSource hwnd = (HwndSource)HwndSource.FromVisual(this);

                plugin.Setting(filePath, hwnd.Handle);
            }
            */
        }

        private void button_recNamePlugIn_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (comboBox_recNamePlugIn.SelectedItem != null)
            {
                string name = comboBox_recNamePlugIn.SelectedItem as string;
                if (String.Compare(name, "なし", true) != 0)
                {
                    string filePath = SettingPath.ModulePath + "\\RecName\\" + name;

                    RecNamePluginClass plugin = new RecNamePluginClass();
                    HwndSource hwnd = (HwndSource)HwndSource.FromVisual(this);

                    plugin.Setting(filePath, hwnd.Handle);
                }
            }
            */
        }
    }

    public class RecFileSetInfoItem
    {
        public RecFileSetInfoItem(RecFileSetInfo folderInfo)
        {
            Info = folderInfo;
        }

        public RecFileSetInfo Info
        {
            get;
            set;
        }
        public override string ToString()
        {
            String view = "";
            if (Info != null)
            {
                view = Info.RecFolder + " (WritePlugIn:" + Info.WritePlugIn + " RecNamePlugIn:" + Info.RecNamePlugIn + ")";
            }
            return view;
        }
    }

    public class TunerSelectInfo
    {
        public TunerSelectInfo(String name, UInt32 id)
        {
            Name = name;
            ID = id;
        }
        public String Name
        {
            get;
            set;
        }
        public UInt32 ID
        {
            get;
            set;
        }
        public override string ToString()
        {
            String view = "";
            if (ID == 0)
            {
                view = "自動";
            }
            else
            {
                view = "ID:" + ID.ToString("X8") + " (" + Name + ")";
            }
            return view;
        }
    }

}

