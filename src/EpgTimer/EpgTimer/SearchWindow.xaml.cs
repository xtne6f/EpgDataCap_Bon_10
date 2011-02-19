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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;

namespace EpgTimer
{
    /// <summary>
    /// SearchWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchWindow : Window
    {
        private CtrlCmdUtil cmd = EpgTimerDef.Instance.CtrlCmd;
        private List<SearchItem> resultList = new List<SearchItem>();

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private UInt32 autoAddID;

        public SearchWindow()
        {
            InitializeComponent();

            listView_result.DataContext = resultList;

        }

        public void SetViewMode(UInt16 mode)
        {
            if (mode == 1)
            {
                button_add_reserve.Visibility = System.Windows.Visibility.Hidden;
                button_add_epgAutoAdd.Visibility = System.Windows.Visibility.Visible;
                button_chg_epgAutoAdd.Visibility = System.Windows.Visibility.Hidden;

                searchKeyView.label_video.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.comboBox_video.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_video_add.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_video_del.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.listBox_video.Visibility = System.Windows.Visibility.Hidden;

                searchKeyView.label_audio.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.comboBox_audio.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_audio_add.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_audio_del.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.listBox_audio.Visibility = System.Windows.Visibility.Hidden;

                Title = "EPG予約条件";
            }
            else if (mode == 2)
            {
                button_add_reserve.Visibility = System.Windows.Visibility.Hidden;
                button_add_epgAutoAdd.Visibility = System.Windows.Visibility.Hidden;
                button_chg_epgAutoAdd.Visibility = System.Windows.Visibility.Visible;

                searchKeyView.label_video.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.comboBox_video.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_video_add.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_video_del.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.listBox_video.Visibility = System.Windows.Visibility.Hidden;

                searchKeyView.label_audio.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.comboBox_audio.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_audio_add.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.button_audio_del.Visibility = System.Windows.Visibility.Hidden;
                searchKeyView.listBox_audio.Visibility = System.Windows.Visibility.Hidden;

                Title = "EPG予約条件";
            }
            else
            {
                button_add_reserve.Visibility = System.Windows.Visibility.Visible;
                button_add_epgAutoAdd.Visibility = System.Windows.Visibility.Visible;
                button_chg_epgAutoAdd.Visibility = System.Windows.Visibility.Hidden;

                Title = "検索";
            }
        }

        public void SetDefSearchKey(SearchKeyInfo defKey)
        {
            searchKeyView.SetDefSearchKey(defKey);
        }
        
        public void SetDefSearchKey(EpgSearchKeyInfo defKey)
        {
            searchKeyView.SetDefSearchKey(defKey);
        }

        public void SetDefRecSetting(RecSettingData defKey)
        {
            recSettingView.SetDefRecSetting(defKey);
        }

        public void SetChgAutoAddID(UInt32 id)
        {
            autoAddID = id;
        }

        private void button_chg_epgAutoAdd_Click(object sender, RoutedEventArgs e)
        {
            EpgAutoAddData addItem = new EpgAutoAddData();
            addItem.dataID = autoAddID;
            EpgSearchKeyInfo searchKey = new EpgSearchKeyInfo();
            searchKeyView.GetSearchKey(ref searchKey);

            RecSettingData recSetKey = new RecSettingData();
            recSettingView.GetRecSetting(ref recSetKey);

            addItem.searchInfo = searchKey;
            addItem.recSetting = recSetKey;

            List<EpgAutoAddData> addList = new List<EpgAutoAddData>();
            addList.Add(addItem);

            if (cmd.SendChgEpgAutoAdd(addList) != 1)
            {
                MessageBox.Show("変更に失敗しました");
            }
            else
            {
                SearchPg();
            }
        }

        private void button_add_epgAutoAdd_Click(object sender, RoutedEventArgs e)
        {
            EpgAutoAddData addItem = new EpgAutoAddData();
            EpgSearchKeyInfo searchKey = new EpgSearchKeyInfo();
            searchKeyView.GetSearchKey(ref searchKey);

            RecSettingData recSetKey = new RecSettingData();
            recSettingView.GetRecSetting(ref recSetKey);

            addItem.searchInfo = searchKey;
            addItem.recSetting = recSetKey;

            List<EpgAutoAddData> addList = new List<EpgAutoAddData>();
            addList.Add(addItem);

            if (cmd.SendAddEpgAutoAdd(addList) != 1)
            {
                MessageBox.Show("追加に失敗しました");
            }
            else
            {
                SearchPg();
            }
        }

        private void button_add_reserve_Click(object sender, RoutedEventArgs e)
        {
            if (listView_result.SelectedItem == null)
            {
                MessageBox.Show("番組が選択されていません");
                return;
            }

            ReserveData item = new ReserveData();
            RecSettingData recSetting = new RecSettingData();
            if (recSettingView.GetRecSetting(ref recSetting) == false)
            {
                return;
            }
            item.RecSetting = recSetting;

            SearchItem select = listView_result.SelectedItem as SearchItem;
            item.Title = select.EventName;
            if (select.EventInfo.StartTimeFlag == 0)
            {
                MessageBox.Show("開始時間未定のため予約できません");
                return;
            }
            else
            {
                item.StartTime = select.EventInfo.start_time;
                item.StartTimeEpg = select.EventInfo.start_time;
            }
            if (select.EventInfo.DurationFlag == 0)
            {
                item.DurationSecond = 10*60;
            }
            else
            {
                item.DurationSecond = select.EventInfo.durationSec;
            }
            item.StationName = select.ServiceName;
            item.OriginalNetworkID = select.EventInfo.original_network_id;
            item.TransportStreamID = select.EventInfo.transport_stream_id;
            item.ServiceID = select.EventInfo.service_id;
            item.EventID = select.EventInfo.event_id;

            List<ReserveData> addList = new List<ReserveData>();
            addList.Add(item);

            uint err = cmd.SendAddReserve(addList);
            SearchPg();
        }
        
        private void tabItem_searchKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchPg();
            }
        }
        
        private void button_search_Click(object sender, RoutedEventArgs e)
        {
            SearchPg();
        }

        private void SearchPg()
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_result.DataContext);
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                dataView.Refresh();
            }
            listView_result.DataContext = null;
            resultList.Clear();

            List<ReserveData> reserve = new List<ReserveData>();
            uint err = cmd.SendEnumReserve(ref reserve);

            List<EpgSearchKeyInfo> keyList = new List<EpgSearchKeyInfo>();
            EpgSearchKeyInfo key = new EpgSearchKeyInfo();

            searchKeyView.GetSearchKey(ref key);

            keyList.Add(key);
            List<EpgEventInfo> list = new List<EpgEventInfo>();

            cmd.SendSearchPg(keyList, ref list);
            foreach (EpgEventInfo info in list)
            {
                SearchItem item = new SearchItem();
                item.EventInfo = info;
                foreach (ReserveData info2 in reserve)
                {
                    if (info.original_network_id == info2.OriginalNetworkID &&
                        info.transport_stream_id == info2.TransportStreamID &&
                        info.service_id == info2.ServiceID &&
                        info.event_id == info2.EventID)
                    {
                        item.IsReserved = true;
                        item.ReserveInfo = info2;
                        break;
                    }
                }

                String serviceName = "";
                searchKeyView.GetServiceName(info.original_network_id, info.transport_stream_id, info.service_id, ref serviceName);
                item.ServiceName = serviceName;
                resultList.Add(item);
            }

            listView_result.DataContext = resultList;
            searchKeyView.searchKeyWordView.SaveSearchLog();

            if (_lastHeaderClicked != null)
            {
                if (_lastHeaderClicked.Column.DisplayMemberBinding != null)
                {
                    string header = ((Binding)_lastHeaderClicked.Column.DisplayMemberBinding).Path.Path;
                    Sort(header, _lastDirection);
                }
                else
                {
                    Sort("Reserved", _lastDirection);
                }
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }
                    if (headerClicked.Column.DisplayMemberBinding != null)
                    {
                        string header = ((Binding)headerClicked.Column.DisplayMemberBinding).Path.Path;
                        Sort(header, direction);
                    }
                    else
                    {
                        Sort("Reserved", direction);
                    }

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }

        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_result.ItemsSource);

            dataView.SortDescriptions.Clear();

            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void listView_result_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView_result.SelectedItem != null)
            {
                SearchItem select = listView_result.SelectedItem as SearchItem;
                if (select.IsReserved == false)
                {
                    //通常
                    AddReserveWindow addDlg = new AddReserveWindow();
                    addDlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                    addDlg.SetEpgEventInfo(select.EventInfo);
                    if (addDlg.ShowDialog() == true)
                    {
                        SearchPg();
                    }
                }
                else
                {
                    ChangeReserveWindow dlg = new ChangeReserveWindow();
                    dlg.Owner = (Window)PresentationSource.FromVisual(this).RootVisual;
                    dlg.SetReserve(select.ReserveInfo);
                    if (select.EventInfo != null)
                    {
                        dlg.SetEpgEventInfo(select.EventInfo);
                    }
                    if (dlg.ShowDialog() == true)
                    {
                        if (dlg.DeleteEnd == false)
                        {
                            List<ReserveData> addList = new List<ReserveData>();
                            addList.Add(dlg.setInfo);
                            cmd.SendChgReserve(addList);
                        }
                        else
                        {
                            List<UInt32> deleteList = new List<uint>();
                            deleteList.Add(select.ReserveInfo.ReserveID);
                            cmd.SendDelReserve(deleteList);
                        }
                        SearchPg();
                    }
                }
            }
        }

    }

    class SearchItem
    {
        private EpgEventInfo eventInfo = null;
        private ReserveData reserveData = null;

        public EpgEventInfo EventInfo
        {
            get { return eventInfo; }
            set { eventInfo = value; }
        }
        public ReserveData ReserveInfo
        {
            get { return reserveData; }
            set { reserveData = value; }
        }
        public String EventName
        {
            get
            {
                String view = "";
                if (eventInfo != null)
                {
                    if (eventInfo.ShortInfo != null)
                    {
                        view = eventInfo.ShortInfo.event_name;
                    }
                }
                return view;
            }
        }
        public String ServiceName
        {
            get;
            set;
        }
        public String NetworkName
        {
            get
            {
                String view = "";
                if (eventInfo != null)
                {
                    if (0x7880 <= eventInfo.original_network_id && eventInfo.original_network_id <= 0x7FE8)
                    {
                        view = "地デジ";
                    }
                    else if (eventInfo.original_network_id == 0x0004)
                    {
                        view = "BS";
                    }
                    else if (eventInfo.original_network_id == 0x0006)
                    {
                        view = "CS1";
                    }
                    else if (eventInfo.original_network_id == 0x0007)
                    {
                        view = "CS2";
                    }
                    else
                    {
                        view = "その他";
                    }

                }
                return view;
            }
        }
        public String StartTime
        {
            get
            {
                String view = "未定";
                if (eventInfo != null)
                {
                    view = eventInfo.start_time.ToString("yyyy/MM/dd(ddd) HH:mm:ss");
                }
                return view;
            }
        }
        public bool IsReserved
        {
            get;
            set;
        }
        public String Reserved
        {
            get
            {
                String view = "";
                if (IsReserved == true)
                {
                    view = "予";
                }
                return view;
            }
        }
        public TextBlock ToolTipView
        {
            get
            {
                if (Settings.Instance.NoToolTip == true)
                {
                    return null;
                }
                String viewTip = "";

                if (eventInfo != null)
                {
                    if (eventInfo.StartTimeFlag == 1)
                    {
                        viewTip += eventInfo.start_time.ToString("yyyy/MM/dd(ddd) HH:mm:ss ～ ");
                    }
                    else
                    {
                        viewTip += "未定 ～ ";
                    }
                    if (eventInfo.DurationFlag == 1)
                    {
                        DateTime endTime = eventInfo.start_time + TimeSpan.FromSeconds(eventInfo.durationSec);
                        viewTip += endTime.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + "\r\n";
                    }
                    else
                    {
                        viewTip += "未定\r\n";
                    }

                    if (eventInfo.ShortInfo != null)
                    {
                        viewTip += eventInfo.ShortInfo.event_name + "\r\n\r\n";
                        viewTip += eventInfo.ShortInfo.text_char + "\r\n\r\n";
                    }
                    if (eventInfo.ExtInfo != null)
                    {
                        viewTip += eventInfo.ExtInfo.text_char + "\r\n\r\n";
                    }

                    //ジャンル
                    viewTip += "ジャンル :\r\n";
                    if (eventInfo.ContentInfo != null)
                    {
                        foreach (EpgContentData info in eventInfo.ContentInfo.nibbleList)
                        {
                            int nibble1 = info.content_nibble_level_1;
                            int nibble2 = info.content_nibble_level_2;
                            UInt16 contentKey1 = (UInt16)(nibble1 << 8 | 0xFF);
                            UInt16 contentKey2 = (UInt16)(nibble1 << 8 | nibble2);
                            if (EpgTimerDef.Instance.ContentKindDictionary.ContainsKey(contentKey1) == true)
                            {
                                viewTip += EpgTimerDef.Instance.ContentKindDictionary[contentKey1];
                            }
                            if (EpgTimerDef.Instance.ContentKindDictionary.ContainsKey(contentKey2) == true)
                            {
                                viewTip += " - " + EpgTimerDef.Instance.ContentKindDictionary[contentKey2];
                            }
                            viewTip += "\r\n";
                        }
                    }
                    viewTip += "\r\n";

                    //映像
                    viewTip += "映像 :";
                    if (eventInfo.ComponentInfo != null)
                    {
                        int streamContent = eventInfo.ComponentInfo.stream_content;
                        int componentType = eventInfo.ComponentInfo.component_type;
                        UInt16 componentKey = (UInt16)(streamContent << 8 | componentType);
                        if (EpgTimerDef.Instance.ComponentKindDictionary.ContainsKey(componentKey) == true)
                        {
                            viewTip += EpgTimerDef.Instance.ComponentKindDictionary[componentKey];
                        }
                        if (eventInfo.ComponentInfo.text_char.Length > 0)
                        {
                            viewTip += "\r\n";
                            viewTip += eventInfo.ComponentInfo.text_char;
                        }
                    }
                    viewTip += "\r\n";

                    //音声
                    viewTip += "音声 :\r\n";
                    if (eventInfo.AudioInfo != null)
                    {
                        foreach (EpgAudioComponentInfoData info in eventInfo.AudioInfo.componentList)
                        {
                            int streamContent = info.stream_content;
                            int componentType = info.component_type;
                            UInt16 componentKey = (UInt16)(streamContent << 8 | componentType);
                            if (EpgTimerDef.Instance.ComponentKindDictionary.ContainsKey(componentKey) == true)
                            {
                                viewTip += EpgTimerDef.Instance.ComponentKindDictionary[componentKey];
                            }
                            if (info.text_char.Length > 0)
                            {
                                viewTip += "\r\n";
                                viewTip += info.text_char;
                            }
                            viewTip += "\r\n";
                            viewTip += "サンプリングレート :";
                            switch (info.sampling_rate)
                            {
                                case 1:
                                    viewTip += "16kHz";
                                    break;
                                case 2:
                                    viewTip += "22.05kHz";
                                    break;
                                case 3:
                                    viewTip += "24kHz";
                                    break;
                                case 5:
                                    viewTip += "32kHz";
                                    break;
                                case 6:
                                    viewTip += "44.1kHz";
                                    break;
                                case 7:
                                    viewTip += "48kHz";
                                    break;
                                default:
                                    break;
                            }
                            viewTip += "\r\n";
                        }
                    }
                    viewTip += "\r\n";

                    //スクランブル
                    if (!(0x7880 <= eventInfo.original_network_id && eventInfo.original_network_id <= 0x7FE8))
                    {
                        if (eventInfo.FreeCAFlag == 0)
                        {
                            viewTip += "無料放送\r\n";
                        }
                        else
                        {
                            viewTip += "有料放送\r\n";
                        }
                        viewTip += "\r\n";
                    }

                    viewTip += "OriginalNetworkID : " + eventInfo.original_network_id.ToString() + " (0x" + eventInfo.original_network_id.ToString("X4") + ")\r\n";
                    viewTip += "TransportStreamID : " + eventInfo.transport_stream_id.ToString() + " (0x" + eventInfo.transport_stream_id.ToString("X4") + ")\r\n";
                    viewTip += "ServiceID : " + eventInfo.service_id.ToString() + " (0x" + eventInfo.service_id.ToString("X4") + ")\r\n";
                    viewTip += "EventID : " + eventInfo.event_id.ToString() + " (0x" + eventInfo.event_id.ToString("X4") + ")\r\n";
                }

                TextBlock block = new TextBlock();
                block.Text = viewTip;
                block.MaxWidth = 400;
                block.TextWrapping = TextWrapping.Wrap;
                return block;
            }
        }
    }


}
