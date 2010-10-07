﻿using System;
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
        private CtrlCmdUtil cmd = EpgTimerNW.NWConnect.Instance.cmd;
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

            uint err = cmd.SendChgEpgAutoAdd(addList);
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            } 
            else if ( err != 1)
            {
                MessageBox.Show("変更に失敗しました");
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

            uint err = cmd.SendAddEpgAutoAdd(addList);
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            }
            else if (err != 1)
            {
                MessageBox.Show("追加に失敗しました");
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
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
            }
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
            if (err == 205)
            {
                MessageBox.Show("サーバーに接続できませんでした");
                return;
            }

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

    }

    class SearchItem
    {
        private EpgEventInfo eventInfo = null;
        public EpgEventInfo EventInfo
        {
            get { return eventInfo; }
            set { eventInfo = value; }
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
