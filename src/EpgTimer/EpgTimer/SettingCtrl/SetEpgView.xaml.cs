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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace EpgTimer
{
    /// <summary>
    /// SetEpgView.xaml の相互作用ロジック
    /// </summary>
    public partial class SetEpgView : UserControl
    {
        private List<ServiceItem2> serviceList;
        private ObservableCollection<EpgCaptime> timeList;
        public SetEpgView()
        {
            InitializeComponent();

            comboBox_HH.DataContext = EpgTimerDef.Instance.HourDictionary.Values;
            comboBox_HH.SelectedIndex = 0;
            comboBox_MM.DataContext = EpgTimerDef.Instance.MinDictionary.Values;
            comboBox_MM.SelectedIndex = 0;

            StringBuilder buff = new StringBuilder(512);
            buff.Clear();

            serviceList = new List<ServiceItem2>();
            try
            {
                foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                {
                    ServiceItem2 item = new ServiceItem2();
                    item.ServiceInfo = info;
                    if (info.EpgCapFlag == 1)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                    serviceList.Add(item);
                }
            }
            catch
            {
            }
            listView_service.DataContext = serviceList;

            if (IniFileHandler.GetPrivateProfileInt("SET", "BSBasicOnly", 1, SettingPath.CommonIniPath) == 1)
            {
                checkBox_bs.IsChecked = true;
            }
            else
            {
                checkBox_bs.IsChecked = false;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "CS1BasicOnly", 1, SettingPath.CommonIniPath) == 1)
            {
                checkBox_cs1.IsChecked = true;
            }
            else
            {
                checkBox_cs1.IsChecked = false;
            }
            if (IniFileHandler.GetPrivateProfileInt("SET", "CS2BasicOnly", 1, SettingPath.CommonIniPath) == 1)
            {
                checkBox_cs2.IsChecked = true;
            }
            else
            {
                checkBox_cs2.IsChecked = false;
            }


            timeList = new ObservableCollection<EpgCaptime>();
            int capCount = IniFileHandler.GetPrivateProfileInt("EPG_CAP", "Count", 0, SettingPath.TimerSrvIniPath);
            if (capCount == 0)
            {
                EpgCaptime item = new EpgCaptime();
                item.IsSelected = true;
                item.Time = "23:00";
                timeList.Add(item);
            }
            else
            {
                for (int i = 0; i < capCount; i++)
                {
                    buff.Clear();
                    EpgCaptime item = new EpgCaptime();
                    IniFileHandler.GetPrivateProfileString("EPG_CAP", i.ToString(), "", buff, 512, SettingPath.TimerSrvIniPath);
                    item.Time = buff.ToString();
                    if (IniFileHandler.GetPrivateProfileInt("EPG_CAP", i.ToString() + "Select", 0, SettingPath.TimerSrvIniPath) == 1)
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                    timeList.Add(item);
                }
            }
            ListView_time.DataContext = timeList;

            textBox_ngCapMin.Text = IniFileHandler.GetPrivateProfileInt("SET", "NGEpgCapTime", 20, SettingPath.TimerSrvIniPath).ToString();
            textBox_ngTunerMin.Text = IniFileHandler.GetPrivateProfileInt("SET", "NGEpgCapTunerTime", 20, SettingPath.TimerSrvIniPath).ToString();
        }

        public void SaveSetting()
        {
            if (checkBox_bs.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "BSBasicOnly", "1", SettingPath.CommonIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "BSBasicOnly", "0", SettingPath.CommonIniPath);
            }
            if (checkBox_cs1.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "CS1BasicOnly", "1", SettingPath.CommonIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "CS1BasicOnly", "0", SettingPath.CommonIniPath);
            }
            if (checkBox_cs2.IsChecked == true)
            {
                IniFileHandler.WritePrivateProfileString("SET", "CS2BasicOnly", "1", SettingPath.CommonIniPath);
            }
            else
            {
                IniFileHandler.WritePrivateProfileString("SET", "CS2BasicOnly", "0", SettingPath.CommonIniPath);
            }

            foreach (ServiceItem2 info in serviceList)
            {
                UInt64 key = ((UInt64)info.ServiceInfo.ONID) << 32 | ((UInt64)info.ServiceInfo.TSID) << 16 | ((UInt64)info.ServiceInfo.SID);
                try
                {
                    if (info.IsSelected == true)
                    {
                        ChSet5.Instance.ChList[key].EpgCapFlag = 1;
                    }
                    else
                    {
                        ChSet5.Instance.ChList[key].EpgCapFlag = 0;
                    }
                }
                catch
                {
                }
            }

            IniFileHandler.WritePrivateProfileString("EPG_CAP", "Count", timeList.Count.ToString(), SettingPath.TimerSrvIniPath);
            for (int i = 0; i < timeList.Count; i++)
            {
                EpgCaptime item = timeList[i] as EpgCaptime;
                IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString(), item.Time, SettingPath.TimerSrvIniPath);
                if (item.IsSelected == true)
                {
                    IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString() + "Select", "1", SettingPath.TimerSrvIniPath);
                }
                else
                {
                    IniFileHandler.WritePrivateProfileString("EPG_CAP", i.ToString() + "Select", "0", SettingPath.TimerSrvIniPath);
                }
            }


            IniFileHandler.WritePrivateProfileString("SET", "NGEpgCapTime", textBox_ngCapMin.Text, SettingPath.TimerSrvIniPath);
            IniFileHandler.WritePrivateProfileString("SET", "NGEpgCapTunerTime", textBox_ngTunerMin.Text, SettingPath.TimerSrvIniPath);
        }

        private void button_allChk_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem2 info in this.serviceList)
            {
                info.IsSelected = true;
            }
        }

        private void button_videoChk_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem2 info in this.serviceList)
            {
                if (info.ServiceInfo.ServiceType == 0x01 || info.ServiceInfo.ServiceType == 0xA5)
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_allClear_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem2 info in this.serviceList)
            {
                info.IsSelected = false;
            }
        }

        private void button_addTime_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_HH.SelectedItem != null && comboBox_MM.SelectedItem != null)
            {
                UInt16 hh = (UInt16)comboBox_HH.SelectedItem;
                UInt16 mm = (UInt16)comboBox_MM.SelectedItem;
                String time = hh.ToString("D2") + ":" + mm.ToString("D2");

                foreach (EpgCaptime info in timeList)
                {
                    if (String.Compare(info.Time, time, true) == 0)
                    {
                        MessageBox.Show("すでに登録済みです");
                        return ;
                    }
                }
                EpgCaptime item = new EpgCaptime();
                item.IsSelected = true;
                item.Time = time;
                timeList.Add(item);
            }
        }

        private void button_delTime_Click(object sender, RoutedEventArgs e)
        {
            if (ListView_time.SelectedItem != null)
            {
                EpgCaptime item = ListView_time.SelectedItem as EpgCaptime;
                timeList.Remove(item);
            }
        }
    }

    class ServiceItem2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool selected = false;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        public ChSet5Item ServiceInfo
        {
            get;
            set;
        }
        public bool IsSelected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        public String ServiceName
        {
            get { return ServiceInfo.ServiceName; }
        }
        public String ToolTipView
        {
            get
            {
                if (Settings.Instance.NoToolTip == true)
                {
                    return null;
                }
                String viewTip = "";

                if (ServiceInfo != null)
                {
                    viewTip =
                        "service_name : " + ServiceInfo.ServiceName + "\r\n" +
                        "service_type : " + ServiceInfo.ServiceType.ToString() + "(0x" + ServiceInfo.ServiceType.ToString("X2") + ")" + "\r\n" +
                        "original_network_id : " + ServiceInfo.ONID.ToString() + "(0x" + ServiceInfo.ONID.ToString("X4") + ")" + "\r\n" +
                        "transport_stream_id : " + ServiceInfo.TSID.ToString() + "(0x" + ServiceInfo.TSID.ToString("X4") + ")" + "\r\n" +
                        "service_id : " + ServiceInfo.SID.ToString() + "(0x" + ServiceInfo.SID.ToString("X4") + ")" + "\r\n" +
                        "partial_reception : " + ServiceInfo.PartialFlag.ToString();
                }
                return viewTip;
            }
        }
    }

    class EpgCaptime : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool selected = false;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        public string Time
        {
            get;
            set;
        }
    }
}
