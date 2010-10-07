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

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// SearchKeyView.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchKeyView : UserControl
    {
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private List<ServiceItem> serviceList = new List<ServiceItem>();
        private Dictionary<Int64, ServiceItem> serviceDict = new Dictionary<long, ServiceItem>();
        private List<DateItem> dateList = new List<DateItem>();
        private List<ComponentKindInfo> videoList = new List<ComponentKindInfo>();
        private List<ComponentKindInfo> audioList = new List<ComponentKindInfo>();

        private SearchKeyInfo defKey = null;

        public SearchKeyView()
        {
            InitializeComponent();

            List<EpgServiceInfo> list = new List<EpgServiceInfo>();
            uint err = cmd.SendEnumService(ref list);
            foreach (EpgServiceInfo info in list)
            {
                Int64 key = ((Int64)info.ONID) << 32 | ((Int64)info.TSID) << 16 | (Int64)info.SID;
                ServiceItem item = new ServiceItem();
                item.ServiceInfo = info;
                serviceDict.Add(key, item);
                serviceList.Add(item);
            }
            listView_service.ItemsSource = serviceList;

            foreach (ComponentKindInfo info in EpgTimerDef.Instance.ComponentKindDictionary.Values)
            {
                if (info.StreamContent != 0x02)
                {
                    videoList.Add(info);
                }
            }
            comboBox_video.DataContext = videoList;
            comboBox_video.SelectedIndex = 0;

            foreach (ComponentKindInfo info in EpgTimerDef.Instance.ComponentKindDictionary.Values)
            {
                if (info.StreamContent == 0x02)
                {
                    audioList.Add(info);
                }
            }
            comboBox_audio.DataContext = audioList;
            comboBox_audio.SelectedIndex = 0;

            comboBox_content.DataContext = EpgTimerDef.Instance.ContentKindDictionary.Values;
            comboBox_content.SelectedIndex = 0;

            comboBox_start_week.DataContext = EpgTimerDef.Instance.DayOfWeekDictionary.Values;
            comboBox_start_week.SelectedIndex = 0;
            comboBox_start_hour.DataContext = EpgTimerDef.Instance.HourDictionary.Values;
            comboBox_start_hour.SelectedIndex = 0;
            comboBox_start_min.DataContext = EpgTimerDef.Instance.MinDictionary.Values;
            comboBox_start_min.SelectedIndex = 0;

            comboBox_end_week.DataContext = EpgTimerDef.Instance.DayOfWeekDictionary.Values;
            comboBox_end_week.SelectedIndex = 6;
            comboBox_end_hour.DataContext = EpgTimerDef.Instance.HourDictionary.Values;
            comboBox_end_hour.SelectedIndex = 23;
            comboBox_end_min.DataContext = EpgTimerDef.Instance.MinDictionary.Values;
            comboBox_end_min.SelectedIndex = 59;

            try
            {
                defKey = new SearchKeyInfo();
                defKey.AndKey = "";
                defKey.NotKey = "";
                defKey.RegExp = Settings.Instance.SearchKeyRegExp;
                defKey.AimaiFlag = Settings.Instance.SearchKeyAimaiFlag;
                defKey.TitleOnly = Settings.Instance.SearchKeyTitleOnly;
                defKey.ContentList = Settings.Instance.SearchKeyContentList;
                defKey.DateItemList = Settings.Instance.SearchKeyDateItemList;
                defKey.ServiceList = new List<Int64>();
                foreach (ChSet5Item info in ChSet5.Instance.ChList.Values)
                {
                    if (info.SearchFlag == 1)
                    {
                        defKey.ServiceList.Add((long)info.Key);
                    }
                }
            }
            catch
            {
                defKey = null;
            }
        }

        public bool GetSearchKey(ref EpgSearchKeyInfo key)
        {
            if (defKey != null)
            {
                key.andKey = defKey.AndKey;
                key.notKey = defKey.NotKey;
                if (defKey.RegExp == true)
                {
                    key.regExpFlag = 1;
                }
                else
                {
                    key.regExpFlag = 0;
                }
                if (defKey.TitleOnly == true)
                {
                    key.titleOnlyFlag = 1;
                }
                else
                {
                    key.titleOnlyFlag = 0;
                }

                foreach (ContentKindInfo info in defKey.ContentList)
                {
                    EpgContentData item = new EpgContentData();
                    item.content_nibble_level_1 = info.Nibble1;
                    item.content_nibble_level_2 = info.Nibble2;
                    key.contentList.Add(item);
                }

                foreach (DateItem info in defKey.DateItemList)
                {
                    key.dateList.Add(info.DateInfo);
                }

                foreach (Int64 info in defKey.ServiceList)
                {
                    key.serviceList.Add(info);
                }
                return true;
            }

            key.andKey = searchKeyWordView.comboBox_keyword.Text;
            key.notKey = searchKeyWordView.comboBox_notKeyword.Text;
            if (checkBox_titleOnly.IsChecked == true)
            {
                key.titleOnlyFlag = 1;
            }
            else
            {
                key.titleOnlyFlag = 0;
            }
            if (checkBox_regExp.IsChecked == true)
            {
                key.regExpFlag = 1;
            }
            else
            {
                key.regExpFlag = 0;
            }

            if (checkBox_aimai.IsChecked == true)
            {
                key.aimaiFlag = 1;
            }
            else
            {
                key.aimaiFlag = 0;
            }

            foreach (ServiceItem info in serviceList)
            {
                if (info.IsSelected == true)
                {
                    Int64 serviceKey = ((Int64)info.ServiceInfo.ONID) << 32 | ((Int64)info.ServiceInfo.TSID) << 16 | ((Int64)info.ServiceInfo.SID);
                    key.serviceList.Add(serviceKey);
                }
            }

            foreach (ContentKindInfo info in listBox_content.Items)
            {
                EpgContentData item = new EpgContentData();
                item.content_nibble_level_1 = info.Nibble1;
                item.content_nibble_level_2 = info.Nibble2;
                key.contentList.Add(item);
            }

            foreach (DateItem info in listBox_date.Items)
            {
                key.dateList.Add(info.DateInfo);
            }

            foreach (ComponentKindInfo info in listBox_video.Items)
            {
                UInt32 item = ((UInt32)info.StreamContent) << 8 | (UInt32)info.ComponentType;
                key.videoList.Add((UInt16)item);
            }

            foreach (ComponentKindInfo info in listBox_audio.Items)
            {
                UInt32 item = ((UInt32)info.StreamContent) << 8 | (UInt32)info.ComponentType;
                key.audioList.Add((UInt16)item);
            }

            return true;
        }

        public void GetSearchKey(ref SearchKeyInfo key)
        {
            if (defKey != null)
            {
                key = defKey;
            }
            key.RegExp = (bool)checkBox_regExp.IsChecked;
            key.AimaiFlag = (bool)checkBox_aimai.IsChecked;
            key.ContentList.Clear();
            foreach (ContentKindInfo info in listBox_content.Items)
            {
                key.ContentList.Add(info);
            }
            key.DateItemList.Clear();
            foreach (DateItem info in listBox_date.Items)
            {
                key.DateItemList.Add(info);
            }
            key.TitleOnly = (bool)checkBox_titleOnly.IsChecked;
            key.ServiceList = new List<long>();
            foreach (ServiceItem info in this.serviceList)
            {
                if (info.IsSelected == true)
                {
                    Int64 serviceKey = ((Int64)info.ServiceInfo.ONID) << 32 | ((Int64)info.ServiceInfo.TSID) << 16 | ((Int64)info.ServiceInfo.SID);
                    key.ServiceList.Add(serviceKey);
                }
            }

        }

        public bool GetServiceName(UInt16 original_network_id, UInt16 transport_stream_id, UInt16 service_id, ref String serviceName)
        {
            Int64 serviceKey = ((Int64)original_network_id) << 32 | ((Int64)transport_stream_id) << 16 | (Int64)service_id;
            if (serviceDict.ContainsKey(serviceKey) == true)
            {
                serviceName = serviceDict[serviceKey].ServiceName;
                return true;
            }
            else
            {
                serviceName = "";
                return false;
            }
        }

        private void button_content_add_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_content.SelectedItem != null)
            {
                foreach (ContentKindInfo info in listBox_content.Items)
                {
                    ContentKindInfo select = comboBox_content.SelectedItem as ContentKindInfo;
                    if (select.Nibble1 == info.Nibble1 && select.Nibble2 == info.Nibble2)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                listBox_content.Items.Add(comboBox_content.SelectedItem);
            }
        }

        private void button_content_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_content.SelectedItem != null)
            {
                listBox_content.Items.RemoveAt(listBox_content.SelectedIndex);
            }
        }

        private void button_date_add_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_start_week.SelectedItem == null ||
                comboBox_start_hour.SelectedItem == null ||
                comboBox_start_min.SelectedItem == null ||
                comboBox_end_week.SelectedItem == null ||
                comboBox_end_hour.SelectedItem == null ||
                comboBox_end_min.SelectedItem == null)
            {
                return;
            }

            DateItem item = new DateItem();
            EpgSearchDateInfo info = new EpgSearchDateInfo();
            DayOfWeekInfo startWeek = comboBox_start_week.SelectedItem as DayOfWeekInfo;
            DayOfWeekInfo endWeek = comboBox_end_week.SelectedItem as DayOfWeekInfo;

            info.startDayOfWeek = startWeek.Value;
            info.startHour = (UInt16)comboBox_start_hour.SelectedItem;
            info.startMin = (UInt16)comboBox_start_min.SelectedItem;
            info.endDayOfWeek = endWeek.Value;
            info.endHour = (UInt16)comboBox_end_hour.SelectedItem;
            info.endMin = (UInt16)comboBox_end_min.SelectedItem;

            String viewText = "";
            viewText = startWeek.DisplayName + " " + info.startHour.ToString("00") + ":" + info.startMin.ToString("00") +
                " ～ " + endWeek.DisplayName + " " + info.endHour.ToString("00") + ":" + info.endMin.ToString("00");

            item.DateInfo = info;
            item.ViewText = viewText;

            listBox_date.Items.Add(item);

        }

        private void button_date_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_date.SelectedItem != null)
            {
                listBox_date.Items.RemoveAt(listBox_date.SelectedIndex);
            }
        }

        private void button_all_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                info.IsSelected = true;
            }
        }

        private void button_video_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if (info.ServiceInfo.service_type == 0x01 || info.ServiceInfo.service_type == 0xA5)
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_bs_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if (info.ServiceInfo.ONID == 0x04 &&
                    (info.ServiceInfo.service_type == 0x01 || info.ServiceInfo.service_type == 0xA5))
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_cs_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if ((info.ServiceInfo.ONID == 0x06 || info.ServiceInfo.ONID == 0x07) &&
                    (info.ServiceInfo.service_type == 0x01 || info.ServiceInfo.service_type == 0xA5))
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_tere_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if ((0x7880 <= info.ServiceInfo.ONID && info.ServiceInfo.ONID <= 0x7FE8) &&
                    (info.ServiceInfo.service_type == 0x01 || info.ServiceInfo.service_type == 0xA5))
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_1seg_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if ((0x7880 <= info.ServiceInfo.ONID && info.ServiceInfo.ONID <= 0x7FE8) &&
                    info.ServiceInfo.partialReceptionFlag == 1)
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }
        }

        private void button_other_on_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                if (info.ServiceInfo.ONID != 0x04 &&
                    info.ServiceInfo.ONID != 0x06 &&
                    info.ServiceInfo.ONID != 0x07 &&
                    !(0x7880 <= info.ServiceInfo.ONID && info.ServiceInfo.ONID <= 0x7FE8)
                    )
                {
                    info.IsSelected = true;
                }
                else
                {
                    info.IsSelected = false;
                }
            }

        }

        private void button_all_off_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServiceItem info in this.serviceList)
            {
                info.IsSelected = false;
            }
        }

        public void SettingGUIMode(bool flag)
        {
            if (flag == true)
            {
                grid_search.RowDefinitions[0].Height = new GridLength(0);
/*                label1.Visibility = System.Windows.Visibility.Collapsed;
                label2.Visibility = System.Windows.Visibility.Collapsed;
                comboBox_keyword.Visibility = System.Windows.Visibility.Collapsed;
                comboBox_notKeyword.Visibility = System.Windows.Visibility.Collapsed;*/
            }
        }

        public void SetDefSearchKey(SearchKeyInfo key)
        {
            defKey = key;

        }

        public void SetDefSearchKey(EpgSearchKeyInfo key)
        {
            try
            {
                defKey = new SearchKeyInfo();
                defKey.AndKey = key.andKey;
                defKey.NotKey = key.notKey;
                if (key.regExpFlag == 1)
                {
                    defKey.RegExp = true;
                }
                else
                {
                    defKey.RegExp = false;
                }
                if (key.aimaiFlag == 1)
                {
                    defKey.AimaiFlag = true;
                }
                else
                {
                    defKey.AimaiFlag = false;
                }
                if (key.titleOnlyFlag == 1)
                {
                    defKey.TitleOnly = true;
                }
                else
                {
                    defKey.TitleOnly = false;
                }
                foreach (EpgContentData info in key.contentList)
                {
                    int nibble1 = info.content_nibble_level_1;
                    int nibble2 = info.content_nibble_level_2;
                    UInt16 contentKey = (UInt16)(nibble1 << 8 | nibble2);
                    if (EpgTimerDef.Instance.ContentKindDictionary.ContainsKey(contentKey) == true)
                    {
                        defKey.ContentList.Add(EpgTimerDef.Instance.ContentKindDictionary[contentKey]);
                    }
                }
                foreach (EpgSearchDateInfo info in key.dateList)
                {
                    DateItem item = new DateItem();

                    String viewText = "";

                    viewText = EpgTimerDef.Instance.DayOfWeekDictionary[info.startDayOfWeek].DisplayName + " " + info.startHour.ToString("00") + ":" + info.startMin.ToString("00") +
                        " ～ " + EpgTimerDef.Instance.DayOfWeekDictionary[info.endDayOfWeek].DisplayName + " " + info.endHour.ToString("00") + ":" + info.endMin.ToString("00");

                    item.DateInfo = info;
                    item.ViewText = viewText;

                    defKey.DateItemList.Add(item);
                }

                defKey.ServiceList = key.serviceList;
            }
            catch
            {
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefKey();
            defKey = null;
        }

        private void LoadDefKey()
        {
            if (defKey != null)
            {
                searchKeyWordView.comboBox_keyword.Text = defKey.AndKey;
                searchKeyWordView.comboBox_notKeyword.Text = defKey.NotKey;
                checkBox_regExp.IsChecked = defKey.RegExp;
                checkBox_aimai.IsChecked = defKey.AimaiFlag;
                foreach (ContentKindInfo info in defKey.ContentList)
                {
                    listBox_content.Items.Add(info);
                }
                foreach (DateItem info in defKey.DateItemList)
                {
                    listBox_date.Items.Add(info);
                }
                checkBox_titleOnly.IsChecked = defKey.TitleOnly;
                foreach (Int64 info in defKey.ServiceList)
                {
                    if (serviceDict.ContainsKey(info) == true)
                    {
                        serviceDict[info].IsSelected = true;
                    }
                }
            }
        }

        private void button_video_add_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_video.SelectedItem != null)
            {
                foreach (ComponentKindInfo info in listBox_video.Items)
                {
                    ComponentKindInfo select = comboBox_video.SelectedItem as ComponentKindInfo;
                    if (select.StreamContent == info.StreamContent && select.ComponentType == info.ComponentType)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                listBox_video.Items.Add(comboBox_video.SelectedItem);
            }
        }

        private void button_video_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_video.SelectedItem != null)
            {
                listBox_video.Items.RemoveAt(listBox_video.SelectedIndex);
            }
        }

        private void button_audio_add_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_audio.SelectedItem != null)
            {
                foreach (ComponentKindInfo info in listBox_audio.Items)
                {
                    ComponentKindInfo select = comboBox_audio.SelectedItem as ComponentKindInfo;
                    if (select.StreamContent == info.StreamContent && select.ComponentType == info.ComponentType)
                    {
                        MessageBox.Show("すでに追加されています");
                        return;
                    }
                }
                listBox_audio.Items.Add(comboBox_audio.SelectedItem);
            }
        }

        private void button_audio_del_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_audio.SelectedItem != null)
            {
                listBox_audio.Items.RemoveAt(listBox_audio.SelectedIndex);
            }
        }

        private void checkBox_regExp_Checked(object sender, RoutedEventArgs e)
        {
            checkBox_aimai.IsChecked = false;
        }

        private void checkBox_aimai_Checked(object sender, RoutedEventArgs e)
        {
            checkBox_regExp.IsChecked = false;
        }

    }

    public class SearchKeyInfo
    {
        public SearchKeyInfo()
        {
            AndKey = "";
            NotKey = "";
            RegExp = false;
            AimaiFlag = false;
            ContentList = new List<ContentKindInfo>();
            DateItemList = new List<DateItem>();
            TitleOnly = false;
            ServiceList = new List<long>();
        }
        public String AndKey
        {
            get;
            set;
        }
        public String NotKey
        {
            get;
            set;
        }
        public bool RegExp
        {
            get;
            set;
        }
        public bool AimaiFlag
        {
            get;
            set;
        }
        public List<ContentKindInfo> ContentList
        {
            get;
            set;
        }
        public List<DateItem> DateItemList
        {
            get;
            set;
        }
        public bool TitleOnly
        {
            get;
            set;
        }
        public List<Int64> ServiceList
        {
            get;
            set;
        }
    }

    class ServiceItem : INotifyPropertyChanged
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


        public EpgServiceInfo ServiceInfo
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
            get { return ServiceInfo.service_name; }
        }
        public String ToolTipView
        {
            get
            {
                String viewTip = "";

                if (ServiceInfo != null)
                {
                    viewTip =
                        "network_name : " + ServiceInfo.network_name + "\r\n" +
                        "ts_name : " + ServiceInfo.ts_name + "\r\n" +
                        "service_provider_name : " + ServiceInfo.service_provider_name + "\r\n" +
                        "service_name : " + ServiceInfo.service_name + "\r\n" +
                        "service_type : " + ServiceInfo.service_type.ToString() + "(0x" + ServiceInfo.service_type.ToString("X2") + ")" + "\r\n" +
                        "original_network_id : " + ServiceInfo.ONID.ToString() + "(0x" + ServiceInfo.ONID.ToString("X4") + ")" + "\r\n" +
                        "transport_stream_id : " + ServiceInfo.TSID.ToString() + "(0x" + ServiceInfo.TSID.ToString("X4") + ")" + "\r\n" +
                        "service_id : " + ServiceInfo.SID.ToString() + "(0x" + ServiceInfo.SID.ToString("X4") + ")" + "\r\n" +
                        "partial_reception : " + ServiceInfo.partialReceptionFlag.ToString();
                }
                return viewTip;
            }
        }
    }

}
